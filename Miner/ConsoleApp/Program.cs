using ConsoleApp.config.maps;
using System;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            new NineDb().Mine();
            Console.ReadKey();
        }
    }
}
