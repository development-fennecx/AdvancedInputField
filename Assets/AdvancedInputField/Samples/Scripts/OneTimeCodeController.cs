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
