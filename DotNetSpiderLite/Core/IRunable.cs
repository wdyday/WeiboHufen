using System.Threading.Tasks;

namespace DotNetSpiderLite.Core
{
	public interface IRunable
	{
		Task RunAsync(params string[] arguments);
		void Run(params string[] arguments);
	}
}
