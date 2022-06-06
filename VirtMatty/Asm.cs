
namespace Vm.Assembler;

public class Assembler
{
    private Interpretter interpretter;

    public Assembler(Interpretter interpretter)
    {
        this.interpretter = interpretter;
    }

    public int[] Assemble(string s, string[] localVariables)
    {
        var labels = new Dictionary<string, int>();
        var jumps = new List<(string, int)>();
        var opCodes = new List<int>();
        var lines = s.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (line.Length == 0)
                continue;

            if (line.EndsWith(':'))
            {
                labels.Add(line.Substring(0, line.Length - 1), opCodes.Count);
                continue;
            }

            var segments = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            string first = segments[0];

            if (!ByteCode.MetaX.TryGetValue(first, out var meta))
                throw new Exception($"Dont understand '{string.Join(" ", segments)}'");

            int code = meta.ByteValue;

            if (meta.ParameterCount == 0) opCodes.AddRange(new[] { code });
            else if (first == "pushi") opCodes.AddRange(new[] { code, int.Parse(segments[1]) });
            else if (first == "pushlocal") opCodes.AddRange(new[] { code, GetPositionFromName(localVariables, segments[1]) });
            else if (first == "pusharg") opCodes.AddRange(new[] { code, GetPositionFromName(localVariables, segments[1]) });
            else if (first == "load") opCodes.AddRange(new[] { code, int.Parse(segments[1]) });
            else if (first == "store") opCodes.AddRange(new[] { code, int.Parse(segments[1]) });
            else if (first == "call") opCodes.AddRange(new[] { code, interpretter.GetString(segments[1]) });
            else if (first == "jmp" || first == "brf" || first == "brt" || first == "jumpless")
            {
                opCodes.AddRange(new[] { code, -1 });
                jumps.Add((segments[1], opCodes.Count - 1));
            }
            else
                throw new Exception($"Forgot to implement: '{string.Join(" ", segments)}'");
        }

        foreach (var (name, pos) in jumps)
        {
            opCodes[pos] = labels[name];
        }

        return opCodes.ToArray();
    }

    int GetPositionFromName(string[] xs, string x)
    {
        for (int i = 0; i < xs.Length; i++)
            if (xs[i] == x)
                return i;
        throw new Exception($"Not found '{x}'");
    }
}

public class Disassembler
{
    public string Disassemble(int[] code, int position)
    {
        int instruction = code[position];
        var meta = ByteCode.Meta[instruction];
        var args = code.AsSpan(position + 1, meta.ParameterCount).ToArray();

        return $"{position,5:D4}: {meta.AsmName} {string.Join(" ", args)}";
    }
}
