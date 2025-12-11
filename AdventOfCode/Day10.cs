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

        // public uint MinPressesJoltDfs()
        // {
        //     var cache = new Dictionary<int, uint>();
        //     foreach (var button in _buttons)
        //     {
        //         var buttonJoltage = new int[_joltageReqs.Length];
        //         foreach (var toggle in button)
        //             buttonJoltage[toggle] += 1;
        //         cache[HashJoltage(buttonJoltage)] = 1;
        //     }

        //     Console.WriteLine("diagram: {0}", _lightDiagram);
        //     uint Dfs(int[] joltage)
        //     {
        //         if (joltage.All(jolt => jolt == 0))
        //             return 0U;

        //         var encoded = HashJoltage(joltage);
        //         if (cache.TryGetValue(encoded, out uint cachedPresses))
        //             return cachedPresses;

        //         var presses = (uint) int.MaxValue;
        //         foreach (var button in _buttons)
        //         {
        //             var newJoltage = joltage.ToArray();
        //             var invalid = false;
        //             foreach (var toggle in button)
        //             {
        //                 if (newJoltage[toggle] <= 0)
        //                 {
        //                     invalid = true;
        //                     break;
        //                 }
        //                 newJoltage[toggle] -= 1;
        //             }
        //             if (invalid)
        //                 continue;

        //             presses = Math.Min(presses, Dfs(newJoltage) + 1);
        //         }

        //         cache[encoded] = presses;
        //         return presses;
        //     }

        //     var presses = Dfs(_joltageReqs);
        //     Console.WriteLine(presses);
        //     return presses;
        // }

        // uses a huge ton of memory
        // can do all lines on actual input except line 113
        public uint MinPressesJolt()
        {
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

            // var seen = new HashSet<int>()
            // {
            //     HashJoltage(startJoltage)
            // };
            var cache = new Dictionary<string, uint>
            {
                [EncodeJoltage(new int[startJoltage.Length])] = 0U
            };

            Console.WriteLine("diagram: {0}", _lightDiagram);
            var queue = new Queue<(int[], uint)>([(startJoltage, 0U)]);
            while (queue.Count > 0)
            {
                var (joltage, presses) = queue.Dequeue();
                if (joltage.All(jolt => jolt == 0))
                {
                    Console.WriteLine("  idk how it managed to get here, presses: {0}", presses);
                    return presses;
                }

                presses += 1U;
                foreach (var button in _buttons)
                {
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

                    // var joltageEncoded = HashJoltage(newJoltage);
                    // if (seen.Contains(joltageEncoded))
                    //     continue;
                    // seen.Add(joltageEncoded);

                    var joltageUsed = _joltageReqs
                        .Select((jolt, i) => jolt - newJoltage[i])
                        .ToArray();
                    var encodedUsed = EncodeJoltage(joltageUsed);
                    if (cache.ContainsKey(encodedUsed))
                        continue;
                    else
                        cache[encodedUsed] = presses;

                    var minRemaining = newJoltage.Min();
                    var minFactoredPresses = uint.MaxValue;
                    var canFactor = false;
                    for (var factor = 2; factor <= minRemaining; ++factor)
                    {
                        if (!newJoltage.All(jolt => jolt % factor == 0))
                            continue;

                        var factoredJoltage = newJoltage
                            .Select(jolt => jolt / factor)
                            .ToArray();
                        var encodedFactor = EncodeJoltage(factoredJoltage);
                        if (cache.TryGetValue(encodedFactor, out uint factoredPress))
                        {
                            Console.WriteLine("  factor: {0} | total: {1}", factor, presses + (uint) factor * factoredPress);
                            canFactor = true;
                            minFactoredPresses = Math.Min(minFactoredPresses, presses + (uint) factor * factoredPress);
                        }
                    }
                    if (canFactor)
                        return minFactoredPresses;

                    queue.Enqueue((newJoltage, presses));
                }
            }
            throw new Exception("no solution found");
        }

        private static int HashJoltage(IEnumerable<int> joltage)
        {
            return EncodeJoltage(joltage).GetHashCode();
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
        foreach (var machine in _manual)
            presses += machine.MinPressesJolt();
        return presses;
    }
}
