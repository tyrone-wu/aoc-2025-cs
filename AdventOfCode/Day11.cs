namespace AdventOfCode;

public class Day11 : BaseDay
{
    private sealed record Device
    {
        public readonly string Name;
        public readonly IEnumerable<string> Attached;

        public Device(string input)
        {
            var split = input.Split([' ', ':'], StringSplitOptions.RemoveEmptyEntries);
            Name = split[0];
            Attached = split[1 ..];
        }

        public override string ToString()
        {
            return $"{Name}: {string.Join(',', Attached)}";
        }
    }

    private readonly Dictionary<string, Device> _devices;

    private Dictionary<string, Device> ParseInput()
    {
        var graph = new Dictionary<string, Device>();
        foreach (var line in File.ReadAllLines(InputFilePath))
        {
            var device = new Device(line);
            graph[device.Name] = device;
        }
        return graph;
    }

    public Day11()
    {
        _devices = ParseInput();
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private uint Part1()
    {
        uint Dfs(string curr)
        {
            if (curr == "out")
                return 1U;

            var paths = 0U;
            foreach (var next in _devices[curr].Attached)
                paths += Dfs(next);
            return paths;
        }

        return Dfs("you");
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private ulong Part2()
    {
        var cache = new Dictionary<(string, bool, bool), ulong>();

        ulong Dfs(string curr, bool dac, bool fft)
        {
            if (curr == "out")
                return dac && fft ? 1UL : 0UL;

            var key = (curr, dac, fft);
            if (cache.TryGetValue(key, out ulong value))
                return value;

            dac = dac || curr == "dac";
            fft = fft || curr == "fft";
            var paths = 0UL;
            foreach (var next in _devices[curr].Attached)
                paths += Dfs(next, dac, fft);

            cache[key] = paths;
            return paths;
        }

        return Dfs("svr", false, false);
    }
}
