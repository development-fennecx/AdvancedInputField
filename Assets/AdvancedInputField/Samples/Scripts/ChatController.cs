//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using AdvancedInputFieldPlugin;
using System.Collections;
using UnityEngine;

namespace AdvancedInputFieldSamples
{
	public class ChatController: MonoBehaviour
	{
		[SerializeField]
		private ChatView view;

		private ChatBot chatBot;

		private void Awake()
		{
			chatBot = new ChatBot();
		}

		private void Start()
		{
			view.AddMessageLeft("Hello");
			view.UpdateChatHistorySize();

			EmojiKeyboard emojiKeyboard = view.MessageInput.GetComponentInChildren<EmojiKeyboard>();

#if(UNITY_ANDROID || UNITY_IOS)
			if(!Application.isEditor || Settings.SimulateMobileBehaviourInEditor)
			{
				NativeKeyboardManager.AddKeyboardHeightChangedListener(OnKeyboardHeightChanged);
			}
#endif

#if(!UNITY_EDITOR) && (UNITY_ANDROID || UNITY_IOS)
			
			emojiKeyboard.gameObject.SetActive(false);
#else
			emojiKeyboard.gameObject.SetActive(true);
#endif
		}

		public void OnMessageInputBeginEdit(BeginEditReason reason)
		{
			Debug.Log("OnMessageInputBeginEdit");
			view.UpdateOriginalMessageInputPosition();

#if(UNITY_ANDROID || UNITY_IOS)
			if(!Application.isEditor || Settings.SimulateMobileBehaviourInEditor)
			{
				OnMessageInputSizeChanged(view.MessageInput.Size); //Move to top of keyboard on mobile on begin edit
			}
#endif
		}

		public void OnMessageInputEndEdit(string result, EndEditReason reason)
		{
			Debug.Log("OnMessageInputEndEdit");
			view.RestoreOriginalMessageInputPosition();
		}

		public void OnMessageInputSizeChanged(Vector2 size)
		{
			Debug.Log("OnMessageInputSizeChanged: " + size);
			view.UpdateMessageInputPosition();
			view.UpdateChatHistorySize();
		}

		public void OnMessageSendClick()
		{
			Debug.Log("OnMessageSendClick");
			string message = view.MessageInput.RichText;
			if(!string.IsNullOrEmpty(message))
			{
				view.AddMessageRight(message);
				view.MessageInput.Clear();
				StartCoroutine(ResponseRoutine());
			}
		}

		private IEnumerator ResponseRoutine()
		{
			yield return new WaitForSeconds(Random.Range(1, 5));

			string response = chatBot.GetResponse();
			view.AddMessageLeft(response);
		}

		public void OnKeyboardHeightChanged(int keyboardHeight)
		{
			Debug.Log("OnKeyboardHeightChanged: " + keyboardHeight);
			view.UpdateKeyboardHeight(keyboardHeight);
			view.UpdateChatHistorySize();
		}
	}
}
