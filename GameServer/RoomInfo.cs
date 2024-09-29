using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using GameShared.MessagePackObjects;

namespace GameServer
{
	public class RoomInfo
	{
		const int defaultMaxPlayers = 4;

		int MaxPlayers { get; }
		string Name { get; }

		private ConcurrentDictionary<string, PlayerInform> _playerList;

		public RoomInfo(string name)
		{
			this.Name = name;
			this.MaxPlayers = defaultMaxPlayers;
			this._playerList = new ConcurrentDictionary<string, PlayerInform>();
		}

		public PlayerInform AddPlayer(string userName, int characterID, string userId)
		{
			int actorNumber = FindNewActorNumber();
			bool host = false;
			if (actorNumber == 0)
			{
				host = true;
			}
			PlayerInform inform = new PlayerInform
			{
				RoomName = this.Name,
				UserName = userName,
				CharacterID = characterID,
				isHost = host,
				UserId = userId,
				ActorNumber = actorNumber,
			};
			
			if (inform.ActorNumber >= 0)
			{
				_playerList.TryAdd(inform.UserId, inform);
			}

			return inform;
		}

		public PlayerInform GetPlayer(string userId)
		{
			PlayerInform player;
			_playerList.TryGetValue(userId, out player);
			return player;
		}

		public PlayerInform[] GetPlayers()
		{
			return _playerList.Values.ToArray();
		}

		public int GetPlayersNum()
		{
			return _playerList.Values.ToArray().Length;
		}

		public bool RemovePlayer(string userId)
		{
			PlayerInform player;
			return _playerList.TryRemove(userId, out player);
		}

		private int FindNewActorNumber()
		{
			var list = _playerList.Values.ToDictionary(player => player.ActorNumber);
			for (int id = 0; id < MaxPlayers; id++)
			{
				if (!list.ContainsKey(id))
				{
					return id;
				}
			}
			return -1;
		}

	}
}
