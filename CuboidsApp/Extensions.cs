using System;
using System.Windows.Threading;

namespace CuboidsApp;

public static class Extensions
{
	public static void SafeInvoke(this Dispatcher dispatcher, Action action)
	{
		if (dispatcher.CheckAccess())
			action();
		else
			dispatcher.Invoke(action);
	}
}