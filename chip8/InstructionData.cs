namespace EmuSharp.Chip8
{
    class InstructionData
    {
        // instruction code
        public ushort code;
        // A 12-bit value, the lowest 12 bits of the instruction
        public ushort NNN;
        // A 4-bit value, the lowest 4 bits of the instruction
        public byte N;
        // A 4-bit value, the lower 4 bits of the high byte of the instruction
        public byte X;
        // A 4-bit value, the upper 4 bits of the low byte of the instruction
        public byte Y;
        // An 8-bit value, the lowest 8 bits of the instruction
        public byte KK;
    }
}