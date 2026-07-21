using System;

namespace VoronoiLib.Structures
{
    public class VPoint
    {
        public double X { get; }
        public double Y { get; }

        internal VPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool ApproxEqual(VPoint other)
        {
            double dist = Math.Sqrt((other.X - X) * (other.X - X) + (other.Y - Y) * (other.Y - Y));
            return dist.ApproxEqual(0);
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }
}
