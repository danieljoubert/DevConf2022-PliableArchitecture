namespace Demo
{
	public class NullProcess<T> : IProcess<T>
	{
		public void Execute(T value)
		{
		}
	}
}
