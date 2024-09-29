using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;

namespace GameClient
{
	public class WaitingText : MonoBehaviour
	{

		public TextMeshProUGUI waitText;
		float waitTime = 1.5f;
		float _time;
		int index;
		string t;

		void Start()
		{
			index = 0;
			_time = 0;
		}

		void Update()
		{
			_time += Time.deltaTime;
			if (_time >= waitTime)
			{
				textChracter();
				_time = 0;
			}

		}

		private void textChracter()
		{
			if (index % 5 == 0)
			{
				t = "*";
				waitText.text = "";
			}
			waitText.text = t;
			t += " *";
			index++;
			//Debug.Log(t);
		}

	}

}

