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

	// 20�b �p�ӎ���
	// 150�b �Q�[������
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

	// �T�[�o�̓����̂��߂̃��\�b�h
	void OnTimeManageGame(DateTime nowTime)
	{
		MatchComponent.Instance.nowTime = nowTime;
		MatchComponent.Instance.tenTimes = true;
		//TimeManageGame(nowTime);
	}

	private void TimeManageGame(DateTime nowTime)
	{
		//Debug.Log("�X�^�[�g�Q�[���t���O");
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

		// �T�[�o�[����o�ߎ��Ԃ��擾
		this.gameSec -= elapsedTime;
		// �\�����Ԃ��T�[�o�[���Ԃɍ��킹��
		this.globalSec = this.gameSec;
		if (GlobalCharaInfo.cpuDict.Count <= GlobalCharaInfo.deadPeople)
		{
			Debug.Log("�S������");
			this.globalSec = -100;
		}

		// globalSec��-9�ɂȂ��Ă�H
		Debug.Log(this.globalSec);
		if (!GlobalCharaInfo.StartGameFlag && this.globalSec <= 0)
		{
			Debug.Log("�����I��");
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
			Debug.Log("�Q�[���I��");
			GlobalCharaInfo.StartGameFlag = false;
			GlobalCharaInfo.ableMove = false;
			//Debug.Log("�L�����폜�O");
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
