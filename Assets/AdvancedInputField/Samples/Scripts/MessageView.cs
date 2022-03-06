// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldSamples
{
	public class MessageView: MonoBehaviour
	{
		private Text messageLabel;

		private void Awake()
		{
			messageLabel = transform.Find("Box/Label").GetComponent<Text>();
		}

		public void ShowMessage(string message)
		{
			gameObject.SetActive(true);
			messageLabel.text = message;
		}

		public void OnHideMessageClick()
		{
			gameObject.SetActive(false);
		}
	}
}
