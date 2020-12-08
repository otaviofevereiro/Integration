using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Integration.Tests;

namespace Integration.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart, targetCount: 10000)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class EventBusBenchmark
    {
        [Benchmark]
        public void Test1()
        {
            new EventBusTest().Test1();
        }
    }
}
