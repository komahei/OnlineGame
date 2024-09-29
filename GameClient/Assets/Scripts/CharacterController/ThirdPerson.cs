using GameShared.MessagePackObjects;
using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
#endif

namespace GameClient
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif

	public class ThirdPerson : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 2.0f;

		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 5.335f;

		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float RotationSmoothTime = 0.12f;

		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;

		[Tooltip("The character used its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.50f;

		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in ground check")]
		public bool Grounded = true;

		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;

		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.28f;

		// 地面のレイヤーマスク
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;


		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;

		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 70.0f;

		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -30.0f;

		[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
		public float CameraAngleOverride = 0.0f;

		/*
		[Tooltip("you can set camera min/max depth in this parameters.")]
		public float CameraFowardDepth = -1.0f;
		public float CameraBackfowardDepth = -6.0f;
		*/

		[Tooltip("For lockinig the camera position on all axis")]
		public bool LockCameraPosition = false;

		// cinemachine
		private float _cinemachineTargetYaw;
		private float _cinemachineTargetPitch;
		private float _cinemachineDepth;
		private Vector3 _cinemachinePosition;


		// player
		private float _speed;
		private float _animationBlend;
		private float _targetRotation = 0.0f;
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

#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private Animator _animator;
		private CharacterController _controller;
		private CharaInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;
		private bool _hasAnimator;

		// MessagePackに送るよう
		public Vector3 messageMove = new Vector3(0, 0, 0);
		public bool isJump = false;
		public bool lAttack = false;
		public bool rAttack = false;

		public GameObject Bomb;
		public float BombRot = 0.0f;
		public GameObject rightHund;
		// x座標のrotationを変える
		public GameObject charaNeck;

		private bool lAttackTiming = true;
		private float _lAttackTime;
		private bool rAttackTiming = true;
		private float _rAttackTime;

		// キャラクタの役割
		//private CharaRole charaRole = new CharaRole();
		private Role myRole;

		private bool isDead;

		private PlaySoundEffect _soundEffect;


		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
			}
		}

		private void Awake()
		{
			// 借入れ
			this.myRole = Role.ESCAPER;
			this.isDead = false;
			lAttackTiming = true;
			rAttackTiming = true;
			_lAttackTime = 0.0f;
			_rAttackTime = 0.0f;
			// get a reference to out main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			// 追従するカメラのオイラー角（y座標）を取得
			_cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
			_hasAnimator = TryGetComponent(out _animator);
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<CharaInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assests/Reinstall Dependencies to fix it");
#endif

			AssignAnimationIDs();

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;

			GameObject soundObject = GameObject.Find("SEManager");
			_soundEffect = soundObject.GetComponent<PlaySoundEffect>();

		}

		private void Update()
		{
			_hasAnimator = TryGetComponent(out _animator);

			JumpAndGravity();
			GroundedCheck();
			if (GlobalCharaInfo.ableMove)
			{
				Move();
			}

			if (this.myRole == Role.CHASER)
			{
				if (GlobalCharaInfo.StartGameFlag)
				{
					AttackPunch();
				}
			}

			if (lAttackTiming) _lAttackTime = 0;
			else
			{
				_lAttackTime += Time.deltaTime;
				if (_lAttackTime > 0.85) lAttackTiming = true;
			}

			if (rAttackTiming) _rAttackTime = 0;
			else
			{
				_rAttackTime += Time.deltaTime;
				if (_rAttackTime > 0.85) rAttackTiming = true;
			}


		}

		private void LateUpdate()
		{
			CameraRotation();
			CameraDepth();
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

		private void CameraRotation()
		{
			// if there is an input and camera position is not fixed
			if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
			{

				// Don't multiply mouse input by Time.deltaTime;
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

				_cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
				_cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
			}

			// clamp our rotations so our values are limited 360 degrees
			_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// Cinemachine will follow this target
			// 上-30 下70
			// +30 /100 * 90
			// BombRotは-90した後絶対値取る
			BombRot = (_cinemachineTargetPitch + CameraAngleOverride + 30) / 100 * 90;
			//Debug.Log((_cinemachineTargetPitch + CameraAngleOverride) + " : " + BombRot + " : ");
			CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);

		}

		private void CameraDepth()
		{
			if (!LockCameraPosition && _input.wheel.y != 0)
			{

				_cinemachineDepth = CinemachineCameraTarget.transform.localPosition.z;
				_cinemachineDepth += (_input.wheel.y / 600.0f);

				if (_cinemachineDepth > 1.5f)
				{
					_cinemachineDepth = 1.5f;
				}
				if (-1.5f > _cinemachineDepth)
				{
					_cinemachineDepth = -1.5f;
				}

				//_cinemachinePosition = new Vector3(CinemachineCameraTarget.transform.position.x, CinemachineCameraTarget.transform.position.y, _cinemachineDepth);
				_cinemachinePosition = new Vector3(0.0f, CinemachineCameraTarget.transform.localPosition.y, _cinemachineDepth);

				CinemachineCameraTarget.transform.localPosition = _cinemachinePosition;

			}
		}

		private void Move()
		{
			// set target speed based on move speed. sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
			//messageMove = new Vector3(_input.move.x, 0.0f, _input.move.y);

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone. and is cheaper than magnitude
			// if there is no input. set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;
			

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped. so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}else{
				_speed = targetSpeed;
			}

			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
			if (_animationBlend < 0.01f) _animationBlend = 0f;

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone. and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
				float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
				//float rotation = Mathf.SmoothDampAngle(CinemachineCameraTarget.transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

				// rotate to face input direction relative to camera position
				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

				// カメラターゲットの回転を加える
				// キャラクタが横を向いた際にカメラも同じ方向を向く

				
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				_cinemachineTargetYaw += inputDirection.x * 0.6f * deltaTimeMultiplier;
				// clamp our rotations so our values are limited 360 degrees
				_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
				
				CinemachineCameraTarget.transform.rotation = Quaternion.Euler(0.0f, _cinemachineTargetYaw, 0.0f);

			}

			Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

			// move the player
			//_controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
			messageMove = targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
			_controller.Move(messageMove);
			//Debug.Log("ThirdPerson : " + inputMagnitude);
			//Debug.Log(_animationBlend);
			//Debug.Log(messageMove);

			// update animator if using character
			if (_hasAnimator)
			{
				_animator.SetFloat(_animIDSpeed, _animationBlend);
				_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
			}


		}

		private void JumpAndGravity()
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
				isJump = _input.jump;
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

			}else{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}else{
					// update animator if using character
					if (_hasAnimator)
					{
						_animator.SetBool(_animIDFreeFall, true);
					}
				}

				// if we are not grounded. do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}

		}


		private async void AttackPunch()
		{
			if (_hasAnimator)
			{
				_animator.SetBool(_animIDAttack, false);
				_animator.SetBool(_animIDThrowAttack, false);
			}

			if (!lAttackTiming || !rAttackTiming || !GlobalCharaInfo.StartGameFlag)
			{
				_input.attackEnemy = false;
				_input.attackFurniture = false;
				return;
			}

			// 左クリック
			lAttack = _input.attackEnemy;
			if (lAttack && lAttackTiming)
			{
				lAttackTiming = false;
				//_animator.SetBool(_animIDAttack, true);
				//_animator.SetBool(_animIDThrowAttack, true);
			}

			// 右クリック
			rAttack = _input.attackFurniture;
			if (rAttack && rAttackTiming)
			{
				//_animator.SetBool(_animIDThrowAttack, true);
				rAttackTiming = false;
			}

			if (rAttack || lAttack)
			{
				if (MatchComponent.Instance.isJoin && GlobalCharaInfo.ableMove)
				{
					//Debug.Log(BombRot);
					PlayerAttackMessage playerAttackMessage = new PlayerAttackMessage
					{
						UserId = GlobalCharaInfo.myInfo.UserId,
						LeftAttack = lAttack,
						RightAttack = rAttack,
						BombAngle = BombRot
					};
					await MatchComponent.Instance.AttackAsync(playerAttackMessage);
				}
			}

			_input.attackEnemy = false;
			_input.attackFurniture = false;

		}

		public void AttackPunch(bool lAttack, bool rAttack, float throwingAngle)
		{
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

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// When selected, draw a gizmom in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
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

		public void setCharacterParameter(Role role)
		{
			this.myRole = role;
			if (role == Role.CHASER || role == Role.ONLINE_CHASER)
			{
				this.transform.localScale = new Vector3(7.50f, 7.50f, 7.50f);
				MoveSpeed = MoveSpeed * 5.0f;
				SprintSpeed = SprintSpeed * 5.0f;
				JumpHeight = JumpHeight * 2.0f;
				Gravity = Gravity * 4.0f;
				if (role != Role.ONLINE_CHASER)
				{
					_cinemachinePosition = new Vector3(0.0f, CinemachineCameraTarget.transform.localPosition.y, -1.0f);
					CinemachineCameraTarget.transform.localPosition = _cinemachinePosition;
				}
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
				if (this.myRole == Role.CHASER && GlobalCharaInfo.StartGameFlag)
				{
					if (_animator.GetBool(_animIDAttack) || _animator.GetBool(_animIDThrowAttack)) Destroy(hit.collider.gameObject);
				}

			}
			*/

			// 爆弾に当たった場合
			if (hit.collider.gameObject.tag == "Bomb")
			{
				Debug.Log("爆弾が当たりまして");
				if (this.myRole == Role.ESCAPER && GlobalCharaInfo.StartGameFlag && !this.isDead)
				{
					Debug.Log("死にまして");
					MatchComponent.Instance.DieCharacter();
					isDead = true;
				}
			}

			// 追跡者に当たった場合
			// 足にColliderを付けるなどすべきかも
			/*
			if (hit.collider.gameObject.tag == "Player" && !this.isDead)
			{
				Role hisRole = hit.collider.gameObject.GetComponent<CPUCharacterController>().getMyRole();
				if (this.myRole == Role.ESCAPER 
					&& hisRole == Role.ONLINE_CHASER
					&& GlobalCharaInfo.StartGameFlag)
				{
					MatchComponent.Instance.DieCharacter();
					isDead = true;
				}
			}
			*/


		}

	}

}

