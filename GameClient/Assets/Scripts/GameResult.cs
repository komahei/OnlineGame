using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem.LowLevel;

namespace GameClient
{
    public class GameResult : MonoBehaviour
    {

        public TextMeshProUGUI resultText;

		// Start is called before the first frame update
		void Start()
		{
			Debug.Log("終了時のスタートゲームフラグ");
			Debug.Log(GlobalCharaInfo.StartGameFlag);

			Cursor.lockState = CursorLockMode.None;

			if (GlobalCharaInfo.myRole == Role.CHASER)
			{
				if (GlobalCharaInfo.cpuDict.Count <= GlobalCharaInfo.deadPeople)
				{
					resultText.text = "YOU WIN";
				}
				else
				{
					resultText.text = "YOU LOSE";
				}
			}

			if (GlobalCharaInfo.myRole == Role.ESCAPER)
			{
				if (GlobalCharaInfo.cpuDict.Count <= GlobalCharaInfo.deadPeople)
				{
					resultText.text = "YOU LOSE";
				}
				else
				{
					resultText.text = "YOU WIN";
				}
			}

			GlobalCharaInfo.cpuDict.Clear();
		}

	}
}