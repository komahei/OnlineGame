using GameShared.Hubs;
using GameShared.MessagePackObjects;
using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameServer
{
	// Chat server processing
	// one class instance for one connection
	public class ChatHub : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
	{
		private IGroup room;
		private string myName;

		public async Task JoinAsync(JoinRequest request)
		{
			this.room = await this.Group.AddAsync(request.RoomName);
			this.myName = request.UserName;
			this.Broadcast(this.room).OnJoin(request.UserName);
		}

		public async Task LeaveAsync()
		{
			if (this.room is not null)
			{
				await this.room.RemoveAsync(this.Context);
				this.Broadcast(this.room).OnLeave(this.myName);
			}
		}

		public async Task SendMessageAsync(string message)
		{
			if (this.room is not null)
			{
				var response = new MessageResponse { UserName = this.myName, Message = message };
				this.Broadcast(this.room).OnSendMessage(response);
			}

			await Task.CompletedTask;
		}

		public Task GenerateException(string message)
		{
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
			return CompletedTask;
		}

	}
}
