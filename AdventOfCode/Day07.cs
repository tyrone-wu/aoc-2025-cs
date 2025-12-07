namespace AdventOfCode;

public class Day07 : BaseDay
{
    private readonly string[] _map;

    public Day07()
    {
        _map = File.ReadAllLines(InputFilePath);
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private int Part1()
    {
        return DfsP1(0, _map[0].IndexOf('S'), []);
    }

    private int DfsP1(int row, int col, HashSet<(int, int)> seen)
    {
        if (seen.Contains((row, col)) || row >= _map.Length)
            return 0;
        seen.Add((row, col));

        var split = 0;
        if (_map[row][col] == '^')
        {
            if (!seen.Contains((row, col - 1)) || !seen.Contains((row, col + 1)))
                split += 1;
            split += DfsP1(row, col - 1, seen);
            split += DfsP1(row, col + 1, seen);
        }
        else
            split += DfsP1(row + 1, col, seen);
        return split;
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private ulong Part2()
    {
        return DfsP2(0, _map[0].IndexOf('S'), []);
    }

    private ulong DfsP2(int row, int col, Dictionary<(int, int), ulong> timelines)
    {
        if (timelines.ContainsKey((row, col)))
            return timelines[(row, col)];
        if (row >= _map.Length)
            return 1UL;

        var split = 0UL;
        if (_map[row][col] == '^')
        {
            split += DfsP2(row, col - 1, timelines);
            split += DfsP2(row, col + 1, timelines);
        }
        else
            split += DfsP2(row + 1, col, timelines);

        timelines[(row, col)] = split;
        return split;
    }
}
