using MessagePack;
//using UnityEngine;

namespace GameShared.MessagePackObjects
{
	[MessagePackObject]
	public class GameStartSettings
	{
		[Key(0)]
		public string UserId { get; set; }

		[Key(1)]
		public int CharacterRole { get; set; }

		[Key(2)]
		public UnityEngine.Vector3 Position { get; set; }

		[Key(3)]
		public int GameSecond { get; set; }

		[Key(4)]
		public int StartProcess { get; set; }
	}
}
