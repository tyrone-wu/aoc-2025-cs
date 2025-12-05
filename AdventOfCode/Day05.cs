namespace AdventOfCode;

public class Day05 : BaseDay
{
    private sealed record IdRange
    {
        public ulong Start;
        private ulong _end;

        public IdRange(string input)
        {
            var pairStr = input.Split('-');
            Start = ulong.Parse(pairStr[0]);
            _end = ulong.Parse(pairStr[1]);
        }

        public bool Contains(ulong id)
        {
            return Start <= id && id <= _end;
        }

        public bool TryMerge(IdRange other)
        {
            var intersect = Contains(other.Start) || Contains(other._end);
            if (intersect)
            {
                Start = Math.Min(Start, other.Start);
                _end = Math.Max(_end, other._end);
            }
            return intersect;
        }

        public ulong Size()
        {
            return _end - Start + 1UL;
        }
    }

    private sealed record IngredientDb
    {
        private readonly List<IdRange> _idRanges;
        private readonly IEnumerable<ulong> _ids;

        public IngredientDb(string input)
        {
            var split = input.Split("\n\n");

            var ranges = split[0]
                .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(rangeStr => new IdRange(rangeStr))
                .OrderBy(range => range.Start)
                .ToList();
            _idRanges = [ranges[0]];
            foreach (var range in ranges[1 ..])
            {
                if (!_idRanges[^1].TryMerge(range))
                    _idRanges.Add(range);
            }

            _ids = [.. split[1]
                .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(ulong.Parse)];
        }

        public int FreshIngredients()
        {
            var fresh = 0;
            foreach (var id in _ids)
            {
                foreach (var range in _idRanges)
                {
                    if (range.Contains(id))
                    {
                        fresh += 1;
                        break;
                    }
                }
            }
            return fresh;
        }

        public ulong TotalFresh()
        {
            var fresh = 0UL;
            foreach (var range in _idRanges)
                fresh += range.Size();
            return fresh;
        }
    }

    private readonly IngredientDb _database;

    public Day05()
    {
        _database = new IngredientDb(File.ReadAllText(InputFilePath));
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private int Part1()
    {
        return _database.FreshIngredients();
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private ulong Part2()
    {
        return _database.TotalFresh();
    }
}
