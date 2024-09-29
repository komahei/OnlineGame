using UnityEngine;
using Cinemachine;

namespace GameClient
{
	public class FollowCamera : MonoBehaviour
	{

		[SerializeField] private CinemachineVirtualCamera followCamera;
		private GameObject CinemachineCameraTarget;

		void Start()
		{
			CinemachineCameraTarget = GameObject.Find("PlayerCameraRoot");
			followCamera.Follow = CinemachineCameraTarget.transform;
		}

	}

}
