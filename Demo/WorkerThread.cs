using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
	public class WorkerThread : IDisposable
	{
		public WorkerThread(object instance, MethodInfo methodInfo, object[] parameters, TimeSpan sleep, int noThreads)
		{
			_instance = instance;
			_methodInfo = methodInfo;
			_parameters = parameters;
			_sleep = sleep;
			_tasks = Enumerable.Range(0, noThreads).Select(i => new Task(Run, TaskCreationOptions.LongRunning)).ToArray();
		}

		private Task[] _tasks;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private readonly object _instance;
		private readonly MethodInfo _methodInfo;
		private readonly object[] _parameters;
		private readonly TimeSpan _sleep;

		public void Start()
		{
			_tasks.ToList().ForEach(t => t.Start());
		}

		private void Run()
		{
			while (!_cancellationTokenSource.Token.IsCancellationRequested)
			{
				try
				{
					_methodInfo.Invoke(_instance, _parameters);
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException != null)
						Debug.WriteLine(ex.InnerException.Message);
					Debug.WriteLine(ex.Message);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
				finally
				{
					Thread.Sleep(_sleep);
				}
			}
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_tasks.ToList().ForEach(t => t.Dispose());
		}
	}
}

