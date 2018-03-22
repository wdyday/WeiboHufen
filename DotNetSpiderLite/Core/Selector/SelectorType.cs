using System;

namespace DotNetSpiderLite.Core.Selector
{
	[Flags]
	public enum SelectorType { XPath, Regex, Css, JsonPath, Enviroment, Id /*[WDY]*/ }
}
