using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RiichiReign.GameComponent;
using UnityEngine;

namespace RiichiReign.Player
{
    public struct PlayerReaction
    {
        public PlayerInstance Player { get; private set; }
        public PlayerAction Reaction { get; private set; }

        internal PlayerReaction(PlayerInstance player, PlayerAction reaction)
        {
            this.Player = player;
            this.Reaction = reaction;
        }
    }

    public class PlayerInstance : IComparable<PlayerInstance>, IEquatable<PlayerInstance>
    {
        [JsonProperty("PlayerNetworkID")]
        public ulong PlayerNetworkID { get; private set; }

        [JsonProperty("hand")]
        public PlayerHand Hand { get; private set; }

        List<PlayerAction> _availableActions = new();

        #region Constructors

        public PlayerInstance(ulong PlayerNetworkID)
        {
            Hand = new PlayerHand();
            this.PlayerNetworkID = PlayerNetworkID;
        }

        [JsonConstructor]
        private PlayerInstance(ulong PlayerNetworkID, PlayerHand hand)
        {
            this.PlayerNetworkID = PlayerNetworkID;
            this.Hand = hand ?? new();
        }

        public PlayerInstance(PlayerInstance other)
        {
            this.Hand = other.Hand;
            this.PlayerNetworkID = other.PlayerNetworkID;
        }

        #endregion

        #region Methods

        #region Turn Logics

        #region Start Phase

        public void DrawInitialTile(Pool pool)
        {
            Tile tile = pool.DrawTile();
            Hand.AddTile(tile);
        }

        #endregion

        #region Draw Phase

        public void DrawPhase(Pool pool)
        {
            Hand.DrawTile(pool);
        }

        #endregion

        #region Action Phase

        public void CheckAvailableAction()
        {
            if (_availableActions != null || _availableActions.Count != 0)
                _availableActions.Clear();

            List<PlayerAction> availableActions = Hand.CheckAvailableAction();
            _availableActions = availableActions;
        }

        #endregion

        #endregion

        #region Hand Management

        public int HandTilesCount() => Hand.TotalTilesCount();

        public string DebugLog()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine($"Player: {PlayerNetworkID}");
            stringBuilder.AppendLine($"Tiles in Hand: {Hand.TilesInHand.Count}");
            stringBuilder.AppendLine($"Melds: {Hand.Melds.Count}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Hand:");
            stringBuilder.AppendLine(Hand.ToString());

            return stringBuilder.ToString();
        }

        #endregion

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Player {PlayerNetworkID}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PlayerInstance);
        }

        public override int GetHashCode()
        {
            return PlayerNetworkID.GetHashCode();
        }

        #endregion

        #region Implementation

        public int CompareTo(PlayerInstance other)
        {
            if (other == null)
                return 1;

            return PlayerNetworkID.CompareTo(other.PlayerNetworkID);
        }

        public bool Equals(PlayerInstance other)
        {
            if (other == null)
                return false;
            return PlayerNetworkID == other.PlayerNetworkID;
        }

        public static bool operator ==(PlayerInstance a, PlayerInstance b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.PlayerNetworkID == b.PlayerNetworkID;
        }

        public static bool operator !=(PlayerInstance a, PlayerInstance b)
        {
            return !(a == b);
        }

        #endregion
    }
}
