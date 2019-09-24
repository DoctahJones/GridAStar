using System;
using System.Diagnostics;

namespace AStarRouting
{
    [DebuggerDisplay("({X},{Y})")]
    public struct Vector2D
    {
        public int X { get; set; }

        public int Y { get; set; }

        public Vector2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(Object obj)
        {
            return obj is Vector2D && this == (Vector2D)obj;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
        public static bool operator ==(Vector2D a, Vector2D b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(Vector2D a, Vector2D b)
        {
            return !(a == b);
        }
    }



}
