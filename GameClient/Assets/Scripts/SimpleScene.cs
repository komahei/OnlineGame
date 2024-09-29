using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameClient
{
	public class SimpleScene : MonoBehaviour
	{

		public void StartScene()
		{
			BeforeSceneFunc();
			bool isConnect = MatchComponent.Instance?.isConnect ?? false;
			if (isConnect)
			{
				MatchComponent.Instance.DisconnectServer();
				Destroy(MatchComponent.Instance.gameObject);
			}
			SceneManager.LoadScene("Start");
		}

		public async void UserSettingsScene()
		{
			// MatchComponent.Instanceがnullの場合とisJoinがfalseの場合 ; false

			bool isJoin = MatchComponent.Instance?.isJoin ?? false;
			//Debug.Log("戻るよん");
			//Debug.Log(isJoin);
			if (isJoin)
			{
				//Debug.Log("部屋出るよん");
				Debug.Log("ロード前");
				await MatchComponent.Instance.LeaveRoom();
			}
			else
			{
				Debug.Log("ロード前");
				await UniTask.CompletedTask;
			}
			Debug.Log("ロード中");
			GlobalCharaInfo.ableMove = false;
			SceneManager.LoadScene("UserSetting");
			Debug.Log("ロード後");

		}


		public void MatchScene()
		{
			BeforeSceneFunc();
			SceneManager.LoadScene("Match");
			GlobalCharaInfo.ableMove = true;
		}

		public void MainScene()
		{
			BeforeSceneFunc();
			SceneManager.LoadScene("Main");
		}

		public void ResultScene()
		{
			BeforeSceneFunc();
			SceneManager.LoadScene("Result");
		}

		public void ExitGame()
		{
			Application.Quit();
		}

		public void BeforeSceneFunc()
		{
			Debug.Log("遷移前");
		}

	}

}
