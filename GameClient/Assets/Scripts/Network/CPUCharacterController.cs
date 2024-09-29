using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameClient
{
	public class CPUCharacterController : MonoBehaviour
	{
		public float MoveSpeed = 2.0f;
		public float SprintSpeed = 5.335f;
		[Range(0.0f, 0.3f)]
		public float RotationSmoothTime = 0.12f;
		public float SpeedChangeRate = 10.0f;
		public float JumpHeight = 1.2f;
		public float Gravity = -15.0f;

		public float JumpTimeout = 0.50f;
		public float FallTimeout = 0.15f;
		public bool Grounded = true;
		public float GroundedOffset = -0.14f;
		public float GroundedRadius = 0.28f;
		public LayerMask GroundLayers;


		private float _speed;
		private float _animationBlend;
		//private float _targetRotation = 0.0f;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		// animation IDs
		private int _animIDSpeed;
		private int _animIDGrounded;
		private int _animIDJump;
		private int _animIDFreeFall;
		private int _animIDMotionSpeed;
		private int _animIDAttack;
		private int _animIDThrowAttack;

		private Animator _animator;
		private CharacterController _controller;
		private const float _threshold = 0.01f;
		private bool _hasAnimator;

		private bool isJump;

		private GameObject chara;

		private Vector3 globalSpeed;

		public GameObject Bomb;
		private GameObject rightHund;
		// x座標のrotationを変える
		public GameObject charaNeck;

		// キャラクタの役割
		//private CharaRole charaRole = new CharaRole();
		private Role myRole;
		public bool AttackBooleanTiming;

		private PlaySoundEffect _soundEffect;


		private struct CharacterInform
		{
			public Vector3 pos { get; set; }
			public Quaternion rot { get; set; }
			public float time { get; set; }

			public CharacterInform(Vector3 pos, Quaternion rot, float time)
			{
				this.pos = pos;
				this.rot = rot;
				this.time = time;
			}
		}

		// 補正のための変数
		private List<CharacterInform> characterInformList = new List<CharacterInform>();
		private float cpuFixedTime = 0.0f;
		private float cpuStartTime = 0.0f;
		private int inforListIdx = 0;
		private Vector3 targetPos;

		private float moveDamp = 12.0f; // 移動減衰
		private int interval = 2; // characterInformList参照の広さ
		private float latency = 0.0f; // 最初は遅延なしで考える


		void Awake()
		{
			// 借入れ
			myRole = Role.ESCAPER;
			globalSpeed = Vector3.zero;
		}

		// Start is called before the first frame update
		void Start()
		{
			_hasAnimator = TryGetComponent(out _animator);
			_controller = GetComponent<CharacterController>();
			AssignAnimationIDs();

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			AttackBooleanTiming = false;

			GameObject soundObject = GameObject.Find("SEManager");
			_soundEffect = soundObject.GetComponent<PlaySoundEffect>();

			cpuStartTime = Time.fixedTime;

			/*
			targetPos = this.transform.position;
			targetRot = this.transform.rotation;
			clientTime = Time.time;
			*/
		}


		// Update is called once per frame
		void Update()
		{
			_hasAnimator = TryGetComponent(out _animator);

			JumpAndGravity();
			GroundedCheck();
			if (GlobalCharaInfo.ableMove)
			{
				MoveAnimation();
			}

			if (_hasAnimator)
			{
				if (!AttackBooleanTiming)
				{
					_animator.SetBool(_animIDAttack, false);
					_animator.SetBool(_animIDThrowAttack, false);
				}
				
			}

		}

		private void FixedUpdate()
		{
			if (GlobalCharaInfo.ableMove)
			{
				LinearInterpolationMove();
			}
		}


		private void GetChildren(GameObject obj)
		{
			Transform children = obj.GetComponentInChildren<Transform>();
			if (children.childCount == 0)
			{
				return;
			}

			foreach (Transform ob in children)
			{
				if (ob.name == "J_Bip_R_Hand")
				{
					//this.rightHund = ob.transform.GetChild(0).gameObject;
					this.rightHund = ob.gameObject;
				}

				if (ob.name == "J_Bip_C_Neck")
				{
					//this.rightHund = ob.transform.GetChild(0).gameObject;
					this.charaNeck = ob.gameObject;
				}

				GetChildren(ob.gameObject);
			}

		}

		private void AssignAnimationIDs()
		{
			_animIDSpeed = Animator.StringToHash("Speed");
			_animIDGrounded = Animator.StringToHash("Grounded");
			_animIDJump = Animator.StringToHash("Jump");
			_animIDFreeFall = Animator.StringToHash("FreeFall");
			_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
			_animIDAttack = Animator.StringToHash("Attack");
			_animIDThrowAttack = Animator.StringToHash("ThrowAttack");
		}

		private void GroundedCheck()
		{
			// set sphere position. with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);


			// update animator if using character
			if (_hasAnimator)
			{
				_animator.SetBool(_animIDGrounded, Grounded);
			}
		}

		private void LinearInterpolationMove()
		{
			var t = Time.fixedTime - cpuStartTime - latency; // 現在の時間
			var i = inforListIdx;
			while (i < characterInformList.Count - interval)
			{
				// 一定期間内にキャラがいる
				var fromInfo = characterInformList[i];
				var toInfo = characterInformList[i + interval - 1];
				if (t >= fromInfo.time && t < toInfo.time)
				{
					// 補間時間を割り出す
					var rate = Mathf.InverseLerp(fromInfo.time, toInfo.time, t);
					// 目的位置を割り出す
					targetPos = Vector3.Lerp(fromInfo.pos, toInfo.pos, rate);
					// 回転はここで指定
					transform.rotation = Quaternion.Slerp(fromInfo.rot, toInfo.rot, rate);
					// インデックス指定
					inforListIdx = Mathf.Max(i - 1, 0);
					// 不要なキャラ情報を削除
					if (inforListIdx > 0)
					{
						characterInformList.RemoveAt(0);
						// 遅延なく参照できた場合は遅延を少し少なくする
						latency = Mathf.Max(latency - Time.fixedDeltaTime, 0.0f);
					}
					break;
				}
				i++;
			}

			// 上手く参照できなかった場合は遅延を大きくする
			if (i >= characterInformList.Count - interval) latency += Time.fixedDeltaTime;

			this.transform.position = Vector3.Lerp(this.transform.position, targetPos, Time.fixedDeltaTime * moveDamp);
		}

		public void Move(Vector3 speed)
		{
			// move the player
			//var sp = new Vector3 ( speed.x, speed.y, 0.0f );
			globalSpeed = speed;
			// キャラクタを動かす
			// 0.2秒ごとに座標を受け取るからここはなし？
			// speed自体はアニメーションに使うからとっておく
			//if (this.chara != null)  _controller?.Move(speed);
		}

		private void MoveAnimation()
		{
			float targetSpeed = MoveSpeed;
			if (globalSpeed.x == 0.0f && globalSpeed.z == 0.0f) targetSpeed = 0.0f;
			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
			if (_animationBlend < 0.01f) _animationBlend = 0f;

			if (_hasAnimator)
			{
				_animator.SetFloat(_animIDSpeed, _animationBlend);
				//_animator.SetFloat(_animIDMotionSpeed, speed.z);
				_animator.SetFloat(_animIDMotionSpeed, 1);
			}
		}

		public void setCharacter(GameObject chara)
		{
			this.chara = chara;
		}

		public void setCharacterInfo(Vector3 pos, Quaternion rot, float time)
		{
			if (this.chara != null)
			{
				if (cpuFixedTime == 0.0f) cpuFixedTime = time;
				time -= cpuFixedTime;
				CharacterInform characterInform = new CharacterInform(pos, rot, time);
				characterInformList.Add(characterInform);
			}
		}

		/*
		public void setClientTime(float time)
		{
			this.clientTime = time;
		}

		public void setPosition(Vector3 pos)
		{
			if (this.chara != null)
			{
				this.targetPos = pos;
			}
		}

		public void setRotation(Quaternion rot) {
			if (this.chara != null)
			{
				this.targetRot = rot;
			}
		}
		*/

		public void leaveDestroy()
		{
			Destroy(this.chara);
			this.chara = null;
		}

		public void setJump(bool jump) { 
			this.isJump = jump;
		}


		public void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// update animator if using character
				if (_hasAnimator)
				{
					_animator.SetBool(_animIDJump, false);
					_animator.SetBool(_animIDFreeFall, false);
				}

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (isJump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2 * Gravity);

					// update animator if using character
					if (_hasAnimator)
					{
						_animator.SetBool(_animIDJump, true);
					}
				}

				// Jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}

			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}
				else
				{
					// update animator if using character
					if (_hasAnimator)
					{
						_animator.SetBool(_animIDFreeFall, true);
					}
				}

				// if we are not grounded. do not jump
				isJump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		public void AttackPunch(bool lAttack, bool rAttack, float throwingAngle)
		{
			if (_hasAnimator)
			{
				_animator.SetBool(_animIDAttack, false);
				_animator.SetBool(_animIDThrowAttack, false);
			}
			if (lAttack)
			{
				//_animator.SetBool(_animIDAttack, lAttack);
				_animator.SetBool(_animIDThrowAttack, lAttack);
			}
			if (rAttack)
			{
				_animator.SetBool(_animIDThrowAttack, rAttack);
			}

			if (lAttack)
			{
				var plusPos = this.gameObject.transform.forward * 10.0f;
				var qloneBomb = Instantiate(Bomb, rightHund.transform.position + plusPos, Quaternion.identity);
				ThrowBomb throwBomb = qloneBomb.GetComponent<ThrowBomb>();
				throwBomb.setWaitTime(1.0f);
				ThrowingBall(qloneBomb, throwingAngle, 30.0f);
			}

			if (rAttack)
			{
				var plusPos = this.gameObject.transform.forward * 10.0f;
				var qloneBomb = Instantiate(Bomb, rightHund.transform.position + plusPos, Quaternion.identity);
				ThrowBomb throwBomb = qloneBomb.GetComponent<ThrowBomb>();
				throwBomb.setWaitTime(3.5f);
				ThrowingBall(qloneBomb, throwingAngle, 50.0f);
			}
		}

		private void ThrowingBall(GameObject obj, float ThrowingAngle, float posVal)
		{
			Vector3 targetPosition = obj.transform.position + this.gameObject.transform.forward * posVal;
			Rigidbody rb = obj.GetComponent<Rigidbody>();
			float angle = ThrowingAngle;
			Vector3 velocity = CalculateVelocity(obj.transform.position, targetPosition, angle);
			rb.AddForce(velocity * rb.mass, ForceMode.Impulse);
		}

		// 標的に命中する射出速度の計算
		private Vector3 CalculateVelocity(Vector3 pointA, Vector3 pointB, float angle)
		{
			// 射出角をラジアンに変換
			float rad = angle * Mathf.PI / 180;

			// 水平方向の距離x
			float x = Vector2.Distance(new Vector2(pointA.x, pointA.z), new Vector2(pointB.x, pointB.z));
			// 垂直方向の距離y
			float y = pointA.y - pointB.y;

			// 斜方投射の公式を初速度について解く
			float speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(x, 2) / (2 * Mathf.Pow(Mathf.Cos(rad), 2) * (x * Mathf.Tan(rad) + y)));

			if (float.IsNaN(speed))
			{
				// 条件を満たす初速を算出できなければVector3.zeroを返す
				return Vector3.zero;
			}
			else
			{
				return (new Vector3(pointB.x - pointA.x, x * Mathf.Tan(rad), pointB.z - pointA.z).normalized * speed);
			}
		}


		private void OnFootstep(AnimationEvent animationEvent)
		{
			if (animationEvent.animatorClipInfo.weight > 0.5f)
			{
				// 歩く音を鳴らす
				_soundEffect.PlayFootStepSE();
			}
		}

		private void OnLand(AnimationEvent animationEvent)
		{
			if (animationEvent.animatorClipInfo.weight > 0.5f)
			{

			}
		}

		public Role getMyRole()
		{
			return myRole;
		}

		public void setCharacterParameter(Role role)
		{
			myRole = role;
			if (role == Role.CHASER || role == Role.ONLINE_CHASER)
			{
				this.transform.localScale = new Vector3(7.50f, 7.50f, 7.50f);
				MoveSpeed = MoveSpeed * 5.0f;
				SprintSpeed = SprintSpeed * 5.0f;
				JumpHeight = JumpHeight * 2.0f;
				Gravity = Gravity * 4.0f;
				GetChildren(this.gameObject);
			}
			else if (role == Role.ESCAPER || role == Role.ONLINE_ESCAPER)
			{
				this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			}

		}

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			/*
			if (hit.collider.gameObject.tag == "Furniture")
			{
				//if (_animator.GetBool(_animIDAttack))
				if (this.myRole == Role.ONLINE_CHASER && GlobalCharaInfo.StartGameFlag)
				{
					Destroy(hit.collider.gameObject);
				}

			}
			*/
		}

	}

}
