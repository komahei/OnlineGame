using Cysharp.Threading.Tasks;
using GameShared.MessagePackObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameClient
{

	public class UserSettingScene : MonoBehaviour
	{

		private const int MAXROOMNUM = 25;
		public Button startButton;
		public TMP_InputField userName;
		public TMP_InputField roomName;
		public int charaId = 1000;

		public TMP_Dropdown dropdown;
		public GameObject image;

		public GameObject errorMessage;

		// Start is called before the first frame update
		async void Start()
		{
			startButton.enabled = false;
			// サーバーへの接続が確認できるまでwaitする
			image.SetActive(true);
			Debug.Log("接続してるか：");
			Debug.Log(MatchComponent.Instance.isConnect);
			bool isConnect = false;
			if (MatchComponent.Instance != null)
			{
				isConnect = MatchComponent.Instance.isConnect;
			}
			//bool isConnect = MatchComponent.Instance?.isConnect ?? false;
			if (!isConnect)
			{
				await MatchComponent.Instance.InitializeClientAsync();
			}
			Debug.Log(MatchComponent.Instance.isConnect);

			isConnect = MatchComponent.Instance?.isConnect ?? false;
			if (isConnect)
			{
				Debug.Log("room確認前");
				await MatchComponent.Instance.CheckRoomNum();
				Debug.Log("room確認後");
				List<string> keys = new List<string>();
				foreach (UsingRoom room in GlobalCharaInfo.usingRoom)
				{
					string key = room.RoomName + " | " + room.RoomPeople.ToString() + " / 4";
					keys.Add(key);
				}
				dropdown.ClearOptions();
				dropdown.AddOptions(keys);
				image.SetActive(false);
			}

		}

		// Update is called once per frame
		void Update()
		{
			useButtonCheck();
		}

		void useButtonCheck()
		{
			if (userName.text != "" &&
				roomName.text != "" &&
				charaId != 1000)
			{
				startButton.enabled = true;
			}
			else
			{
				startButton.enabled = false;
			}
			
		}

		public void clickStartButton()
		{
			errorMessage.SetActive(false);
			if (GlobalCharaInfo.usingRoom.Length >= MAXROOMNUM)
			{
				bool createRoom = false;
				foreach (UsingRoom room in GlobalCharaInfo.usingRoom)
				{
					if (room.RoomName == roomName.text)
					{
						createRoom = true;
					}
				}
				if (createRoom)
				{
					errorMessage.SetActive(true);
					TextMeshProUGUI errorText = errorMessage.GetComponent<TextMeshProUGUI>();
					errorText.text = "25 room only now";
					return;
				}
			}
			GlobalCharaInfo.myInfo.CharacterID = charaId;
			GlobalCharaInfo.myInfo.UserName = userName.text;
			GlobalCharaInfo.myInfo.RoomName = roomName.text;
			GlobalCharaInfo.myInfo.UserId = Guid.NewGuid().ToString();
			SceneManager.LoadScene("Match");
			GlobalCharaInfo.ableMove = true;
			MatchComponent.Instance.JoinRoom();
		}

		public void click0()
		{
			this.charaId = 0;
		}

		public void click1()
		{
			this.charaId = 1;
		}

		public void click2()
		{
			this.charaId = 2;
		}

		public void click3()
		{
			this.charaId = 3;
		}
	}


}
