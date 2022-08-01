using AdvancedInputFieldPlugin;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldSamples
{
	public class RegisterView: MonoBehaviour
	{
		private ScrollRect scrollRect;
		private AdvancedInputField emailInput;
		private AdvancedInputField passwordInput;
		private AdvancedInputField confirmPasswordInput;
		private AdvancedInputField firstNameInput;
		private AdvancedInputField lastNameInput;
		private Button registerButton;

		public string Email { get { return emailInput.Text; } }
		public string Password { get { return passwordInput.Text; } }
		public string ConfirmPassword { get { return confirmPasswordInput.Text; } }
		public string FirstName { get { return firstNameInput.Text; } }
		public string LastName { get { return lastNameInput.Text; } }

		private void Awake()
		{
			scrollRect = transform.Find("ScrollView").GetComponent<ScrollRect>();
			Transform centerGroup = scrollRect.content.Find("CenterGroup");
			emailInput = centerGroup.Find("Email").GetComponentInChildren<AdvancedInputField>();
			passwordInput = centerGroup.Find("Password").GetComponentInChildren<AdvancedInputField>();
			confirmPasswordInput = centerGroup.Find("ConfirmPassword").GetComponentInChildren<AdvancedInputField>();
			firstNameInput = centerGroup.Find("FirstName").GetComponentInChildren<AdvancedInputField>();
			lastNameInput = centerGroup.Find("LastName").GetComponentInChildren<AdvancedInputField>();
			registerButton = centerGroup.Find("RegisterButton").GetComponentInChildren<Button>();
		}

		private void OnEnable()
		{
			scrollRect.verticalNormalizedPosition = 1;
			emailInput.Clear();
			passwordInput.Clear();
			confirmPasswordInput.Clear();
			firstNameInput.Clear();
			lastNameInput.Clear();
			registerButton.interactable = false;
		}

		public void EnableRegisterButton()
		{
			registerButton.interactable = true;
		}

		public void DisableRegisterButton()
		{
			registerButton.interactable = false;
		}

		public void ShowPassword()
		{
			passwordInput.VisiblePassword = true;
		}

		public void HidePassword()
		{
			passwordInput.VisiblePassword = false;
		}

		public void ShowConfirmPassword()
		{
			confirmPasswordInput.VisiblePassword = true;
		}

		public void HideConfirmPassword()
		{
			confirmPasswordInput.VisiblePassword = false;
		}
	}
}
