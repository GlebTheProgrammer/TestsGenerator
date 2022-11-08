using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace TestsGeneratorDll.Includes
{
    internal class MethodsProvider
    {
        object syn = new object();

        int numberOfTasksToComplete;
        List<MethodInfo> methodInfos = new List<MethodInfo>();

        public List<MethodInfo> GetMethodsInfo(List<Assembly> assemblies)
        {
            numberOfTasksToComplete = assemblies.Count;

            foreach (var assembly in assemblies)
            {
                ThreadPool.QueueUserWorkItem(GetTestMethodsFromASingleAssembly, assembly);
            }

            lock (syn)
                if (numberOfTasksToComplete > 0)
                    Monitor.Wait(syn);

            return methodInfos;
        }


        //  MethodInfo[] methods = type.GetMethods( BindingFlags.Public);


        private void GetTestMethodsFromASingleAssembly(object assemblyObj)
        {
            var assembly = (Assembly)assemblyObj;

            Type[] types = assembly.GetExportedTypes();

            foreach (Type type in types)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Instance);
                foreach (MethodInfo method in methods)
                {
                    lock (syn)
                        methodInfos.Add(method);
                }
            }

            lock (syn)
            {
                numberOfTasksToComplete -= 1;

                if(numberOfTasksToComplete == 0)
                    Monitor.Pulse(syn);
            }
        }
    }
}
