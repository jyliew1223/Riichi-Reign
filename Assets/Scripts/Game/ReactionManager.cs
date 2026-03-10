using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using RiichiReign.Player;
using Unity.Multiplayer.PlayMode;
using UnityEditor.Networking.PlayerConnection;

namespace RiichiReign.UnityComponent
{
    internal class ReactionManager : MonoBehaviour
    {
        public static ReactionManager Instance { get; private set; }

        #region Unity Logics

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);

            }
            else
            {
                Instance = this;
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Public Methods

        public IEnumerator WaitForPlayerActionCoroutine(TurnPhase currentPhase, System.Action<List<PlayerReaction>> callback)
        {
            switch (currentPhase)
            {
                case TurnPhase.StartTurn:
                    List<PlayerReaction> kyushukyuhaiResult = new();

                    // Kyushukyuhai check
                    foreach (var player in PlayerManager.Instance.PlayerList)
                    {
                        StartCoroutine(player.KyushukyuhaiAbortCoroutine(
                            (result) => kyushukyuhaiResult.Add(result)));
                    }

                    while (kyushukyuhaiResult.Count < PlayerManager.Instance.PlayerList.Count)
                    {
                        yield return null;
                    }

                    callback?.Invoke(kyushukyuhaiResult);
                    break;
            }
        }

        #endregion
    }
}