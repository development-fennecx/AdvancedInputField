// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldSamples
{
	public class MultilineController: MonoBehaviour
	{
		[SerializeField]
		private ScrollRect scrollRect;

		private void OnEnable()
		{
			scrollRect.verticalNormalizedPosition = 1;
		}
	}
}
