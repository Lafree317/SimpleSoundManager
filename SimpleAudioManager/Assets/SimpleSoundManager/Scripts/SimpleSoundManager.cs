﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using LightGive;

/// <summary>
/// シンプルなサウンドを管理するマネージャー
/// </summary>
public class SimpleSoundManager : LightGive.SingletonMonoBehaviour<SimpleSoundManager>
{
	private const int DefaultSePlayerNum = 10;
	private const int DefaultBgmPlayerNum = 2;
	private const float DefaultVolume = 1.0f;
	private const float DefaultSePitch = 1.0f;
	private const float DefaultSeDelay = 0.0f;
	private const float DefaultSeFadeTime = 0.0f;
	private const float DefaultMinDistance = 1.0f;
	private const float DefaultMaxDistance = 500.0f;

	//セーブキー
	private const string SaveKeyVolumeTotal = "SaveKeyVolumeTotal";
	private const string SaveKeyVolumeBgm = "SaveKeyVolumeBgm";
	private const string SaveKeyVolumeSe = "SaveKeyVolumeSe";

	private readonly Vector3 DefaultPos = Vector3.zero;

	/// <summary>
	/// 全てのBGM音声ファイルのリスト
	/// </summary>
	[AudioClipInfo, SerializeField]
	public List<AudioClipInfo> bgmAudioClipList = new List<AudioClipInfo>();
	/// <summary>
	/// 全てのSE音声ファイルのリスト
	/// </summary>
	[AudioClipInfo, SerializeField]
	public List<AudioClipInfo> seAudioClipList = new List<AudioClipInfo>();
	/// <summary>
	/// 使用するBGMのDictionary
	/// </summary>
	public Dictionary<string, AudioClipInfo> bgmDictionary;
	/// <summary>
	/// 使用するSEのDictionary
	/// </summary>
	public Dictionary<string, AudioClipInfo> seDictionary;

	//実際に使用するプレイヤーのリスト
	public List<SoundEffectPlayer> sePlayerList = new List<SoundEffectPlayer>();
	//実際に使用するBGMのプレイヤーのリスト
	public List<BackGroundMusicPlayer> bgmPlayerList = new List<BackGroundMusicPlayer>();

	/// <summary>
	/// オーディオミキサー
	/// </summary>
	[SerializeField]
	public AudioMixerGroup bgmAudioMixerGroup;
	[SerializeField]
	public AudioMixerGroup seAudioMixerGroup;

	/// <summary>
	/// 全体の音量
	/// </summary>
	[SerializeField]
	private float totalVolume = DefaultVolume;
	/// <summary>
	/// BGMの音量
	/// </summary>
	[SerializeField]
	private float bgmVolume = DefaultVolume;
	/// <summary>
	/// SEの音量
	/// </summary>
	[SerializeField]
	private float seVolume = DefaultVolume;
	/// <summary>
	/// オーディオソースの数
	/// </summary>
	[SerializeField]
	public int sePlayerNum = DefaultSePlayerNum;

	[SerializeField]
	public int bgmPlayerNum = DefaultSePlayerNum;

	/// <summary>
	/// Volumeを変更した時に保存するか
	/// </summary>
	[SerializeField]
	public bool volumeChangeToSave = false;
	[SerializeField]
	public bool volumeLoadAwake = false;


	private int bgmPlayerIndex = 0;

	public float TotalVolume
	{
		get { return totalVolume; }
		set
		{
			totalVolume = value;
			ChangeAllVolume();

			if (volumeChangeToSave)
				SaveVolume();
		}
	}

	public float SEVolume
	{
		get { return seVolume; }
		set
		{
			seVolume = value;
			ChangeAllVolume();

			if (volumeChangeToSave)
				SaveVolume();
		}
	}

	public float BGMVolume
	{
		get { return bgmVolume; }
		set
		{
			bgmVolume = value;
			ChangeAllVolume();

			if (volumeChangeToSave)
				SaveVolume();
		}
	}

	protected override void Awake()
	{
		base.Awake();

		bgmPlayerList.Clear();
		sePlayerList.Clear();

		for (int i = 0; i < bgmPlayerNum; i++)
		{
			GameObject bgmPlayerObj = new GameObject("BGMPlayerObj" + i.ToString());
			bgmPlayerObj.transform.SetParent(this.gameObject.transform);
			BackGroundMusicPlayer bgmPlayer = bgmPlayerObj.AddComponent<BackGroundMusicPlayer>();
			bgmPlayerList.Add(bgmPlayer);
			bgmPlayerObj.SetActive(false);
		}

		for (int i = 0; i < sePlayerNum; i++)
		{
			GameObject sePlayerObj = new GameObject("SEPlayerObj" + i.ToString());
			sePlayerObj.transform.SetParent(this.gameObject.transform);
			SoundEffectPlayer sePlayer = sePlayerObj.AddComponent<SoundEffectPlayer>();
			sePlayerList.Add(sePlayer);
			sePlayerObj.SetActive(false);
		}

		//Dictionaryを初期化
		bgmDictionary = new Dictionary<string, AudioClipInfo>();
		seDictionary = new Dictionary<string, AudioClipInfo>();
		for (int i = 0; i < bgmAudioClipList.Count; i++)
			bgmDictionary.Add(bgmAudioClipList[i].audioName, bgmAudioClipList[i]);
		for (int i = 0; i < seAudioClipList.Count; i++)
			seDictionary.Add(seAudioClipList[i].audioName, seAudioClipList[i]);



		if (volumeLoadAwake)
			LoadVolume();
	}

	private void Update()
	{
		for (int i = 0; i < sePlayerList.Count; i++)
		{
			var player = sePlayerList[i];
			if (!player.IsPlaying)
				continue;
			player.PlayerUpdate();
		}

		for (int i = 0; i < bgmPlayerList.Count; i++)
		{
			var player = bgmPlayerList[i];
			if (!player.IsPlaying)
				continue;
			player.PlayerUpdate();
		}
	}

	public void PlayBGM(AudioNameBGM _audioName)
	{
		PlayBGM(_audioName.ToString(), bgmVolume * totalVolume, true, 0.0f, 0.0f, 0.0f, false, 0.0f, 1.0f);
	}


	/// <summary>
	/// BGMを再生する
	/// </summary>
	/// <param name="_audioName">SEの名前</param>
	public void PlayBGM(string _audioName)
	{
		PlayBGM(_audioName, bgmVolume * totalVolume, true, 0.0f, 0.0f, 0.0f, false, 0.0f, 1.0f);
	}

	public void StopBGM()
	{
		for (int i = 0; i < bgmPlayerList.Count; i++)
		{
			var bgmPlayer = bgmPlayerList[i];
			bgmPlayer.Stop();
		}
	}


	/// <summary>
	/// クロスフェードしながらBGMを再生する
	/// </summary>
	/// <param name="_audioName">流すBGMの名前</param>
	/// <param name="_volume"></param>
	/// <param name="_isLoop"></param>
	/// <param name="_fadeTime"></param>
	/// <param name="_crossFadeRate"></param>
	public void PlayCrossFadeBGM(string _audioName, float _fadeTime, float _crossFadeRate, float _volume = DefaultVolume, bool _isLoop = true)
	{
		PlayBGM(_audioName, bgmVolume * totalVolume * _volume, _isLoop, _fadeTime, _fadeTime, _crossFadeRate, false, 0.0f, 0.0f);
	}

	public void PlayCrossFadeBGM(string _audioName, float _fadeInTime, float _fadeOutTime, float _crossFadeRate, float _volume = DefaultVolume, bool _isLoop = true)
	{
		PlayBGM(_audioName, bgmVolume * totalVolume * _volume, _isLoop, _fadeInTime, _fadeOutTime, _crossFadeRate, false, 0.0f, 0.0f);
	}

	private void PlayBGM(string _audioName, float _volume, bool _isLoop, float _fadeInTime, float _fadeOutTime, float _crossFadeRate, bool _isCheckLoopPoint, float _loopStartTime = 0.0f, float _loopEndTime = 0.0f)
	{
		if (!bgmDictionary.ContainsKey(_audioName))
		{
			Debug.Log("その名前のBGMは見つかりませんでした。");
			return;
		}

		_volume = Mathf.Clamp01(_volume);
		_crossFadeRate = 1.0f - Mathf.Clamp01(_crossFadeRate);

		var clipInfo = bgmDictionary[_audioName];
		var player = GetBgmPlayer();
		var isFade = (_fadeInTime != 0.0f || _fadeOutTime != 0.0f);

		if (isFade)
		{
			player.FadeIn(_fadeInTime, (_crossFadeRate * _fadeInTime));
			for (int i = 0; i < bgmPlayerList.Count; i++)
			{
				if (bgmPlayerList[i].IsPlaying)
					bgmPlayerList[i].FadeOut(_fadeOutTime);
			}
		}
		else
		{
			StopBGM();
		}

		player.Play(clipInfo.clip, _isLoop, isFade, _isCheckLoopPoint, _volume, _loopStartTime, _loopEndTime);
	}

	public void PauseBGM()
	{
		for (int i = 0; i < bgmPlayerList.Count; i++)
		{
			bgmPlayerList[i].Pause();
		}
	}

	public bool IsPlayingBGM()
	{
		for (int i = 0; i < bgmPlayerList.Count; i++)
		{
			if (bgmPlayerList[i].IsPlaying)
				return true;
		}
		return false;
	}

	public void PlaySE2D(AudioNameSE _audioName, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, false, 1, _fadeInTime, _fadeOutTime, false, Vector3.zero, null, DefaultMinDistance, DefaultMaxDistance, _onStart, _onComplete);
	}
	public void PlaySE2D(string _audioName, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, false, 1, _fadeInTime, _fadeOutTime, false, Vector3.zero, null, DefaultMinDistance, DefaultMaxDistance, _onStart, _onComplete);
	}


	public void PlaySE2DLoop(AudioNameSE _audioName, int _loopCount, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, false, _loopCount, _fadeInTime, _fadeOutTime, false, Vector3.zero, null, DefaultMinDistance, DefaultMaxDistance, _onStart, _onComplete);
	}
	public void PlaySE2DLoop(string _audioName, int _loopCount, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, false, _loopCount, _fadeInTime, _fadeOutTime, false, Vector3.zero, null, DefaultMinDistance, DefaultMaxDistance, _onStart, _onComplete);
	}


	public void PlaySE2DLoopInfinity(AudioNameSE _audioName, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), seVolume, _delay, _pitch, true, 1, _fadeInTime, _fadeOutTime, false, Vector3.zero, null, DefaultMinDistance, DefaultMaxDistance, _onStart, _onComplete);
	}
	public void PlaySE2DLoopInfinity(string _audioName, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, seVolume, _delay, _pitch, true, 1,_fadeInTime, _fadeOutTime, false, Vector3.zero, null, DefaultMinDistance, DefaultMaxDistance, _onStart, _onComplete);
	}


	public void PlaySE3D(AudioNameSE _audioName, Vector3 _soundPos, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, false, 1, _fadeInTime, _fadeOutTime, true, _soundPos, null, DefaultMinDistance, DefaultMaxDistance, _onStart, _onComplete);
	}
	public void PlaySE3D(AudioNameSE _audioName, Vector3 _soundPos, float _minDistance, float _maxDistance, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, false, 1, _fadeInTime, _fadeOutTime, true, _soundPos, null, _minDistance, _maxDistance, null, null);
	}
	public void PlaySE3D(AudioNameSE _audioName, GameObject _chaseObj, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, false, 1, _fadeInTime, _fadeOutTime, true, _chaseObj.transform.position, _chaseObj, DefaultMinDistance, DefaultMaxDistance, null, null);
	}
	public void PlaySE3D(string _audioName, Vector3 _soundPos, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, false, 1, _fadeInTime, _fadeOutTime, true, _soundPos, null, DefaultMinDistance, DefaultMaxDistance, null, null);
	}
	public void PlaySE3D(string _audioName, Vector3 _soundPos, float _minDistance, float _maxDistance, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, false, 1, _fadeInTime, _fadeOutTime, true, _soundPos, null, _minDistance, _maxDistance, null, null);
	}
	public void PlaySE3D(string _audioName, GameObject _chaseObj, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, false, 1, _fadeInTime, _fadeOutTime, true, _chaseObj.transform.position, _chaseObj, DefaultMinDistance, DefaultMaxDistance, null, null);
	}

	public void PlaySE3DLoop(AudioNameSE _audioName, Vector3 _soundPos, int _loopCount, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, false, _loopCount, _fadeInTime, _fadeOutTime, true, _soundPos, null, DefaultMinDistance, DefaultMaxDistance, null, null);
	}
	public void PlaySE3DLoop(AudioNameSE _audioName, Vector3 _soundPos, int _loopCount, float _minDistance, float _maxDistance, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, false, _loopCount, _fadeInTime, _fadeOutTime, true, _soundPos, null, _minDistance, _maxDistance, null, null);
	}
	public void PlaySE3DLoop(AudioNameSE _audioName, GameObject _chaseObj, int _loopCount, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, false, _loopCount, _fadeInTime, _fadeOutTime, true, _chaseObj.transform.position, _chaseObj, DefaultMinDistance, DefaultMaxDistance, null, null);
	}
	public void PlaySE3DLoop(string _audioName, Vector3 _soundPos, int _loopCount, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, false, _loopCount, _fadeInTime, _fadeOutTime, true, _soundPos, null, DefaultMinDistance, DefaultMaxDistance, null, null);
	}
	public void PlaySE3DLoop(string _audioName, Vector3 _soundPos, int _loopCount, float _minDistance, float _maxDistance, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, false, _loopCount, _fadeInTime, _fadeOutTime, true, _soundPos, null, _minDistance, _maxDistance, null, null);
	}
	public void PlaySE3DLoop(string _audioName, GameObject _chaseObj, int _loopCount, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, false, _loopCount, _fadeInTime, _fadeOutTime, true, _chaseObj.transform.position, _chaseObj, DefaultMinDistance, DefaultMaxDistance, null, null);
	}

	public void PlaySE3DLoopInfinity(AudioNameSE _audioName, Vector3 _soundPos, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, true, 1, _fadeInTime, _fadeOutTime, true, _soundPos, null, DefaultMinDistance, DefaultMaxDistance, null, null);
	}
	public void PlaySE3DLoopInfinity(AudioNameSE _audioName, Vector3 _soundPos, float _minDistance, float _maxDistance, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, true, 1, _fadeInTime, _fadeOutTime, true, _soundPos, null, _minDistance, _maxDistance, null, null);
	}
	public void PlaySE3DLoopInfinity(AudioNameSE _audioName, GameObject _chaseObj, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName.ToString(), _seVolume, _delay, _pitch, true, 1, _fadeInTime, _fadeOutTime, true, _chaseObj.transform.position, _chaseObj, DefaultMinDistance, DefaultMaxDistance, null, null);
	}
	public void PlaySE3DLoopInfinity(string _audioName, Vector3 _soundPos, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, true, 1, _fadeInTime, _fadeOutTime, true, _soundPos, null, DefaultMinDistance, DefaultMaxDistance, null, null);
	}
	public void PlaySE3DLoopInfinity(string _audioName, Vector3 _soundPos, float _minDistance, float _maxDistance, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, true, 1, _fadeInTime, _fadeOutTime, true, _soundPos, null, _minDistance, _maxDistance, null, null);
	}
	public void PlaySE3DLoopInfinity(string _audioName, GameObject _chaseObj, float _seVolume = DefaultVolume, float _delay = DefaultSeDelay, float _pitch = DefaultSePitch, float _fadeInTime = DefaultSeFadeTime, float _fadeOutTime = DefaultSeFadeTime, UnityAction _onStart = null, UnityAction _onComplete = null)
	{
		PlaySE(_audioName, _seVolume, _delay, _pitch, true, 1, _fadeInTime, _fadeOutTime, true, _chaseObj.transform.position, _chaseObj, DefaultMinDistance, DefaultMaxDistance, null, null);
	}


	private void PlaySE(string _audioName, float _volume, float _delay, float _pitch, bool _isLoopInfinity, int _loopCount, float _fadeInTime, float _fadeOutTime, bool _is3dSound, Vector3 _soundPos, GameObject _chaseObj, float _minDistance, float _maxDistance, UnityAction _onStart, UnityAction _onComplete)
	{
		if (!seDictionary.ContainsKey(_audioName))
		{
			Debug.Log("SE with that name does not exist :" + _audioName);
			return;
		}
		var clipInfo = seDictionary[_audioName];
		var spatialBlend = (_is3dSound) ? 1.0f : 0.0f;

		SoundEffectPlayer sePlayer = GetSePlayer();
		sePlayer.audioSource.clip = clipInfo.clip;
		sePlayer.Pitch = _pitch;
		sePlayer.transform.position = _soundPos;
		sePlayer.audioSource.spatialBlend = spatialBlend;
		sePlayer.chaseObj = _chaseObj;
		sePlayer.LoopCount = _loopCount;
		sePlayer.Volume = _volume;
		sePlayer.Delay = _delay;
		sePlayer.callbackOnComplete = _onComplete;
		sePlayer.callbackOnStart = _onStart;
		sePlayer.IsFade = (_fadeInTime != 0.0f || _fadeOutTime != 0.0f);
		sePlayer.IsLoopInfinity = _isLoopInfinity;

		if (sePlayer.IsFade)
		{
			_fadeInTime = Mathf.Clamp(_fadeInTime, 0.0f, clipInfo.clip.length);
			_fadeOutTime = Mathf.Clamp(_fadeOutTime, 0.0f, clipInfo.clip.length);

			Keyframe key1 = new Keyframe(0.0f, 0.0f, 0.0f, 1.0f);
			Keyframe key2 = new Keyframe(_fadeInTime, 1.0f, 0.0f, 0.0f);
			Keyframe key3 = new Keyframe(clipInfo.clip.length - _fadeOutTime, 1.0f, 0.0f, 0.0f);
			Keyframe key4 = new Keyframe(clipInfo.clip.length, 0.0f, 0.0f, 1.0f);

			AnimationCurve animCurve = new AnimationCurve(key1, key2, key3, key4);
			sePlayer.animationCurve = animCurve;
		}

		if (_is3dSound)
		{
			sePlayer.audioSource.minDistance = _minDistance;
			sePlayer.audioSource.maxDistance = _maxDistance;
		}

		sePlayer.Play();
	}


	public bool IsPauseSE(AudioNameSE _audioName)
	{
		return IsPauseSE(_audioName.ToString());
	}
	public bool IsPauseSE(string _audioName)
	{
		for (int i = 0; i < sePlayerList.Count; i++)
		{
			if (!sePlayerList[i].IsActive)
				continue;
			if (sePlayerList[i].audioSource.clip.name == _audioName && sePlayerList[i].IsPause)
				return true;
		}
		return false;
	}

	public bool IsPlayingSE(AudioNameSE _audioName)
	{
		return IsPlayingSE(_audioName.ToString());
	}
	public bool IsPlayingSE(string _audioName)
	{
		for (int i = 0; i < sePlayerList.Count; i++)
		{
			if (!sePlayerList[i].IsActive)
				continue;
			if (sePlayerList[i].IsPlaying && sePlayerList[i].audioSource.clip.name == _audioName)
				return true;
		}
		return false;
	}

	public void PauseSE(AudioNameSE _audioName)
	{
		PauseSE(_audioName.ToString());
	}
	public void PauseSE(string _audioName)
	{
		for (int i = 0; i < sePlayerList.Count; i++)
		{
			if (!sePlayerList[i].IsActive || !sePlayerList[i].IsPlaying)
				continue;
			if (sePlayerList[i].audioSource.clip.name == _audioName && !sePlayerList[i].IsPause)
				sePlayerList[i].Pause();
		}
	}

	public void Resume(AudioNameSE _audioName)
	{
		Resume(_audioName.ToString());
	}
	public void Resume(string _audioName)
	{
		for (int i = 0; i < sePlayerList.Count; i++)
		{
			if (!sePlayerList[i].IsActive)
				continue;
			if (sePlayerList[i].audioSource.clip.name == _audioName && sePlayerList[i].IsPause)
				sePlayerList[i].Resume();
		}
	}

	public void StopSE(AudioNameSE _audioName)
	{
		StopSE(_audioName.ToString());
	}
	public void StopSE(string _audioName)
	{
		for (int i = 0; i < sePlayerList.Count; i++)
		{
			if (!sePlayerList[i].IsActive)
				continue;
			if (sePlayerList[i].audioSource.clip.name == _audioName)
				sePlayerList[i].Stop();
		}
	}

	public void StopAllSE()
	{
		for (int i = 0; i < sePlayerList.Count; i++)
		{
			if (!sePlayerList[i].IsActive || !sePlayerList[i].IsPlaying)
				continue;
			sePlayerList[i].Stop();
		}
	}

	public void ChangeAllVolume()
	{
		for (int i = 0; i < sePlayerList.Count; i++)
		{
			if (!sePlayerList[i].IsActive)
				continue;
			sePlayerList[i].ChangeVolume();
		}
	}


	public void SaveVolume()
	{
		PlayerPrefs.SetFloat(SaveKeyVolumeTotal, TotalVolume);
		PlayerPrefs.SetFloat(SaveKeyVolumeBgm, BGMVolume);
		PlayerPrefs.SetFloat(SaveKeyVolumeSe, SEVolume);
	}
	public void LoadVolume()
	{
		totalVolume = PlayerPrefs.GetFloat(SaveKeyVolumeTotal, DefaultVolume);
		bgmVolume = PlayerPrefs.GetFloat(SaveKeyVolumeBgm, DefaultVolume);
		seVolume = PlayerPrefs.GetFloat(SaveKeyVolumeSe, DefaultVolume);
	}

	public AudioClip GetAudioClip(AudioNameBGM _audioName)
	{
		for (int i = 0; i < bgmAudioClipList.Count; i++)
		{
			if (bgmAudioClipList[i].clip.name == _audioName.ToString())
				return bgmAudioClipList[i].clip;
		}
		return null;
	}

	public AudioClip GetAudioClip(AudioNameSE _audioName)
	{
		for (int i = 0; i < seAudioClipList.Count; i++)
		{
			if (seAudioClipList[i].clip.name == _audioName.ToString())
				return seAudioClipList[i].clip;
		}
		return null;
	}


	private BackGroundMusicPlayer GetBgmPlayer()
	{
		for (int i = 0; i < bgmPlayerList.Count; i++)
		{
			if (bgmPlayerList[i].IsPlaying)
				continue;
			return bgmPlayerList[i];
		}
		return bgmPlayerList[0];
	}

	private SoundEffectPlayer GetSePlayer()
	{
		for (int i = 0; i < sePlayerList.Count; i++)
		{
			if (sePlayerList[i].IsPlaying)
				continue;
			return sePlayerList[i];
		}
		return sePlayerList[0];
	}
}