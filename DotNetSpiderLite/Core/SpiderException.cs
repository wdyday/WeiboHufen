using System;

namespace DotNetSpiderLite.Core
{
	public class SpiderException : Exception
	{
		public SpiderException(string msg) : base(msg) { }

		public SpiderException(string msg, Exception e) : base(msg, e) { }
	}
}
