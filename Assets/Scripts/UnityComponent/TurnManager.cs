using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RiichiReign.GameComponent;
using RiichiReign.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public enum TurnPhase
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
        TurnPhase _currentPhase = TurnPhase.None;
        int _currentPlayerIndex = 0;
        PlayerInstance _currentPlayer => PlayerManager.Server.PlayerList[_currentPlayerIndex];
        int _playerCount => PlayerManager.Server.PlayerList.Count;
        int _turnCount = 0;

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

        public PlayerInstance StartTurn()
        {
            StartCoroutine(TurnLoopCoroutine());
            return null;
        }

        IEnumerator TurnLoopCoroutine()
        {
            _currentPhase = TurnPhase.StartTurn;

            StartTurnCoroutine();
            _currentPhase++;

            while (_currentPhase != TurnPhase.EndTurn)
            {
                _turnCount++;

                if (_turnCount > 100) // Prevent endless loop
                {
                    Debug.Log($"[{GetType().Name}] Break Turn Loop due to endless loop");
                    break;
                }

                switch (_currentPhase)
                {
                    case TurnPhase.DrawPhase:
                        DrawPhase(_currentPlayer);
                        goto case TurnPhase.ActionPhase;
                    case TurnPhase.ActionPhase:
                        yield return StartCoroutine(ActionPhaseCoroutine(_currentPlayer));
                        break;
                    default:
                        throw new UnexpectedEnumValueException<TurnPhase>(_currentPhase);
                }

                _currentPlayerIndex = _currentPlayerIndex++ % _playerCount;
            }

            EndTurn();
        }

        #endregion

        #region StartTurn Logics

        void StartTurnCoroutine()
        {
            // Initialize and shuffle the pool
            InitPool();
            Debug.Log($"[{GetType().Name}] Pool initialized and shuffled.");

            // Simulate drawing tiles for a player
            DealInitialTiiles(pool);
            Debug.Log($"[{GetType().Name}] Turn started, players have drawn their initial tiles.");
        }

        public void DealInitialTiiles(Pool pool)
        {
            if (!PlayerManager.Server.IsReady)
                Debug.LogError(
                    $"[{GetType().Name}] Getting PlayerInstance while it's not Ready",
                    this
                );

            for (int i = 0; i < 13; i++)
            {
                foreach (PlayerInstance player in PlayerManager.Server.PlayerList)
                {
                    if (player.HandTilesCount() == 13)
                    {
                        continue;
                    }
                    player.DrawInitialTile(pool);
                }
            }

            PlayerManager.Server.SyncPlayerData();
        }

        void InitPool()
        {
            pool = new();
            pool.InitializePool();
            pool.Shuffle();
        }

        #endregion

        #region DrawPhase Logic

        void DrawPhase(PlayerInstance currentPlayer)
        {
            if (!PlayerManager.Server.IsReady)
                Debug.LogError(
                    $"[{GetType().Name}] Getting PlayerInstance while it's not Ready",
                    this
                );

            currentPlayer.DrawPhase(pool);

            PlayerManager.Server.SyncPlayerData();

            Debug.Log($"[{GetType().Name}] {currentPlayer} drawn tile");
        }

        #endregion

        #region ActionPhase Logic

        IEnumerator ActionPhaseCoroutine(PlayerInstance player)
        {
            player.CheckAvailableAction();
            yield return StartCoroutine(
                ReactionManager.Instance.WaitForPlayerInputCoroutine(
                    player,
                    (callback) =>
                    {
                        switch (callback.Action)
                        {
                            case GameAction.Discard:
                                Debug.Log(
                                    $"[{GetType().Name}] Player choose to dicard tile: {callback.RelatedTile.ToString()}"
                                );
                                break;
                            default:
                                break;
                        }
                    }
                )
            );
        }

        #endregion

        #region EndTurn Logics

        void EndTurn()
        {
            _currentPhase = TurnPhase.None;
            Debug.Log($"[{GetType().Name}] Turn ended.");
        }

        #endregion

        #endregion

        #region Reslove Action Logics

        #endregion
    }
}
