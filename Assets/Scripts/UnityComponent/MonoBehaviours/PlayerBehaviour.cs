using RiichiReign.DataPackets;
using RiichiReign.UnityUIToolKitComponent;
using Unity.Collections;
using Unity.Netcode;

namespace RiichiReign.UnityComponent
{
    public class PlayerBehaviour : NetworkBehaviour
    {
        public static PlayerBehaviour LocalPlayerInstance { get; private set; }
        public HandPacket PlayerHand { get; private set; } = null;
        private NetworkVariable<FixedString64Bytes> _networkPlayerID = new(
            "Player",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public string PlayerID
        {
            get => _networkPlayerID.Value.ToString();
            private set => _networkPlayerID.Value = value;
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

        public void UpdatePlayerData(PlayerDataPacket packet)
        {
            if (packet.PlayerID != PlayerID)
                throw new System.Exception($"[{GetType().Name}] Player ID not match!");

            Points = packet.Points;
            WindValue = packet.WindValue;
        }

        public void UpdatePlayerHand(PlayerHandPacket packet)
        {
            if (!IsOwner)
                throw new System.Exception($"[{GetType().Name}] Instance is not own by owner!");

            PlayerHand = packet;
            PlayerUIController.Instance.UpdatePlayerHand(LocalPlayerInstance.PlayerID);
        }

        public void UpdatePlayerHand(OpponentHandPacket packet)
        {
            if (IsOwner || packet.PlayerID == LocalPlayerInstance.PlayerID)
                throw new System.Exception(
                    $"[{GetType().Name}] Updating Owner Hand with OpponentHand object!"
                );

            PlayerHand = packet;
            PlayerUIController.Instance.UpdatePlayerHand(packet.PlayerID);
        }
    }
}
