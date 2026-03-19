using RiichiReign.MahjongEngine;
using RiichiReign.UnityUIToolKitComponent;
using Unity.Collections;
using Unity.Netcode;

namespace RiichiReign.UnityComponent
{
    public class PlayerBehaviour : NetworkBehaviour
    {
        public static PlayerBehaviour LocalPlayerInstance { get; private set; }
        public Hand PlayerHand { get; private set; } = null;
        private NetworkVariable<FixedString64Bytes> _networkPlayerID = new(
            "Player",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public string PlayerID
        {
            get => _networkPlayerID.Value.ToString();
            set => _networkPlayerID.Value = value;
        }

        public int Points { get; private set; }
        public int WindValue { get; private set; }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
                return;

            LocalPlayerInstance = this;
            Invoke(nameof(Register), 1f);
        }

        public override void OnNetworkDespawn()
        {
            if (LocalPlayerInstance == this)
            {
                LocalPlayerInstance = null;
            }
        }

        private void Register()
        {
            if (!IsOwner)
                throw new System.Exception($"[{GetType().Name}] Instance is not own by owner!");

            PlayerID = System.Guid.NewGuid().ToString();
            PlayerManager.Instance.RegisterPlayer(this);
        }

        public void SetPlayerData(string playerID, int points, int windValue)
        {
            if (IsOwner)
                throw new System.Exception($"[{GetType().Name}] Cannot set local player ID!");

            PlayerID = playerID;
            Points = points;
            WindValue = windValue;
        }

        public void UpdatePlayerData(int points, int windValue)
        {
            Points = points;
            WindValue = windValue;
        }

        public void UpdatePlayerHand(PlayerHand localPlayerHand)
        {
            if (!IsOwner)
                throw new System.Exception($"[{GetType().Name}] Instance is not own by owner!");

            PlayerHand = localPlayerHand;
            PlayerUIController.Instance.UpdatePlayerHand(PlayerID);
        }

        public void UpdatePlayerHand(OpponentHand opponentHand)
        {
            if (IsOwner || opponentHand.PlayerID == LocalPlayerInstance.PlayerID)
                throw new System.Exception(
                    $"[{GetType().Name}] Updating Owner Hand with OpponentHand object!"
                );

            PlayerHand = opponentHand;
            PlayerUIController.Instance.UpdatePlayerHand(PlayerID);
        }
    }
}
