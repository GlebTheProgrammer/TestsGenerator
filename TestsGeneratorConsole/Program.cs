using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestsGeneratorDll;

namespace TestsGeneratorConsole
{


    internal class Program
    {
        static void Main(string[] args)
        {

            List<string> files = new List<string>()
            {
                @"C:\Users\Gleb\Desktop\For Git\TestsGenerator\TestsGeneratorConsole\bin\Debug\TestsGeneratorConsole.exe",
                @"C:\Users\Gleb\Desktop\For Git\TestsGenerator\TestsGeneratorDll\bin\Debug\TestsGeneratorDll.dll"
            };
            TestsGenerator.GenerateXUnitTests(files, @"C:\Users\Gleb\Desktop\Results", 10);


        }

    }


    public class Tests
    {

        static void TestOne()
        {
            Console.WriteLine(nameof(TestOne));
        }

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
                static string HelloHleb()
                {
                    Console.WriteLine("Hello");
                    return String.Empty;
                }
            }



            static string HelloHleb()
            {
                Console.WriteLine("Hello");
                return String.Empty;
            }
        }


        static string HelloWorld(string yoooooo)
        {
            Console.WriteLine("Hello");
            return String.Empty;
        }
    }



    public class MyClass
    {
        public void FirstMethod()
        {
            Console.WriteLine("First method");
        }

        public void SecondMethod()
        {
            Console.WriteLine("Second method");
        }

        public void ThirdMethod(int a)
        {
            Console.WriteLine("Third method (int)");
        }

        public void ThirdMethod(double a)
        {
            Console.WriteLine("Third method (double)");
        }
    }
}
