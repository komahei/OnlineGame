﻿using System.Threading.Tasks;
using GameShared.MessagePackObjects;

namespace GameShared.Hubs
{
	// Server -> Client API
	public interface IChatHubReceiver
	{

		void OnJoin(string name);

		void OnLeave(string name);

		void OnSendMessage(MessageResponse message);

		//Task<string> HelloAsync(string name, int age);

	}
}
