namespace AdventOfCode;

public class Day12 : BaseDay
{
    private sealed record Present
    {
        public readonly byte Id;
        public IEnumerable<(byte, byte)>[] Shapes;

        public Present(string input)
        {
            var lines = input.Split('\n');
            Id = byte.Parse(lines[0][.. ^1]);

            Shapes = new IEnumerable<(byte, byte)>[8];
            var originalShape = new List<(byte, byte)>();
            for (byte x = 0; x < 3; ++x)
            {
                for (byte y = 0; y < 3; ++y)
                {
                    if (lines[x + 1][y] == '#')
                        originalShape.Add((x, y));
                }
            }
            Shapes[0] = originalShape;

            for (int i = 1; i < 4; ++i)
                Shapes[i] = RotateCw(Shapes[i - 1]);
            for (int i = 4; i < 8; ++i)
                Shapes[i] = Mirror(Shapes[i - 4]);
        }

        private static IEnumerable<(byte, byte)> RotateCw(IEnumerable<(byte, byte)> shape)
        {
            return shape.Select(xy => (xy.Item2, (byte) (3 - 1 - xy.Item1)));
        }

        private static IEnumerable<(byte, byte)> Mirror(IEnumerable<(byte, byte)> shape)
        {
            return shape.Select(xy => (xy.Item1, (byte) (3 - 1 - xy.Item2)));
        }

        public override string ToString()
        {
            return string.Join(',', Shapes[4]);
        }
    }

    private sealed record Region
    {
        public readonly byte Width;
        public readonly byte Height;
        public readonly byte[] Presents;

        public Region(string input)
        {
            var split = input.Split([' ', 'x', ':'], StringSplitOptions.RemoveEmptyEntries);
            Width = byte.Parse(split[0]);
            Height = byte.Parse(split[1]);
            Presents = [.. split[2 ..].Select(byte.Parse)]; ;
        }

        public override string ToString()
        {
            return $"{Width}x{Height}: {string.Join(',', Presents)}";
        }
    }

    private readonly (Present[], Region[]) _input;

    private (Present[], Region[]) ParseInput()
    {
        var split = File.ReadAllText(InputFilePath).Split("\n\n");
        return (
            [.. split[0 .. ^1].Select(section => new Present(section))],
            [.. split[^1]
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => new Region(line))]
        );
    }

    public Day12()
    {
        _input = ParseInput();
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private uint Part1()
    {
        var (presents, regions) = _input;
        var fit = 0U;
        foreach (var region in regions)
        {
            var seen = new HashSet<(int, int, int, int)>();
            var occupied = new HashSet<(byte, byte)>(region.Width * region.Height);

            bool Dfs()
            {
                int presentId = -1;
                for (int i = 0; i < region.Presents.Length; ++i)
                {
                    if (region.Presents[i] > 0)
                    {
                        presentId = i;
                        region.Presents[i] -= 1;
                        break;
                    }
                }
                if (presentId == -1)
                    return true;

                for (byte x = 0; x <= region.Width - 3; ++x)
                {
                    for (byte y = 0; y <= region.Height - 3; ++y)
                    {
                        for (var variantIdx = 0; variantIdx < 8; ++variantIdx)
                        {
                            var hash = (x, y, presentId, variantIdx);
                            if (seen.Contains(hash))
                                continue;
                            seen.Add(hash);

                            var shapeVariant = presents[presentId].Shapes[variantIdx];
                            if (shapeVariant.Any(xy => occupied.Contains(((byte) (x + xy.Item1), (byte) (y + xy.Item2)))))
                                continue;

                            foreach (var xy in shapeVariant)
                                occupied.Add(((byte) (x + xy.Item1), (byte) (y + xy.Item2)));

                            if (Dfs())
                                return true;

                            foreach (var xy in shapeVariant)
                                occupied.Remove(((byte) (x + xy.Item1), (byte) (y + xy.Item2)));
                        }
                    }
                }
                region.Presents[presentId] += 1;

                return false;
            }

            if (Dfs())
            {
                fit += 1U;
                Console.WriteLine("{0} fit", region.ToString());
            }
            else
                Console.WriteLine("{0} not fit", region.ToString());
        }
        return fit;
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private int Part2()
    {
        return -1;
    }
}
