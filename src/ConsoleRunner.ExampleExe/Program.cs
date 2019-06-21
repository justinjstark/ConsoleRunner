using System;
using System.Threading;

namespace ConsoleRunner.ExampleExe
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "CRASH") throw new Exception("OH NOES!");

            Console.WriteLine(args[0]);

            Thread.Sleep(TimeSpan.FromSeconds(double.Parse(args[1])));
        }
    }
}
