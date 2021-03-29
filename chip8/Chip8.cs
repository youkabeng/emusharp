namespace EmuSharp.Chip8

{
    class Chip8
    {
        // Chip-8 system has 4k memory
        private byte[] ram = new byte[0x1000];

        // 16 general purpose 8-bit registers
        private byte[] v = new byte[0x0F];

        // The program counter is used to store the currently executing address
        private ushort pc;

        // The stack pointer is used to point to the topmost level of the stack
        private byte sp;

        // The stack is used to store the address that the interpreter shoud return to when finished with a subroutine
        private ushort[] stack = new ushort[0x0F];

        // Monochrome display
        private const byte displayWidth = 64;
        private const byte displayHeight = 32;
        private bool[,] display = new bool[displayHeight, displayWidth];

        // time register
        private byte dt;

        // sound register
        private byte st;

        public Chip8()
        {
            pc = 0x200;
        }

        void tick()
        {
            // Fetch Opcode
            // Decode Opcode
            // Execute Opcode
            // Update timers
        }

        void Sys(ushort addr)
        {
            // ignored
        }

        void Cls()
        {
            for (var i = 0; i < displayHeight; i++)
            {
                for (var j = 0; j < displayWidth; j++)
                {
                    display[i, j] = false;
                }
            }
        }

        void Ret()
        {
            pc = Pop();
        }

        void Jp(InstructionData data)
        {
            pc = data.NNN;
        }

        void Call(InstructionData data)
        {
            Push(pc);
            pc = data.NNN;
        }

        void Push(ushort value)
        {
            stack[sp++] = value;
        }

        ushort Pop()
        {
            return stack[--sp];
        }

    }
}