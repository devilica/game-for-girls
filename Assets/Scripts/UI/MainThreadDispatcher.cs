using System;
using System.Collections.Generic;
using UnityEngine;

namespace DressUpGame.UI
{
    /// <summary>
    /// Runs actions on Unity's main thread (required for Time, UI, and scene APIs).
    /// </summary>
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher instance;
        private static readonly Queue<Action> PendingActions = new Queue<Action>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureExists()
        {
            if (instance != null)
            {
                return;
            }

            GameObject host = new GameObject("MainThreadDispatcher");
            instance = host.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(host);
        }

        public static void Run(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (instance == null)
            {
                EnsureExists();
            }

            lock (PendingActions)
            {
                PendingActions.Enqueue(action);
            }
        }

        private void Update()
        {
            while (true)
            {
                Action action;
                lock (PendingActions)
                {
                    if (PendingActions.Count == 0)
                    {
                        break;
                    }

                    action = PendingActions.Dequeue();
                }

                action.Invoke();
            }
        }
    }
}
