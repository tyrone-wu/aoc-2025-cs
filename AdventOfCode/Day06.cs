namespace AdventOfCode;

public class Day06 : BaseDay
{
    private readonly string[] _input;

    public Day06()
    {
        _input = File.ReadAllLines(InputFilePath);
    }

    public override ValueTask<string> Solve_1() => new($"{Part1()}");

    private ulong Part1()
    {
        var numbers = new ulong[_input.Length - 1][];
        for (int i = 0; i < numbers.Length; ++i)
        {
            numbers[i] = [.. _input[i]
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(ulong.Parse)
            ];
        }

        return _input[^1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select((opStr, col) => (opStr, col))
            .Aggregate(0UL, (acc, operation) => {
                var (op, col) = operation;
                var result = numbers[0][col];
                for (int row = 1; row < numbers.Length; ++row)
                {
                    if (op == "+")
                        result += numbers[row][col];
                    else
                        result *= numbers[row][col];
                }
                return acc + result;
            });
    }

    public override ValueTask<string> Solve_2() => new($"{Part2()}");

    private ulong Part2()
    {
        var numMatrix = _input[.. ^1];
        var rows = numMatrix.Length;
        var operatorLine = _input[^1];

        var ans = 0UL;
        var currCol = 0;
        while (currCol < operatorLine.Length)
        {
            var nextOpIdx = NextOpIndex(operatorLine, currCol);
            var cutoff = nextOpIdx - 1;
            var op = operatorLine[currCol];

            var result = op == '+' ? 0UL : 1UL;
            for (int col = currCol; col < cutoff; ++col)
            {
                var number = 0UL;
                for (int row = 0; row < rows; ++row)
                {
                    if (numMatrix[row][col] != ' ')
                        number = number * 10UL + ((ulong) (numMatrix[row][col] - '0'));
                }
                if (op == '+')
                    result += number;
                else
                    result *= number;
            }

            ans += result;
            currCol = nextOpIdx;
        }

        return ans;
    }

    private static int NextOpIndex(string operatorLine, int currIdx)
    {
        for (int i = currIdx + 1; i < operatorLine.Length; ++i)
        {
            if (operatorLine[i] != ' ')
                return i;
        }
        return operatorLine.Length + 1;
    }
}
