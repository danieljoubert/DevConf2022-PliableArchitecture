using System.Diagnostics;

namespace Demo
{
  public class RatePerSecondPerformanceCounter : PerformanceCounterBase
  {
    public RatePerSecondPerformanceCounter(string category, string name)
      : base(category, name, string.Empty)
    {
    }

    public RatePerSecondPerformanceCounter(string category, string name, string instance)
      : base(category, name, instance)
    {
    }

    public override CounterCreationData[] GetCounterCreationData()
    {
      return new[] { new CounterCreationData(Name, CounterHelp, PerformanceCounterType.RateOfCountsPerSecond64) };
    }

    public void Increment()
    {
      PerformanceCounter.Increment();
    }

    public void IncrementBy(long value)
    {
      PerformanceCounter.IncrementBy(value);
    }
  }
}