using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	[RequireComponent(typeof(ActionBar))]
	public class ActionBarLocalizer: MonoBehaviour
	{
		private LocalizationData[] localizations;

		private ActionBar actionBar;

		private void Awake()
		{
			actionBar = GetComponent<ActionBar>();
			localizations = Settings.Localizations;
		}

		private void OnEnable()
		{
			LocalizationData localization = FindLocalization();
			if(localization != null)
			{
				ApplyLocalization(localization);
			}
		}

		public LocalizationData FindLocalization()
		{
			SystemLanguage language = Application.systemLanguage;
			LocalizationData localization = null;

			int length = localizations.Length;
			for(int i = 0; i < length; i++)
			{
				if(localizations[i].Language == language)
				{
					localization = localizations[i];
					break;
				}
			}

			if(localization != null) { return localization; }

			for(int i = 0; i < length; i++)
			{
				if(localizations[i].Language == SystemLanguage.English) //Default to English
				{
					localization = localizations[i];
					break;
				}
			}

			return localization;
		}

		public void ApplyLocalization(LocalizationData localization)
		{
			int length = localization.LocalizedStrings.Length;
			for(int i = 0; i < length; i++)
			{
				LocalizedString localizedString = localization.LocalizedStrings[i];
				switch(localizedString.key)
				{
					case LocalizationKey.COPY: actionBar.CopyText = localizedString.value; break;
					case LocalizationKey.CUT: actionBar.CutText = localizedString.value; break;
					case LocalizationKey.PASTE: actionBar.PasteText = localizedString.value; break;
					case LocalizationKey.SELECT_ALL: actionBar.SelectAllText = localizedString.value; break;
					case LocalizationKey.REPLACE: actionBar.ReplaceText = localizedString.value; break;
				}
			}
		}
	}
}
