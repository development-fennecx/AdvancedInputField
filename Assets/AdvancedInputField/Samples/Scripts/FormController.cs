//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using AdvancedInputFieldPlugin;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldSamples
{
	public class FormController: MonoBehaviour
	{
		[SerializeField]
		public ResultsController resultsControl;

		[SerializeField]
		private FormView view;

		[SerializeField]
		private ScrollRect scrollRect;

		private void OnEnable()
		{
			if(view != null)
			{
				view.gameObject.SetActive(true);
			}
		}

		private void Start()
		{
			scrollRect.verticalNormalizedPosition = 1;

			TextAsset countriesAsset = Resources.Load<TextAsset>("countries");
			if(countriesAsset != null)
			{
				string[] countries = countriesAsset.text.Split('\n');
				view.CreateCountryDropdownOptions(countries);
			}
			else
			{
				Debug.LogWarning("Countries resources file not found");
			}
		}

		private void OnDisable()
		{
			if(view != null)
			{
				view.gameObject.SetActive(false);
			}
		}

		public void OnCountryInputChanged(string text)
		{
			view.UpdateCountryOptions(text);
		}

		public void OnCountryDropdownChanged(int index)
		{
			Debug.Log("OnCountryDropdownChanged: " + index);
			view.ApplyCountryOption(index);
		}

		public void OnInputEnd(string result, EndEditReason reason)
		{
			if(AreAllFieldsFilledIn())
			{
				view.EnableSubmitButton();
			}
			else
			{
				view.DisableSubmitButton();
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
			if(string.IsNullOrEmpty(view.Username)) { return false; }
			else if(string.IsNullOrEmpty(view.Password)) { return false; }
			else if(string.IsNullOrEmpty(view.Email)) { return false; }
			else if(string.IsNullOrEmpty(view.Telephone)) { return false; }
			else if(string.IsNullOrEmpty(view.FirstName)) { return false; }
			else if(string.IsNullOrEmpty(view.LastName)) { return false; }
			else if(string.IsNullOrEmpty(view.Country)) { return false; }
			else if(string.IsNullOrEmpty(view.City)) { return false; }
			else if(string.IsNullOrEmpty(view.YearlyIncome)) { return false; }
			else if(string.IsNullOrEmpty(view.HourlyWage)) { return false; }

			return true;
		}

		public void OnSubmitClick()
		{
			FormData formData = new FormData();
			formData.username = view.Username;
			formData.password = view.Password;
			formData.email = view.Email;
			formData.telephone = view.Telephone;
			formData.firstName = view.FirstName;
			formData.lastName = view.LastName;
			formData.country = view.Country;
			formData.city = view.City;
			formData.yearlyIncome = int.Parse(view.YearlyIncome);
			formData.hourlyWage = double.Parse(view.HourlyWage);
			formData.comments = view.Comments;

			resultsControl.FormData = formData;
			resultsControl.gameObject.SetActive(true);
			gameObject.SetActive(false);
		}
	}
}
