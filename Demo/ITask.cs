namespace Demo
{
	public interface ITask<TIn, TOut>
	{
		TOut Execute(TIn value);
	}
}
