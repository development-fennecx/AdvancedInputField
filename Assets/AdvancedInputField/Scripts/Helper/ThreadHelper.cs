using System;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class that has helper methods for scheduling actions on the main thread</summary>
	public class ThreadHelper: MonoBehaviour
	{
		/// <summary>List to store actions until they can be invoked on the main thread</summary>
		private ThreadsafeQueue<Action> actionsOnMainThread;

		public static ThreadHelper Instance { get; private set; }

		public static void CreateInstance()
		{
			GameObject gameObject = new GameObject("ThreadHelper");
			Instance = gameObject.AddComponent<ThreadHelper>();
			DontDestroyOnLoad(gameObject);
		}

		#region UNITY
		private void Awake()
		{
			actionsOnMainThread = new ThreadsafeQueue<Action>();
		}

		private void Update()
		{
			ExecuteActionsOnMainThread();
		}

		private void OnDestroy()
		{
			if(actionsOnMainThread != null)
			{
				actionsOnMainThread.Clear();
			}

			Instance = null;
		}
		#endregion

		#region PUBLIC_METHODS
		/// <summary>Schedules given action on the main thread</summary>
		public static void ScheduleActionOnUnityThread(System.Action action)
		{
			if(Instance.actionsOnMainThread != null)
			{
				Instance.actionsOnMainThread.Enqueue(action);
			}
		}
		#endregion

		#region PRIVATE_METHODS
		/// <summary>Executes stored actions on the main thread</summary>
		private void ExecuteActionsOnMainThread()
		{
			if(actionsOnMainThread != null)
			{
				while(actionsOnMainThread.Count > 0)
				{
					Action savedAction = actionsOnMainThread.Dequeue();
					savedAction.Invoke();
				}
			}
		}
		#endregion
	}
}