
internal class AsmParser
{
    private Interpretter interpretter;

    public AsmParser(Interpretter interpretter)
    {
        this.interpretter = interpretter;
    }

    public int[] Parse(string s)
    {
        var result = s.Split('\n')
            .Select((x, i) => new { line = i, x = x.Trim().Split(' ') })
            .Where(x => x.x.Length > 0 && !(x.x[0] == ""))
            .SelectMany(x =>
            {
                return x.x[0].ToLower() switch
                {
                    "pushi" => new[] { Instruction.PushI, int.Parse(x.x[1]) },
                    "call" => new[] {Instruction.MethodCall, interpretter.GetString(x.x[1])},
                    "add" => new[] { Instruction.Add },
                    "print" => new[] { Instruction.Print },
                    "stop" => new[] { Instruction.Stop },
                    "ret" => new[] { Instruction.MethodReturn },
                    _ => throw new Exception($"Dont understand '{string.Join(" ", x.x)}'"),
                };
            })
            .ToArray();

        return result;
    }
}
