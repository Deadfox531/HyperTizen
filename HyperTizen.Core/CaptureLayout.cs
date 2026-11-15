using System;

namespace HyperTizen.Core
{
    public enum Edge
    {
        Top,
        Right,
        Bottom,
        Left
    }

    // A place on the screen whose color is measured. X and Y run from 0 to 1.
    public struct CapturePoint
    {
        public double X;
        public double Y;
        public Edge Edge;

        public CapturePoint(double x, double y, Edge edge)
        {
            X = x;
            Y = y;
            Edge = edge;
        }
    }

    // How many zones each edge of the screen has, and where their capture points are.
    public sealed class CaptureLayout
    {
        public const int MaxTopBottom = 16;
        public const int MaxLeftRight = 12;

        // How far in from the edge of the screen the points sit.
        public const double DefaultInset = 0.05;

        // The deepest the top and bottom points may be moved in, as a share of the screen height.
        public const double MaxVerticalInset = 0.3;

        public static readonly CaptureLayout Default = new CaptureLayout(4, 4, 3, 3);

        public CaptureLayout(int top, int bottom, int left, int right)
            : this(top, bottom, left, right, DefaultInset)
        {
        }

        // verticalInset moves the top and bottom points in from their edges, past black bars.
        public CaptureLayout(int top, int bottom, int left, int right, double verticalInset)
        {
            if (double.IsNaN(verticalInset) || verticalInset < DefaultInset || verticalInset > MaxVerticalInset)
                throw new ArgumentOutOfRangeException(
                    nameof(verticalInset), "The vertical inset runs from " + DefaultInset + " to " + MaxVerticalInset + ".");

            if (!IsValid(top, bottom, left, right))
                throw new ArgumentOutOfRangeException(
                    nameof(top), "Zones per edge: top and bottom 0 to " + MaxTopBottom + ", left and right 0 to " + MaxLeftRight + ", at least one in total.");

            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
            VerticalInset = verticalInset;

            // Top, right, bottom, left; along an edge from left to right or from top to bottom.
            var points = new CapturePoint[top + right + bottom + left];
            int next = 0;
            for (int i = 0; i < top; i++) points[next++] = new CapturePoint((i + 0.5) / top, verticalInset, Edge.Top);
            for (int i = 0; i < right; i++) points[next++] = new CapturePoint(1 - DefaultInset, (i + 0.5) / right, Edge.Right);
            for (int i = 0; i < bottom; i++) points[next++] = new CapturePoint((i + 0.5) / bottom, 1 - verticalInset, Edge.Bottom);
            for (int i = 0; i < left; i++) points[next++] = new CapturePoint(DefaultInset, (i + 0.5) / left, Edge.Left);
            Points = points;
        }

        public int Top { get; }
        public int Bottom { get; }
        public int Left { get; }
        public int Right { get; }

        // How far in from the top and bottom edges the top and bottom points sit.
        public double VerticalInset { get; }

        // The same zones with the top and bottom points moved; this layout when nothing changes.
        public CaptureLayout WithVerticalInset(double verticalInset)
        {
            return verticalInset == VerticalInset ? this : new CaptureLayout(Top, Bottom, Left, Right, verticalInset);
        }

        // One point per zone. Do not change the array.
        public CapturePoint[] Points { get; }

        public static bool IsValid(int top, int bottom, int left, int right)
        {
            return top >= 0 && top <= MaxTopBottom
                && bottom >= 0 && bottom <= MaxTopBottom
                && left >= 0 && left <= MaxLeftRight
                && right >= 0 && right <= MaxLeftRight
                && top + bottom + left + right > 0;
        }

        public int Count(Edge edge)
        {
            switch (edge)
            {
                case Edge.Top: return Top;
                case Edge.Right: return Right;
                case Edge.Bottom: return Bottom;
                default: return Left;
            }
        }

        // Where in Points an edge's first zone is.
        public int Offset(Edge edge)
        {
            switch (edge)
            {
                case Edge.Top: return 0;
                case Edge.Right: return Top;
                case Edge.Bottom: return Top + Right;
                default: return Top + Right + Bottom;
            }
        }
    }
}
