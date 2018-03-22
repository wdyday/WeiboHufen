using System;

namespace DotNetSpiderLite.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public interface ISpider : IDisposable, IControllable, IAppBase
    {
		Site Site { get; }
	}
}
