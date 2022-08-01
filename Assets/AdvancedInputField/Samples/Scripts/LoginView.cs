using AdvancedInputFieldPlugin;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldSamples
{
	public class LoginView: MonoBehaviour
	{
		private ScrollRect scrollRect;
		private AdvancedInputField emailInput;
		private AdvancedInputField passwordInput;
		private Button loginButton;

		public string Email { get { return emailInput.Text; } }
		public string Password { get { return passwordInput.Text; } }

		private void Awake()
		{
			scrollRect = transform.Find("ScrollView").GetComponent<ScrollRect>();
			Transform centerGroup = scrollRect.content.Find("CenterGroup");
			emailInput = centerGroup.Find("Email").GetComponentInChildren<AdvancedInputField>();
			passwordInput = centerGroup.Find("Password").GetComponentInChildren<AdvancedInputField>();
			loginButton = centerGroup.Find("LoginButton").GetComponentInChildren<Button>();
		}

		private void OnEnable()
		{
			scrollRect.verticalNormalizedPosition = 1;
			emailInput.Clear();
			passwordInput.Clear();
			loginButton.interactable = false;
		}

		public void EnableLoginButton()
		{
			loginButton.interactable = true;
		}

		public void DisableLoginButton()
		{
			loginButton.interactable = false;
		}

		public void ShowPassword()
		{
			passwordInput.VisiblePassword = true;
		}

		public void HidePassword()
		{
			passwordInput.VisiblePassword = false;
		}
	}
}
