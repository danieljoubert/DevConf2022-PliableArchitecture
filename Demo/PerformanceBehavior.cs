using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Interception.InterceptionBehaviors;
using Unity.Interception.PolicyInjection.Pipeline;

namespace Demo
{
  public class PerformanceBehavior : IInterceptionBehavior
  {
    private readonly ConcurrentDictionary<string, AverageTimerPerformanceCounter> _avgPerformanceCounters =
      new ConcurrentDictionary<string, AverageTimerPerformanceCounter>();
    private readonly ConcurrentDictionary<string, RatePerSecondPerformanceCounter> _ratePerformanceCounters =
      new ConcurrentDictionary<string, RatePerSecondPerformanceCounter>();

    public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
    {
      Guard.ArgumentNotNull(input, nameof(input));
      Guard.ArgumentNotNull(getNext, nameof(getNext));

      Stopwatch sw = new Stopwatch();
      sw.Start();
      IMethodReturn result = getNext()(input, getNext);
      sw.Stop();

      UpdateCounters(input, sw);
      return result;
    }

    private void UpdateCounters(IMethodInvocation input, Stopwatch sw)
    {
      if (PerformanceCounterCategory.Exists(CounterCategory))
      {
        string key = input.Target.GetType().Name + ":" + input.MethodBase.Name;
        UpdateCounters(key, sw);
      }
    }

    private void UpdateCounters(string key, Stopwatch sw)
    {
      if (PerformanceCounterCategory.CounterExists("Duration", CounterCategory))
      {
        AverageTimerPerformanceCounter avgCounter = _avgPerformanceCounters.GetOrAdd(key, AddTimerCounter);
        avgCounter.IncrementBy(sw.ElapsedTicks, 1);
      }

      if (PerformanceCounterCategory.CounterExists("Rate", CounterCategory))
      {
        RatePerSecondPerformanceCounter rateCounter = _ratePerformanceCounters.GetOrAdd(key, AddRateCounter);
        rateCounter.Increment();
      }
    }

    private AverageTimerPerformanceCounter AddTimerCounter(string instance)
    {
      string prefix = InstancePrefix ?? string.Empty;
      return new AverageTimerPerformanceCounter(CounterCategory, "Duration", prefix + instance);
    }

    private RatePerSecondPerformanceCounter AddRateCounter(string instance)
    {
      string prefix = InstancePrefix ?? string.Empty;
      return new RatePerSecondPerformanceCounter(CounterCategory, "Rate", prefix + instance);
    }

    public IEnumerable<Type> GetRequiredInterfaces()
    {
      return new Type[0];
    }

    public bool WillExecute { get { return true; } }

    public string CounterCategory { get; set; }

    public string InstancePrefix { get; set; }
  }
}
