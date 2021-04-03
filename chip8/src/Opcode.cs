namespace EmuSharp.Chip8
{
    class Opcode
    {
        // opcode
        public ushort code;
        // A 12-bit value, the lowest 12 bits of the opcode 
        public ushort NNN;
        // A 8-bit value, the lowest 8 bits of the opcode
        public byte NN;
        // A 4-bit value, the lowest 4 bits of the opcode
        public byte N;
        // A 4-bit value, the lower 4 bits of the high byte of the opcode
        public byte X;
        // A 4-bit value, the upper 4 bits of the low byte of the opcode
        public byte Y;
    }
}