﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Example2 : MonoBehaviour
{
	[SerializeField]
	private Dropdown m_dropDownIntroBgmName;
	[SerializeField]
	private Dropdown m_dropDownBgmName;
	[SerializeField]
	private InputField m_inputFadeInTime;
	[SerializeField]
	private InputField m_inputFadeOutTime;
	[SerializeField]
	private Toggle m_toggleIsLoop;
	[SerializeField]
	private Text m_textSceneTitle;

	[SerializeField]
	private Slider m_sliderPlayTime;
	[SerializeField]
	private Slider m_sliderVolume;
	[SerializeField]
	private Slider m_sliderDelayBgm;
	[SerializeField]
	private Slider m_sliderCrossFadeRate;
	[SerializeField]
	private Text m_textShowVolume;
	[SerializeField]
	private Text m_textShowDelay;
	[SerializeField]
	private Text m_textShowCrossFadeRate;

	//ButtonList
	[SerializeField]
	private Button m_buttonPlay;
	[SerializeField]
	private Button m_buttonPause;
	[SerializeField]
	private Button m_buttonStop;


	//CalledTextList
	[SerializeField]
	private Example1_CalledText calledTextIntroStart;
	[SerializeField]
	private Example1_CalledText calledTextIntroComplete;
	[SerializeField]
	private Example1_CalledText calledTextMainStart;
	[SerializeField]
	private Example1_CalledText calledTextMainComplete;


	//Spectrum
	[SerializeField]
	private Example1_Spectrum[] m_spectrum;
	[SerializeField]
	private int m_spectrumWidth = 100;

	private bool m_isPause = false;
	private BackGroundMusicPlayer m_player;

	private string selectBgmIntroName
	{
		get
		{
			if (m_dropDownIntroBgmName.value == 0)
				return "";

			var idx = m_dropDownIntroBgmName.value;
			var itemName = m_dropDownIntroBgmName.options[idx];
			return itemName.text;
		}
	}

	private string selectBgmMainName
	{
		get
		{
			var idx = m_dropDownBgmName.value;
			var itemName = m_dropDownBgmName.options[idx];
			return itemName.text;
		}
	}

	private void Start()
	{
		OnSliderChangeVolume();
		OnSliderChangeDelay();
		OnSliderChangeCrossFadeRate();

		m_buttonPlay.gameObject.SetActive(true);
		m_buttonStop.gameObject.SetActive(true);
		m_buttonPause.gameObject.SetActive(false);

		string[] enumNames = System.Enum.GetNames(typeof(SoundNameBGM));
		List<string> names = new List<string>(enumNames);
		m_dropDownBgmName.ClearOptions();
		m_dropDownBgmName.AddOptions(names);
		m_dropDownIntroBgmName.ClearOptions();
		m_dropDownIntroBgmName.AddOptions(names);

		for (int i = 0; i < m_spectrum.Length; i++)
		{
			m_spectrum[i].min = i * m_spectrumWidth;
			m_spectrum[i].maximam = (i * m_spectrumWidth) + m_spectrumWidth;
		}
	}

	private void Update()
	{
		if (m_player == null)
			return;

		if (m_player.isActive)
		{
			m_sliderPlayTime.value = m_player.Length;
		}
		else
		{
			m_sliderPlayTime.value = 0.0f;
		}
	}

	public void OnButtonDownSceneReload()
	{
		SceneManager.LoadScene(0);
	}

	public void OnSliderChangeVolume()
	{
		m_textShowVolume.text = (m_sliderVolume.value * 100.0f).ToString("0") + " %";
	}

	public void OnButtonDownPlay()
	{
		if (m_isPause)
		{
			Debug.Log("Resume");
			SimpleSoundManager.Instance.ResumeBGM();
			m_isPause = false;
		}
		else
		{
			Debug.Log(selectBgmMainName);

			//Play
			Hashtable ht = new Hashtable();
			ht.Add(SimpleSoundManager.HashParam_BGM.introSoundName, selectBgmIntroName);
			ht.Add(SimpleSoundManager.HashParam_BGM.isLoop, m_toggleIsLoop.isOn);
			ht.Add(SimpleSoundManager.HashParam_BGM.volume, m_sliderVolume.value);
			ht.Add(SimpleSoundManager.HashParam_BGM.delay, m_sliderDelayBgm.value);
			ht.Add(SimpleSoundManager.HashParam_BGM.fadeInTime, (m_inputFadeInTime.text == "") ? 0.0f : float.Parse(m_inputFadeInTime.text));
			ht.Add(SimpleSoundManager.HashParam_BGM.fadeOutTime, (m_inputFadeOutTime.text == "") ? 0.0f : float.Parse(m_inputFadeOutTime.text));
			ht.Add(SimpleSoundManager.HashParam_BGM.crossFadeRate, m_sliderCrossFadeRate.value);
			ht.Add(SimpleSoundManager.HashParam_BGM.onIntroStart, new UnityAction(() => calledTextIntroStart.Show()));
			ht.Add(SimpleSoundManager.HashParam_BGM.onIntroComplete, new UnityAction(() => calledTextIntroComplete.Show()));
			ht.Add(SimpleSoundManager.HashParam_BGM.onMainStart, new UnityAction(() => calledTextMainStart.Show()));
			ht.Add(SimpleSoundManager.HashParam_BGM.onMainComplete, new UnityAction(OnPlayBgmComplete));

			m_player = SimpleSoundManager.Instance.PlayBGM(selectBgmMainName, ht);

			if (m_player == null)
				return;

			for (int i = 0; i < m_spectrum.Length; i++)
			{
				m_spectrum[i].audioSource = m_player.source;
			}
		}

		m_buttonPause.gameObject.SetActive(true);
	}

	void OnPlayBgmComplete()
	{
		if (!m_player.isPlaying)
			m_buttonPause.gameObject.SetActive(false);
	}

	public void OnButtonDownPause()
	{
		if (!m_player.isPlaying)
			return;

		m_isPause = true;
		SimpleSoundManager.Instance.PauseBGM();
		m_buttonPause.gameObject.SetActive(false);
		//m_buttonPlay.gameObject.SetActive(true);
	}

	public void OnButtonDownStop()
	{
		SimpleSoundManager.Instance.StopBGM();
		m_buttonPause.gameObject.SetActive(false);
		m_buttonPlay.gameObject.SetActive(true);
	}

	public void OnSliderChangeCrossFadeRate()
	{
		m_textShowCrossFadeRate.text = (m_sliderCrossFadeRate.value * 100).ToString("0") + " %";
	}

	public void OnSliderChangeDelay()
	{
		m_textShowDelay.text = m_sliderDelayBgm.value.ToString("F2") + " sec";
	}

	public void OnButtonDownNextScene()
	{
		var no = SceneManager.GetActiveScene().buildIndex;
		no++;
		if (no >= SceneManager.sceneCountInBuildSettings) { no = 0; }
		SceneManager.LoadScene(no);
	}
}
