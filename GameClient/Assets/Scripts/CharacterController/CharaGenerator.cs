using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;


namespace GameClient
{
	[DefaultExecutionOrder(-1)]
	public class CharaGenerator : MonoBehaviour
	{
		public GameObject[] amaturePrefab = new GameObject[4];
		public GameObject followCamera;

		public int cId;
		public Role role;

		void Start()
		{
			//GameObject chara = Instantiate(amaturePrefab[GlobalCharaInfo.myInfo.CharacterID], new Vector3(0.0f, 2.0f, 0.0f), Quaternion.identity);
			//chara.name = "playerYou";
			GlobalCharaInfo.StartGameFlag = true;
			Vector3 pos = new Vector3(0.0f, 20.5f, 0.0f);
			Instantiate(followCamera);
			generateMe(pos);
		}

		void Update()
		{

		}

		private void generateMe(Vector3 pos)
		{
			
			var player = Instantiate(amaturePrefab[cId], pos, Quaternion.identity);
			player.name = "playerYou";
			var controller = player.GetComponent<ThirdPerson>();
			controller.setCharacterParameter(role);
		}
	}

}
