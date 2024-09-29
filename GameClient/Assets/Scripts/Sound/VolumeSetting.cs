using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace GameClient
{
	public class VolumeSetting : MonoBehaviour
	{
		[SerializeField] private AudioMixer audioMixer;
		[SerializeField] private Slider volumeBGMSlider;
		[SerializeField] private Slider volumeSESlider;

		// Start is called before the first frame update
		void Start()
		{
			if (volumeBGMSlider != null)
			{
				volumeBGMSlider.onValueChanged.AddListener(SetAudioMixerBGM);
				SetSliderPos("BGM", volumeBGMSlider);
			}

			if (volumeSESlider != null)
			{
				volumeSESlider.onValueChanged.AddListener(SetAudioMixerSE);
				SetSliderPos("SE", volumeSESlider);
			}

		}

		private void SetSliderPos(string Music, Slider slider)
		{
			audioMixer.GetFloat(Music, out float volume);
			slider.value = volume;
		}

		// BGM
		public void SetAudioMixerBGM(float value)
		{
			/*
			// 5íiäKï‚ê≥
			value /= 5;
			// -80Å`0Ç…ïœä∑
			var volume = Mathf.Clamp(Mathf.Log10(value) * 20f, -80f, 0f);
			audioMixer.SetFloat("BGM", volume);
			*/
			audioMixer.SetFloat("BGM", value);
		}

		// SE
		public void SetAudioMixerSE(float value)
		{
			/*
			// 5íiäKï‚ê≥
			value /= 5;
			// -80Å`0Ç…ïœä∑
			var volume = Mathf.Clamp(Mathf.Log10(value) * 20f, -80f, 0f);
			audioMixer.SetFloat("SE", volume);
			*/
			audioMixer.SetFloat("SE", value);
		}

	}

}

