using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestsGeneratorDll.Includes
{
    internal class XUnitTestsProvider
    {
        object syn = new object();

        List<string> xUnitTestsClasses = new List<string>();
        int numberOfCurrentlyWorkingThreads = 0;
        int restriction;

        public List<string> GetXUnitTests(List<MethodInfo> methodInfos, int restriction)
        {
            this.restriction = restriction;

            while (methodInfos.Count != 0)
            {
                MethodInfo method = methodInfos.First();

                List<MethodInfo> classMethods = new List<MethodInfo>(methodInfos.Where(methodItem => methodItem.DeclaringType == method.DeclaringType).ToList());

                GenerateTestsFromClassMethods(classMethods);

                foreach (var deletedMethod in classMethods)
                    methodInfos.Remove(deletedMethod);

            }

            lock (syn)
                if (numberOfCurrentlyWorkingThreads != 0)
                    Monitor.Wait(syn);

            return xUnitTestsClasses;
        }

        private async void GenerateTestsFromClassMethods(List<MethodInfo> methodInfos)
        {
            lock (syn)
            {
                if (numberOfCurrentlyWorkingThreads == restriction)
                    Monitor.Wait(syn);
                else
                    numberOfCurrentlyWorkingThreads++;
            }

            await DoWork(methodInfos);
        }

        private Task DoWork(List<MethodInfo> methodInfos)
        {
            StringBuilder result = new StringBuilder(string.Empty);

            MethodInfo method = methodInfos.First();

            result.Append("public class " + method.DeclaringType.FullName.Replace(method.DeclaringType.Namespace.ToString(), "").ToString().Replace(".", "").Replace("+", "_") + "TESTS\n");
            result.Append("{\n\n");

            foreach (var classMethod in methodInfos)
            {
                result.Append("\t[Fact]\n");
                result.Append($"\tpublic void {classMethod.Name}_");

                if (classMethod.GetParameters().Length > 0)
                {
                    result.Append("With");
                    string paramStr = string.Empty;
                    foreach (var parameter in classMethod.GetParameters())
                    {
                        paramStr += $"{parameter.ParameterType.Name.Replace("`","")}And";
                    }
                    paramStr = paramStr.Remove(paramStr.Length - 3, 3);
                    paramStr += classMethod.GetParameters().Length == 1 ? "Parameter_" : "Parameters_";
                    result.Append(paramStr);

                }
                else
                {
                    result.Append("WithoutParameters_");
                }

                result.Append("ReturnAssertFailure\n");
                result.Append("\t{\n");

                result.Append("\t\tAssert.Fail(\"autogenerated\");\n");

                result.Append("\t}\n\n");
            }

            result.Append("}\n\n");

            lock (syn)
            {
                xUnitTestsClasses.Add(result.ToString());
                numberOfCurrentlyWorkingThreads--;

                if (numberOfCurrentlyWorkingThreads == restriction - 1)
                    Monitor.Pulse(syn);

                if (numberOfCurrentlyWorkingThreads == 0)
                    Monitor.Pulse(syn);
            }

            return Task.CompletedTask;
        }


    }
}
