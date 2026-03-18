using System;
using Newtonsoft.Json;

namespace RiichiReign.MahjongEngine
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

    public class Player
    {
        [JsonProperty("hand")]
        public PlayerHand Hand { get; private set; }

        [JsonProperty("points")]
        public int Points { get; private set; }

        [JsonProperty("windValue")]
        public int WindValue { get; private set; }

        [JsonProperty("playerID")]
        public string PlayerID { get; private set; }

        #region Constructors

        public Player(string playerID)
        {
            Hand = new PlayerHand();
            this.PlayerID = playerID;
        }

        [JsonConstructor]
        public Player(PlayerHand hand, int points, int windValue, string playerID)
        {
            this.Hand = hand ?? new();
            this.Points = points;
            this.WindValue = windValue;
            this.PlayerID = playerID;
        }

        public Player(Player other)
        {
            this.Hand = other.Hand;
            this.PlayerID = other.PlayerID;
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

        public void Sethand(PlayerHand hand)
        {
            Hand = hand;
        }

        public void SetWindValue(int value)
        {
            if (value <= 0)
                throw new Exception("Wind value cannot be nagetive!");

            if (value > 4)
                throw new Exception("Wind value cannot larger than 4!");

            WindValue = value;
        }

        public void AddTile(Wall wall)
        {
            Hand.AddTile(wall.DrawTile());
        }

        public void DrawTile(Wall wall)
        {
            Hand.DrawTile(wall.DrawTile());
        }

        #region Overrides

        public override string ToString()
        {
            return $"Player {PlayerID}";
        }

        // public override bool Equals(object obj)
        // {
        //     if (obj is Player other)
        //     {
        //         return this.PlayerID == other.PlayerID;
        //     }
        //     return false;
        // }

        // public override int GetHashCode()
        // {
        //     return PlayerID != null ? PlayerID.GetHashCode() : 0;
        // }

        #endregion

        #region Implementation
        #endregion
    }
}
