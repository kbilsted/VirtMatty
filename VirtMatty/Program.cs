
var interpretter = new Interpretter();
int fooPtr = interpretter.AddString("foo");
interpretter.AddMethod("Main", new int[]
{
    (int)Instruction.PushI, 1,
    (int)Instruction.PushI, 2,
    (int)Instruction.Add,
    (int)Instruction.Dup,
    (int)Instruction.Print,
    (int)Instruction.MethodCall, fooPtr,
    (int)Instruction.Stop
});
interpretter.AddMethod("foo", new[] { 
    (int)Instruction.PushI, 3, 
    (int)Instruction.PushI, 5,
    (int)Instruction.Add,
    (int)Instruction.Print,
    (int)Instruction.MethodReturn
});
interpretter.Start();


public enum Instruction : int
{
    PushI,
    Add,
    Dup,
    Print,
    MethodCall,
    MethodReturn,
    Stop
}


class Interpretter
{
    int Ip = 0;
    int Sp = -1;

    int codeIndex = 0;
    public int[] Stack = new int[100000];
    public int[] Mem = new int[100000];
    public int[] Code = new int[100000];
    public List<string> StringConstantPool = new List<string>();
    Dictionary<string, int> Methods = new Dictionary<string, int>();
    public void AddMethod(string name, int[] code)
    {
        Methods.Add(name, codeIndex);
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
        Ip = Methods["Main"];

        while (true)
        {
            Instruction instruction = (Instruction)Code[Ip];

            switch (instruction)
            {
                case Instruction.PushI:
                    Ip++;
                    Stack[++Sp] = Code[Ip];
                    Ip++;
                    break;

                case Instruction.Add:
                    if (Sp < 1) throw new Exception($"invalid Stack pointer {Sp}");
                    Stack[Sp - 1] = Stack[Sp] + Stack[Sp - 1];
                    Sp--;
                    Ip++;
                    break;

                case Instruction.Print:
                    if (Sp < 0) throw new Exception($"invalid Stack pointer {Sp}");
                    Print(Stack[Sp]);
                    Sp--;
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
                    Stack[++Sp] = Ip;
                    Ip = Methods[name];
                    break;


                case Instruction.MethodReturn:
                    Ip =Stack[Sp--] ;
                    break;

                default:
                    throw new Exception($"Do not understand instruction {instruction}");
            }
        }
    }


    public void Print(object o)
    {
        Printed.Add(o);
        Console.WriteLine(o);
    }

}