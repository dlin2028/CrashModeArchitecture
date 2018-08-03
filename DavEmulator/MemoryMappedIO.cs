using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavEmulator
{
    class MemoryMappedIO
    {
        private Memory<ushort> memoryMapSpace;

        public ushort Random
        {
            get
            {
                return memoryMapSpace.Span[0];
            }
            set
            {
                memoryMapSpace.Span[0] = value;
            }
        }
        public ushort WriteIntFlag
        {
            get
            {
                return memoryMapSpace.Span[1];
            }
            set
            {
                memoryMapSpace.Span[1] = value;
            }
        }
        public ushort WriteInt
        {
            get
            {
                return memoryMapSpace.Span[2];
            }
            set
            {
                memoryMapSpace.Span[2] = value;
            }
        }
        public ushort WriteChar
        {
            get
            {
                return memoryMapSpace.Span[3];
            }
            set
            {
                memoryMapSpace.Span[3] = value;
            }
        }
        public ushort WriteCharFlag
        {
            get
            {
                return memoryMapSpace.Span[4];
            }
            set
            {
                memoryMapSpace.Span[4] = value;
            }
        }
        public ushort ReadShortFlag
        {
            get
            {
                return memoryMapSpace.Span[5];
            }
            set
            {
                memoryMapSpace.Span[5] = value;
            }
        }
        public ushort ReadShort
        {
            get
            {
                return memoryMapSpace.Span[6];
            }
            set
            {
                memoryMapSpace.Span[6] = value;
            }
        }
        public ushort ReadChar
        {
            get
            {
                return memoryMapSpace.Span[7];
            }
            set
            {
                memoryMapSpace.Span[7] = value;
            }
        }
        public ushort ReadCharFlag
        {
            get
            {
                return memoryMapSpace.Span[8];
            }
            set
            {
                memoryMapSpace.Span[8] = value;
            }
        }
        public ushort InputType
        {
            get
            {
                return memoryMapSpace.Span[9];
            }
            set
            {
                memoryMapSpace.Span[9] = value;
            }
        }
        public ushort Screen(int x, int y)
        {
            return memoryMapSpace.Span[10 + x + y * 10];
        }

        public MemoryMappedIO(Memory<ushort> mapSpace)
        {
            memoryMapSpace = mapSpace;

            memoryMapSpace.Span[1] = 0;
        }
    }
}
