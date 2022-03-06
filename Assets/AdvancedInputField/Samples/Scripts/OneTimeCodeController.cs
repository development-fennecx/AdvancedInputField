// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldSamples
{
	public class OneTimeCodeController: MonoBehaviour
	{
		[SerializeField]
		private Text instructionLabel;

		private void Awake()
		{
			instructionLabel.gameObject.SetActive(false);
		}

		public void OnRequestCodeClick()
		{
#if UNITY_ANDROID
			AdvancedInputFieldPlugin.NativeKeyboardManager.StartListeningForOneTimeCodes(); //Only needed for Android
#endif
			instructionLabel.gameObject.SetActive(true);
		}
	}
}
