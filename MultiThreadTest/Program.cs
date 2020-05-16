using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadTest
{
    class Program
    {
        static List<Task> tasks = new List<Task>();

        private static object o = new object();

        static long minValue = 1_000_000_000;
        static long maxValue = 2_000_000_000;

        static long result;
        static readonly long oneThreadNumber = (maxValue - minValue) / 10;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var th = Thread.CurrentThread; th.Name = "Main";

            var t = DateTime.Now;
            for (long i = minValue; i < maxValue; i += oneThreadNumber)
            {
                var i1 = i;
                tasks.Add(new Task(() => CountNums(i1)));
            }

            foreach (var s in tasks) s.Start();

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"Время с многопоточностью - {(DateTime.Now - t).TotalMilliseconds} мс:  {result}\n");


            Console.ReadKey();
        }

        static void CountNums(long minNumber)
        {
            int res = 0;
            for (long i = minNumber; i < minNumber + oneThreadNumber; i++)
            {
                if (i % 10 != 0)
                {
                    int num = i.ToString().Sum(c => c - '0');
                    if (num % (i % 10) != 0) continue;
                        res++;
                }
                
            }
            lock (o)
            {
                result += res;
            }
        }
        
    }
}
