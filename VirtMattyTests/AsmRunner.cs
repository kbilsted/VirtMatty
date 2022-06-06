using Vm.Assembler;

static class AsmRunner
{
    public static List<object> Go(string mainAsm, params (string method, string[] args, string asm)[] methods)
    {
        var interpretter = new Interpretter(new Disassembler());
        // interpretter.Trace = false;

        var asmP = new Assembler(interpretter);

        foreach (var (method, args, asm) in methods)
        {
            if (args == null) throw new ArgumentNullException("args");
            args.ToList().ForEach(x => interpretter.AddString(x));
            interpretter.AddString(method);
            interpretter.AddMethod(method, args, asmP.Assemble(asm, args));
        }

        interpretter.AddMethod("Main", new string[0], asmP.Assemble(mainAsm, new string[0]));

        interpretter.Start();

        return interpretter.Printed;
    }
}
