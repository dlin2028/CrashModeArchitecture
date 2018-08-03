using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DavEmulator
{
    class MemoryMap
    {
        private ushort[] memory = new ushort[0x10000];
        public MemoryMap(byte[] programData)
        {
            Span<ushort> memorySpan = memory.AsSpan();
            Span<ushort> programSpace = memorySpan.Slice(0x8000);
            Span<byte> programSpaceBytes = MemoryMarshal.AsBytes(programSpace);
            programData.CopyTo(programSpaceBytes);

            // Find progmem
            Span<uint> programSpaceTemp = MemoryMarshal.Cast<ushort, uint>(programSpace);
            for(int i = 0; i < programSpaceTemp.Length; i++)
            {
                if(programSpaceTemp[i] == 0xFFFFFFFF)
                {
                    progmemLocation = i;
                }
            }

        }

        public ref ushort this[int index]
        {
            get
            {
                return ref memory[index];
            }
        }

        private ReadOnlySpan<byte> programSpace => MemoryMarshal.AsBytes(memory.AsSpan().Slice(0x8000, progmemLocation));

        private int progmemLocation;
        public ReadOnlySpan<byte> ProgMem => MemoryMarshal.AsBytes(memory.AsSpan().Slice(0x8000 + progmemLocation));

        public Span<ushort> Stack => memory.AsSpan().Slice(0x4000, 0x4000);
        public Span<ushort> RAM => memory.AsSpan().Slice(0x0080, 0x4000 - 0x0080);
        public Memory<ushort> MMIO => memory.AsMemory().Slice(0, 0x0080);

        public ReadOnlySpan<byte> GetInstructionWidth(int ip) => MemoryMarshal.AsBytes(memory.AsSpan().Slice(ip, 2));
    }
}
