using DavLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DavEmulator
{
    class Program
    {
        public static ushort instructionPointer = 0;
        public static ushort stackPointer = 0;
        public static Registers Reggies;
        public static MemoryMap MemoryMap;
        public static MemoryMappedIO MemoryMappedIO;
        public static int ProgMem;

        static void Main(string[] args)
        {
            Reggies = new Registers();
            ProgMem = -1;
            byte[] program = File.ReadAllBytes(@"C:\Users\David Lin\Desktop\yer\output");
            
            MemoryMap = new MemoryMap(program);

            MemoryMappedIO = new MemoryMappedIO(MemoryMap.MMIO);


            Random rng = new Random();

            instructionPointer = 0x8000;
            bool isFailing = false;

            string output = "";

            ThreadStart job = new ThreadStart(DrawScreen);
            Thread thread = new Thread(job);
            thread.Start();

            while (true)
            {
                try
                {
                    //Console.WriteLine("Line " + (instructionPointer - 0x8000) / 2 + " Instruction number " + (instructionPointer - 0x8000).ToString());
                    ReadOnlySpan<byte> currentInstruction = MemoryMap.GetInstructionWidth(instructionPointer);

                    if (instructionPointer % 2 != 0)
                    {
                        //output += "YOUR INSTRUCTION POINTER IS MISALIGNED, BAD STUFF IS ABOUT TO GO DOWN \n";
                        isFailing = true;
                    }

                    OpCodes opCode = (OpCodes)currentInstruction[0];
                    if (Actions.ContainsKey(opCode))
                    {

                        Action<byte, byte, byte> action = Actions[opCode];

                        byte a = currentInstruction[1];
                        byte b = currentInstruction[2];
                        byte c = currentInstruction[3];
                        //Console.WriteLine("Operation: " + opCode);
                        //Console.WriteLine("Hex: " + ((int)opCode).ToString("X2") + " " + ((int)a).ToString("X2") + " " + ((int)b).ToString("X2") + " " + ((int)c).ToString("X2"));

                        instructionPointer += 2;
                        action.Invoke(a, b, c);
                    }
                    else
                    {
                        //output += "INVALID INSTRUCTION, YOU INCOMPETENT IMBECILE!\n";
                        //output += ((int)opCode).ToString("X2") + " IS NOT AN INSTRUCTION!!!!!!!!!";
                        isFailing = true;
                    }
                    

                    MemoryMappedIO.Random = (ushort)rng.Next(0, ushort.MaxValue);

                    if (MemoryMappedIO.WriteCharFlag != 0)
                    {
                        MemoryMappedIO.WriteCharFlag = 0;
                        Console.WriteLine("OUTPUT: " + (char)MemoryMappedIO.WriteChar);
                        Console.ReadKey();
                    }
                    if (MemoryMappedIO.WriteIntFlag != 0)
                    {
                        MemoryMappedIO.WriteIntFlag = 0;
                        Console.WriteLine("OUTPUT: " + MemoryMappedIO.WriteInt);
                        Console.ReadKey();
                    }


                    if (Console.KeyAvailable)
                    {
                        if (MemoryMappedIO.InputType == 1)
                        {
                            output += "input: ushort";
                            MemoryMappedIO.ReadShort = ushort.Parse(Console.ReadKey().KeyChar.ToString());
                            MemoryMappedIO.ReadShortFlag = 1;
                        }
                        else if(MemoryMappedIO.InputType == 2)
                        {
                            output += "input: char";
                            MemoryMappedIO.ReadChar = Console.ReadKey().KeyChar;
                            MemoryMappedIO.ReadCharFlag = 1;
                        }
                    }
                    output += "-----------------------------------------";
                    //Console.WriteLine(output);

                    output = "";
                    
                    if (isFailing)
                    {
                        Console.ReadKey();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error line " + instructionPointer / 4 + " Instruction number  " + instructionPointer.ToString() + "\nError:" + e.ToString());
                    Console.ReadKey();
                }
            }
        }

        static void DrawScreen()
        {
            while(true)
            {
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        Console.BackgroundColor = (ConsoleColor)(MemoryMappedIO.Screen(i, j));
                        Console.Write("   ");
                    }
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine();
                }
                Console.ForegroundColor = ConsoleColor.White;
                Thread.Sleep(50);
                Console.Clear();
            }
        }

        static unsafe Dictionary<OpCodes, Action<byte, byte, byte>> Actions = new Dictionary<OpCodes, Action<byte, byte, byte>>()
        {
            [OpCodes.nop] = (a, b, c) => { },
            [OpCodes.jmp] = (a, b, c) => { instructionPointer = (ushort)(0x8000 + c + (b << 8)); },
            [OpCodes.jnz] = (a, b, c) => { if (Reggies[a] != 0) instructionPointer = (ushort)(0x8000 + c + (b << 8)); },
            [OpCodes.jzo] = (a, b, c) => { if (Reggies[a] == 0) instructionPointer = (ushort)(0x8000 + c + (b << 8)); },
            [OpCodes.ret] = (a, b, c) => { instructionPointer = Reggies[0xA0]; },
            [OpCodes.call] = (a, b, c) => { Reggies[0xA0] = (ushort)(instructionPointer + 2); instructionPointer = (ushort)(c + (b << 8)); },
            [OpCodes.or] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] | Reggies[c]); },
            [OpCodes.xor] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] ^ Reggies[c]); },
            [OpCodes.and] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] & Reggies[c]); },
            [OpCodes.not] = (a, b, c) => { Reggies[a] = (ushort)(~Reggies[c]); },
            [OpCodes.add] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] + Reggies[c]); },
            [OpCodes.sub] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] - Reggies[c]); },
            [OpCodes.mul] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] * Reggies[c]); },
            [OpCodes.mod] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] % Reggies[c]); },
            [OpCodes.lsh] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] << Reggies[c]); },
            [OpCodes.rsh] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] >> Reggies[c]); },
            [OpCodes.eql] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] == Reggies[c] ? 1 : 0); },
            [OpCodes.gra] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] > Reggies[c] ? 1 : 0); },
            [OpCodes.les] = (a, b, c) => { Reggies[a] = (ushort)(Reggies[b] < Reggies[c] ? 1 : 0); },
            [OpCodes.psh] = (a, b, c) => { MemoryMap.Stack[stackPointer++] = (byte)(Reggies[c]); MemoryMap.Stack[stackPointer++] = (byte)(Reggies[c] >> 8); },
            [OpCodes.pop] = (a, b, c) => { Reggies[c] = MemoryMap.Stack[stackPointer];  stackPointer -= 2; },
            [OpCodes.lod] = (a, b, c) => { Reggies[a] = MemoryMap[(ushort)(c + (b << 8))]; },
            [OpCodes.sto] = (a, b, c) => { MemoryMap[c + (b << 8)] = Reggies[a]; },
            [OpCodes.mov] = (a, b, c) => { Reggies[b] = Reggies[c]; },
            [OpCodes.set] = (a, b, c) => { Reggies[a] = (ushort)(c + (b << 8)); },
            [OpCodes.lodi] = (a, b, c) => { Reggies[b] = MemoryMap[Reggies[c]]; },
            [OpCodes.stoi] = (a, b, c) => { MemoryMap[Reggies[b]] = Reggies[c]; },
            [OpCodes.jmpi] = (a, b, c) => { instructionPointer = MemoryMap[Reggies[c]]; },
            [OpCodes.jzoi] = (a, b, c) => { if (Reggies[a] == 0 && Reggies[b] == 0) instructionPointer = MemoryMap[Reggies[c]]; },
            [OpCodes.jnzi] = (a, b, c) => { if (Reggies[a] != 0 && Reggies[b] != 0) instructionPointer = MemoryMap[Reggies[c]]; },
            [OpCodes.cali] = (a, b, c) => { Reggies[0xA0] = (ushort)(instructionPointer + 2); instructionPointer = MemoryMap[Reggies[c]]; },
            [OpCodes.pgm] = (a, b, c) => { Reggies[a] = (MemoryMap.ProgMem[c + (b << 8)]); }
        };
    }
}
