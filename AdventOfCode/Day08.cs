namespace AdventOfCode;

public class Day08 : BaseDay
{
    private readonly struct Coord
    {
        public readonly int X;
        private readonly int _y;
        private readonly int _z;

        public Coord(string input)
        {
            var split = input.Split(',');
            X = int.Parse(split[0]);
            _y = int.Parse(split[1]);
            _z = int.Parse(split[2]);
        }

        public ulong EuclidDistSquared(Coord other)
        {
            var dx = (ulong) Math.Abs(X - other.X);
            var dy = (ulong) Math.Abs(_y - other._y);
            var dz = (ulong) Math.Abs(_z - other._z);
            return dx * dx + dy * dy + dz * dz;
        }

        public override string ToString()
        {
            return $"{X},{_y},{_z}";
        }
    }

    private readonly List<Coord> _coords;

    private IEnumerable<Coord> ParseInput()
    {
        foreach (var line in File.ReadAllLines(InputFilePath))
            yield return new Coord(line);
    }

    public Day08()
    {
        _coords = [.. ParseInput()];
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private ulong Part1()
    {
        const int connections = 1000;

        var edges = GetEdges();
        var unused = _coords.ToHashSet();
        var circuits = new List<HashSet<Coord>>(unused.Count);
        var edgeIdx = 0;
        for (int i = 0; i < connections; ++i)
        {
            var (_, a, b) = edges[edgeIdx];
            ++edgeIdx;
            ConnectBoxes(circuits, unused, a, b);
        }

        return circuits
            .Select(circuit => circuit.Count)
            .OrderDescending()
            .Take(3)
            .Aggregate(1UL, (acc, size) => acc * ((ulong) size));
    }

    private static void ConnectBoxes(List<HashSet<Coord>> circuits, HashSet<Coord> unused, Coord a, Coord b)
    {
        if (!unused.Contains(a) && !unused.Contains(b))
        {
            var (circuitA, i) = GetCircuit(circuits, a);
            var (circuitB, j) = GetCircuit(circuits, b);
            if (i != j)
            {
                circuitA.UnionWith(circuitB);
                circuits.RemoveAt(j);
            }
        }
        else if (unused.Contains(a) && unused.Contains(b))
        {
            circuits.Add([a, b]);
            unused.Remove(a);
            unused.Remove(b);
        }
        else
        {
            var (inCircuit, toAdd) = unused.Contains(b) ? (a, b) : (b, a);
            foreach (var circuit in circuits)
            {
                if (circuit.Contains(inCircuit))
                {
                    circuit.Add(toAdd);
                    break;
                }
            }
            unused.Remove(toAdd);
        }
    }

    private static (HashSet<Coord>, int) GetCircuit(List<HashSet<Coord>> circuits, Coord coord)
    {
        for (int i = 0; i < circuits.Count; ++i)
        {
            if (circuits[i].Contains(coord))
                return (circuits[i], i);
        }
        throw new Exception("dummy");
    }

    private List<(ulong, Coord, Coord)> GetEdges()
    {
        var capacity = _coords.Count * (_coords.Count - 1) / 2;
        var edges = new List<(ulong, Coord, Coord)>(capacity);
        for (int i = 0; i < _coords.Count; ++i)
        {
            var a = _coords[i];
            for (int j = i + 1; j < _coords.Count; ++j)
            {
                var b = _coords[j];
                edges.Add((a.EuclidDistSquared(b), a, b));
            }
        }
        edges.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        return edges;
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private ulong Part2()
    {
        var edges = GetEdges();
        var unused = _coords.ToHashSet();
        var circuits = new List<HashSet<Coord>>(unused.Count);
        var edgeIdx = 0;
        Coord? lastA = null;
        Coord? lastB = null;
        while (unused.Count > 0 || circuits.Count != 1)
        {
            var (_, a, b) = edges[edgeIdx];
            ++edgeIdx;
            ConnectBoxes(circuits, unused, a, b);
            lastA = a;
            lastB = b;
        }
        return (ulong) lastA.Value.X * (ulong) lastB.Value.X;
    }
}
