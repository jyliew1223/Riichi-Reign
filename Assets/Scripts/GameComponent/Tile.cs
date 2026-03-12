using System;
using Newtonsoft.Json;

namespace RiichiReign.GameComponent
{
    public enum TileType
    {
        Invisible,
        Manzu,
        Pinzu,
        Souzu,
        Wind,
        Dragon,
    }

    public class Tile : IComparable<Tile>, IEquatable<Tile>
    {
        [JsonProperty("type")]
        public TileType Type { get; protected set; }

        [JsonProperty("value")]
        public int Value { get; protected set; }

        [JsonProperty("isRedDora")]
        public bool IsRedDora { get; protected set; }

        #region Constructor

        public Tile()
        {
            Type = TileType.Invisible;
            Value = -1;
            IsRedDora = false;
        }

        [JsonConstructor]
        private Tile(int value, TileType type, bool isRedDora)
        {
            this.Type = type;
            this.Value = value;
            this.IsRedDora = isRedDora;
        }

        public Tile(TileType type, int value, bool isRedDora = false)
        {
            Type = type;
            Value = value;
            IsRedDora = isRedDora;
        }

        #endregion

        #region Methods

        public bool IsTerminal()
        {
            return (
                (Type == TileType.Manzu || Type == TileType.Pinzu || Type == TileType.Souzu)
                    && (Value == 1 || Value == 9)
                || Type == TileType.Dragon
                || Type == TileType.Wind
            );
        }

        public string GetTextureKey()
        {
            string key = String.Empty;

            switch (Type)
            {
                case TileType.Invisible:
                    key = "Back";
                    break;
                case TileType.Manzu:
                    key = $"Man{Value}";
                    if (IsRedDora)
                        key += "-Dora";
                    break;
                case TileType.Pinzu:
                    key = $"Pin{Value}";
                    if (IsRedDora)
                        key += "-Dora";
                    break;
                case TileType.Souzu:
                    key = $"Sou{Value}";
                    if (IsRedDora)
                        key += "-Dora";
                    break;
                case TileType.Wind:
                    switch (Value)
                    {
                        case 1:
                            key = "Ton";
                            break;
                        case 2:
                            key = "Nan";
                            break;
                        case 3:
                            key = "Shaa";
                            break;
                        case 4:
                            key = "Pei";
                            break;
                    }
                    break;
                case TileType.Dragon:
                    switch (Value)
                    {
                        case 1:
                            key = "Haku";
                            break;
                        case 2:
                            key = "Hatsu";
                            break;
                        case 3:
                            key = "Chun";
                            break;
                    }
                    break;
                default:
                    key = "blank";
                    break;
            }

            return key;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Type} {Value} {(IsRedDora ? "(Red Dora)" : "")}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals(obj as Tile);
        }

        public override int GetHashCode()
        {
            return (Type, Value).GetHashCode();
        }

        #endregion

        #region Implementations

        public int CompareTo(Tile other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (other == null)
                return 1;

            int typeComparison = Type.CompareTo(other.Type);
            if (typeComparison != 0)
                return typeComparison;

            int valueComparison = Value.CompareTo(other.Value);
            if (valueComparison != 0)
                return valueComparison;

            return IsRedDora.CompareTo(other.IsRedDora);
        }

        public bool Equals(Tile other)
        {
            if (other == null)
                return false;

            bool result = Type == other.Type && Value == other.Value;

            return result;
        }

        public static bool operator ==(Tile a, Tile b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(Tile a, Tile b)
        {
            return !(a == b);
        }
        #endregion
    }
}
