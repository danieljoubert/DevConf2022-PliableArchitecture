namespace Demo
{
	public interface IProcess<TIn>
	{
		void Execute(TIn value);
	}

	public class ProcessLink<TIn, TOut> : IProcess<TIn> where TOut : class
	{
		public ProcessLink(ITask<TIn, TOut> task)
			: this(task, new IProcess<TOut>[0])
		{
		}

		public ProcessLink(ITask<TIn, TOut> task, IProcess<TOut> nextProcess)
			: this(task, new[] { nextProcess })
		{
		}

		public ProcessLink(ITask<TIn, TOut> task, IProcess<TOut>[] nextProcesses)
		{
			_task = task;
			_nextProcesses = nextProcesses;
		}

		private readonly ITask<TIn, TOut> _task;
		private readonly IProcess<TOut>[] _nextProcesses;

		public void Execute(TIn value)
		{
			TOut result = _task.Execute(value);
			if (result == default(TOut))
				return;
			foreach (var nextProcess in _nextProcesses)
				nextProcess.Execute(result);
		}
	}
}
