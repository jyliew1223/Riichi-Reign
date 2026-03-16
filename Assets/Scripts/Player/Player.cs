using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using NUnit.Framework.Constraints;
using RiichiReign.GameComponent;
using UnityEngine.TextCore;

namespace RiichiReign.GamePlayer
{
    public struct PlayerReaction
    {
        public Player Player { get; private set; }
        public PlayerAction Reaction { get; private set; }

        internal PlayerReaction(Player player, PlayerAction reaction)
        {
            this.Player = player;
            this.Reaction = reaction;
        }
    }

    public class Player : IComparable<Player>, IEquatable<Player>
    {
        [JsonProperty("PlayerNetworkID")]
        public ulong PlayerNetworkID { get; private set; }

        [JsonProperty("hand")]
        public PlayerHand Hand { get; private set; }

        public int Points { get; private set; }
        public int WindValue { get; private set; }

        #region Constructors

        public Player()
        {
            Hand = new PlayerHand();
        }

        public Player(ulong PlayerNetworkID)
        {
            Hand = new PlayerHand();
            this.PlayerNetworkID = PlayerNetworkID;
        }

        [JsonConstructor]
        private Player(ulong PlayerNetworkID, PlayerHand hand)
        {
            this.PlayerNetworkID = PlayerNetworkID;
            this.Hand = hand ?? new();
        }

        public Player(Player other)
        {
            this.Hand = other.Hand;
            this.PlayerNetworkID = other.PlayerNetworkID;
        }

        #endregion

        public void SetPoint(int points)
        {
            Points = points;
        }

        public void ResetHand()
        {
            Hand = new();
        }

        public void SetWindValue(int value)
        {
            if (value <= 0)
                throw new Exception("Wind value cannot be nagetive!");

            if (value > 4)
                throw new Exception("Wind value cannot larger than 4!");

            WindValue = value;
        }

        public void DrawTile(Wall wall)
        {
            Hand.AddTile(wall.DrawTile());
        }

        #region Overrides

        public override string ToString()
        {
            return $"Player {PlayerNetworkID}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Player);
        }

        public override int GetHashCode()
        {
            return PlayerNetworkID.GetHashCode();
        }

        #endregion

        #region Implementation

        public int CompareTo(Player other)
        {
            if (other == null)
                return 1;

            return PlayerNetworkID.CompareTo(other.PlayerNetworkID);
        }

        public bool Equals(Player other)
        {
            if (other == null)
                return false;
            return PlayerNetworkID == other.PlayerNetworkID;
        }

        public static bool operator ==(Player a, Player b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.PlayerNetworkID == b.PlayerNetworkID;
        }

        public static bool operator !=(Player a, Player b)
        {
            return !(a == b);
        }

        #endregion
    }
}
