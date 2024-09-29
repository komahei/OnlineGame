using MessagePack;

namespace GameShared.MessagePackObjects
{
	[MessagePackObject]
	public struct MessageResponse
	{
		[Key(0)]
		public string UserName { get; set; }

		[Key(1)]
		public string Message { get; set; }
	}
}
