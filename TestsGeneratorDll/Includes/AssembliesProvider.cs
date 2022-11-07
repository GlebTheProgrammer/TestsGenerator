using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace TestsGeneratorDll.Includes
{
    internal class AssembliesProvider
    {
        int runningCount;
        object sync = new object();

        List<string> filePaths;
        List<Assembly> filesAssemblies = new List<Assembly>();

        public List<Assembly> GetAssemblies(List<string> filePaths, int restriction)
        {
            this.filePaths = filePaths;

            List<Func<string, Assembly>> functionsToExecute = new List<Func<string, Assembly>>();

            for(int i = 0; i < filePaths.Count; i++)
                functionsToExecute.Add(GetAssembly);


            GetAssembliesByUsingMonitor(functionsToExecute, restriction);

            return filesAssemblies;

        }


        public void GetAssembliesByUsingMonitor(List<Func<string, Assembly>> functions, int restriction)
        {
            object param;
            int filePathIndex = 0;

            runningCount = functions.Count;
            ThreadPool.SetMaxThreads(restriction, restriction);

            

            foreach (Func<string, Assembly> function in functions)
            {
                param = new List<object>() { function, filePathIndex };

                ThreadPool.QueueUserWorkItem(AddAssemblyAction, param);

                filePathIndex += 1;
            }

            lock (sync)
                if (runningCount > 0)
                    Monitor.Wait(sync);
        }

        private void AddAssemblyAction(object state)
        {
            List<object> parameters = (List<object>)state;

            var function = (Func<string, Assembly>)parameters[0];
            string filePath = filePaths[(int)parameters[1]];

            Assembly assembly = function(filePath);

            lock (sync)
            {
                filesAssemblies.Add(assembly);

                runningCount--;

                if (runningCount == 0)
                    Monitor.Pulse(sync);
            }
        }







        // Method for getting assembly from file path
        private Assembly GetAssembly(string filePath)
        {
            return Assembly.LoadFile(filePath);
        }

    }
}
