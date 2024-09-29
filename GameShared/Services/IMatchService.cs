using MagicOnion;
using MessagePack;

namespace GameShared.Services
{
    public interface IMatchService : IService<IMatchService>
    {
		UnaryResult GenerateException(string message);
		UnaryResult SendReportAsync(string message);
	}
}
