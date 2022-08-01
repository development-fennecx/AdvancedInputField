using System;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	public enum LocalizationKey { CUT, COPY, PASTE, SELECT_ALL, REPLACE }

	[Serializable]
	public class LocalizedString
	{
		public LocalizationKey key;
		public string value;
	}

	public class LocalizationData: ScriptableObject
	{
		[Tooltip("The system language")]
		[SerializeField, CustomName("System Language")]
		private SystemLanguage language;

		[Tooltip("The localized strings")]
		[SerializeField, CustomName("Localized Strings")]
		private LocalizedString[] localizedStrings;

		public SystemLanguage Language { get { return language; } }
		public LocalizedString[] LocalizedStrings { get { return localizedStrings; } }
	}
}
