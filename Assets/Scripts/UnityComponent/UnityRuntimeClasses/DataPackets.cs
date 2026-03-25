using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;
using RiichiReign.MahjongEngine;

namespace RiichiReign.DataPackets
{
    public class PlayerDataPacket
    {
        [JsonProperty("playerID")]
        public string PlayerID;

        [JsonProperty("points")]
        public int Points;

        [JsonProperty("windValue")]
        public int WindValue;

        [JsonConstructor]
        PlayerDataPacket(string playerID, int points, int windValue)
        {
            PlayerID = playerID;
            Points = points;
            WindValue = windValue;
        }

        public PlayerDataPacket(Player player)
        {
            PlayerID = player.PlayerID;
            Points = player.Points;
            WindValue = player.WindValue;
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"ID: {PlayerID}");
            sb.AppendLine($"Points: {Points}");
            sb.AppendLine($"Wind: {WindValue}");

            return sb.ToString();
        }
    }

    public abstract class HandPacket { }

    public class PlayerHandPacket : HandPacket
    {
        [JsonProperty("tempTile")]
        public Tile TempTile { get; private set; }

        [JsonProperty("tileInHand")]
        public List<Tile> TilesInHand { get; private set; }

        [JsonProperty("melds")]
        public List<IMeldType> Melds { get; private set; }

        public PlayerHandPacket(PlayerHand hand)
        {
            TempTile = hand.TempTile;
            TilesInHand = hand.TilesInHand;
            Melds = hand.Melds;
        }

        [JsonConstructor]
        PlayerHandPacket(Tile tempTile, List<Tile> tileInHand, List<IMeldType> melds)
        {
            TempTile = tempTile;
            TilesInHand = tileInHand;
            Melds = melds;
        }
    }

    public class OpponentHandPacket : HandPacket
    {
        [JsonProperty("playerID")]
        public string PlayerID;

        [JsonProperty("tileCount")]
        public int TileCount;

        [JsonProperty("hasTempTile")]
        public bool HasTempTile;

        public OpponentHandPacket(string playerID, PlayerHand hand)
        {
            PlayerID = playerID;
            TileCount = hand.TilesInHand.Count;
            HasTempTile = hand.TempTile != null;
        }

        [JsonConstructor]
        private OpponentHandPacket(string playerID, int tileCount, bool hasTempTile)
        {
            PlayerID = playerID;
            TileCount = tileCount;
            HasTempTile = hasTempTile;
        }
    }
}
