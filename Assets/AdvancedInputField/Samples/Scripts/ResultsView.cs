//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

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
