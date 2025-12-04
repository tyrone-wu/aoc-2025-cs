namespace AdventOfCode;

public class Day04 : BaseDay
{
    private readonly HashSet<(int, int)> _grid;

    private HashSet<(int, int)> ParseInput()
    {
        var grid = new HashSet<(int, int)>();
        var lines = File.ReadAllLines(InputFilePath);
        var rows = lines.Length;
        var cols = lines[0].Length;
        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < cols; ++col)
            {
                if (lines[row][col] == '@')
                    grid.Add((row, col));
            }
        }
        return grid;
    }

    public Day04()
    {
        _grid = ParseInput();
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private int Part1()
    {
        var markedRolls = new HashSet<(int, int)>();
        MarkRolls(markedRolls);
        return markedRolls.Count;
    }

    private void MarkRolls(HashSet<(int, int)> buffer)
    {
        foreach (var coord in _grid)
        {
            if (NumAdjacent(coord.Item1, coord.Item2) < 4)
                buffer.Add(coord);
        }
    }

    private int NumAdjacent(int row, int col)
    {
        var adjacent = 0;
        for (int i = row - 1; i <= row + 1; ++i)
        {
            for (int j = col - 1; j <= col + 1; ++j)
            {
                if (i == row && j == col)
                    continue;

                if (_grid.Contains((i, j)))
                    adjacent += 1;
            }
        }
        return adjacent;
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private int Part2()
    {
        var ans = 0;
        var markedRolls = new HashSet<(int, int)>();

        MarkRolls(markedRolls);
        while (markedRolls.Count > 0)
        {
            ans += markedRolls.Count;
            _grid.ExceptWith(markedRolls);
            markedRolls.Clear();
            MarkRolls(markedRolls);
        }
        return ans;
    }
}
