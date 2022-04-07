using System;
using System.Messaging;

namespace Demo
{
	public interface IQueueReader
	{
		void Dequeue();
	}

	public class MsmqQueueReader : IQueueReader
	{
		public MsmqQueueReader(string queuePath, IProcess<string> task)
			: this(queuePath, new[] { task })
		{
		}

		public MsmqQueueReader(string queuePath, IProcess<string>[] tasks)
		{
			_messageQueue = new MessageQueue(queuePath);
			_messageQueue.Formatter = new BinaryMessageFormatter();
			_tasks = tasks;
		}

		private readonly MessageQueue _messageQueue;
		private readonly IProcess<string>[] _tasks;

		public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(1);
		public void Dequeue()
		{
			try
			{
				var msg = _messageQueue.Receive(ReceiveTimeout);
				string text = msg.Body as string;
				foreach (var task in _tasks)
					task.Execute(text);
			}
			catch (MessageQueueException ex)
			{
				if (ex.ErrorCode == -2147467259) //Timeout reading from queue.
					return;
				throw;
			}
		}
	}
}
