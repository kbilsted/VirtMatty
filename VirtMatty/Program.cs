using System.Text;
using Vm.Assembler;

var interpretter = new Interpretter(new Disassembler());
var asm = new Assembler(interpretter);
int fooPtr = interpretter.AddString("foo");
int add5Ptr = interpretter.AddString("Add5");
int arg0Ptr = interpretter.AddString("arg0");

interpretter.AddMethod("Main", new string[0], asm.Assemble(@"
pushi 1
pushi 2
add
print
call foo
halt
", new string[0])
);

interpretter.AddMethod("Main_old", new string[0], new int[]
{
    ByteCode.PushI, 1,
    ByteCode.PushI, 2,
    ByteCode.Add,
    ByteCode.Print,
    // call
    ByteCode.Call, fooPtr,
    // call
    ByteCode.PushI, 4, // arg 0
    ByteCode.Call, add5Ptr,
    ByteCode.Halt
});
interpretter.AddMethod("foo", new string[0], asm.Assemble(@"
pushi 3
pushi 5
add
print
ret
", new string[0]));
interpretter.AddMethod("Add5", new[] { "arg0" }, new[] {
    ByteCode.PushI, 5,
    ByteCode.PushLocal, arg0Ptr,
    ByteCode.Add,
    ByteCode.Print,
    ByteCode.Return
});
interpretter.Start();


public static class ByteCode
{
    public const int PushI = 0;
    public const int PushArg = 2;
    public const int PushLocal = 3;
    public const int LoadGlobal = 4;
    public const int StoreGlobal = 5;
    public const int Pop = 6;
    public const int Jump = 7;
    public const int JumpLess = 8;
    public const int Cmp = 9;
    public const int BranchTrue = 11;
    public const int BranchFalse = 12;
    public const int Add = 20;
    public const int Sub = 21;
    public const int Dup = 22;
    public const int Print = 30;
    public const int Call = 32;
    public const int Return = 64;
    public const int Halt = 128;

    private static readonly ByteCodeMeta[] Ms = new ByteCodeMeta[]
    {
        new (PushI,"pushi", 1),
        new (PushArg,"pushArg", 1),
        new (PushLocal,"pushlocal", 1),
        new (LoadGlobal,"load", 1),
        new (StoreGlobal,"store", 1),
        new (Pop,"pop", 0),
        new (Jump,"jmp", 1),
        new (JumpLess,"jumpless", 1),
        new (Cmp,"cmp", 0),
        new (BranchTrue,"brt", 1),
        new (BranchFalse,"brf", 1),
        new (Add,"add", 0),
        new (Sub,"sub", 0),
        new (Dup,"dup", 0),
        new (Print,"print", 0),
        new (Call,"call", 1),
        new (Return,"ret", 0),
        new (Halt,"halt", 0),
    };

    public static Dictionary<int, ByteCodeMeta> Meta = Ms.ToDictionary(x => x.ByteValue, x => x);
    public static Dictionary<string, ByteCodeMeta> MetaX = Ms.ToDictionary(x => x.AsmName.ToLower(), x => x);
}

public record ByteCodeMeta(int ByteValue, string AsmName, int ParameterCount);


public record MethodInfo(string Name, string[] ParameterNames, int MemoryIndex);

public record StackFrame(MethodInfo method, int ip, int sp, int bp);

public enum ConditionalFlags
{
    FL_POS = 1 << 0, /* P */
    FL_ZRO = 1 << 1, /* Z */
    FL_NEG = 1 << 2, /* N */
};

public class Interpretter
{
    public bool Trace = true;
    int IP = 0;
    int RSP = -1;
    int RCond = 0;

    int codeIndex = 0;
    public int[] Stack = new int[100000];
    public Stack<StackFrame> frames = new Stack<StackFrame>();
    public int[] Mem = new int[100000];
    public int[] Code = new int[100000];
    public List<string> StringConstantPool = new List<string>();
    Dictionary<string, MethodInfo> Methods = new Dictionary<string, MethodInfo>();
    public List<object> Printed = new List<object>();

    Disassembler Disassembler { get; }

    public Interpretter(Disassembler disassembler)
    {
        Disassembler = disassembler;
    }

    void UpdateCond(int result)
    {
        if (result > 0) RCond = (int)ConditionalFlags.FL_POS;
        if (result == 0) RCond = (int)ConditionalFlags.FL_ZRO;
        if (result < 0) RCond = (int)ConditionalFlags.FL_NEG;
    }

    public void AddMethod(string name, string[] parameterNames, int[] code)
    {
        AddString(name);

        Methods.Add(name, new MethodInfo(name, parameterNames, codeIndex));

        for (int i = 0; i < code.Length; i++)
        {
            Code[codeIndex] = code[i];
            codeIndex++;
        }

        // separator
        for (int i = 0; i < 3; i++)
            Code[codeIndex++] = -2;
    }

    public int AddString(string s)
    {
        for (int i = 0; i < StringConstantPool.Count; i++)
        {
            if (StringConstantPool[i] == s)
                return i;
        }

        StringConstantPool.Add(s);
        return StringConstantPool.Count - 1;
    }

    public int GetString(string s)
    {
        for (int i = 0; i < StringConstantPool.Count; i++)
        {
            if (StringConstantPool[i] == s)
                return i;
        }
        throw new Exception($"Not found '{s}'");
    }

    public void Start()
    {
        MethodInfo info = Methods["Main"];
        IP = info.MemoryIndex;
        frames.Push(new StackFrame(Methods["Main"], IP, RSP, 0));

        string name;
        int offset;

        while (true)
        {
            if (RSP > 1000)
                break;

            if (Trace)
                PrintTrace();

            int instruction = Code[IP];
            switch (instruction)
            {
                case ByteCode.PushI:
                    IP++;
                    Stack[++RSP] = Code[IP];
                    IP++;
                    break;

                case ByteCode.PushLocal:
                    {
                        IP++;
                        offset = Code[IP];
                        IP++;
                        Stack[++RSP] = Stack[frames.Peek().bp + frames.Peek().method.ParameterNames.Length + offset];
                    }
                    break;

                case ByteCode.PushArg:
                    {
                        IP++;
                        offset = Code[IP];
                        IP++;
                        Stack[++RSP] = Stack[frames.Peek().bp + offset + 1];
                    }
                    break;

                case ByteCode.LoadGlobal:
                        IP++;
                        offset = Code[IP];
                        IP++;
                        Stack[++RSP] = Mem[offset];
                    break;

                case ByteCode.StoreGlobal:
                        IP++;
                        offset = Code[IP];
                        IP++;
                        Mem[offset] = Stack[RSP];
                        DeStack();
                    break;

                case ByteCode.Add:
                    IP++;
                    Stack[RSP - 1] = Stack[RSP - 1] + Stack[RSP];
                    DeStack();
                    break;

                case ByteCode.Sub:
                    IP++;
                    Stack[RSP - 1] = Stack[RSP - 1] - Stack[RSP];
                    DeStack();
                    break;

                case ByteCode.Print:
                    IP++;
                    Print(Stack[RSP]);
                    DeStack();
                    break;

                case ByteCode.Dup:
                    IP++;
                    Stack[RSP + 1] = Stack[RSP];
                    RSP++;
                    break;

                case ByteCode.Halt:
                    Console.WriteLine("*** HALT ***");
                    return;

                case ByteCode.Call:
                    IP++;
                    name = StringConstantPool[Code[IP++]];
                    MethodInfo method = Methods[name];
                    frames.Push(new StackFrame(method, IP, RSP, RSP - method.ParameterNames.Length));
                    IP = Methods[name].MemoryIndex;
                    break;

                case ByteCode.Return:
                    var frame = frames.Pop();
                    IP = frame.ip;
                    RSP = frame.sp;
                    DeStack(frame.method.ParameterNames.Length);
                    break;

                case ByteCode.Jump:
                    IP = Code[++IP];
                    break;

                case ByteCode.JumpLess:
                    IP++;
                    if (RCond == (int)ConditionalFlags.FL_NEG)
                        IP = Code[IP];
                    else
                        IP++;
                    break;

                case ByteCode.Cmp:
                    IP++;
                    UpdateCond(Stack[RSP - 1] - Stack[RSP]);
                    DeStack(2);
                    break;

                case ByteCode.BranchTrue:
                    {
                        IP++;
                        var cond = Stack[RSP];
                        DeStack();
                        if (cond == 1)
                            IP = Code[IP];
                        else
                            IP++;
                    }
                    break;

                case ByteCode.BranchFalse:
                    {
                        IP++;
                        var cond = Stack[RSP];
                        DeStack();
                        if (cond == 0)
                            IP = Code[IP];
                        else
                            IP++;
                    }
                    break;

                case ByteCode.Pop:
                    IP++;
                    DeStack();
                    break;

                default:
                    throw new Exception($"Do not understand instruction {instruction}");
            }

            void DeStack(int times = 1)
            {
                for (int i = 0; i < times; i++)
                {
                    Stack[RSP] = 0;
                    RSP--;
                }
                if (RSP < -1) throw new Exception("Stack pointer below -1");
            }
        }
    }

    void PrintTrace()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        string diassembly = Disassembler.Disassemble(Code, IP);
        string stackprint = string.Join(",", Stack.Take(RSP+1));
        Console.WriteLine($"{diassembly,-20} [{stackprint,-33}] sp:{RSP,2} bp:{frames.Peek().bp} cs:{frames.Count,2}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    void Print(object o)
    {
        Printed.Add(o);
        Console.WriteLine(o);
    }
}
