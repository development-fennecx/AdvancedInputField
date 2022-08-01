
using UnityEngine;

namespace AdvancedInputFieldSamples
{
	public class ChatBot
	{
		public string[] RESPONSES = new string[]
		{
			"OK", "Alright", "Interesting", "I might be a bot", "Tell me more", "Awesome", "Good job", "I don't know",
			"I'm confused", "How are you doing?", "Random static noise", "Beep Beep", "What are you doing right now?",
			"Can you tell me a nice story?", "It's freezing here", "I'm going to the moon", "Whatever", "Hello and bye bye",
			"That's strange", "Where are you?", "Uhm....", "That might be another issue", "Nope", "Not my problem",
			"Is this a sample scene?", "Where am I?", "Who am I?", "What should I do?", "Nevermind", "No problem"
		};

		public string GetResponse()
		{
			int length = RESPONSES.Length;
			int randomIndex = Random.Range(0, length);

			return RESPONSES[randomIndex];
		}
	}
}
