// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;

namespace AdvancedInputFieldSamples
{
	public class ResultsController: MonoBehaviour
	{
		[SerializeField]
		public FormController formControl;

		[SerializeField]
		private ResultsView view;

		public FormData FormData { get; set; }

		private void OnEnable()
		{
			if(view != null)
			{
				view.gameObject.SetActive(true);
				view.UpdateUI(FormData);
			}
		}

		private void OnDisable()
		{
			if(view != null)
			{
				view.gameObject.SetActive(false);
			}
		}

		public void OnBackClick()
		{
			formControl.gameObject.SetActive(true);
			gameObject.SetActive(false);
		}
	}
}
