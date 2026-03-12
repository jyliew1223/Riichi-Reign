namespace RiichiReign.GameComponent
{
    [System.Serializable]
    public abstract class IMeldType { }

    [System.Serializable]
    class Pon : IMeldType
    {
        public Tile Tile { get; private set; }

        public Pon(Tile tile)
        {
            Tile = tile;
        }
    }

    [System.Serializable]
    class Chi : IMeldType
    {
        public Tile Tile1 { get; private set; }
        public Tile Tile2 { get; private set; }
        public Tile Tile3 { get; private set; }

        public Chi(Tile tile1, Tile tile2, Tile tile3)
        {
            Tile1 = tile1;
            Tile2 = tile2;
            Tile3 = tile3;
        }
    }

    [System.Serializable]
    class Kan : IMeldType
    {
        public Tile Tile { get; protected set; }
    }

    [System.Serializable]
    class CloseKan : Kan
    {
        public CloseKan(Tile tile)
        {
            this.Tile = tile;
        }
    }

    [System.Serializable]
    class OpenKan : Kan
    {
        public OpenKan(Tile tile)
        {
            this.Tile = tile;
        }
    }

    [System.Serializable]
    class AddKan : Kan
    {
        public AddKan(Tile tile)
        {
            this.Tile = tile;
        }
    }
}
