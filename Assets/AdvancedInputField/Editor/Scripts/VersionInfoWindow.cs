// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	[InitializeOnLoad]
	public class VersionInfoWindowLauncher
	{
		static VersionInfoWindowLauncher()
		{
			EditorApplication.update += Startup;
		}

		static void Startup()
		{
			EditorApplication.update -= Startup;

			VersionInfoWindow window = (VersionInfoWindow)EditorWindow.GetWindowWithRect(typeof(VersionInfoWindow), new Rect(0, 0, 640, 480), true, VersionInfoWindow.TITLE);
			window.Initialize();

			if(!window.DontShowOnStartup)
			{
				window.Show();
			}
			else
			{
				window.Close();
			}
		}
	}

	public class VersionInfoWindow: EditorWindow
	{
		public const string TITLE = "Advanced Input Field";
		public const string VERSION_NAME = "2.1.6";
		public const string RELEASE_NOTES_RESOURCE_PATH = "AdvancedInputField/release_notes";
		public const string CHANGELOG_RESOURCE_PATH = "AdvancedInputField/changelog";
		public const string DONT_SHOW_ON_STARTUP_KEY = "DONT_SHOW_ON_STARTUP_" + VERSION_NAME;

		private Vector2 releaseNotesScrollPosition;
		private Vector2 changelogScrollPosition;
		private bool dontShowOnStartup;
		private string releaseNotesText;
		private string changelogText;

		public bool DontShowOnStartup { get { return dontShowOnStartup; } }

		public void Initialize()
		{
			dontShowOnStartup = EditorPrefs.GetBool(DONT_SHOW_ON_STARTUP_KEY, false);
			releaseNotesText = Resources.Load<TextAsset>(RELEASE_NOTES_RESOURCE_PATH).text;
			changelogText = Resources.Load<TextAsset>(CHANGELOG_RESOURCE_PATH).text;
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Version " + VERSION_NAME, EditorStyles.boldLabel);
			EditorStyles.label.wordWrap = true;

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Release Notes: ", EditorStyles.boldLabel);
			releaseNotesScrollPosition = EditorGUILayout.BeginScrollView(releaseNotesScrollPosition, GUILayout.Width(640), GUILayout.Height(160));
			EditorGUILayout.LabelField(releaseNotesText, EditorStyles.label);
			EditorGUILayout.EndScrollView();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Changelog: ", EditorStyles.boldLabel);
			changelogScrollPosition = EditorGUILayout.BeginScrollView(changelogScrollPosition, GUILayout.Width(640), GUILayout.Height(160));
			EditorGUILayout.LabelField(changelogText, EditorStyles.label);
			EditorGUILayout.EndScrollView();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			dontShowOnStartup = EditorGUILayout.Toggle("Don't show on startup", dontShowOnStartup);
			EditorPrefs.SetBool(DONT_SHOW_ON_STARTUP_KEY, dontShowOnStartup);

			EditorGUILayout.Space();
			if(GUILayout.Button("Close"))
			{
				Close();
			}
		}
	}
}
