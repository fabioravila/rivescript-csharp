using Microsoft.CSharp;
using RiveScript.Macro;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;

namespace RiveScript.Lang
{
    public class CSharpObjectHandler : IObjectHandler
    {
        const string ns = "RiveScript.Objects";
        static string rs_engine_class_name = typeof(RiveScriptEngine).Name;
        readonly string currentAssembly = null;
        readonly string riveAssembly;

        readonly IDictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        readonly IDictionary<string, ISubroutine> macros = new Dictionary<string, ISubroutine>();
        readonly RiveScriptEngine rs;

        public CSharpObjectHandler(RiveScriptEngine rs) : this(rs, true) { }

        public CSharpObjectHandler(RiveScriptEngine rs, bool tryAddEntryAssembly)
        {
            this.rs = rs ?? throw new ArgumentNullException(nameof(rs), "RiveScript instance muts not be null");

            //Get entry assembly name
            if (tryAddEntryAssembly && Assembly.GetEntryAssembly() != null)
            {
                currentAssembly = System.IO.Path.GetFileName(Assembly.GetEntryAssembly().Location);
            }

            riveAssembly = System.IO.Path.GetFileName(typeof(IObjectHandler).Assembly.Location);
        }

        public string Call(string name, RiveScriptEngine rs, string[] args)
        {
            if (!macros.ContainsKey(name))
                return "ERR: Could not find a object code for " + name + ".";

            return macros[name].Call(rs, args);
        }

        public void Load(string name, string[] code)
        {
            ValidateCode(name, code);

            var ass = CreateAssembly(name, code);
            var method = ass.GetType(ns + "." + name).GetMethod("method");
            var del = (Func<RiveScriptEngine, string[], string>)Delegate.CreateDelegate(typeof(Func<RiveScriptEngine, string[], string>), method);

            assemblies.AddOrUpdate(name, ass);
            macros.AddOrUpdate(name, new DelegateMacro(del));
        }

        public void AddSubroutine(string name, ISubroutine subroutine)
        {
            if (subroutine == null)
                throw new ArgumentNullException(nameof(subroutine), "Subroutine must not be null");

            macros.AddOrUpdate(name, subroutine);
        }



        protected Assembly CreateAssembly(string name, string[] code)
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

            //Add refernce to RiveScript assembly
            parameters.ReferencedAssemblies.Add(riveAssembly);
            //Add reference to current execution assemblie
            if (false == string.IsNullOrWhiteSpace(currentAssembly))
                parameters.ReferencedAssemblies.Add(currentAssembly);


            //Find all references and usings
            for (int i = 0; i < code.Length; i++)
            {
                var line = code[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();
                if (line.StartsWith("reference"))
                {
                    var ra = line.Replace("reference", "")
                                 .Replace(" ", "")
                                 .Replace(";", "");

                    ra = ra.EndsWith(".dll") ? ra : (ra + ".dll");
                    parameters.ReferencedAssemblies.Add(ra);
                    code[i] = "";
                }
                else if (line.StartsWith("using") && false == line.StartsWith("using (")
                                                  && false == line.StartsWith("using("))
                {
                    final.Add(line); //Early add usings
                    code[i] = "";
                }
            }

            final.Add("namespace " + ns);
            final.Add("{");
            final.Add("   public class " + name);
            final.Add("   {");
            final.Add($"       public static string method({rs_engine_class_name} rs, string[] args)");
            final.Add("       {");
            final.AddRange(code);
            final.Add("       }");
            final.Add("   }");
            final.Add("}");


            using (var provider = new CSharpCodeProvider())
            {
                var result = provider.CompileAssemblyFromSource(parameters, string.Join(Environment.NewLine, final));
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

        protected void ValidateCode(string name, string[] code)
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
