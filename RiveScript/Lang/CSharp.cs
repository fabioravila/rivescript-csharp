using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiveScript.lang
{
    public class CSharp : IObjectHandler
    {
        private CSharpCodeProvider provider = new CSharpCodeProvider();
        private IDictionary<string, string[]> codes = new Dictionary<string, string[]>();

        public string onCall(string name, string user, string[] args)
        {
            if (false == codes.ContainsKey(name))
                return "ERR: Could not find a object code for " + name + ".";

            //I will compile the code in memory
            //TODO: Make a cache of memory assemblies
            var parameters = new CompilerParameters();

            //Add some basic references
            //TODO: Add custom dlls names
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            //Adjust the code for a commom definition use
            var transformed = new List<string>();
            var code = codes[name];

            var className = "object_" + name;

            //TODO: Put custom usings
            transformed.Add("using System;");
            transformed.Add("namespace RiveScript");
            transformed.Add("{");
            transformed.Add("   public class " + className);
            transformed.Add("   {");
            transformed.Add("       public static string method(string user, string[] args)");
            transformed.Add("       {");

            for (int i = 0; i < code.Length; i++)
            {
                transformed.Add(code[i]);
            }

            transformed.Add("       }");
            transformed.Add("   }");
            transformed.Add("}");

            var result = provider.CompileAssemblyFromSource(parameters, String.Join(Environment.NewLine, transformed));
            if (result.Errors.HasErrors)
            {
                var sb = new StringBuilder();

                foreach (CompilerError error in result.Errors)
                {
                    sb.AppendLine(string.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }

                throw new InvalidOperationException("ERR: " + sb.ToString());
            }


            var objectCsharp = result.CompiledAssembly.GetType("RiveScript." + className);

            var method = objectCsharp.GetMethod("method");

            return (string)method.Invoke(null, new object[] { user, args });
        }

        public bool onLoad(string name, string[] code)
        {
            ValidateCode(code);

            if (false == codes.ContainsKey(name))
                codes.Add(name, code);
            else
                codes[name] = code;

            return true;
        }


        private void ValidateCode(string[] code)
        {
            if (code == null || code.Length == 0)
                throw new InvalidOperationException("No source code found");

            var codestring = string.Join("", code);

            if (code.Contains("namespace") || code.Contains("lass") || code.Contains("struct"))
                throw new InvalidOperationException("The object csharp code do nor have a class or namespace, just a usings and inner code of a  |string method(string user, string[] args)|");

        }
    }
}
