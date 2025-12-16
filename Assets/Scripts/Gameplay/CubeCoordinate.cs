namespace Gameplay
{
    public struct CubeCoordinate
    {
        public int Q; 
        public int R; 
        public int S; 
        public CubeCoordinate(int q, int r, int s)
        {
            this.Q = q;
            this.R = r;
            this.S = s;
        }
        public override string ToString() => $"({(Q > 0 ? "+" + Q : Q)}, {(R > 0 ? "+" + R : R)}, {(S > 0 ? "+" + S : S)})";
    }
}