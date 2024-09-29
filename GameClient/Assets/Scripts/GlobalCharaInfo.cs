using UnityEngine;
using GameShared.MessagePackObjects;
using System.Collections;
using System.Collections.Generic;

namespace GameClient
{
	public static class GlobalCharaInfo
	{
		public static bool ableMove = false;
		public static bool StartGameFlag = false;
		public static int deadPeople = 0;
		public static PlayerInform myInfo;
		public static Role myRole;
		public static UsingRoom[] usingRoom;
		public static Dictionary<string, int> userIds = new Dictionary<string, int>();
		public static Dictionary<string, CPUCharacterController> cpuDict = new Dictionary<string, CPUCharacterController>();
	}
}

