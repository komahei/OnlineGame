using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient
{
	public class ThrowBomb : MonoBehaviour
	{
		float waitTime = 2.3f;
		float _time;
		Rigidbody rb;
		Renderer render;
		SphereCollider col;

		public ParticleSystem particle;
		private ParticleSystem explosion;

		[SerializeField, Range(0F, 90F), Tooltip("éÀèoÇ∑ÇÈäpìx")]
		private float ThrowingAngle;

		private bool timeIsOver;

		private PlaySoundEffect _soundEffect;

		// Start is called before the first frame update
		void Start()
		{
			rb = GetComponent<Rigidbody>();
			render = GetComponent<Renderer>();
			col = GetComponent<SphereCollider>();
			_time = 0;
			timeIsOver = false;
			GameObject soundObject = GameObject.Find("SEManager");
			_soundEffect = soundObject.GetComponent<PlaySoundEffect>();
			//ThrowingBall();
		}

		// Update is called once per frame
		void Update()
		{
			_time += Time.deltaTime;
			if (_time >= waitTime && !timeIsOver)
			{
				explosion = Instantiate(particle);
				explosion.transform.position = this.transform.position;
				explosion.transform.parent = this.transform;
				render.enabled = false;
				explosion.Play();
				col.radius = 5.0f;
				timeIsOver = true;
				// îöî≠âπÇñ¬ÇÁÇ∑
				_soundEffect.PlayExplosionSE();
			}
			if (timeIsOver)
			{
				if (explosion == null)
				{
					Destroy(this.gameObject);
				}
			}
		}

		private void OnCollisionEnter(Collision col)
		{
			if (col.gameObject.tag == "Furniture")
			{
				if (timeIsOver)
				{
					Destroy(col.gameObject);
				}
			}
		}

		public void setWaitTime(float time)
		{
			waitTime = time;
		}

	}

}
