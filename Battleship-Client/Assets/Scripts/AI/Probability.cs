namespace BattleshipGame.AI
{
    public struct Probability
    {
        public int Cell;
        public float Value;

        public Probability(int cell, float value)
        {
            Value = value;
            Cell = cell;
        }
    }
}