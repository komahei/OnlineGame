using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GameClient
{
	public class CharaInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public Vector2 wheel;
		public bool jump;
		public bool sprint;
		public bool attackEnemy;
		public bool attackFurniture;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM

		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnWheel(InputValue value)
		{
			if (cursorInputForLook)
			{
				WheelInput(value.Get<Vector2>());
			}
		}

		public void OnAttackEnemy(InputValue value)
		{
			AttackEnemyInput(value.isPressed);
		}

		public void OnAttackFurniture(InputValue value)
		{
			AttackFurnitureInput(value.isPressed);
		}

		public void OnEscCursor(InputValue value)
		{
			EscapeCursor();
		}

#endif

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void WheelInput(Vector2 newMouseWheel)
		{
			wheel = newMouseWheel;
		}

		public void AttackEnemyInput(bool newAttackEnemyState)
		{
			attackEnemy = newAttackEnemyState;
		}

		public void AttackFurnitureInput(bool newAttackFurnitureState)
		{
			attackFurniture = newAttackFurnitureState;
		}

		public void EscapeCursor()
		{
			cursorLocked = !cursorLocked;
			SetCursorState(cursorLocked);
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

	}

}
