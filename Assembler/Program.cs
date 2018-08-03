using DavLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assembler
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> lines = File.ReadAllLines(@"C:\Users\David Lin\Desktop\asm\screen.txt").ToList();

            Dictionary<string, int> labels = new Dictionary<string, int>();

            int progMem = lines.Count;
            //remove comments extra lines and spaces
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("PROGMEM"))
                {
                    Console.WriteLine("PROGMEM FOUND AT LINE " + i.ToString());
                    lines[i] = "FFFFFFFF";
                    progMem = i;
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

            //progmem time
            for (int i = progMem; i < lines.Count; i++)
            {
                if (lines[i].Contains(":"))
                {
                    int location = 0;
                    for (int j = 0; j < i; j++)
                    {
                        location += lines[j].Length;
                    }

                    Console.WriteLine("PROGMEM LABEL " + lines[i].Substring(0, lines[i].IndexOf(":")) + " FOUND AT BYTE " + location);

                    labels.Add(lines[i].Substring(0, lines[i].IndexOf(":")), location);
                    lines[i] = lines[i].Remove(0, lines[i].IndexOf(":") + 1);
                }
            }

            //add dem labels
            for (int i = 0; i < progMem; i++)
            {
                if (lines[i].Contains(":"))
                {
                    Console.WriteLine("LABEL " + lines[i].Replace(':', ' ') + " FOUND AT LINE " + (i * 2).ToString());
                    labels.Add(lines[i].Replace(':', ' '), i * 2);
                    lines[i] = "00000000";
                }
            }
            //replace them with lines[i] numbers
            for (int i = 0; i < progMem; i++)
            {
                foreach (string name in labels.Keys)
                {
                    lines[i] = lines[i].Replace(name.Trim(), labels[name].ToString("X4"));
                }
            }
            //replace reg with memory value
            for (int i = 0; i < progMem; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    lines[i] = lines[i].Replace("r" + j.ToString(), (0xA0 + j).ToString("X"));
                }
                int[] values = (int[])Enum.GetValues(typeof(OpCodes));
                values = values.OrderByDescending(c => c).ToArray();
                foreach (var value in values)
                {
                    string name = Enum.GetName(typeof(OpCodes), value);
                    lines[i] = lines[i].Replace(name, value.ToString("X2"));
                }
            }
            //add padding and spacing

            for (int i = 0; i < progMem; i++)
            {
                if (lines[i].Length < 8)
                {
                    lines[i] = lines[i].Insert(2, "00");
                }
            }
            
            string allLines = "";
            foreach (var line in lines)
            {
                allLines += line;
            }

            byte[] output = Enumerable.Range(0, allLines.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(allLines.Substring(x, 2), 16))
                     .ToArray();
            
            File.WriteAllBytes(@"C:\Users\David Lin\Desktop\yer\output", output.ToArray());
           
            for (int i = 0; i < progMem; i++)
            {
                lines[i] = lines[i].Insert(2, " ");
                lines[i] = lines[i].Insert(5, " ");
                lines[i] = lines[i].Insert(8, " ");
            }
            File.WriteAllLines(@"C:\Users\David Lin\Desktop\yer\output2", lines.ToArray());

            Console.WriteLine("stuff happend");
            Console.ReadKey();
        }
    }
}
