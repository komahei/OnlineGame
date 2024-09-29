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
			// MatchComponent.Instance��null�̏ꍇ��isJoin��false�̏ꍇ ; false

			bool isJoin = MatchComponent.Instance?.isJoin ?? false;
			//Debug.Log("�߂���");
			//Debug.Log(isJoin);
			if (isJoin)
			{
				//Debug.Log("�����o����");
				Debug.Log("���[�h�O");
				await MatchComponent.Instance.LeaveRoom();
			}
			else
			{
				Debug.Log("���[�h�O");
				await UniTask.CompletedTask;
			}
			Debug.Log("���[�h��");
			GlobalCharaInfo.ableMove = false;
			SceneManager.LoadScene("UserSetting");
			Debug.Log("���[�h��");

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
			Debug.Log("�J�ڑO");
		}

	}

}
