using MessagePack;

namespace GameShared.MessagePackObjects
{
	[MessagePackObject]
	public struct PlayerInform
	{
		[Key(0)]
		public string RoomName { get; set; }

		[Key(1)]
		public string UserName { get; set; }

		[Key(2)]
		public int CharacterID { get; set; }

		[Key(3)]
		public bool isHost { get; set; }

		[Key(4)]
		public string UserId { get; set; }

		[Key(5)]
		public int ActorNumber { get; set; }
	}
}
