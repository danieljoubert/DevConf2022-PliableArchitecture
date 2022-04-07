using System;

namespace Demo
{
	public interface IDataStorable
	{
		object Id { get; set; }
		void Validate();
		bool IsNew { get; }
	}

	public interface IDataStorable<T> : IDataStorable
	{
		new T Id { get; set; }
	}


	[Serializable]
	public abstract class DataStorable<T> : IDataStorable<T>
	{
		#region Implementation of IDataStorable<T>
		public abstract T Id { get; set; }

		object IDataStorable.Id
		{
			get { return Id; }
			set { Id = (T)value; }
		}

		public bool IsNew { get { return Equals(Id, default(T)); } }

		public virtual void Validate()
		{
		}
		#endregion
	}
}

