//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using AdvancedInputFieldPlugin;
using UnityEngine;

namespace AdvancedInputFieldSamples
{
	public class RegisterController: MonoBehaviour
	{
		[SerializeField]
		private RegisterView view;

		[SerializeField]
		private MessageView messageView;

		[SerializeField]
		public LoginController loginController;

		private void OnEnable()
		{
			if(view != null)
			{
				view.gameObject.SetActive(true);
			}
		}

		private void OnDisable()
		{
			if(view != null)
			{
				view.gameObject.SetActive(false);
			}
		}

		public void OnInputEnd(string result, EndEditReason reason)
		{
			if(AreAllFieldsFilledIn())
			{
				view.EnableRegisterButton();
			}
			else
			{
				view.DisableRegisterButton();
			}
		}

		public void OnPasswordVisibilityToggle(bool isON)
		{
			if(isON)
			{
				view.ShowPassword();
			}
			else
			{
				view.HidePassword();
			}
		}

		public void OnConfirmPasswordVisibilityToggle(bool isON)
		{
			if(isON)
			{
				view.ShowConfirmPassword();
			}
			else
			{
				view.HideConfirmPassword();
			}
		}

		private bool AreAllFieldsFilledIn()
		{
			if(string.IsNullOrEmpty(view.Email)) { return false; }
			else if(string.IsNullOrEmpty(view.Password)) { return false; }

			return true;
		}

		public void OnRegisterClick()
		{
			if(view.ConfirmPassword != view.Password)
			{
				messageView.ShowMessage("Passwords don't match");
			}
			else
			{
				NativeKeyboardManager.SaveCredentials("fennecx.com");
				loginController.gameObject.SetActive(true);
				messageView.ShowMessage("Created account for: " + view.Email);
				gameObject.SetActive(false);
			}
		}

		public void OnBackClick()
		{
			loginController.gameObject.SetActive(true);
			gameObject.SetActive(false);
		}
	}
}
