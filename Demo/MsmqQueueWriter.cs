using System.Messaging;

namespace Demo
{
	public class MsmqQueueWriter : IProcess<string>
	{
		public MsmqQueueWriter(string queuePath)
		{
			_messageQueue = new MessageQueue(queuePath);
		}

		private readonly MessageQueue _messageQueue;

		public void Execute(string value)
		{
			var msg = new Message(value);
			msg.Formatter = new BinaryMessageFormatter();
			_messageQueue.Send(msg);
		}
	}
}
