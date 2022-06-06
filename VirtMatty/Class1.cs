using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtMatty.Vm
{
    public class Registers
    {
        public const int R0 = 0;
        public const int R1 = 1;
        public const int R2 = 2;
        public const int R3 = 3;
        public const int R4 = 4;
        public const int R5 = 5;
        public const int R6 = 6;
        public const int R7 = 7;
        public const int R8 = 8;
        public const int PC = 9; /* program counter */
        public const int COND = 10;

        public int[] Rx = new int[12];

        public void SetConditional(ConditionalFlags f) => Rx[COND] = (int)f;

        public void UpdateConditional(int register)
        {
            if (Rx[register] == 0)
            {
                Rx[COND] = (int)ConditionalFlags.FL_ZRO;
            }
            else if (Rx[register] >> 15 == 1) /* a 1 in the left-most bit indicates negative */
            {
                Rx[COND] = (int)ConditionalFlags.FL_NEG;
            }
            else
            {
                Rx[COND] = (int)ConditionalFlags.FL_POS;
            }
        }
        public void SetPc(int position) => Rx[PC] = position;
        public void IncPc() { Rx[PC]++; }

        public int GetPcPP => Rx[PC]++;

    }

    class Vm
    {
        int[] Memory = new int[32768];
        Registers registers = new Registers();

        public void Run(int[] opCodes)
        {
            registers.SetConditional(ConditionalFlags.FL_ZRO);
            registers.SetPc(0);

            bool running = true;
            while (running)
            {
                OpCode op = (OpCode)Memory[registers.GetPcPP];
                switch (op)
                {
                    case OpCode.OP_ADD: OP_Add.Execute(registers, Memory); break;
                    case OpCode.OP_AND: OP_And.Execute(registers, Memory); break;
                    case OpCode.OP_NOT: OP_Not.Execute(registers, Memory); break;
                    case OpCode.OP_LD: OP_LD.Execute(registers, Memory); break;
                    default: throw new ArgumentException("Invalid opcode");
                };
            }
        }


    }

    public class OP_Add
    {
        public int Destination, Source1, Source2;
        public void Write(List<int> stream)
        {
            stream.AddRange(new[] { (int)OpCode.OP_ADD, Destination, Source1, Source2 });
        }

        public static void Execute(Registers r, int[] memory)
        {
            r.IncPc();
            int Destination = memory[r.GetPcPP];
            int Source1 = memory[r.GetPcPP];
            int Source2 = memory[r.GetPcPP];

            r.Rx[Destination] = r.Rx[Source1] + r.Rx[Source2];
            r.UpdateConditional(Destination);
        }
    }


    public class OP_And
    {
        public int Destination, Source1, Source2;
        public void Write(List<int> stream)
        {
            stream.AddRange(new[] { (int)OpCode.OP_AND, Destination, Source1, Source2 });
        }

        public static void Execute(Registers r, int[] memory)
        {
            r.IncPc();
            int Destination = memory[r.GetPcPP];
            int Source1 = memory[r.GetPcPP];
            int Source2 = memory[r.GetPcPP];

            r.Rx[Destination] = r.Rx[Source1] & r.Rx[Source2];
            r.UpdateConditional(Destination);
        }
    }

    public class OP_Not
    {
        public int Destination, Source1;
        public void Write(List<int> stream)
        {
            stream.AddRange(new[] { (int)OpCode.OP_NOT, Destination, Source1 });
        }

        public static void Execute(Registers r, int[] memory)
        {
            r.IncPc();
            int Destination = memory[r.GetPcPP];
            int Source1 = memory[r.GetPcPP];

            r.Rx[Destination] = ~r.Rx[Source1];
            r.UpdateConditional(Destination);
        }
    }

    public class OP_LD
    {
        public int Destination, Address;
        public void Write(List<int> stream)
        {
            stream.AddRange(new[] { (int)OpCode.OP_LD, Destination, Address });
        }

        public static void Execute(Registers r, int[] memory)
        {
            r.IncPc();
            int Destination = memory[r.GetPcPP];
            int Address = memory[r.GetPcPP];

            r.Rx[Destination] = memory[Address];
            r.UpdateConditional(Destination);
        }
    }


    public enum OpCode
    {
        OP_BR = 0, /* branch */
        OP_ADD,    /* add  */
        OP_LD,     /* load */
        OP_ST,     /* store */
        OP_JSR,    /* jump register */
        OP_AND,    /* bitwise and */
        OP_LDR,    /* load register */
        OP_STR,    /* store register */
        OP_RTI,    /* unused */
        OP_NOT,    /* bitwise not */
        OP_LDI,    /* load indirect */
        OP_STI,    /* store indirect */
        OP_JMP,    /* jump */
        OP_RES,    /* reserved (unused) */
        OP_LEA,    /* load effective address */
        OP_TRAP    /* execute trap */
    };


    public enum ConditionalFlags
    {
        FL_POS = 1 << 0, /* P */
        FL_ZRO = 1 << 1, /* Z */
        FL_NEG = 1 << 2, /* N */
    };

}
