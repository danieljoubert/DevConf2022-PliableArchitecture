using System.Diagnostics;

namespace Demo
{
  public class AverageTimerPerformanceCounter : PerformanceCounterBase
  {
    public AverageTimerPerformanceCounter(string category, string name)
      : base(category, name, string.Empty)
    {
    }

    public AverageTimerPerformanceCounter(string category, string name, string instance)
      : base(category, name, instance)
    {
    }

    private PerformanceCounter _performanceCounterBase;

    public override CounterCreationData[] GetCounterCreationData()
    {
      return new[]
      {
        new CounterCreationData(Name, CounterHelp, PerformanceCounterType.AverageTimer32),
        new CounterCreationData(Name + "Base", CounterHelp, PerformanceCounterType.AverageBase)
      };
    }

    protected PerformanceCounter PerformanceCounterBase
    {
      get { return _performanceCounterBase ?? (_performanceCounterBase = new PerformanceCounter(Category, Name + "Base", Instance, false)); }
    }

    public void IncrementBy(long ticks, int actions)
    {
      PerformanceCounter.IncrementBy(ticks);
      PerformanceCounterBase.IncrementBy(actions);
    }
  }
}