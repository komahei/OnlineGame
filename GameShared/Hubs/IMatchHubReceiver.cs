using GameShared.MessagePackObjects;
using MagicOnion;
using System;
using System.Threading.Tasks;

namespace GameShared.Hubs
{
	// Server -> Client API
	public interface IMatchHubReceiver
	{
		void OnMove(PlayerCharacterParameter param);

		void OnAttack(PlayerAttackMessage playerAttackMessage);

		void OnJoin(PlayerInform inform);

		void OnLeave(PlayerInform inform);

		void OnStartGame(GameStartSettings[] startSets);

		void OnPreparedGame();

		void OnGameTimeManage(DateTime nowTime);

		void OnDie(string userId);

	}
}
