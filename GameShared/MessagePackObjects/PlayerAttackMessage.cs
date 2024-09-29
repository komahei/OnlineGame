using MessagePack;

namespace GameShared.MessagePackObjects
{
	[MessagePackObject]
	public class PlayerAttackMessage
	{
		[Key(0)]
		public string UserId { get; set; }

		[Key(1)]
		public bool LeftAttack { get; set; }

		[Key(2)]
		public bool RightAttack { get; set; }

		[Key(3)]
		public float BombAngle { get; set; }
	}
}
