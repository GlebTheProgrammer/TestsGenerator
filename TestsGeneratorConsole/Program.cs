﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorConsole
{


    internal class Program
    {
        static List<MethodInfo> GetTestMethods(Assembly assembly)
        {
            List<MethodInfo> testMethods = new List<MethodInfo>();
            Type[] types = assembly.GetExportedTypes();
            foreach (Type type in types)
            {
                MethodInfo[] methods = type.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                foreach (MethodInfo method in methods)
                {
                    TestMethodAttribute[] attributes = (TestMethodAttribute[])method
                    .GetCustomAttributes(typeof(TestMethodAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                        testMethods.Add(method);
                }
            }
            return testMethods;
        }


        static void Main(string[] args)
        {
            // Получаем информацию о текущей сборке 
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Запускаем метод для получения всех методов из текущей сборки для тестирования
            List<MethodInfo> methods = GetTestMethods(assembly);

            while (methods.Count != 0)
            {
                MethodInfo method = methods.First();

                Console.WriteLine("public class " + method.DeclaringType.FullName.Replace(method.DeclaringType.Namespace.ToString(), "").ToString().Replace(".", "").Replace("+", "_") + "TESTS");
                Console.WriteLine("{\n");

                List<MethodInfo> classMethods = methods.Where(methodItem => methodItem.DeclaringType == method.DeclaringType).ToList();
                foreach (var classMethod in classMethods)
                {
                    Console.WriteLine("\t[Fact]");
                    Console.Write($"\tpublic void {classMethod.Name}_");

                    if (classMethod.GetParameters().Length > 0)
                    {
                        Console.Write("With");
                        string paramStr = string.Empty;
                        foreach (var parameter in classMethod.GetParameters())
                        {
                            paramStr += $"{parameter.ParameterType.Name}And";
                        }
                        paramStr = paramStr.Remove(paramStr.Length - 3, 3);
                        paramStr += classMethod.GetParameters().Length == 1 ? "Parameter_" : "Parameters_";
                        Console.Write(paramStr);

                    }
                    else
                    {
                        Console.Write($"WithoutParameters_");
                    }

                    Console.WriteLine("ReturnAssertFailure");
                    Console.WriteLine("\t{");

                    Console.WriteLine("\t\tAssert.Fail(\"autogenerated\");");

                    Console.WriteLine("\n\t}\n");

                    methods.Remove(classMethod);
                }

                Console.WriteLine("}\n");

                methods.Remove(method);
            }
            
        }

    }




    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TestMethodAttribute : Attribute
    {
        public TestMethodAttribute()
        {
        }
        public TestMethodAttribute(string category)
        {
            Category = category;
        }
        public string Category { get; set; }
        public int Priority { get; set; }
    }


    public class Tests
    {
        [TestMethod("Тестируем", Priority = 1)]
        static void TestOne()
        {
            Console.WriteLine(nameof(TestOne));
        }
        [TestMethod("Тестируем", Priority = 1)]
        static void TestTwo()
        {
            Console.WriteLine(nameof(TestTwo));
        }
    }

    public class Gleb
    {
        public class Hleb
        {
            public class Bad
            {
                [TestMethod("Тестируем")]
                static string HelloHleb()
                {
                    Console.WriteLine("Hello");
                    return String.Empty;
                }
            }


            [TestMethod("Тестируем")]
            static string HelloHleb()
            {
                Console.WriteLine("Hello");
                return String.Empty;
            }
        }


        [TestMethod("Тестируем")]
        static string HelloWorld(string yoooooo)
        {
            Console.WriteLine("Hello");
            return String.Empty;
        }
    }
}
