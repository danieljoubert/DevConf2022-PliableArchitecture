using System;
using System.Linq;
using System.Collections.Concurrent;

namespace Demo
{
	public interface ICache
	{
		object GetOrAdd(string key, Func<object> addValueFactory);
		void RemoveExpiredValues();
	}

	public class CacheItem
	{
		public CacheItem(object value)
		{
			Value = value;
			Created = DateTime.Now;
		}

		public object Value { get; private set; }

		public DateTime Created { get; private set; }
	}

	public class Cache : ICache
	{
		private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

		public TimeSpan CacheDuration { get; set; }

		public object GetOrAdd(string key, Func<object> addValueFactory)
		{
			CacheItem cacheItem = _cache.GetOrAdd(key, k => new CacheItem(addValueFactory()));
			return cacheItem.Value;
		}

		public void RemoveExpiredValues()
		{
			DateTime now = DateTime.Now;
			_cache
				.Where(kv => kv.Value.Created.Add(CacheDuration) <= now)
				.Select(kv => kv.Key)
				.ToList()
				.ForEach(key => _cache.TryRemove(key, out _));
		}
	}
}
