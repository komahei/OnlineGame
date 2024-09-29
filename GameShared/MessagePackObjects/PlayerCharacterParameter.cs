using MessagePack;
//using UnityEngine;

namespace GameShared.MessagePackObjects
{
	[MessagePackObject]
	public class PlayerCharacterParameter
	{
		[Key(0)]
		public string UserName { get; set; }

		[Key(1)]
		public string UserId { get; set; }

		// キャラクタの位置等

		[Key(2)]
		public float time { get; set; }

		[Key(3)]
		public UnityEngine.Vector3 Position { get; set; }

		[Key(4)]
		public UnityEngine.Quaternion Rotation { get; set; }

		[Key(5)]
		public UnityEngine.Vector3 Move {  get; set; }

		[Key(6)]
		public bool Jump { get; set; }

	}
}
