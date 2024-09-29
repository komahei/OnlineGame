using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient
{
	public class PlaySoundEffect : MonoBehaviour
	{

		AudioSource aud;
		[SerializeField] AudioClip inputSE;
		[SerializeField] AudioClip cursorSE;
		[SerializeField] AudioClip footstepSE;
		[SerializeField] AudioClip selectSE;
		[SerializeField] AudioClip explosionSE;

		void Start()
		{
			aud = GetComponent<AudioSource>();
		}

		public void PlayInputSE()
		{
			aud.PlayOneShot(inputSE);
		}

		public void PlayCursorSE()
		{
			aud.PlayOneShot(cursorSE);
		}

		public void PlayFootStepSE()
		{
			aud.PlayOneShot(footstepSE);
		}

		public void PlaySelectSE()
		{
			aud.PlayOneShot(selectSE);
		}

		public void PlayExplosionSE()
		{
			aud.PlayOneShot(explosionSE);
		}

	}

}
