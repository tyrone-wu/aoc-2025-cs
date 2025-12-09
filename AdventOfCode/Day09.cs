using System.IO.Compression;

namespace AdventOfCode;

public class Day09 : BaseDay
{
    private struct Coord(int x, int y)
    {
        public int X = x;
        public int Y = y;

        public Coord(string input) : this(0, 0)
        {
            var split = input.Split(',');
            X = int.Parse(split[0]);
            Y = int.Parse(split[1]);
        }

        public readonly ulong Area(Coord other)
        {
            var width = (ulong) Math.Abs(X - other.X) + 1U;
            var height = (ulong) Math.Abs(Y - other.Y) + 1U;
            return width * height;
        }

        public override readonly string ToString()
        {
            return $"{X},{Y}";
        }
    }

    private readonly List<Coord> _redTiles;

    private IEnumerable<Coord> ParseInput()
    {
        foreach (var line in File.ReadAllLines(InputFilePath))
            yield return new Coord(line);
    }

    public Day09()
    {
        _redTiles = [.. ParseInput()];
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private ulong Part1()
    {
        var maxArea = 0UL;
        for (int i = 0; i < _redTiles.Count; ++i)
        {
            var a = _redTiles[i];
            for (int j = i + 1; j < _redTiles.Count; ++j)
                maxArea = Math.Max(maxArea, a.Area(_redTiles[j]));
        }
        return maxArea;
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private readonly struct Range(int point, int start, int end)
    {
        public readonly int Point = point;
        public readonly int Start = Math.Min(start, end);
        public readonly int End = Math.Max(start, end);
    }

    // 2023 day 10 algo, stoopid but whatever
    // run time: 2min 51s
    private ulong Part2()
    {
        var ranges = new List<Range>();
        for (int i = 1; i < _redTiles.Count; ++i)
        {
            var a = _redTiles[i - 1];
            var b = _redTiles[i];
            if (a.X == b.X)
                ranges.Add(new Range(a.X, a.Y, b.Y));
        }
        if (_redTiles[0].X == _redTiles[^1].X)
            ranges.Add(new Range(_redTiles[0].X, _redTiles[0].Y, _redTiles[^1].Y));

        var inside = new HashSet<(int, int)>();
        var notInside = new HashSet<(int, int)>();

        bool IsPerimeterInside(Coord a, Coord b)
        {
            var (xMin, xMax) = a.X < b.X ? (a.X, b.X) : (b.X, a.X);
            var (yMin, yMax) = a.Y < b.Y ? (a.Y, b.Y) : (b.Y, a.Y);

            bool IsInside(int x, int y)
            {
                if (notInside.Contains((x, y)))
                    return false;

                if (!inside.Contains((x, y)))
                {
                    if (!RayCast(ranges, x, y))
                    {
                        notInside.Add((x, y));
                        return false;
                    }
                    inside.Add((x, y));
                }
                return true;
            }

            for (var x = xMin; x <= xMax; ++x)
            {
                if (!(IsInside(x, a.Y) && IsInside(x, b.Y)))
                    return false;
            }
            for (var y = yMin; y <= yMax; ++y)
            {
                if (!(IsInside(a.X, y) && IsInside(b.X, y)))
                    return false;
            }
            return true;
        }

        var maxArea = 0UL;
        for (int i = 0; i < _redTiles.Count; ++i)
        {
            var a = _redTiles[i];
            for (int j = i + 1; j < _redTiles.Count; ++j)
            {
                var b = _redTiles[j];
                if (a.X == b.X || a.Y == b.Y)
                    continue;

                var area = a.Area(b);
                if (area > maxArea && IsPerimeterInside(a, b))
                    maxArea = area;
            }
        }
        return maxArea;
    }

    private static bool RayCast(IEnumerable<Range> ranges, int point, int rangePoint)
    {
        int crossLeftStart = 0;
        int crossRightStart = 0;
        int crossLeftEnd = 0;
        int crossRightEnd = 0;
        foreach (var range in ranges)
        {
            if (range.Point < point)
            {
                if (range.Start <= rangePoint && rangePoint < range.End)
                    crossLeftStart += 1;
                if (range.Start < rangePoint && rangePoint <= range.End)
                    crossLeftEnd += 1;
            }
            else if (range.Point > point)
            {
                if (range.Start <= rangePoint && rangePoint < range.End)
                    crossRightStart += 1;
                if (range.Start < rangePoint && rangePoint <= range.End)
                    crossRightEnd += 1;
            }
        }
        return crossLeftStart % 2 == 1 || crossRightStart % 2 == 1 || crossLeftEnd % 2 == 1 || crossRightEnd % 2 == 1;
    }
}
