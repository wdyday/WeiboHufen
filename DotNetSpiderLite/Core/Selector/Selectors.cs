using System.Collections.Generic;

namespace DotNetSpiderLite.Core.Selector
{
	/// <summary>
	/// Convenient methods for selectors.
	/// </summary>
	public class Selectors
	{
		private static readonly Dictionary<string, ISelector> Cache = new Dictionary<string, ISelector>();
		private static readonly DefaultSelector DefaultSelector = new DefaultSelector();
		private static readonly object Locker = new object();

		public static ISelector Regex(string expr)
		{
			lock (Locker)
			{
				if (!Cache.ContainsKey(expr))
				{
					Cache.Add(expr, new RegexSelector(expr));
				}
				return Cache[expr];
			}
		}

		public static ISelector Css(string expr)
		{
			lock (Locker)
			{
				if (!Cache.ContainsKey(expr))
				{
					Cache.Add(expr, new CssHtmlSelector(expr));
				}
				return Cache[expr];
			}
		}

		public static ISelector Css(string expr, string attrName)
		{
			lock (Locker)
			{
				if (!Cache.ContainsKey(expr + attrName))
				{
					Cache.Add(expr + attrName, new CssHtmlSelector(expr, attrName));
				}
				return Cache[expr + attrName];
			}
		}

		public static ISelector Regex(string expr, int group)
		{
			lock (Locker)
			{
				if (!Cache.ContainsKey(expr))
				{
					Cache.Add(expr, new RegexSelector(expr, group));
				}
				return Cache[expr];
			}
		}

		public static ISelector SmartContent()
		{
			lock (Locker)
			{
				return Cache["SmartContentSelector"];
			}
		}

		public static ISelector XPath(string expr)
		{
			lock (Locker)
			{
				if (!Cache.ContainsKey(expr))
				{
					Cache.Add(expr, new XPathSelector(expr));
				}
				return Cache[expr];
			}
		}

		public static ISelector Default()
		{
			return DefaultSelector;
		}

		public static ISelector Enviroment(string expr)
		{
			lock (Locker)
			{
				if (!Cache.ContainsKey(expr))
				{
					Cache.Add(expr, new EnviromentSelector(expr));
				}
				return Cache[expr];
			}
		}

		public static ISelector JsonPath(string expr)
		{
			lock (Locker)
			{
				if (!Cache.ContainsKey(expr))
				{
					Cache.Add(expr, new JsonPathSelector(expr));
				}
				return Cache[expr];
			}
		}
	}
}