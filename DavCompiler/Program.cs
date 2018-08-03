using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavCompiler
{

    class Program
    {
        static void Main(string[] args)
        {
            List<string> lines = File.ReadAllLines(@"C:\Users\David Lin\Desktop\yer\yes.txt").ToList();

            List<Dictionary<string, int>> vars = new List<Dictionary<string, int>>();
            vars.Add(new Dictionary<string, int>());
            vars.Add(new Dictionary<string, int>());
            int currentReg = 2;
            int currentLabel = 1;
            bool precedingStatement = false;

            Stack<string> jmpStack = new Stack<string>();
            Stack<int> varStack = new Stack<int>();
           
            List<string> asm = new List<string>();
            asm.Add("set r1 00 01");
            int progMem = lines.Count;

            //remove comments and find progmem
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("PROGMEM"))
                {
                    Console.WriteLine("PROGMEM FOUND AT LINE " + i.ToString());
                }
                if (lines[i].Contains(";"))
                {
                    lines[i] = lines[i].Remove(lines[i].IndexOf(";"));
                }
                if (lines[i].Contains("//"))
                {
                    lines[i] = lines[i].Remove(lines[i].IndexOf("//"));
                }
                lines[i] = lines[i].Replace(" ", "");
                if (lines[i] == "")
                {
                    lines.RemoveAt(i);
                    i--;
                    progMem--;
                    continue;
                }
            }

            int scope = 0;
            for (int i = 0; i < progMem; i++)
            {
                for (int j = 0; j < lines[i].Count(f => f == '{'); j++)
                {
                    vars.Add(new Dictionary<string, int>());
                    Console.WriteLine("Scope = " + (++scope));
                    if (!precedingStatement)
                    {
                        jmpStack.Push("nop");
                    }
                }
                for (int j = 0; j < lines[i].Count(f => f == '}'); j++)
                {
                    vars.RemoveAt(vars.Count - 1);
                    currentReg--;

                    string label = jmpStack.Pop();
                    if (label != "nop")
                    {
                        asm.Add(label);
                    }
                    Console.WriteLine("Scope = " + (--scope).ToString());
                }

                if (lines[i].Contains("let"))
                {
                    int value = 0;
                    if (lines[i].Contains("="))
                    {
                        value = int.Parse(lines[i].Substring(lines[i].IndexOf("=") + 1));
                    }
                    string temp = lines[i].Replace("let", "");

                    Console.WriteLine("FOUND VARIABLE " + temp.Remove(temp.IndexOf("=")) + " VALUE = " + value.ToString());
                    Console.WriteLine("SETTING REGISTER " + currentReg);

                    vars[scope].Add(temp.Remove(temp.IndexOf("=")), currentReg);

                    asm.Add("set r" + currentReg.ToString() + " " + ToHex(value, 4));
                    currentReg++;
                }
                else if (lines[i].Contains("if"))
                {
                    Console.Write("Found If statement ");
                    precedingStatement = true;

                    string var1 = lines[i].Substring(3, lines[i].IndexOfAny("=><".ToCharArray()) - 3);
                    string var2 = lines[i].Substring(lines[i].LastIndexOfAny("=><".ToCharArray()) + 1, lines[i].IndexOf(")") - lines[i].LastIndexOfAny("=><".ToCharArray()) - 1);


                    int reg1 = 0;
                    int reg2 = 0;
                    for (int j = scope; j >= 0; j--)
                    {
                        if (vars[scope].ContainsKey(var1))
                        {
                            reg1 = vars[scope][var1];
                        }
                    }
                    for (int j = scope; j >= 0; j--)
                    {
                        if (vars[scope].ContainsKey(var2))
                        {
                            reg2 = vars[scope][var2];
                        }
                    }
                    if (reg1 == 0 || reg2 == 0)
                    {
                        throw new ArgumentNullException();
                    }

                    Console.Write("reg" + reg1.ToString() + " = " + var1 + " reg" + reg2 + " = " + var2);

                    if (lines[i].Contains("=="))
                    {
                        asm.Add("eql r" + currentReg + " r" + reg1.ToString() + " r" + reg2.ToString());
                        Console.WriteLine(" (==)");
                    }
                    else if (lines[i].Contains(">="))
                    {
                        asm.Add("sub r" + currentReg + " r" + reg1.ToString() + " r1");
                        asm.Add("gra r" + (currentReg + 1) + " r" + currentReg + " r" + reg2.ToString());
                        currentReg++;
                        Console.WriteLine(" (>=)");
                    }
                    else if (lines[i].Contains("<="))
                    {
                        asm.Add("add r" + currentReg + " r" + reg1.ToString() + " r1");
                        asm.Add("les r" + (currentReg + 1) + " r" + currentReg + " r" + reg2.ToString());
                        currentReg++;
                        Console.WriteLine(" (<=)");
                    }
                    else if (lines[i].Contains(">"))
                    {
                        asm.Add("gra r" + currentReg + " r" + reg1.ToString() + " r" + reg2.ToString());
                        Console.WriteLine(" (>)");
                    }
                    else
                    {
                        asm.Add("les r" + currentReg + " r" + reg1.ToString() + " r" + reg2.ToString());
                        Console.WriteLine(" (<)");
                    }
                    string labelName = "if" + (currentLabel++).ToString();
                    asm.Add("jzo r" + currentReg + " " + labelName);
                    jmpStack.Push(labelName + ":");
                }
                else if (lines[i].Contains("while"))
                {
                    precedingStatement = true;

                    asm.Add("goto whileEnd" + currentLabel);
                    asm.Add(":whileStart" + currentLabel);

                    string var1 = lines[i].Substring(6, lines[i].IndexOfAny("=><".ToCharArray()) - 6);
                    string var2 = lines[i].Substring(lines[i].LastIndexOfAny("=><".ToCharArray()) + 1, lines[i].IndexOf(")") - lines[i].LastIndexOfAny("=><".ToCharArray()) - 1);
                    
                    int reg1 = 0;
                    int reg2 = 0;
                    for (int j = scope; j >= 0; j--)
                    {
                        if (vars[scope].ContainsKey(var1))
                        {
                            reg1 = vars[scope][var1];
                        }
                    }
                    for (int j = scope; j >= 0; j--)
                    {
                        if (vars[scope].ContainsKey(var2))
                        {
                            reg2 = vars[scope][var2];
                        }
                    }
                    if (reg1 == 0 || reg2 == 0)
                    {
                        throw new ArgumentNullException();
                    }

                    Console.Write("reg" + reg1.ToString() + " = " + var1 + " reg" + reg2 + " = " + var2);

                    if (lines[i].Contains("=="))
                    {
                        asm.Add("eql r" + currentReg + " r" + reg1.ToString() + " r" + reg2.ToString());
                        Console.WriteLine(" (==)");
                    }
                    else if (lines[i].Contains(">="))
                    {
                        asm.Add("sub r" + currentReg + " r" + reg1.ToString() + " r1");
                        asm.Add("gra r" + (currentReg + 1) + " r" + currentReg + " r" + reg2.ToString());
                        currentReg++;
                        Console.WriteLine(" (>=)");
                    }
                    else if (lines[i].Contains("<="))
                    {
                        asm.Add("add r" + currentReg + " r" + reg1.ToString() + " r1");
                        asm.Add("les r" + (currentReg + 1) + " r" + currentReg + " r" + reg2.ToString());
                        currentReg++;
                        Console.WriteLine(" (<=)");
                    }
                    else if (lines[i].Contains(">"))
                    {
                        asm.Add("gra r" + currentReg + " r" + reg1.ToString() + " r" + reg2.ToString());
                        Console.WriteLine(" (>)");
                    }
                    else
                    {
                        asm.Add("les r" + currentReg + " r" + reg1.ToString() + " r" + reg2.ToString());
                        Console.WriteLine(" (<)");
                    }

                    jmpStack.Push("whileEnd" + currentLabel + ":\njnz r" + currentReg + " whileStart" + currentLabel);
                    currentLabel++;
                }
                else if(lines[i].Contains("for"))
                {
                    precedingStatement = true;

                    string temp = lines[i].Remove(0, 4);

                    string var = temp.Substring(0, temp.IndexOf(","));
                    temp = temp.Remove(0, temp.IndexOf(",") - 1);
                    int start = int.Parse(temp.Substring(0, temp.IndexOf(",")));
                    temp = temp.Remove(0, temp.IndexOf(",") - 1);
                    int finish = int.Parse(temp.Substring(0, temp.IndexOf(",")));

                    bool reverse = start < finish;

                    int reg1 = currentReg++;
                    int reg2 = currentReg++;

                    asm.Add("set r" + reg1 + " 00 00");
                    asm.Add("set r" + reg2 + " " + ToHex(finish, 4));
                    asm.Add("goto forEnd" + currentLabel);
                    asm.Add(":forStart" + currentLabel);
                    
                    if (!reverse)
                    {
                        asm.Add("gra r" + currentReg + " r" + reg1.ToString() + " r" + reg2.ToString());
                    }
                    else
                    {
                        asm.Add("les r" + currentReg + " r" + reg1.ToString() + " r" + reg2.ToString());
                    }

                    jmpStack.Push("forEnd" + currentLabel + ":\njnz r" + currentReg + " forStart" + currentLabel);
                    currentLabel++;
                }
            }


            File.WriteAllLines(@"C:\Users\David Lin\Desktop\yer\wow0", asm.ToArray());
            Console.ReadKey();
        }
        public static string ToHex(int num, int width)
        {
            string s = num.ToString("X" + width.ToString());

            var list = Enumerable
                .Range(0, s.Length / 2)
                .Select(i => s.Substring(i * 2, 2))
                .ToList();
            return string.Join(" ", list);
        }
    }
}
