using System.Collections.Generic;

namespace DotNetSpiderLite.HtmlAgilityPack
{
	/// <summary>
    /// Represents a selector implementation over an arbitrary type of elements.
    /// </summary>
    public delegate IEnumerable<TElement> Selector<TElement>(IEnumerable<TElement> elements);
}