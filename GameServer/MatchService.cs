using GameShared.Services;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace GameServer
{
	public class MatchService : ServiceBase<IMatchService>, IMatchService
	{
		//private readonly ILogger logger;

		public MatchService()
		{

		}

		/*
		public MatchService(ILogger<MatchService> logger)
		{
			this.logger = logger;
		}
		*/

		public UnaryResult GenerateException(string message)
		{
			throw new System.NotImplementedException(message);
		}

		public UnaryResult SendReportAsync(string message)
		{
			//logger.LogDebug($"{message}");
			return UnaryResult.CompletedResult;
		}
	}
}
