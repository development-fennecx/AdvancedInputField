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
