using MagicOnion;
using MessagePack;

namespace GameShared.Services
{
	public interface IChatService : IService<IChatService>
	{
		UnaryResult GenerateException(string message);
		UnaryResult SendReportAsync(string message);
	}
}
