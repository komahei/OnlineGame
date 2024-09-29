using System;
using System.Collections;
using System.Collections.Generic;
using GameShared.MessagePackObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace GameClient
{
	public class MatchSetting : MonoBehaviour
	{
		public GameObject errorMessage;
		public Button startButton;

		void Awake()
		{
			Debug.Log("Matching Awake");
			if (GlobalCharaInfo.userIds.Count > 0)
			{
				Debug.Log("Matching Awake Join");
				MatchComponent.Instance.OnAlreadyJoin();
			}

		}

		void Start()
		{
			startButton.enabled = GlobalCharaInfo.myInfo.isHost;
		}

		void Update()
		{
			startButton.enabled = GlobalCharaInfo.myInfo.isHost;
		}

		public void GameStart()
		{
			errorMessage.SetActive(false);
			int pNum = GlobalCharaInfo.userIds.Count;
			if (pNum < 2)
			{
				errorMessage.SetActive(true);
			}
			else
			{
				GlobalCharaInfo.ableMove = false;
				//MatchComponent.Instance.StartGame();
				MatchComponent.Instance.PreparedGame();
				//SceneManager.LoadScene("Main");
			}
			
		}

	}

}
