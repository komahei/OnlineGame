using GameShared.MessagePackObjects;
using MagicOnion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameShared.Hubs
{
	// Client -> Server API (Streaming)
	public interface IChatHub : IStreamingHub<IChatHub, IChatHubReceiver>
	{
		Task JoinAsync(JoinRequest request);

		Task LeaveAsync();

		Task SendMessageAsync(string message);

		Task GenerateException(string message);

		// It is not called because it is a method as a sample of arguments.
		//Task SampleMethod(List<int> sampleList, Dictionary<int, string> sampleDictionary);

	}
}
