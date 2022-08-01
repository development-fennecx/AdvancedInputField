//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using AdvancedInputFieldPlugin;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldSamples
{
	public class FormView: MonoBehaviour
	{
		private AdvancedInputField usernameInput;
		private AdvancedInputField passwordInput;
		private AdvancedInputField emailInput;
		private AdvancedInputField telephoneInput;
		private AdvancedInputField firstNameInput;
		private AdvancedInputField lastNameInput;
		private AdvancedInputField countryInput;
		private InputFieldDropdown countryDropdown;
		private List<InputFieldDropdown.OptionData> countryDropdownOptions;
		private AdvancedInputField cityInput;
		private AdvancedInputField yearlyIncomeInput;
		private AdvancedInputField hourlyWageInput;
		private AdvancedInputField commentsInput;
		private Button submitButton;

		public string Username { get { return usernameInput.Text; } }
		public string Password { get { return passwordInput.Text; } }
		public string Email { get { return emailInput.Text; } }
		public string Telephone { get { return telephoneInput.Text; } }
		public string FirstName { get { return firstNameInput.Text; } }
		public string LastName { get { return lastNameInput.Text; } }
		public string Country { get { return countryInput.Text; } }
		public string City { get { return cityInput.Text; } }
		public string YearlyIncome { get { return yearlyIncomeInput.Text; } }
		public string HourlyWage { get { return hourlyWageInput.Text; } }
		public string Comments { get { return commentsInput.Text; } }

		private void Awake()
		{
			ScrollRect scrollRect = transform.Find("ScrollView").GetComponent<ScrollRect>();
			Transform centerGroup = scrollRect.content.Find("CenterGroup");
			usernameInput = centerGroup.Find("Username").GetComponentInChildren<AdvancedInputField>();
			passwordInput = centerGroup.Find("Password").GetComponentInChildren<AdvancedInputField>();
			emailInput = centerGroup.Find("Email").GetComponentInChildren<AdvancedInputField>();
			telephoneInput = centerGroup.Find("Telephone").GetComponentInChildren<AdvancedInputField>();
			firstNameInput = centerGroup.Find("FirstName").GetComponentInChildren<AdvancedInputField>();
			lastNameInput = centerGroup.Find("LastName").GetComponentInChildren<AdvancedInputField>();
			countryInput = centerGroup.Find("Country").GetComponentInChildren<AdvancedInputField>();
			countryDropdown = centerGroup.Find("Country").GetComponentInChildren<InputFieldDropdown>();
			cityInput = centerGroup.Find("City").GetComponentInChildren<AdvancedInputField>();
			yearlyIncomeInput = centerGroup.Find("YearlyIncome").GetComponentInChildren<AdvancedInputField>();
			hourlyWageInput = centerGroup.Find("HourlyWage").GetComponentInChildren<AdvancedInputField>();
			commentsInput = centerGroup.Find("Comments").GetComponentInChildren<AdvancedInputField>();
			submitButton = centerGroup.Find("SubmitButton").GetComponent<Button>();

			countryDropdownOptions = new List<InputFieldDropdown.OptionData>();
		}

		public void CreateCountryDropdownOptions(string[] countries)
		{
			countryDropdownOptions.Clear();

			int length = countries.Length;
			for(int i = 0; i < length; i++)
			{
				string country = countries[i];
				if(string.IsNullOrEmpty(country)) { continue; }

				countryDropdownOptions.Add(new InputFieldDropdown.OptionData(country));
			}
		}

		private void OnEnable()
		{
			usernameInput.Clear();
			passwordInput.Clear();
			emailInput.Clear();
			telephoneInput.Clear();
			firstNameInput.Clear();
			lastNameInput.Clear();
			countryInput.Clear();
			cityInput.Clear();
			yearlyIncomeInput.Clear();
			hourlyWageInput.Clear();
			commentsInput.Clear();
			submitButton.interactable = false;
		}

		public void EnableSubmitButton()
		{
			submitButton.interactable = true;
		}

		public void DisableSubmitButton()
		{
			submitButton.interactable = false;
		}

		public void ShowPassword()
		{
			passwordInput.VisiblePassword = true;
		}

		public void HidePassword()
		{
			passwordInput.VisiblePassword = false;
		}

		public void UpdateCountryOptions(string countryText)
		{
			if(string.IsNullOrEmpty(countryText))
			{
				countryDropdown.Hide();
			}
			else
			{
				Predicate<InputFieldDropdown.OptionData> predicate = (option => option.text.IndexOf(countryText, StringComparison.InvariantCultureIgnoreCase) != -1);
				countryDropdown.options = countryDropdownOptions.FindAll(predicate);
				countryDropdown.Show();
			}
		}

		public void ApplyCountryOption(int index)
		{
			countryInput.Text = countryDropdown.options[index].text;
			countryInput.SetCaretToTextEnd();
		}
	}
}