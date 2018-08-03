using DavLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Disassembler
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            List<string> lines = File.ReadAllLines(@"C:\Users\David Lin\Desktop\yer\output").ToList();
            
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    lines[i] = lines[i].Replace("A" + j.ToString(), "r" + j.ToString());
                }

                int[] values = (int[])Enum.GetValues(typeof(OpCodes));
                values = values.OrderByDescending(c => c).ToArray();

                foreach (var value in values)
                {
                    string name = Enum.GetName(typeof(OpCodes), value);

                    if (lines[i].Substring(0, 3).Contains(value.ToString("X2")))
                    {
                        lines[i] = lines[i].Substring(0, 3).Replace(value.ToString("X2"), name.Trim()) + lines[i].Substring(3);
                    }
                }
            }


            File.WriteAllLines(@"C:\Users\David Lin\Desktop\yer\disassembled", lines);

            Console.WriteLine("stuff happend");
            Console.ReadKey();
        }
    }
}
