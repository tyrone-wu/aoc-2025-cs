namespace AdventOfCode;

public class Day02 : BaseDay
{
    private readonly struct IdRange
    {
        private readonly ulong _start;
        private readonly ulong _end;

        public IdRange(string input)
        {
            var pairStr = input.Split('-');
            _start = ulong.Parse(pairStr[0]);
            _end = ulong.Parse(pairStr[1]);
        }

        public IEnumerable<ulong> GetInvalidP1()
        {
            for (ulong id = _start; id <= _end; ++id)
            {
                var numStr = id.ToString();
                var halfDigits = numStr.Length / 2;
                if (numStr[..halfDigits] == numStr[halfDigits..])
                    yield return id;
            }
        }

        public IEnumerable<ulong> GetInvalidP2()
        {
            for (ulong id = _start; id <= _end; ++id)
            {
                if (IsInvalidP2(id))
                    yield return id;
            }
        }

        private static bool IsInvalidP2(ulong id)
        {
            var numStr = id.ToString();
            var halfDigits = numStr.Length / 2;
            for (int len = 1; len <= halfDigits; ++len)
            {
                if (numStr.Length % len != 0)
                    continue;

                var isInvalid = true;
                for (int sIdx = len; sIdx + len <= numStr.Length; sIdx += len)
                {
                    if (numStr[(sIdx - len) .. sIdx] != numStr[sIdx .. (sIdx + len)])
                    {
                        isInvalid = false;
                        break;
                    }
                }
                if (isInvalid)
                    return true;
            }
            return false;
        }
    }

    private readonly IEnumerable<IdRange> _input;

    private IEnumerable<IdRange> ParseInput()
    {
        var input = File.ReadAllText(InputFilePath);
        foreach (var rangeStr in input.Split([',', '\n'], StringSplitOptions.RemoveEmptyEntries))
            yield return new IdRange(rangeStr);
    }

    public Day02()
    {
        _input = ParseInput();
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private ulong Part1()
    {
        var ans = 0UL;
        foreach (var range in _input)
        {
            foreach (var invalidId in range.GetInvalidP1())
                ans += invalidId;
        }
        return ans;
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private ulong Part2()
    {
        var ans = 0UL;
        foreach (var range in _input)
        {
            foreach (var invalidId in range.GetInvalidP2())
                ans += invalidId;
        }
        return ans;
    }
}
