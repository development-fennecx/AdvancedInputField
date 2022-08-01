//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using AdvancedInputFieldPlugin;
using UnityEngine;

namespace AdvancedInputFieldSamples
{
	public class LoginController: MonoBehaviour
	{
		[SerializeField]
		private LoginView view;

		[SerializeField]
		private MessageView messageView;

		[SerializeField]
		public RegisterController registerController;

		private void OnEnable()
		{
			if(view != null)
			{
				view.gameObject.SetActive(true);
			}

			Debug.Log("Resetting autofill");
			NativeKeyboardManager.ResetAutofill();
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
				view.EnableLoginButton();
			}
			else
			{
				view.DisableLoginButton();
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

		private bool AreAllFieldsFilledIn()
		{
			if(string.IsNullOrEmpty(view.Email)) { return false; }
			else if(string.IsNullOrEmpty(view.Password)) { return false; }

			return true;
		}

		public void OnLoginClick()
		{
			NativeKeyboardManager.SaveCredentials("fennecx.com");
			messageView.ShowMessage("Logged in as: " + view.Email);
		}

		public void OnRegisterClick()
		{
			registerController.gameObject.SetActive(true);
			gameObject.SetActive(false);
		}
	}
}
