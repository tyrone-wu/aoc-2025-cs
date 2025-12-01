namespace AdventOfCode;

public class Day01 : BaseDay
{
    private enum Direction
    {
        Left,
        Right
    }

    private class Rotation
    {
        public readonly Direction Direction;
        public readonly int Distance;

        public Rotation(string input)
        {
            Direction = input[0] == 'L' ? Direction.Left : Direction.Right;
            Distance = int.Parse(input[1..]);
        }
    }

    private struct Dial(int pointer)
    {
        public int Pointer = pointer;

        public void Turn(Rotation rotation)
        {
            if (rotation.Direction == Direction.Left)
                Pointer -= rotation.Distance;
            else
                Pointer += rotation.Distance;
            Pointer = ((Pointer % 100) + 100) % 100;
        }
    }

    private readonly IEnumerable<Rotation> _input;

    private IEnumerable<Rotation> ParseInput()
    {
        foreach (var line in File.ReadAllLines(InputFilePath))
            yield return new Rotation(line);
    }

    public Day01()
    {
        _input = ParseInput();
    }

    public override ValueTask<string> Solve_1() => new($"{Part1Math()}");

    private int Part1()
    {
        var dial = new Dial(50);
        var ans = 0;
        foreach (var rotation in _input)
        {
            dial.Turn(rotation);
            if (dial.Pointer == 0)
                ans += 1;
        }
        return ans;
    }

    private int Part1Math()
    {
        var dial = 50;
        var ans = 0;
        foreach (var line in File.ReadAllLines(InputFilePath))
        {
            var dist = int.Parse(line[1..]);
            if (line[0] == 'L')
                dist *= -1;
            dial += dist;
            if (dial % 100 == 0)
                ans += 1;
        }
        return ans;
    }

    public override ValueTask<string> Solve_2() => new($"{Part2Math()}");

    private int Part2()
    {
        var dial = new Dial(50);
        var ans = 0;
        foreach (var rotation in _input)
        {
            var prevPtr = dial.Pointer;
            dial.Turn(rotation);
            if (dial.Pointer == 0)
                ans += 1;

            if (rotation.Direction == Direction.Left && rotation.Distance > prevPtr)
            {
                var distAfterZero = rotation.Distance - prevPtr;
                if (!(prevPtr == 0 || distAfterZero % 100 == 0)) // basically in place of ceiling division
                    ans += 1;
                ans += distAfterZero / 100;
            }
            else if (rotation.Direction == Direction.Right && prevPtr + rotation.Distance > 100)
            {
                var distAfterZero = rotation.Distance - ((100 - prevPtr) % 100);
                if (!(prevPtr == 0 || distAfterZero % 100 == 0)) // basically in place of ceiling division
                    ans += 1;
                ans += distAfterZero / 100;
            }
        }
        return ans;
    }

    private int Part2Math()
    {
        var dial = 50;
        foreach (var line in File.ReadAllLines(InputFilePath))
        {
            var dist = int.Parse(line[1..]);
            if (line[0] == 'L')
            {
                var dialRem = dial % 100;
                var distRem = dist % 100;

                if (dialRem == distRem)
                    dial += 100 - dialRem;
                else if (dialRem > distRem)
                    dial -= distRem;
                else
                {
                    if (dialRem != 0)
                        dial += 100;
                    dial += 100 - distRem;
                }
                dial += dist - distRem;
            }
            else
                dial += dist;
        }
        return dial / 100;
    }
}
