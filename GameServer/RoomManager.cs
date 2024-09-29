using System.Collections.Concurrent;
using System.Numerics;
using GameShared.MessagePackObjects;

namespace GameServer
{
	public class RoomManager
	{
		public static readonly RoomManager Instance = new RoomManager();

		private ConcurrentDictionary<string, RoomInfo> _roomList;
		private ConcurrentDictionary<string, string> _playerRoomMap;

		private RoomManager()
		{
			_roomList = new ConcurrentDictionary<string, RoomInfo>();
			_playerRoomMap = new ConcurrentDictionary<string, string>();
		}

		public bool CreateRoom(string roomName)
		{
			return _roomList.TryAdd(roomName, new RoomInfo(roomName));
		}

		public void RemoveRoom(string roomName)
		{
			RoomInfo roomInfo;
			_roomList.TryRemove(roomName, out roomInfo);
		}

		public RoomInfo GetRoom(string roomName)
		{
			RoomInfo roomInfo;
			_roomList.TryGetValue(roomName, out roomInfo);
			return roomInfo;
		}

		public string[] GetRoomNames() {
			return _roomList.Keys.ToArray();
		}

		public PlayerInform JoinOrCreateRoom(string roomName, string playerName, int characterId, string userId)
		{
			RoomInfo roomInfo = _roomList.GetOrAdd(roomName, new RoomInfo(roomName));

			PlayerInform player = roomInfo.GetPlayer(userId);
			if (player.UserId == null)
			{
				player = roomInfo.AddPlayer(playerName, characterId, userId);
				if (player.ActorNumber >= 0)
				{
					_playerRoomMap.TryAdd(userId, roomName);
				}
			}

			return player;

		}

		public bool LeaveRoom(string userId)
		{
			string roomName;
			RoomInfo roomInfo;
			if (_playerRoomMap.TryRemove(userId, out roomName))
			{
				_roomList.TryGetValue(roomName,out roomInfo);

				if (roomInfo != null)
				{
					Console.WriteLine("削除するっす");
					return roomInfo.RemovePlayer(userId);
				}
			}

			return false;
		}

	}
}
