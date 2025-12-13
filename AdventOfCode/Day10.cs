namespace AdventOfCode;

public class Day10 : BaseDay
{
    private sealed record Machine
    {
        private readonly int _lightDiagram = 0;
        private readonly int[][] _buttons;
        private readonly int[] _joltageReqs;

        public Machine(string input)
        {
            var split = input.Split(' ');
            for (int i = 1; i < split[0].Length - 1; ++i)
            {
                if (split[0][i] == '#')
                    _lightDiagram |= 1 << (i - 1);
            }
            _buttons = [.. split[1 .. ^1]
                .Select(str => str[1 .. ^1]
                    .Split(',')
                    .Select(int.Parse)
                    .ToArray()
                )
            ];
            _joltageReqs = [.. split[^1][1 .. ^1]
                .Split(',')
                .Select(int.Parse)
            ];
        }

        public uint MinPressesLight()
        {
            var seen = new HashSet<int>([_lightDiagram]);
            var queue = new Queue<(int, uint)>([(_lightDiagram, 0U)]);

            while (queue.Count > 0)
            {
                var (light, presses) = queue.Dequeue();
                if (light == 0)
                    return presses;

                foreach (var button in _buttons)
                {
                    var newLight = light;
                    foreach (var toggle in button)
                        newLight ^= 1 << toggle;

                    if (seen.Contains(newLight))
                        continue;
                    seen.Add(newLight);

                    queue.Enqueue((newLight, presses + 1U));
                }
            }
            throw new Exception("no solution found");
        }

        // basically this idea: https://www.reddit.com/r/adventofcode/comments/1pity70/2025_day_10_solutions/ntb36sb/
        // since my bfs + factoring heuristic can't finish the OOM lines
        public uint MinPressesJoltDfs()
        {
            Console.WriteLine("  diagram: {0}", _lightDiagram);

            uint Dfs(int[] joltage, int[] buttonIdxs)
            {
                if (joltage.All(jolt => jolt == 0))
                    return 0U;

                IEnumerable<(int, int)> MinJoltTargets()
                {
                    var minJolt = joltage
                        .Where(jolt => jolt != 0)
                        .Min();
                    return joltage
                        .Select((jolt, i) => (jolt, i))
                        .Where(jolt => jolt.jolt == minJolt);
                }

                var minPresses = (uint) ushort.MaxValue;
                foreach (var (minJolt, minJoltIdx) in MinJoltTargets())
                {
                    var matchedButtonIdxs = buttonIdxs
                        .Where(i => _buttons[i].Contains(minJoltIdx))
                        .ToArray();
                    if (matchedButtonIdxs.Length == 0)
                        continue;

                    var prunedButtonIdxs = buttonIdxs.Where(i => !matchedButtonIdxs.Contains(i)).ToArray();

                    IEnumerable<int[]> GenerateFreqs()
                    {
                        var freqs = new int[matchedButtonIdxs.Length];
                        freqs[^1] = minJolt;
                        yield return freqs.ToArray();

                        while (true)
                        {
                            var fromIdx = freqs.Length - 1;
                            while (fromIdx > 0 && freqs[fromIdx] == 0)
                                fromIdx -= 1;
                            if (fromIdx == 0)
                                break;

                            var tmp = freqs[fromIdx];
                            freqs[fromIdx - 1] += 1;
                            freqs[fromIdx] = 0;
                            freqs[^1] = tmp - 1;

                            yield return freqs.ToArray();
                        }
                    }

                    foreach (var matchedButtonFreqs in GenerateFreqs())
                    {
                        var newJoltage = joltage.ToArray();
                        var invalid = false;
                        for (int i = 0; i < matchedButtonFreqs.Length; ++i)
                        {
                            var freq = matchedButtonFreqs[i];
                            if (freq == 0)
                                continue;

                            var buttonIdx = matchedButtonIdxs[i];
                            foreach (var toggle in _buttons[buttonIdx])
                            {
                                if (newJoltage[toggle] < freq)
                                {
                                    invalid = true;
                                    break;
                                }
                                newJoltage[toggle] -= freq;
                            }
                            if (invalid)
                                break;
                        }
                        if (!invalid)
                            minPresses = Math.Min(minPresses, Dfs(newJoltage, prunedButtonIdxs) + (uint) minJolt);
                    }
                }

                return minPresses;
            }

            var presses = Dfs(_joltageReqs, [.. Enumerable.Range(0, _buttons.Length)]);
            Console.WriteLine("  total: {0}", presses);
            return presses;
        }

        // uses a huge ton of memory
        // can solve some lines instantly, some a few mins, but a couple lines OOM
        // OOM lines: 3, 55, 106, 107, 113, 161
        public uint MinPressesJoltBfs()
        {
            Console.WriteLine("  diagram: {0}", _lightDiagram);
            var startJoltage = _joltageReqs.ToArray();

            var edgeCaseIdx = _joltageReqs.IndexOf(1);
            if (edgeCaseIdx != -1)
            {
                foreach (var button in _buttons)
                {
                    if (button.Contains(edgeCaseIdx))
                    {
                        foreach (var toggle in button)
                            startJoltage[toggle] -= 1;
                        break;
                    }
                }
            }

            // cache number of presses to get to joltage array
            // if remaining joltage to target is factor of something computed already
            // key: joltage array, value: presses
            var pressesCache = new Dictionary<(long, long), uint>
            {
                [HashJoltage(new int[startJoltage.Length])] = 0U
            };
            // cache the (presses, factor) at that point in time for remaining joltage array to reach target
            // if we computed a factor but don't have the joltage in `pressesCache`, we instead cache the needed
            // joltage for that factor and hope we encounter it in the future
            // key: remaining factored joltage array, value: (presses at that time, factor at that time)
            var factorCache =  new Dictionary<(long, long), (uint, uint)>();

            var queue = new Queue<(int[], uint)>([(startJoltage, 0U)]);
            while (queue.Count > 0)
            {
                var (joltage, presses) = queue.Dequeue();
                if (joltage.All(jolt => jolt == 0))
                {
                    Console.WriteLine("  pure brute force | presses: {0}", presses);
                    return presses;
                }

                presses += 1U;
                for (int i = _buttons.Length - 1; i >= 0; --i) // foreach (var button in _buttons)
                {
                    var button = _buttons[i];
                    if (edgeCaseIdx != -1 && button.Contains(edgeCaseIdx))
                        continue;

                    var newJoltage = joltage.ToArray();
                    var invalid = false;
                    foreach (var toggle in button)
                    {
                        if (newJoltage[toggle] <= 0)
                        {
                            invalid = true;
                            break;
                        }
                        newJoltage[toggle] -= 1;
                    }
                    if (invalid)
                        continue;

                    var joltageUsed = _joltageReqs.Select((jolt, i) => jolt - newJoltage[i]);
                    var usedJoltageHash = HashJoltage(joltageUsed);
                    if (pressesCache.ContainsKey(usedJoltageHash))
                        continue;
                    else
                        pressesCache[usedJoltageHash] = presses;

                    if (factorCache.TryGetValue(usedJoltageHash, out (uint, uint) factored))
                    {
                        var unfactoredPresses = factored.Item1 + factored.Item2 * presses;
                        Console.WriteLine("  future factor: {0} | total: {1}", factored.Item2, unfactoredPresses);
                        return unfactoredPresses;
                    }

                    var minRemainingJolt = newJoltage.Min();
                    var minUnfactoredPresses = uint.MaxValue;
                    var factorExists = false;
                    for (var factor = 2; factor <= minRemainingJolt; ++factor)
                    {
                        if (!newJoltage.All(jolt => jolt % factor == 0))
                            continue;

                        var factoredJoltage = newJoltage.Select(jolt => jolt / factor);
                        var factorHash = HashJoltage(factoredJoltage);
                        if (pressesCache.TryGetValue(factorHash, out uint factoredPress))
                        {
                            var unfactoredPresses = presses + (uint) factor * factoredPress;
                            minUnfactoredPresses = Math.Min(minUnfactoredPresses, unfactoredPresses);
                            factorExists = true;
                            Console.WriteLine("  past factor: {0} | total: {1}", factor, minUnfactoredPresses);
                        }
                        else
                            factorCache[factorHash] = (presses, (uint) factor);
                    }
                    if (factorExists)
                        return minUnfactoredPresses;

                    queue.Enqueue((newJoltage, presses));
                }
            }
            throw new Exception("no solution found");
        }

        // string ends up causing collision somehow
        // largest jolt is 275 in my input
        private static (long, long) HashJoltage(IEnumerable<int> joltage)
        {
            var hash = new long[2];
            foreach (var (jolt, i) in joltage.Select((jolt, i) => ((long) jolt, i)))
                hash[i / 5] = (hash[i / 5] << 9) | jolt;
            return (hash[0], hash[1]);
        }

        private static string EncodeJoltage(IEnumerable<int> joltage)
        {
            return string.Join(',', joltage);
        }
    }

    private readonly IEnumerable<Machine> _manual;

    private IEnumerable<Machine> ParseInput()
    {
        foreach (var line in File.ReadAllLines(InputFilePath))
            yield return new Machine(line);
    }

    public Day10()
    {
        _manual = ParseInput();
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private uint Part1()
    {
        var presses = 0U;
        foreach (var machine in _manual)
            presses += machine.MinPressesLight();
        return presses;
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private uint Part2()
    {
        var presses = 0U;
        foreach (var (machine, i) in _manual.Select((machine, i) => (machine, i)))
        {
            Console.WriteLine("line {0}", i + 1);
            presses += machine.MinPressesJoltDfs();
        }
        return presses;

        // var oomLines = new int[]{ 3, 55, 106, 107, 113, 161 };
        // var presses = 0U;
        // foreach (var (machine, i) in _manual.Select((machine, i) => (machine, i)))
        // {
        //     if (oomLines.Contains(i))
        //         continue;

        //     Console.WriteLine("line {0}", i + 1);
        //     presses += machine.MinPressesJoltBfs();
        // }
        // return presses;
    }
}
