using System.Collections;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using RiichiReign.Player;
using RiichiReign.UnityUIToolKitComponent;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    internal class ReactionManager : NetworkBehaviour
    {
        [SerializeField]
        private float _reactionTimeOut = 20f;

        [HideInInspector]
        public bool IsReady { get; private set; } = false;

        [HideInInspector]
        public PlayerAction ReturnedResponse { get; private set; }
        public NetworkVariable<bool> IsWaitingResponse = new(false);

        private static ReactionManager _instance;
        private PlayerUIController _localPlayerUI;

        #region Netcode Logics

        public override void OnNetworkSpawn()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            IsReady = true;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"[{GetType().Name}] Network spawned:");
            stringBuilder.AppendLine("IsServer: " + IsServer);
            stringBuilder.AppendLine("IsLocalPlayer: " + IsLocalPlayer);
            stringBuilder.AppendLine("IsHost: " + IsHost);
            stringBuilder.AppendLine("IsClient: " + IsClient);

            Debug.Log(stringBuilder.ToString());
        }

        public override void OnNetworkDespawn()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public static ReactionManager Server =>
            _instance.IsServer
                ? _instance
                : throw new System.Exception(
                    $"[{_instance.GetType().Name}] Local instance is not server instance"
                );
        public static ReactionManager Local =>
            _instance.IsClient
                ? _instance
                : throw new System.Exception(
                    $"[{_instance.GetType().Name}] Local instance is not client instance"
                );

        #endregion

        #region Public Methods

        public void RegisterPlayerUI(PlayerUIController playerUI)
        {
            _localPlayerUI = playerUI;
            Debug.Log($"[{GetType().Name}] _localPlayerUI assigned", _localPlayerUI.gameObject);
        }

        public void PromptPlayerInput(PlayerInstance player)
        {
            if (!IsServer)
                throw new System.Exception(
                    $"[{GetType().Name}] Calling server method in client machine"
                );

            ReturnedResponse = null;

            string json = JsonConvert.SerializeObject(player);

            IsWaitingResponse.Value = true;
            StartPromptPlayerInputClientRPC(json);
        }

        public void ResetResponse()
        {
            ReturnedResponse = null;
        }

        private IEnumerator StartCountDownRoutine(string json)
        {
            float timePassed = 0f;

            while (timePassed < _reactionTimeOut)
            {
                if (!IsWaitingResponse.Value)
                    yield break;

                timePassed += Time.deltaTime;
                yield return null;
            }

            StartNoResponseServerRPC(json);
        }

        #endregion

        #region RPCs

        [Rpc(SendTo.ClientsAndHost)]
        private void StartPromptPlayerInputClientRPC(string json)
        {
            PlayerInstance player = JsonConvert.DeserializeObject<PlayerInstance>(json);

            if (player.PlayerNetworkID != _localPlayerUI.LocalNetworkObjectID)
            {
                Debug.Log($"[{GetType().Name}] Local player not authorize to response");
                return;
            }

            StartCoroutine(StartCountDownRoutine(json));
            StartCoroutine(
                _localPlayerUI.PrompForPlayerInputRoutine(
                    (response) =>
                    {
                        if (response == null)
                            return;

                        string json = JsonConvert.SerializeObject(response);
                        StartResponseToServerRPC(json);
                    }
                )
            );

            Debug.Log($"[{GetType().Name}] Waiting {player} response");
        }

        [Rpc(SendTo.Server)]
        private void StartResponseToServerRPC(string json)
        {
            Debug.Log($"[{GetType().Name}] Response Received: {json.Prettify()}");
            ReturnedResponse = JsonConvert.DeserializeObject<PlayerAction>(json);

            IsWaitingResponse.Value = false;
        }

        [Rpc(SendTo.Server)]
        private void StartNoResponseServerRPC(string json)
        {
            PlayerInstance player = JsonConvert.DeserializeObject<PlayerInstance>(json);

            if (ReturnedResponse == null)
            {
                Debug.Log($"[{GetType().Name}] No response received, Treated as Skip");
                ReturnedResponse = new(GameAction.Discard, player.Hand.TempTile);
            }

            IsWaitingResponse.Value = false;
        }

        #endregion
    }
}
