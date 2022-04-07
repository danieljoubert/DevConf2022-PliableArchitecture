using Microsoft.Practices.Unity.Configuration;
using System;
using Unity;

namespace Demo
{
	internal class Program
	{
		static void Main(string[] args)
		{
			IUnityContainer unity = new UnityContainer();
			unity.LoadConfiguration();
			Console.ReadKey();
		}
	}
}
