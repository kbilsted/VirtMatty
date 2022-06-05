
var interpretter = new Interpretter();
int fooPtr = interpretter.AddString("foo");
int add5Ptr = interpretter.AddString("Add5");
int arg0Ptr = interpretter.AddString("arg0");
interpretter.AddMethod("Main", new string[0], new int[]
{
    Instruction.PushI, 1,
    Instruction.PushI, 2,
    Instruction.Add,
    Instruction.Print,
    // call
    Instruction.MethodCall, fooPtr,
    // call
    Instruction.PushI, 4, // arg 0
    Instruction.MethodCall, add5Ptr,
    Instruction.Stop
});
interpretter.AddMethod("foo", new string[0], new[] {
    Instruction.PushI, 3,
    Instruction.PushI, 5,
    Instruction.Add,
    Instruction.Print,
    Instruction.MethodReturn
});
interpretter.AddMethod("Add5", new[] { "arg0" }, new[] {
    Instruction.PushI, 5,
    Instruction.PushLocalVar, arg0Ptr,
    Instruction.Add,
    Instruction.Print,
    Instruction.Pop,
    Instruction.MethodReturn
});
interpretter.Start();


public static class Instruction
{
    public const int PushI = 0;
    public const int PushLocalVar = 1;
    public const int Pop = 2;
    public const int Add = 4;
    public const int Dup = 8;
    public const int Print = 16;
    public const int MethodCall = 32;
    public const int MethodReturn = 64;
    public const int Stop = 128;

    public static string ToString(int instruction)
    {
        if (instruction == 0) return "PushI";
        if (instruction == 1) return "PushLocalVar";
        if (instruction == 2) return "Pop";
        if (instruction == 4) return "Add";
        if (instruction == 8) return "Dup";
        if (instruction == 16) return "Print";
        if (instruction == 32) return "MethodCall";
        if (instruction == 64) return "MethodReturn";
        if (instruction == 128) return "Stop";
        throw new NotImplementedException($"instruction {instruction}");
    }
}




public record MethodInfo(string Name, string[] ParameterNames, int MemoryIndex);

public record StackFrame(MethodInfo method, int ip, int sp, int bp);

class Interpretter
{
    int Ip = 0;
    int Sp = -1;
    int Bp = 0;

    int codeIndex = 0;
    public int[] Stack = new int[100000];
    public Stack<StackFrame> frames = new Stack<StackFrame>();
    //public List<MemoryCell> Data = new List<MemoryCell>();
    public int[] Mem = new int[100000];
    public int[] Code = new int[100000];
    public List<string> StringConstantPool = new List<string>();
    Dictionary<string, MethodInfo> Methods = new Dictionary<string, MethodInfo>();
    public void AddMethod(string name, string[] parameterNames, int[] code)
    {
        Methods.Add(name, new MethodInfo(name, parameterNames, codeIndex));

        for (int i = 0; i < code.Length; i++)
        {
            Code[codeIndex] = code[i];
            codeIndex++;
        }
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

    public List<object> Printed = new List<object>();

    public void Start()
    {
        Ip = Methods["Main"].MemoryIndex;

        while (true)
        {
            string name;
            int instruction = Code[Ip];
            Console.WriteLine($"* {Instruction.ToString(instruction),-10} ip:{Ip,3} sp:{Sp,3} :: {string.Join(",", Stack.Take(5))}");

            switch (instruction)
            {
                case Instruction.PushI:
                    Ip++;
                    Stack[++Sp] = Code[Ip];
                    Ip++;
                    break;

                case Instruction.PushLocalVar:
                    Ip++;
                    name = StringConstantPool[Code[Ip]];
                    Ip++;
                    break;

                case Instruction.Add:
                    if (Sp < 1) throw new Exception($"invalid Stack pointer {Sp}");
                    Stack[Sp - 1] = Stack[Sp] + Stack[Sp - 1];
                    DeStack();
                    Ip++;
                    break;

                case Instruction.Print:
                    if (Sp < 0) throw new Exception($"invalid Stack pointer {Sp}");
                    Print(Stack[Sp]);
                    DeStack();
                    Ip++;
                    break;

                case Instruction.Dup:
                    Stack[Sp + 1] = Stack[Sp];
                    Sp++;
                    Ip++;
                    break;

                case Instruction.Stop:
                    Console.WriteLine($"stack: {Sp} instP: {Ip}");
                    return;

                case Instruction.MethodCall:
                    Ip++;
                    name = StringConstantPool[Code[Ip++]];
                    MethodInfo method = Methods[name];
                    frames.Push(new StackFrame(method, Ip, Sp, Sp - method.ParameterNames.Length));
                    Ip = Methods[name].MemoryIndex;
                    break;

                case Instruction.MethodReturn:
                    Ip = Stack[Sp];
                    DeStack();
                    break;

                case Instruction.Pop:
                    DeStack();
                    Ip++;
                    break;

                default:
                    throw new Exception($"Do not understand instruction {instruction}");
            }

            void DeStack()
            {
                Stack[Sp] = 0;
                Sp--;
            }
        }
    }


    public void Print(object o)
    {
        Printed.Add(o);
        Console.WriteLine(o);
    }

}