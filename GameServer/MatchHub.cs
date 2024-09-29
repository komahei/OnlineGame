using GameShared.Hubs;
using GameShared.MessagePackObjects;
using MagicOnion.Server.Hubs;


namespace GameServer
{
	
    // Matching Server processing
    // one class instance for one connection
    public class MatchHub : StreamingHubBase<IMatchHub, IMatchHubReceiver>, IMatchHub
    {

		private IGroup room;
		private PlayerInform inform;
		private bool isJoin = false;
		private string currentRoom;

		public async Task MoveAsync(PlayerCharacterParameter param)
		{
			//Console.WriteLine("Move");
			this.BroadcastExceptSelf(this.room).OnMove(param);
		}

		public async Task AttackAsync(PlayerAttackMessage playerAttackMessage)
		{
			Console.WriteLine("Attack");
			this.Broadcast(this.room).OnAttack(playerAttackMessage);
		}

		public async Task<PlayerInform[]> JoinAsync(JoinRequest request)
		{
			Console.WriteLine("Join");
			Guid connectionId = this.Context.ContextId;

			this.inform = RoomManager.Instance.JoinOrCreateRoom(
				request.RoomName, request.UserName, request.CharacterID, request.UserId
				);

			if (this.inform.ActorNumber >= 0)
			{
				// 部屋の生成
				this.room = await this.Group.AddAsync(request.RoomName);
				//this.Broadcast(this.room).OnJoin(this.inform);
				this.Broadcast(this.room).OnJoin(this.inform);
				PlayerInform[] roomPlayers = RoomManager.Instance.GetRoom(request.RoomName).GetPlayers();
				isJoin = true;
				this.currentRoom = request.RoomName;
				return roomPlayers;
			}else
			{
				throw new Exception("this room is not empty");
			}
			
			
		}

		public async Task<UsingRoom[]> RoomsAsync()
		{
			string[] roomNames = RoomManager.Instance.GetRoomNames();
			UsingRoom[] usingRoom = new UsingRoom[roomNames.Length];

			for (int i=0; i<roomNames.Length; i++)
			{
				int num = RoomManager.Instance.GetRoom(roomNames[i]).GetPlayersNum();
				usingRoom[i] = new UsingRoom
				{
					RoomName = roomNames[i],
					RoomPeople = num
				};
			}
			

			return usingRoom;
		}

		public async Task LeaveAsync()
		{
			Console.WriteLine(this.inform.UserName);
			if (this.room is not null)
			{
				await this.room.RemoveAsync(this.Context);
				// 部屋の削除
				RoomManager.Instance.LeaveRoom(this.inform.UserId);
				int num = RoomManager.Instance.GetRoom(this.inform.RoomName).GetPlayersNum();
				if (num == 0)
				{
					RoomManager.Instance.RemoveRoom(this.inform.RoomName);
				}
				this.Broadcast(this.room).OnLeave(this.inform);
				isJoin = false;
			}
		}

		public async Task StartGameAsync()
		{
			Console.WriteLine("GameStart");
			PlayerInform[] players = RoomManager.Instance.GetRoom(this.currentRoom).GetPlayers();
			// 0=鬼 1=逃走者
			GameStartSettings[] settings = new GameStartSettings[players.Length];	
			for (int i = 0; i < players.Length; i++)
			{
				settings[i] = new GameStartSettings
				{
					UserId = players[i].UserId,
					CharacterRole = 1,
					Position = new UnityEngine.Vector3(3.0f * i, 20.5f, 0.0f),
					GameSecond = 180,
					StartProcess = 1
				};
			}

			var rand = new Random();
			int val = rand.Next(0, players.Length);
			settings[val].CharacterRole = 0;

			this.Broadcast(this.room).OnStartGame(settings);
			await Task.CompletedTask;
		}

		public async Task PreparedGameAsync()
		{
			Console.WriteLine("ゲームスタート準備");
			this.Broadcast(this.room).OnPreparedGame();
			await Task.CompletedTask;
		}

		public async Task GameTimeManageAsync()
		{
			Console.WriteLine("サーバー時間送信");
			DateTime now = new DateTime();
			now = DateTime.Now;
			this.Broadcast(this.room).OnGameTimeManage(now);
			await Task.CompletedTask;
		}

		public async Task DieCharacterAsync(string userId)
		{
			this.Broadcast(this.room).OnDie(userId);
			await Task.CompletedTask;
		}

		public async Task GenerateException(string message)
		{
			await Task.CompletedTask;
			throw new Exception(message);
		}

		protected override ValueTask OnConnecting()
		{
			// handle connection if needed.
			Console.WriteLine($"client connected {this.Context.ContextId}");
			return CompletedTask;
		}

		protected override ValueTask OnDisconnected()
		{
			// handle disconnection if needed.
			// on disconnecting, if automatically removed this connection from group.
			if (isJoin)
			{
				RoomManager.Instance.LeaveRoom(this.inform.UserId);
				this.room.RemoveAsync(this.Context);
				int num = RoomManager.Instance.GetRoom(this.inform.RoomName).GetPlayersNum();
				if (num == 0)
				{
					RoomManager.Instance.RemoveRoom(this.inform.RoomName);
				}
				this.Broadcast(this.room).OnLeave(this.inform);
				isJoin = false;
			}
			return CompletedTask;
		}

	}
}
