using System.Collections;
using System.Text;
using RiichiReign.GameComponent;
using RiichiReign.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public enum TurnPhase
    {
        None,
        StartRound,
        DrawPhase,
        ActionPhase,
        ChangePlayerPhase,
        EndTurn,
        EndRound,
    }

    internal class RoundManager : MonoBehaviour
    {
        public static RoundManager Instance { get; private set; }

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
                Destroy(gameObject);
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

        public PlayerInstance StartRound()
        {
            StartCoroutine(TurnLoopRoutine());
            return null;
        }

        IEnumerator TurnLoopRoutine()
        {
            _currentPhase = TurnPhase.StartRound;

            StartRoundRoutine();
            _currentPhase++;

            while (_currentPhase != TurnPhase.EndRound)
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
                        _currentPhase++;
                        break;
                    case TurnPhase.ActionPhase:
                        yield return StartCoroutine(
                            ActionPhaseRoutine(
                                _currentPlayer,
                                (playerResponse) =>
                                {
                                    _currentPhase = ResolvePlayerResponse(
                                        _currentPhase,
                                        playerResponse
                                    );
                                }
                            )
                        );
                        break;
                    case TurnPhase.EndTurn:
                        _currentPlayerIndex++;
                        _currentPlayerIndex %= _playerCount;
                        _currentPhase = TurnPhase.DrawPhase;
                        break;
                    default:
                        throw new UnexpectedEnumValueException<TurnPhase>(_currentPhase);
                }
            }

            EndRound();
        }

        #endregion

        #region StartRound Logics

        void StartRoundRoutine()
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

        IEnumerator ActionPhaseRoutine(PlayerInstance player, System.Action<PlayerAction> callback)
        {
            ReactionManager.Server.PromptPlayerInput(player);

            while (ReactionManager.Server.IsWaitingResponse.Value)
            {
                Debug.Log("RoundManager Coroutine");
                yield return null;
            }

            callback?.Invoke(ReactionManager.Server.ReturnedResponse);
            ReactionManager.Server.ResetResponse();
        }

        #endregion

        #region EndRound Logics

        void EndRound()
        {
            _currentPhase = TurnPhase.None;
            Debug.Log($"[{GetType().Name}] Turn ended.");
        }

        #endregion

        #endregion

        #region Reslove Action Logics

        private TurnPhase ResolvePlayerResponse(TurnPhase phase, PlayerAction response)
        {
            StringBuilder sb = new();

            sb.AppendLine($"[{GetType().Name}] {_currentPlayer} Response: ");
            sb.AppendLine();
            sb.AppendLine(response.ToString());

            Debug.Log(sb.ToString());

            switch (phase)
            {
                case TurnPhase.ActionPhase:
                    return TurnPhase.EndTurn;
                default:
                    break;
            }

            Debug.LogWarning("Flow to endround");
            return TurnPhase.EndRound;
        }

        #endregion
    }
}
