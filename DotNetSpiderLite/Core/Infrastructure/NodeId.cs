
/* Unmerged change from project 'DotNetSpiderLite.Core(net45)'
Before:
using System.IO;

#if NET_CORE
After:
#if NET_CORE
*/

#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace DotNetSpiderLite.Core.Infrastructure
{
	public static class NodeId
	{
		public static readonly string Id;

		static NodeId()
		{
			Id = Env.Ip;
		}
	}
}
