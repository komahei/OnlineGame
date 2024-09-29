using System;
using GameShared.Hubs;
using GameShared.MessagePackObjects;
using GameShared.Services;
using Grpc.Core;
using MagicOnion.Client;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace GameClient
{

	[DefaultExecutionOrder(-1)]
	public class MatchComponent : MonoBehaviour, IMatchHubReceiver
	{
		public static MatchComponent Instance { get {  return _instance; } }
		private static MatchComponent _instance;

		private CancellationTokenSource shutdownCancellation = new CancellationTokenSource();
		private ChannelBase channel;

		public IMatchHub streamingClient;
		public IMatchService client;

		public delegate void OnMoveHandler(PlayerCharacterParameter param);
		public OnMoveHandler OnMovePlayerCharacter;

		public delegate void OnAttackHandler(PlayerAttackMessage playerAttackMessage);
		public OnAttackHandler OnAttackPlayerCharacter;

		public delegate void OnJoinOrLeaveHandler(PlayerInform inform);
		public OnJoinOrLeaveHandler OnPlayerJoin;
		public OnJoinOrLeaveHandler OnPlayerLeave;

		public delegate void OnAlreadyJoinHandler();
		public OnAlreadyJoinHandler OnPlayerAlreadyJoin;

		public delegate void OnStartGameHandler(GameStartSettings[] startSets);
		public OnStartGameHandler OnPlayerStartGame;

		public delegate void OnGameTimeManageHandler(DateTime nowTime);
		public OnGameTimeManageHandler OnGameTimeManager;

		public delegate void OnDieCharacterHandler(string userId);
		public OnDieCharacterHandler OnDieCharacter;

		public bool isConnect = false;
		public bool isJoin = false;
		private bool isSelfDisConnected;

		public bool useNetManager = false;
		public bool useGameManager = false;


		public bool tenTimes = false;
		public DateTime nowTime;


		public GameObject ErrorPopup;


		void Awake()
		{
			// instanceがすでにあったら自分を消去する
			if (_instance && this != _instance)
			{
				Destroy(this.gameObject);
			}

			_instance = this;
			// Scene遷移で破棄されないようにする
			DontDestroyOnLoad(this);
		}

		public void Start()
		{
			// サーバへの接続
			//if (!isConnect) await this.InitializeClientAsync();
			// ルームへの入室
			//this.JoinRoom();
		}

		async void OnDestroy()
		{
			// Clean up Hub and channel
			shutdownCancellation.Cancel();

			if (this.streamingClient != null) await this.streamingClient.DisposeAsync();
			if (this.channel != null) await this.channel.ShutdownAsync();
		}

		public async Task InitializeClientAsync()
		{
			// HTTP通信
			this.channel = GrpcChannelx.ForAddress(SystemConstants.ServerUrl);

			int connectNum = 0;
			while (!shutdownCancellation.IsCancellationRequested)
			{
				try
				{
					Debug.Log($"Connecting to the server...");
					this.streamingClient = await StreamingHubClient.ConnectAsync<IMatchHub, IMatchHubReceiver>(this.channel, this, cancellationToken: shutdownCancellation.Token);
					this.RegisterDisconnectEvent(streamingClient);
					Debug.Log($"Conneection is established...");
					this.isConnect = true;
					Debug.Log("Connect変数は : " + isConnect);
					break;
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					connectNum++;
					if (connectNum > 9)
					{
						SceneManager.LoadScene("Start");
						Destroy(this.gameObject);
						break;
					}
				}

				Debug.Log($"Failed to connect to the server. Retry after 5 seconds");
				await Task.Delay(5 * 1000);
			}

			Debug.Log("Successed!!!");
			if (isConnect) this.client = MagicOnionClient.Create<IMatchService>(this.channel);
		}

		public async void RegisterDisconnectEvent(IMatchHub streamingClient)
		{
			try
			{
				// you can wait disconnected event
				await streamingClient.WaitForDisconnect();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
			finally
			{
				// try-to-reconnect? logging event? close? etc...
				if (this.isSelfDisConnected)
				{
					// there is no particular meaning
					await Task.Delay(2000);

					// reconnect
					await this.ReconnectServerAsync();
				}
			}
		}

		public async void DisconnectServer()
		{
			if (this.isJoin) this.LeaveRoom();
			await this.streamingClient.DisposeAsync();
		}

		public async void ReconnectInitializedServer()
		{
			if (this.channel != null)
			{
				var chan = this.channel;
				if (chan == Interlocked.CompareExchange(ref this.channel, null, chan))
				{
					await chan.ShutdownAsync();
					this.channel = null;
				}
			}

			if (this.streamingClient != null)
			{
				var streamClient = this.streamingClient;
				if (streamClient == Interlocked.CompareExchange(ref this.streamingClient, null, streamClient))
				{
					await streamClient.DisposeAsync();
					this.streamingClient = null;
				}
			}

			if (this.channel == null && this.streamingClient == null)
			{
				await this.InitializeClientAsync();
			}


		}


		private async Task ReconnectServerAsync()
		{
			Debug.Log($"Reconnecting to the server");
			this.streamingClient = await StreamingHubClient.ConnectAsync<IMatchHub, IMatchHubReceiver>(this.channel, this);
			this.RegisterDisconnectEvent(streamingClient);
			Debug.Log("Reconnected.");

			this.isSelfDisConnected = false;
		}

		#region Client -> Server (Streaming)

		// MatchシーンからMainシーンへ
		// ゲームのスタートボタンを押した時の
		// client <--> serverプログラム

		public async void StartGame()
		{
			await this.streamingClient.StartGameAsync();
		}

		public async void PreparedGame()
		{
			await this.streamingClient.PreparedGameAsync();
		}

		public async void GetGameTime()
		{
			await this.streamingClient.GameTimeManageAsync();
		}

		public async void DieCharacter()
		{
			await this.streamingClient.DieCharacterAsync(GlobalCharaInfo.myInfo.UserId);
		}

		public async void JoinRoom()
		{
			GlobalCharaInfo.userIds.Clear();
			GlobalCharaInfo.cpuDict.Clear();

			try
			{
				var request = new JoinRequest
				{
					RoomName = GlobalCharaInfo.myInfo.RoomName,
					UserName = GlobalCharaInfo.myInfo.UserName,
					CharacterID = GlobalCharaInfo.myInfo.CharacterID,
					UserId = GlobalCharaInfo.myInfo.UserId
				};

				// 上手くいかなかったらエラーが投げられる
				PlayerInform[] players = await this.streamingClient.JoinAsync(request);

				foreach (var p in players)
				{
					if (p.UserId.Equals(GlobalCharaInfo.myInfo.UserId))
					{
						GlobalCharaInfo.myInfo.isHost = p.isHost;
						GlobalCharaInfo.myInfo.ActorNumber = p.ActorNumber;
					}
					else
					{
						this.OnJoin(p);
					}
				}

				this.isJoin = true;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				GameObject errorPop = Instantiate(ErrorPopup);
				GameObject canvas = GameObject.Find("Canvas");
				errorPop.transform.SetParent(canvas.transform, false);
				TextMeshProUGUI errorText = errorPop.transform.Find("ResonText").gameObject.GetComponent<TextMeshProUGUI>();
				errorText.text = "This room is not Empty";
			}
		}


		public async Task CheckRoomNum()
		{
			GlobalCharaInfo.usingRoom = await this.streamingClient.RoomsAsync();
		}


		public async Task MoveAsync(PlayerCharacterParameter param)
		{
			if (isJoin)
			{
				await this.streamingClient.MoveAsync(param);
			}
		}

		public async Task AttackAsync(PlayerAttackMessage playerAttackMessage)
		{
			if (isJoin)
			{
				await this.streamingClient.AttackAsync(playerAttackMessage);
			}
		}

		public async Task LeaveRoom()
		{
			//Debug.Log("退出するよん0");
			this.isJoin = false;
			GlobalCharaInfo.ableMove = false;
			//Debug.Log("キャラ削除前");
			foreach (var cpu in GlobalCharaInfo.cpuDict.Values)
			{
				cpu.leaveDestroy();
			}
			GlobalCharaInfo.userIds.Clear();
			GlobalCharaInfo.cpuDict.Clear();
			//Debug.Log("キャラ削除後");

			await this.streamingClient.LeaveAsync();


		}

		public async void GenerateException()
		{
			// hub
			if (!this.isJoin) return;
			await this.streamingClient.GenerateException("client exception(streaminghub)!");
		}

		#endregion

		#region Server -> Client (Streaming)
		public void OnMove(PlayerCharacterParameter param)
		{
			if (this.isJoin)
			{
				OnMovePlayerCharacter?.Invoke(param);
			}
			
		}

		public void OnAttack(PlayerAttackMessage playerAttackMessage)
		{
			if (this.isJoin)
			{
				OnAttackPlayerCharacter.Invoke(playerAttackMessage);
			}
		}

		public void OnJoin(PlayerInform inform)
		{
			//GlobalCharaInfo.userIds.Add(inform.UserId);
			//Debug.Log("JOINJOINJOIN");
			//Debug.Log(inform.UserId);
			OnPlayerJoin?.Invoke(inform);
		}

		public void OnAlreadyJoin()
		{
			OnPlayerAlreadyJoin?.Invoke();
		}

		public void OnLeave(PlayerInform inform)
		{
			//GlobalCharaInfo.userIds.Remove(inform.UserId);
			//Debug.Log("退出するよん1");
			OnPlayerLeave?.Invoke(inform);
			//DisconnectServer();
			
		}

		public void OnDie(string userId)
		{
			OnDieCharacter?.Invoke(userId);
		}

		public void OnStartGame(GameStartSettings[] startSets)
		{
			OnPlayerStartGame?.Invoke(startSets);
		}

		public void OnPreparedGame()
		{
			GlobalCharaInfo.deadPeople = 0;
			GlobalCharaInfo.cpuDict.Clear();
			SceneManager.LoadScene("Main");
			if (GlobalCharaInfo.myInfo.isHost)
			{
				this.StartGame();
			}

		}

		public void OnGameTimeManage(DateTime nowTime)
		{
			OnGameTimeManager?.Invoke(nowTime);
		}

		#endregion

	}


}
