using BenchmarkDotNet.Running;
using System;

namespace Integration.Benchmark
{
    class Program
    {
        protected Program() { }

        static void Main(string[] args)
        {
            BenchmarkRunner.Run<EventBusBenchmark>();
        }
    }
}
