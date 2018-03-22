using System.Collections.Generic;
using HtmlAgilityPack;

namespace DotNetSpiderLite.Core.Selector
{
	public abstract class BaseHtmlSelector : ISelector
	{
		public abstract bool HasAttribute();
		public abstract dynamic Select(HtmlNode element);
		public abstract List<dynamic> SelectList(HtmlNode element);

		public virtual dynamic Select(dynamic text)
		{
			if (text != null)
			{
				if (text is string)
				{
					HtmlDocument document = new HtmlDocument {OptionAutoCloseOnEnd = true};
					document.LoadHtml(text);
					return Select(document.DocumentNode);
				}
				else
				{
					return Select(text as HtmlNode);
				}
			}
			return null;
		}

		public virtual List<dynamic> SelectList(dynamic text)
		{
			if (text != null)
			{
				if (text is HtmlNode htmlNode)
				{
					return SelectList(htmlNode);
				}
				else
				{
					HtmlDocument document = new HtmlDocument {OptionAutoCloseOnEnd = true};
					document.LoadHtml(text);
					return SelectList(document.DocumentNode);
				}
			}
			else
			{
				return new List<dynamic>();
			}
		}
	}
}