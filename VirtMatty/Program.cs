
var interpretter = new Interpretter();
int fooPtr = interpretter.AddString("foo");
int add5Ptr = interpretter.AddString("Add5");
int arg0Ptr = interpretter.AddString("arg0");
interpretter.AddMethod("Main", 0, new int[]
{
    (int)Instruction.PushI, 1,
    (int)Instruction.PushI, 2,
    (int)Instruction.Add,
    (int)Instruction.Print,
    // call
    (int)Instruction.MethodCall, fooPtr,
    // call
    (int)Instruction.PushI, 4, // arg 0
    (int)Instruction.MethodCall, add5Ptr,
    (int)Instruction.Stop
});
interpretter.AddMethod("foo", 0, new[] {
    (int)Instruction.PushI, 3,
    (int)Instruction.PushI, 5,
    (int)Instruction.Add,
    (int)Instruction.Print,
    (int)Instruction.MethodReturn
});
interpretter.AddMethod("Add5", ], new[] {
    (int)Instruction.PushI, 5,
    (int)Instruction.Add,
    (int)Instruction.Print,
    (int)Instruction.Pop,
    (int)Instruction.MethodReturn
});
interpretter.Start();


public enum Instruction : int
{
    PushI,
    PushLocalVar,
    Pop,
    Add,
    Dup,
    Print,
    MethodCall,
    MethodReturn,
    Stop
}



public record  MethodInfo(string Name, string[] parameterNames, int MemoryIndex);


class Interpretter
{
    int Ip = 0;
    int Sp = -1;
    int Bp = 0;

    int codeIndex = 0;
    public int[] Stack = new int[100000];
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
        for (int i = 0; i <  StringConstantPool.Count; i++)
        {
            if (StringConstantPool[i] == s)
                return i;
        }

        StringConstantPool.Add(s);
        return StringConstantPool.Count- 1;
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
            Instruction instruction = (Instruction)Code[Ip];
            Console.WriteLine($"* {instruction,-10} ip:{Ip,3} sp:{Sp,3} :: {string.Join(",",Stack.Take(5))}");

            switch (instruction)
            {
                case Instruction.PushI:
                    Ip++;
                    Stack[++Sp] = Code[Ip];
                    Ip++;
                    break;

                case Instruction.PushLocalVar:
                    Ip++;
                   ....find navn... 
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
                    var name = StringConstantPool[Code[Ip++]];

                    for (int x = 0; x < Methods[name].NumberOfParameters; x++)
                        Stack[Sp - x + 1] = Stack[Sp - x];
                    Sp++;
                    Stack[Sp-Methods[name].NumberOfParameters] = Ip;
                    
                    Ip = Methods[name].MemoryIndex;
                    break;

                case Instruction.MethodReturn:
                    Ip =Stack[Sp];
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