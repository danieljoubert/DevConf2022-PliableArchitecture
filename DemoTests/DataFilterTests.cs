using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Demo.Fakes;
using System;

namespace Demo.Tests
{
	[TestClass()]
	public class DataFilterTests
	{
		[TestMethod()]
		public void ExecuteTest1()
		{
			IDataContext dataContext = new StubIDataContext()
			{
				RetrieveQueryStringType = (s, t) => new List<object>()
			};
			var target = new DataFilter(dataContext);
			Location location = new Location();
			Location expected = location;
			Location actual = target.Execute(location);
			Assert.IsNull(actual);
		}

		[TestMethod()]
		public void ExecuteTest2()
		{
			IDataContext dataContext = new StubIDataContext()
			{
				RetrieveQueryStringType = (s, t) => new List<object>() { new Device() }
			};
			var target = new DataFilter(dataContext);
			Location location = new Location();
			Location expected = location;
			Location actual = target.Execute(location);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod()]
		public void ExecuteTest3()
		{
			IDataContext dataContext = new StubIDataContext();
			var target = new DataFilter(dataContext);
			Location location = null;
			Location expected = location;
			Location actual = target.Execute(location);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod()]
		[ExpectedException(exceptionType: typeof(ArgumentNullException))]
		public void ExecuteTest4()
		{
			IDataContext dataContext = null;
			var target = new DataFilter(dataContext);
			Location location = null;
			Location expected = location;
			Location actual = target.Execute(location);
			Assert.AreEqual(expected, actual);
		}

	}
}