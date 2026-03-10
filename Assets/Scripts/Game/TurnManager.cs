using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RiichiReign.GameComponent;
using RiichiReign.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    internal enum TurnPhase
    {
        None,
        StartTurn,
        DrawPhase,
        ActionPhase,
        EndTurn,
    }

    internal class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        Pool pool;
        TurnPhase currentPhase = TurnPhase.None;
        int currentPlayerIndex = 0;
        int turnCount = 0;

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

        #region Turn Logic

        #region Turn Loop Logic

        public PlayerInstance StartTurn(List<PlayerInstance> players)
        {
            StartCoroutine(TurnLoopCoroutine());
            return null;
        }

        IEnumerator TurnLoopCoroutine()
        {
            currentPhase = TurnPhase.StartTurn;

            yield return StartCoroutine(
                StartTurnCoroutine(
                    (callback) =>
                    {
                        ResloveReactionsCallback(callback);
                    }
                )
            );

            while (currentPhase != TurnPhase.EndTurn)
            {
                turnCount++;

                if (turnCount > 100) // Prevent endless loop
                {
                    Debug.Log($"[{GetType().Name}] Break Turn Loop due to endless loop");
                    break;
                }

                switch (currentPhase)
                {
                    case TurnPhase.DrawPhase:
                        DrawPhase();
                        currentPhase++;
                        continue;
                    case TurnPhase.ActionPhase:
                        break;
                    default:
                        throw new UnexpectedEnumValueException<TurnPhase>(currentPhase);
                }
            }

            EndTurn();
        }

        #endregion

        #region StartTurn Logics

        IEnumerator StartTurnCoroutine(System.Action<List<PlayerReaction>> callback)
        {
            // Initialize and shuffle the pool
            InitPool();
            Debug.Log($"[{GetType().Name}] Pool initialized and shuffled.");

            // Simulate drawing tiles for a player
            DealInitialTiiles(pool);
            Debug.Log($"[{GetType().Name}] Turn started, players have drawn their initial tiles.");

            PlayerManager.Instance.StartSyncPlayerDataRPC();

            // Kyushukyuhai check
            yield return StartCoroutine(
                ReactionManager.Instance.WaitForPlayerActionCoroutine(
                    currentPhase,
                    (reactions) => callback?.Invoke(reactions)
                )
            );

            Debug.Log($"[{GetType().Name}] Kyushukyuhai checks done");

            callback?.Invoke(null);
        }

        public void DealInitialTiiles(Pool pool)
        {
            for (int i = 0; i < 13; i++)
            {
                foreach (PlayerInstance player in PlayerManager.Instance.PlayerList)
                {
                    if (player.HandTilesCount() == 13)
                    {
                        continue;
                    }
                    player.DrawInitialTile(pool);
                }
            }
        }

        void InitPool()
        {
            pool = new();
            pool.InitializePool();
            pool.Shuffle();
        }

        #endregion

        #region DrawPhase Logic

        void DrawPhase()
        {
            PlayerManager.Instance.PlayerList[currentPlayerIndex].DrawPhase(pool);
        }

        #endregion

        #region EndTurn Logics

        void EndTurn()
        {
            currentPhase = TurnPhase.None;
            Debug.Log($"[{GetType().Name}] Turn ended.");
        }

        #endregion

        #endregion

        #region Reslove Action Logics

        void ResloveReactionsCallback(List<PlayerReaction> reactions)
        {
            if (reactions == null || reactions.Count == 0)
            {
                Debug.Log($"[{GetType().Name}] No reactions, proceeding to the next phase.");
                currentPhase++;
                return;
            }

            if (reactions.Any(r => r.Reaction == PlayerAction.Kyushukyuhai))
            {
                Debug.Log($"[{GetType().Name}] Kyushukyuhai declared, ending the round.");
                currentPhase = TurnPhase.EndTurn;
                return;
            }
        }

        #endregion

        #region Display

        void CheckPlayersHand()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine("Player's hand:\n");

            foreach (var player in PlayerManager.Instance.PlayerList)
            {
                stringBuilder.AppendLine(player.ToString());
            }

            Debug.Log($"[{GetType().Name}] " + stringBuilder.ToString());
        }

        #endregion
    }
}
