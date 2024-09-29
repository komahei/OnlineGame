using GameShared.MessagePackObjects;
using MagicOnion;
using System.Threading.Tasks;

namespace GameShared.Hubs
{
	// Client -> Server API (Streaming)
	public interface IMatchHub : IStreamingHub<IMatchHub, IMatchHubReceiver>
	{
		Task MoveAsync(PlayerCharacterParameter param);

		Task AttackAsync(PlayerAttackMessage playerAttackMessage);

		Task<PlayerInform[]> JoinAsync(JoinRequest request);

		Task<UsingRoom[]> RoomsAsync();

		Task StartGameAsync();

		Task PreparedGameAsync();

		Task GameTimeManageAsync();

		Task DieCharacterAsync(string userId);

		Task LeaveAsync();

		Task GenerateException(string message);
	}

}
