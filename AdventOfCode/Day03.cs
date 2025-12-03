namespace AdventOfCode;

public class Day03 : BaseDay
{
    private sealed record Bank
    {
        private readonly List<byte> _batteries;

        public Bank(string input)
        {
            _batteries = [.. input.Select(battery => (byte)(battery - '0'))];
        }

        public ulong LargestJoltage(int digits)
        {
            var largestJoltage = 0UL;
            var frontUsedIdx = 0;
            for (int backReservedIdx = _batteries.Count - digits + 1; backReservedIdx <= _batteries.Count; ++backReservedIdx)
            {
                var max = _batteries[frontUsedIdx .. backReservedIdx].Max();
                frontUsedIdx += _batteries[frontUsedIdx .. backReservedIdx].IndexOf(max) + 1;
                largestJoltage = largestJoltage * 10UL + max;
            }
            return largestJoltage;
        }
    }

    private readonly IEnumerable<Bank> _input;

    private IEnumerable<Bank> ParseInput()
    {
        foreach (var line in File.ReadAllLines(InputFilePath))
            yield return new Bank(line);
    }

    public Day03()
    {
        _input = ParseInput();
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private ulong Part1()
    {
        var sumJoltage = 0UL;
        foreach (var bank in _input)
            sumJoltage += bank.LargestJoltage(2);
        return sumJoltage;
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private ulong Part2()
    {
        var sumJoltage = 0UL;
        foreach (var bank in _input)
            sumJoltage += bank.LargestJoltage(12); 
        return sumJoltage;
    }
}
