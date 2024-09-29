using MessagePack;

namespace GameShared.MessagePackObjects
{
	// Send Room Name and Room People Value
	[MessagePackObject]
	public struct UsingRoom
	{
		[Key(0)]
		public string RoomName {  get; set; }

		[Key(1)]
		public int RoomPeople { get; set; }
	}
}
