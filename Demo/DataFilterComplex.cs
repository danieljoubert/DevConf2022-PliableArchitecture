using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Demo
{
	public class DataFilterComplex : IDisposable
	{
		public DataFilterComplex(int noThreads, string sourceQueuePath, string destinationQueuePath, string connectionString)
		{
			_tasks = Enumerable.Range(0, noThreads).Select(i => new Task(Process, TaskCreationOptions.LongRunning)).ToArray();
			_sourceQueue = new MessageQueue(sourceQueuePath);
			_sourceQueue.Formatter = new BinaryMessageFormatter();
			_destinationQueue = new MessageQueue(destinationQueuePath);
			_destinationQueue.Formatter = new BinaryMessageFormatter();
			_dbConnection = new SqlConnection(connectionString);
		}

		private Task[] _tasks;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private readonly MessageQueue _sourceQueue;
		private readonly MessageQueue _destinationQueue;
		private readonly SqlConnection _dbConnection;

		public void Process()
		{
			List<string> imeis = new List<string>();
			using (SqlCommand cmd = new SqlCommand("select Imei from Vehicle", _dbConnection))
			using (SqlDataReader dr = cmd.ExecuteReader())
			{
				if (dr.NextResult())
					while (dr.Read())
					{
						imeis.Add(dr["Imei"].ToString());
					}
			}

			while (!_cancellationTokenSource.Token.IsCancellationRequested)
			{
				try
				{
					var msg = _sourceQueue.Receive();
					var xml = msg.Body as string;

					var xmlLocation = XElement.Parse(xml);
					var imei = ((IEnumerable)xmlLocation.XPathEvaluate("/Imei")).Cast<XElement>().FirstOrDefault()?.Value;

					if (imeis.Contains(imei))
					{
						var sendMessage = new Message(xml);
						sendMessage.Formatter = new BinaryMessageFormatter();
						_destinationQueue.Send(sendMessage);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
		}

		public void Dispose()
		{
			_cancellationTokenSource.Dispose();
			_tasks.ToList().ForEach(t => t.Dispose());
			_sourceQueue?.Dispose();
			_destinationQueue?.Dispose();
			_dbConnection?.Dispose();
		}
	}
}
