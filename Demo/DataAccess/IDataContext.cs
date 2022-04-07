using System;
using System.Collections.Generic;

namespace Demo
{
	public interface IDataContext
	{
		IDataStorable Update(IDataStorable value);
		ICollection<object> RetrieveQuery(string query, Type type);
		void Delete(object itemToDelete);
	}
}
