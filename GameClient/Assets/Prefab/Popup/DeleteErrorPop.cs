using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameClient{
	public class DeleteErrorPop : MonoBehaviour{

		public void DeleteSoundEffect()
		{
			GameObject se = GameObject.Find("SEManager");
			PlaySoundEffect pse = se.GetComponent<PlaySoundEffect>();
			pse.PlaySelectSE();
		}

		public void DeleteCursorEffect()
		{
			GameObject se = GameObject.Find("SEManager");
			PlaySoundEffect pse = se.GetComponent<PlaySoundEffect>();
			pse.PlayCursorSE();
		}

		public async void DeleteThisPopup()
		{
			Destroy(this.gameObject);
			bool isJoin = MatchComponent.Instance?.isJoin ?? false;
			if (isJoin)
			{
				await MatchComponent.Instance.LeaveRoom();
			}
			else
			{
				await UniTask.CompletedTask;
			}
			GlobalCharaInfo.ableMove = false;
			SceneManager.LoadScene("UserSetting");
		}
	}

}
