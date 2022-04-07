using System.Diagnostics;

namespace Demo
{
  public abstract class PerformanceCounterBase
  {
    protected PerformanceCounterBase(string category, string name, string instance)
    {
      _category = category;
      _name = name;
      _instance = instance;
      CounterHelp = string.Empty;
    }

    private readonly string _category;
    private readonly string _name;
    private readonly string _instance;
    private PerformanceCounter _performanceCounter;

    public abstract CounterCreationData[] GetCounterCreationData();

    protected PerformanceCounter PerformanceCounter
    {
      get { return _performanceCounter ?? (_performanceCounter = new PerformanceCounter(_category, _name, _instance, false)); }
    }

    public string Name { get { return _name; } }

    public string Category { get { return _category; } }

    public string Instance { get { return _instance; } }

    public string CounterHelp { get; set; }
  }
}
