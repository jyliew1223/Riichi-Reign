using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RiichiReign.GamePlayer;
using RiichiReign.UnityUIToolKitComponent;
using Unity.Netcode;
using Unity.Services.Relay.Http;
using UnityEngine;

namespace RiichiReign.UnityComponent
{
    public class PlayerManager : NetworkBehaviour
    {
        public static PlayerManager Instance;

        Player _localPlayer;
        PlayerUIController _playerUI;

        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Debug.Log($"[{GetType().Name}] Setting Singleton.....");
            Instance = this;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterPlayer(ulong networkID, PlayerUIController playerUI)
        {
            _playerUI = playerUI;
            Debug.Log($"[{GetType().Name}][Client] Sending ID {networkID} to server...");
            RegisterPlayerServerRpc(networkID);
        }

        [Rpc(SendTo.Server)]
        public void RegisterPlayerServerRpc(ulong networkID, RpcParams rpcParams = default)
        {
            Debug.Log(
                $"[{GetType().Name}][Server] Player {networkID} Received. Creating new player."
            );

            Player newPlayer = GameManager.Instance.AddPlayer(networkID);
            string json = JsonConvert.SerializeObject(newPlayer);

            StringBuilder sb = new();
            sb.AppendLine(
                $"[{GetType().Name}][Server] Player {networkID} created. Sending back to client."
            );
            sb.AppendLine();
            sb.AppendLine($"JSON: {json}");

            Debug.Log(sb.ToString());

            var targetParams = new RpcParams
            {
                Send = new RpcSendParams
                {
                    Target = RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp),
                },
            };

            ConfirmRegistrationRpc(json, targetParams);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void ConfirmRegistrationRpc(string json, RpcParams rpcParams = default)
        {
            _localPlayer = JsonConvert.DeserializeObject<Player>(json);

            if (_localPlayer != null)
            {
                StringBuilder sb = new();
                sb.AppendLine(
                    $"[{GetType().Name}][Client] Successfully registered and received Player object!"
                );
                sb.AppendLine();
                sb.AppendLine($"Player: {_localPlayer}");

                Debug.Log(sb.ToString());
            }
            else
            {
                Debug.LogWarning(
                    $"[{GetType().Name}][Client] Received JSON but _localPlayer is still null!"
                );
                ;
            }
        }
    }
}
