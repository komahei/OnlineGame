using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient
{

	public class CpuGenerator : MonoBehaviour
	{
		public GameObject[] amaturePrefab = new GameObject[4];
		public int charaNum = 0;

		public void generateCharacter(string userId, int characterId)
		{
			GameObject chara = Instantiate(amaturePrefab[characterId], new Vector3(0.0f, 2.0f, 0.0f), Quaternion.identity);
			chara.name = userId;
			// ‚±‚±‚Ì‘‚«•û‚Ç‚¤‚È‚Ì‚©H¡ŒãC³‚µ‚ã‚×‚«‚©‚à
			CPUCharacterController controller = chara.GetComponent<CPUCharacterController>();
			controller.setCharacter(chara);
			GlobalCharaInfo.cpuDict.Add(userId, controller);
		}
		
	}


}

