using BenchmarkDotNet.Running;

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
