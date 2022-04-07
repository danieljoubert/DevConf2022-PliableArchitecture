using System;
using System.Collections.Generic;
using System.Globalization;

namespace Demo
{
	public class DataContextWithCache : IDataContext
	{
		public DataContextWithCache(IDataContext dataContext, ICache cache)
		{
			_dataContext = dataContext;
			_cache = cache;
		}

		private readonly IDataContext _dataContext;
		private readonly ICache _cache;

		public IDataStorable Update(IDataStorable value)
		{
			return _dataContext.Update(value);
		}

		public ICollection<object> RetrieveQuery(string query, Type type)
		{
			string key = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", query, type.FullName);
			return (ICollection<object>)_cache.GetOrAdd(key, () => _dataContext.RetrieveQuery(query, type));
		}

		public void Delete(object itemToDelete)
		{
			_dataContext.Delete(itemToDelete);
		}
	}
}
