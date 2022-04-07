using System;

namespace Demo
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
	public class ValidatedNotNullAttribute : Attribute
	{
	}
}
