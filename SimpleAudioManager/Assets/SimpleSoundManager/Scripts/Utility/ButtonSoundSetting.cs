﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightGive
{
	[RequireComponent(typeof(Button))]
	public class ButtonSoundSetting : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField, Range(0.0f, 1.0f)]
		private float volume;
		[SerializeField]
		private AudioNameSE m_EnterAudioName;
		[SerializeField]
		private AudioNameSE m_ExitAudioName;
		[SerializeField]
		private AudioNameSE m_ClickAudioName;

		private UnityEvent eventPointerEnter = new UnityEvent();
		private UnityEvent eventPointerExit = new UnityEvent();

		void Awake()
		{
			var b = this.gameObject.GetComponent<Button>();
			if (!b) { return; }

			if (m_EnterAudioName != AudioNameSE.None)
				eventPointerEnter.AddListener(() => SimpleSoundManager.Instance.PlaySound2D(m_EnterAudioName));
			if (m_ExitAudioName != AudioNameSE.None)
				eventPointerExit.AddListener(() => SimpleSoundManager.Instance.PlaySound2D(m_ExitAudioName));
			if (m_ClickAudioName != AudioNameSE.None)
				b.onClick.AddListener(() => SimpleSoundManager.Instance.PlaySound2D(m_ClickAudioName));
		}

		public void OnPointerEnter(PointerEventData ped)
		{
			if (eventPointerEnter != null)
				eventPointerEnter.Invoke();
		}
		public void OnPointerExit(PointerEventData ped)
		{
			if (eventPointerExit != null)
				eventPointerExit.Invoke();
		}
	}
}
