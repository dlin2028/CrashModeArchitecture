using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DavEmulator
{
    class Registers
    {
        private ushort[] memory;
        public Span<byte> Bytes => MemoryMarshal.AsBytes(memory.AsSpan());
        
        public ref ushort this[int index]
        {
            get
            {
                return ref memory[index - 0xA0];
            }
        }

        public Registers()
        {
            memory = new ushort[32];
        }
    }
}
