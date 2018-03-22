﻿using System;

namespace DotNetSpiderLite.Core.Redial
{
	public class NetworkCenter
	{
		private static readonly Lazy<NetworkCenter> Instance = new Lazy<NetworkCenter>(() => new NetworkCenter());

		public static NetworkCenter Current => Instance.Value;

		public IRedialExecutor Executor { get; set; }

		private NetworkCenter()
		{
		}

		public void Execute(string name, Action action)
		{
			if (Executor != null)
			{
				Executor.Execute(name, action);
			}
			else
			{
				action();
			}
		}

		public void Execute(string name, Action<dynamic> action, dynamic obj)
		{
			if (Executor != null)
			{
				Executor.Execute(name, action, obj);
			}
			else
			{
				action(obj);
			}
		}

		public T Execute<T>(string name, Func<T> func)
		{
			if (Executor != null)
			{
				return Executor.Execute(name, func);
			}
			else
			{
				return func();
			}
		}

		public T Execute<T>(string name, Func<dynamic, T> func, dynamic obj)
		{
			if (Executor != null)
			{
				return Executor.Execute(name, func, obj);
			}
			else
			{
				return func(obj);
			}
		}
	}
}
