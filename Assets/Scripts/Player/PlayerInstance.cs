using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using NUnit.Framework;
using RiichiReign.GameComponent;
using RiichiReign.UnityComponent;
using Unity.Netcode;
using UnityEditor.PackageManager;
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
        [JsonProperty("playerID")]
        public int playerID { get; private set; }

        [JsonProperty("hand")]
        public PlayerHand Hand { get; private set; }

        private PlayerAction selectedAction = PlayerAction.None;
        private float timeoutDuration = 1f;

        #region Constructors

        public PlayerInstance()
        {
            Hand = new PlayerHand();
            playerID = UnityEngine.Random.Range(1, 10000);
            Debug.Log("Generating new PlayerInstance:" + playerID);
        }

        [JsonConstructor]
        private PlayerInstance(int playerID, PlayerHand hand)
        {
            this.playerID = playerID;
            this.Hand = hand ?? new();
        }

        public PlayerInstance(PlayerInstance other)
        {
            this.Hand = other.Hand;
            this.playerID = other.playerID;
        }

        ~PlayerInstance()
        {
            Debug.Log("Destorying Player: " + this);
        }

        #endregion

        #region Methods

        #region Turn Logics

        #region Extra Game Actions

        public void DrawInitialTile(Pool pool)
        {
            Tile tile = pool.DrawTile();
            Hand.AddTile(tile);
        }

        #endregion

        public IEnumerator KyushukyuhaiAbortCoroutine(System.Action<PlayerReaction> callback)
        {
            if (Hand.CheckKyushukyuhai() == PlayerAction.Kyushukyuhai)
            {
                selectedAction = PlayerAction.None;

                float elapsedTime = 0;
                while (elapsedTime < timeoutDuration)
                {
                    if (selectedAction != PlayerAction.None)
                    {
                        callback?.Invoke(new(this, selectedAction));
                        yield break;
                    }

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                if (elapsedTime > timeoutDuration)
                {
                    Debug.Log(
                        $"Player {playerID} did not select an action within the timeout period."
                    );
                }
            }

            callback?.Invoke(new(this, selectedAction));
        }

        public void DrawPhase(Pool pool)
        {
            Hand.DrawTile(pool);
        }

        #endregion

        #region Hand Management

        public int HandTilesCount() => Hand.TotalTilesCount();

        #endregion

        #endregion

        #region Overrides

        public override string ToString()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine($"Player: {playerID}");
            stringBuilder.AppendLine($"Tiles in Hand: {Hand.TilesInHand.Count}");
            stringBuilder.AppendLine($"Melds: {Hand.Melds.Count}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Hand:");
            stringBuilder.AppendLine(Hand.ToString());

            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PlayerInstance);
        }

        public override int GetHashCode()
        {
            return playerID.GetHashCode();
        }

        #endregion

        #region Implementation

        public int CompareTo(PlayerInstance other)
        {
            if (other == null)
                return 1;

            return playerID.CompareTo(other.playerID);
        }

        public bool Equals(PlayerInstance other)
        {
            if (other == null)
                return false;
            return playerID == other.playerID;
        }

        public static bool operator ==(PlayerInstance a, PlayerInstance b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.playerID == b.playerID;
        }

        public static bool operator !=(PlayerInstance a, PlayerInstance b)
        {
            return !(a == b);
        }

        #endregion
    }
}
