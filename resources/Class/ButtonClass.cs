namespace LeDosSaperos.resources.Class
{
    internal class ButtonClass
    {
        int id;
        public bool isBomb;
        bool isFlagged;
        bool isRevealed;
        int nearBombs;

        public ButtonClass(int id, bool isBomb, bool isRevealed, bool isFlagged)
        {
            this.id = id;
            this.isBomb = isBomb;
            this.isFlagged = isFlagged;
            this.isRevealed = isRevealed;
            this.nearBombs = 0; 
        }

        public int ID
        {
            get => id;
            set => id = value;
        }

        public bool IsBomb
        {
            get => isBomb;
            set => isBomb = value;
        }

        public bool IsFlagged
        {
            get => isFlagged;
            set => isFlagged = value;
        }

        public bool IsRevealed
        {
            get => isRevealed;
            set => isRevealed = value;
        }

        public int NearBombs 
        {
            get => nearBombs;
            set => nearBombs = value;
        }
    }
}
