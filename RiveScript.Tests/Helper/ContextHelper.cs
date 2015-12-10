using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript.Tests.Helper
{
    public static class ContextHelper
    {
        public static string Property = "Context_Property";

        /// <summary>
        /// Allows setting the Entry Assembly when needed. 
        /// Use AssemblyUtilities.SetEntryAssembly() as first line in ad hoc tests
        /// </summary>
        /// <param name="assembly">Assembly to set as entry assembly</param>
        public static void SetEntryAssembly(Assembly assembly)
        {
            var manager = new AppDomainManager();
            if (manager != null)
            {
                var entryAssemblyfield = manager.GetType().GetField("m_entryAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
                entryAssemblyfield.SetValue(manager, assembly);
            }

            var domain = AppDomain.CurrentDomain;
            if (domain != null)
            {
                var domainManagerField = domain.GetType().GetField("_domainManager", BindingFlags.Instance | BindingFlags.NonPublic);
                domainManagerField.SetValue(domain, manager);
            }
        }

    }
}
