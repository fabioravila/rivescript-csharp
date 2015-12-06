using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RiveScript.lang
{
    public class CSharp : IObjectHandler
    {
        //THINK: Shoud i have cache assemblies, objects or func delegates
        private IDictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        private string ns = "RiveScript.Objects";


        public string onCall(string name, RiveScript rs, string[] args)
        {
            if (false == assemblies.ContainsKey(name))
                return "ERR: Could not find a object code for " + name + ".";

            var ass = assemblies[name];

            var method = ass.GetType(ns + "." + name).GetMethod("method");

            var del = (Func<RiveScript, string[], string>)Delegate.CreateDelegate(typeof(Func<RiveScript, string[], string>), method);

            return del(rs, args);
        }

        public bool onLoad(string name, string[] code)
        {
            ValidateCode(name, code);
            var ass = CreateAssembly(name, code);

            if (false == assemblies.ContainsKey(name))
            {
                assemblies.Add(name, ass);
            }
            else
            {
                assemblies[name] = ass;
            }

            return true;
        }


        private Assembly CreateAssembly(string name, string[] code)
        {
            /*
            * The For now we will create one assembly for onLoad code call
            * on memory to cache and avoid IO problems.
            * To allow user add references a new keyword was created
            * Ex: 
            * reference System.Data.dll
            * reference CustomAssembly.dll
            * reference Custom2
            *
            * All the *using* and *reference* have to be befora all the statement code
            */
            var parameters = new CompilerParameters();
            var final = new List<string> {
                                           "using System;",
                                           "using RiveScript;"
                                            };

            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            parameters.ReferencedAssemblies.Add("System.dll");//Add basic assemblies
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("RiveScript.dll");


            //Find all references and usings
            for (int i = 0; i < code.Length; i++)
            {
                var line = code[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();
                if (line.StartsWith("reference"))
                {
                    var ra = Extract(line);
                    ra = ra.EndsWith(".dll") ? ra : (ra + ".dll");
                    parameters.ReferencedAssemblies.Add(ra);
                    code[i] = "";
                }
                else if (line.StartsWith("using"))
                {
                    var ra = Extract(line);
                    final.Add(ra); //Early add usings
                    code[i] = "";
                }
            }


            final.Add("namespace " + ns);
            final.Add("{");
            final.Add("   public class " + name);
            final.Add("   {");
            final.Add("       public static string method(RiveScript rs, string[] args)");
            final.Add("       {");
            final.AddRange(code);
            final.Add("       }");
            final.Add("   }");
            final.Add("}");


            using (var provider = new CSharpCodeProvider())
            {
                var result = provider.CompileAssemblyFromSource(parameters, String.Join(Environment.NewLine, final));
                if (result.Errors.HasErrors)
                {
                    var sr = "";
                    foreach (CompilerError error in result.Errors)
                    {
                        sr += string.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText);
                    }

                    throw new InvalidOperationException("ERR: object " + name + " - " + sr);
                }

                return result.CompiledAssembly;
            }
        }

        public string Extract(string line)
        {
            return line.Replace("referente", "")
                       .Replace(" ", "")
                       .Replace(";", "");
        }

        public void ValidateCode(string name, string[] code)
        {
            if (code == null || code.Length == 0)
                throw new InvalidOperationException("ERR: object " + name + " - No source code found");

            var cs = string.Join("", code);

            if (cs.Contains("namespace ") || cs.Contains("class ") || cs.Contains("struct "))
                throw new InvalidOperationException("ERR: object " + name + " - Csharp code do not have class or namespace, just a using, reference and inner code of a |string method(RiveScript rs, string[] args)|");

            if (false == cs.Contains("return"))
                throw new InvalidOperationException("ERR: object " + name + " - Has no return statement");
        }
    }
}
