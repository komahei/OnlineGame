using GameClient;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

	public TextMeshProUGUI gameTimes;
	public string gameTurn;

	public int globalSec;
	float _time;

	// 20秒 用意時間
	// 150秒 ゲーム時間
	private int gameSec;


	private bool startTime;
	private bool stopGame;
	private DateTime startTimeNow;
	private int timeIndex;


	void Awake()
	{

		if (!MatchComponent.Instance.useGameManager)
		{
			MatchComponent.Instance.OnGameTimeManager += OnTimeManageGame;
			MatchComponent.Instance.useGameManager = true;
		}

		GlobalCharaInfo.StartGameFlag = false;
		gameTurn = "Start Game ... ";
		startTime = false;
		stopGame = false;
		gameSec = 20;
		globalSec = 20;
		timeIndex = 0;
		_time = 0;

	}

	// Start is called before the first frame update
	void Start()
    {
		
		if (GlobalCharaInfo.myInfo.isHost)
		{
			MatchComponent.Instance.GetGameTime();
		}

	}

    // Update is called once per frame
    void Update()
    {
        if (!stopGame)
		{
			_time += Time.deltaTime;
			if (_time >= 1)
			{
				TextChange();
				timeIndex += 1;
				_time = 0;
			}
			if (timeIndex >= 10)
			{
				timeIndex = 0;
				if (GlobalCharaInfo.myInfo.isHost) MatchComponent.Instance.GetGameTime();
			}
		}

		if (MatchComponent.Instance.tenTimes)
		{
			MatchComponent.Instance.tenTimes = false;
			TimeManageGame(MatchComponent.Instance.nowTime);
		}

    }

	void TextChange()
	{
		globalSec--;
		if (globalSec < 0) globalSec = 0;
		gameTimes.text = gameTurn + globalSec.ToString();
	}

	// サーバの同期のためのメソッド
	void OnTimeManageGame(DateTime nowTime)
	{
		MatchComponent.Instance.nowTime = nowTime;
		MatchComponent.Instance.tenTimes = true;
		//TimeManageGame(nowTime);
	}

	private void TimeManageGame(DateTime nowTime)
	{
		//Debug.Log("スタートゲームフラグ");
		//Debug.Log(GlobalCharaInfo.StartGameFlag);

		if (!GlobalCharaInfo.StartGameFlag) this.gameSec = 20;
		else this.gameSec = 150;
		int elapsedTime = 0;
		if (!this.startTime)
		{
			this.startTimeNow = nowTime;
			elapsedTime = 0;
			this.startTime = true;
		}
		else
		{
			TimeSpan timeSpan = nowTime - this.startTimeNow;
			elapsedTime = (int)(timeSpan.TotalMilliseconds / 1000);
		}

		// サーバーから経過時間を取得
		this.gameSec -= elapsedTime;
		// 表示時間をサーバー時間に合わせる
		this.globalSec = this.gameSec;
		if (GlobalCharaInfo.cpuDict.Count <= GlobalCharaInfo.deadPeople)
		{
			Debug.Log("全員死んだ");
			this.globalSec = -100;
		}

		// globalSecが-9になってる？
		Debug.Log(this.globalSec);
		if (!GlobalCharaInfo.StartGameFlag && this.globalSec <= 0)
		{
			Debug.Log("準備終了");
			this.gameTimes.text = "START";

			this.globalSec = 150;
			this.startTimeNow = nowTime;

			this.gameTurn = "TIME LIMIT : ";
			GlobalCharaInfo.StartGameFlag = true;
		}
		else if (GlobalCharaInfo.StartGameFlag && this.globalSec <= 0)
		{
			this.stopGame = true;
			this.gameTimes.text = "FINISH";
			Debug.Log("ゲーム終了");
			GlobalCharaInfo.StartGameFlag = false;
			GlobalCharaInfo.ableMove = false;
			//Debug.Log("キャラ削除前");
			foreach (var cpu in GlobalCharaInfo.cpuDict.Values)
			{
				cpu.leaveDestroy();
			}
			GameObject player = GameObject.Find("playerYou");
			Destroy(player);
			//GlobalCharaInfo.userIds.Clear();
			SceneManager.LoadScene("Result");
			//MatchComponent.Instance.OnGameTimeManager -= OnTimeManageGame;
		}
	}

}
