// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using AdvancedInputFieldPlugin;
using UnityEngine;

namespace AdvancedInputFieldSamples
{
	public class ResultsView: MonoBehaviour
	{
		private AdvancedInputField textField;

		private void Awake()
		{
			textField = transform.Find("TextField").GetComponent<AdvancedInputField>();
		}

		public void UpdateUI(FormData formData)
		{
			textField.Text = formData.ToString();
		}
	}
}
