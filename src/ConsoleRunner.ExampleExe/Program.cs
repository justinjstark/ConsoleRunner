using System;
using System.Threading;

namespace ConsoleRunner.ExampleExe
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(TimeSpan.FromSeconds(double.Parse(args[0])));

            if (args.Length > 1)
            {
                if (args[1] == "CRASH") throw new Exception("OH NOES!");

                Console.WriteLine(args[1]);
            }
        }
    }
}
