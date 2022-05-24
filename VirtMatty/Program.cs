
var interpretter = new Interpretter();
interpretter.Code = new int[]
{
    (int)Instruction.PushI, 1,
    (int)Instruction.PushI, 2,
    (int)Instruction.Add,
    (int)Instruction.Print,
    (int)Instruction.Stop
};
interpretter.Start();


public enum Instruction : int
{
    PushI,
    Add,
    Print,
    Stop
}


class Interpretter
{
    int Ip = 0;
    int Sp = -1;

    public int[] Data = new int[100000];
    public int[] Code = new int[100000];
    public List<object> Printed = new List<object>();

    public void Start()
    {
        while (true)
        {

            Instruction instruction = (Instruction)Code[Ip];

            switch (instruction)
            {
                case Instruction.PushI:
                    Ip++;
                    Data[++Sp] = Code[Ip++];
                    break;

                case Instruction.Add:
                    if (Sp < 1) throw new Exception($"invalid Stack pointer {Sp}");
                    Data[Sp - 1] = Data[Sp] + Data[Sp - 1];
                    Sp--;
                    Ip++;
                    break;

                case Instruction.Print:
                    if (Sp < 0) throw new Exception($"invalid Stack pointer {Sp}");
                    Print(Data[Sp]);
                    Sp--;
                    Ip++;
                    break;

                case Instruction.Stop:
                    return;
            }
        }
    }


    public void Print(object o)
    {
        Printed.Add(o);
        Console.WriteLine(o);
    }

}