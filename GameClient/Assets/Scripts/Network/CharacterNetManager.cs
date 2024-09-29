using Cysharp.Threading.Tasks;
using GameClient;
using GameShared.MessagePackObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterNetManager : MonoBehaviour
{

	public GameObject[] myAmaturePrefab = new GameObject[4];
	public GameObject[] amaturePrefab = new GameObject[4];

	public GameObject followCamera;

	private GameObject player;
    private ThirdPerson controller;

	private float frequencyMoveTime = 0.0f;

    // Start is called before the first frame update

    void Awake()
    {
        if (!MatchComponent.Instance.useNetManager)
        {
			MatchComponent.Instance.OnMovePlayerCharacter += OnMoveRemotePlayerCharacter;
			MatchComponent.Instance.OnAttackPlayerCharacter += OnAttackRemotePlayerCharacter;
			MatchComponent.Instance.OnPlayerJoin += OnJoin;
			MatchComponent.Instance.OnPlayerLeave += OnLeave;
			MatchComponent.Instance.OnPlayerAlreadyJoin += OnAlreadyJoin;
			MatchComponent.Instance.OnDieCharacter += OnDie;
			MatchComponent.Instance.OnPlayerStartGame += OnStartGame;
			MatchComponent.Instance.useNetManager = true;
		}
		
	}

    void Start()
    {
		//await UniTask.WaitUntil(() => MatchComponent.Instance.isJoin);
		//player = GameObject.Find("playerYou");
		//controller = player.GetComponent<ThirdPerson>();

		//generator = cpugenerator.GetComponent<CpuGenerator>();
	}

    // Update is called once per frame
    async void Update()
    {
		if (MatchComponent.Instance.isJoin && GlobalCharaInfo.ableMove)
		{
			if (controller != null)
			{
				frequencyMoveTime += Time.deltaTime;
				// 0.2ïbÇÃïpìxÇ≈à⁄ìÆÇÃëóêM
				if (frequencyMoveTime >= 0.2f)
				{
					frequencyMoveTime = 0.0f;
					var move = controller.messageMove;
					PlayerCharacterParameter param = new PlayerCharacterParameter
					{
						UserName = GlobalCharaInfo.myInfo.UserName,
						UserId = GlobalCharaInfo.myInfo.UserId,
						time = Time.time,
						Position = player.transform.position,
						Rotation = player.transform.rotation,
						Move = move,
						Jump = controller.isJump,
					};
					await MatchComponent.Instance.MoveAsync(param);
				}
				
			}
			
		}

        
    }

    private void OnMoveRemotePlayerCharacter(PlayerCharacterParameter param)
    {
		if (!(param.UserId.Equals(GlobalCharaInfo.myInfo.UserId)) && GlobalCharaInfo.cpuDict.ContainsKey(param.UserId))
		{
			CPUCharacterController cpu = GlobalCharaInfo.cpuDict[param.UserId];
			cpu?.Move(param.Move);
			cpu?.setJump(param.Jump);
			//cpu?.setRotation(param.Rotation);
			//cpu?.setClientTime(param.time);
			//cpu?.setPosition(param.Position);
			cpu?.setCharacterInfo(param.Position, param.Rotation, param.time);
		}
	}

	private void OnAttackRemotePlayerCharacter(PlayerAttackMessage playerAttackMessage)
	{
		float bombAngle = Mathf.Clamp(playerAttackMessage.BombAngle, 0.0f, 90.0f);
		bombAngle = (90.0f - bombAngle) * 0.67f;
		//Debug.Log(bombAngle);
		if (playerAttackMessage.UserId.Equals(GlobalCharaInfo.myInfo.UserId))
		{
			this.controller.AttackPunch(playerAttackMessage.LeftAttack, playerAttackMessage.RightAttack, bombAngle);
		}
		else if (!(playerAttackMessage.UserId.Equals(GlobalCharaInfo.myInfo.UserId)) && GlobalCharaInfo.cpuDict.ContainsKey(playerAttackMessage.UserId))
		{
			CPUCharacterController cpu = GlobalCharaInfo.cpuDict[playerAttackMessage.UserId];
			cpu.AttackBooleanTiming = true;
			cpu.AttackPunch(playerAttackMessage.LeftAttack, playerAttackMessage.RightAttack, bombAngle);
			cpu.AttackBooleanTiming = false;
		}


	}

	private void OnJoin(PlayerInform player)
    {
        //Debug.Log(player.UserId);
		Vector3 pos = new Vector3( 0.0f, 1.5f, 0.0f );
		int roleNum = 1;
		if (player.UserId.Equals(GlobalCharaInfo.myInfo.UserId))
		{
			generateMe(pos);
		}
		else
		{
			roleNum = 3;
		}
		
		generateCharacter(player.UserId, player.CharacterID, pos, (Role) roleNum);

	}

	private void OnStartGame(GameStartSettings[] startSets)
	{
		Vector3 pos = new Vector3(0.0f, 20.5f, 0.0f);
		generateMe(pos);
		for (int i = 0; i < startSets.Length; i++)
		{
			string id = startSets[i].UserId;
			int roleNum = startSets[i].CharacterRole;
			int charaId;
			if (id.Equals(GlobalCharaInfo.myInfo.UserId))
			{
				charaId = GlobalCharaInfo.myInfo.CharacterID;
			}
			else
			{
				roleNum += 2;
				charaId = GlobalCharaInfo.userIds[id];
			}
			Role role = (Role) roleNum;
			generateCharacter(id, charaId, startSets[i].Position, role);
		}

		GlobalCharaInfo.ableMove = true;

	}

	private void OnAlreadyJoin()
	{
		Vector3 pos = new Vector3(0.0f, 10.5f, 0.0f);
		generateMe(pos);
		foreach (var chara in GlobalCharaInfo.userIds)
		{
			if (chara.Key != GlobalCharaInfo.myInfo.UserId) generateCharacter(chara.Key, chara.Value, pos, Role.ONLINE_ESCAPER);
		}
		generateCharacter(GlobalCharaInfo.myInfo.UserId, GlobalCharaInfo.myInfo.CharacterID, pos, Role.ESCAPER);
	}

	private void OnDie(string userId)
	{
		if (userId.Equals(GlobalCharaInfo.myInfo.UserId))
		{
			Destroy(this.player);
			this.player = null;
			this.controller = null;
		}
		else
		{
			CPUCharacterController cpu = GlobalCharaInfo.cpuDict[userId];
			cpu.leaveDestroy();
			if (!GlobalCharaInfo.cpuDict.ContainsKey(userId))
			{
				GlobalCharaInfo.cpuDict.Remove(userId);
			}
		}
		GlobalCharaInfo.deadPeople += 1;
	}

    private async void OnLeave(PlayerInform player)
    {
		Debug.Log("ëﬁèoÇ∑ÇÈÇÊÇÒ2");

		if (player.UserId.Equals(GlobalCharaInfo.myInfo.UserId))
		{
			if (!GlobalCharaInfo.userIds.ContainsKey(player.UserId))
			{
				GlobalCharaInfo.userIds.Remove(player.UserId);
			}
			Destroy(this.player);
			this.player = null;
			this.controller = null;
		}
		else
		{
			if (GlobalCharaInfo.cpuDict.ContainsKey(player.UserId))
			{
				CPUCharacterController cpu = GlobalCharaInfo.cpuDict[player.UserId];
				cpu.leaveDestroy();
				GlobalCharaInfo.cpuDict.Remove(player.UserId);
			}

			if (GlobalCharaInfo.userIds.ContainsKey(player.UserId))
			{
				GlobalCharaInfo.userIds.Remove(player.UserId);
			}
		}

		if (player.isHost && !(player.UserId.Equals(GlobalCharaInfo.myInfo.UserId)))
		{
			await MatchComponent.Instance.LeaveRoom();
			SceneManager.LoadScene("UserSetting");
		}

	}

	private void generateMe(Vector3 pos)
	{
		int cId = GlobalCharaInfo.myInfo.CharacterID;
		this.player = Instantiate(myAmaturePrefab[cId], pos, Quaternion.identity);
		this.player.name = "playerYou";
		Instantiate(followCamera);
		this.controller = this.player.GetComponent<ThirdPerson>();
	}

	private void generateCharacter(string userId, int characterId, Vector3 generatePos, Role role)
	{
		if (userId.Equals(GlobalCharaInfo.myInfo.UserId))
		{
			this.controller.setCharacterParameter(role);
			this.player.transform.position = generatePos;
			GlobalCharaInfo.myRole = role;
			
			Debug.Log("ÉLÉÉÉâçÏÇÈÇÊÅ`");
			if (!GlobalCharaInfo.userIds.ContainsKey(userId))
			{
				Debug.Log("keyì¸ÇÍÇÈÇÊ");
				GlobalCharaInfo.userIds.Add(userId, characterId);
			}
			
		}
		else
		{
			Debug.Log("Character Generate");
			// ñÑñvñhé~
			Vector3 prePos = new Vector3(0.0f, 20.5f, 0.0f);
			GameObject chara = Instantiate(amaturePrefab[characterId], prePos, Quaternion.identity);
			chara.transform.position = generatePos;
			chara.name = userId;
			CPUCharacterController controller = chara.GetComponent<CPUCharacterController>();
			controller.setCharacter(chara);
			controller.setCharacterParameter(role);
			if (!GlobalCharaInfo.userIds.ContainsKey(userId))
			{
				GlobalCharaInfo.userIds.Add(userId, characterId);
			}

			if (!GlobalCharaInfo.cpuDict.ContainsKey(userId))
			{
				GlobalCharaInfo.cpuDict.Add(userId, controller);
			}

			
		}
		

	}

}
