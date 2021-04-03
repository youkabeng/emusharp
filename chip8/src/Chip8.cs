using System;

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

        // The stack is used to store the address that the interpreter should return to when finished with a subroutine
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

        public void LoadProgram(byte[] data) => Array.Copy(data, 0, ram, 0x200, data.Length);

        public void Tick()
        {
            // Fetch Opcode
            // Decode Opcode
            // Execute Opcode
            // Update timers
        }

        // 00E0
        // Clears the screen
        void CLS()
        {
            for (var i = 0; i < displayHeight; i++)
            {
                for (var j = 0; j < displayWidth; j++)
                {
                    display[i, j] = false;
                }
            }
        }

        // 00EE
        // Returns from a subroutine
        void RET()
        {
            pc = pop();
        }

        // 0NNN
        // Calls machine code routine at NNN
        void callMachineRoutineAtNNN(ushort addr)
        {
            // ignored
        }

        // 1NNN
        // Jumps to address NNN
        void jumpToNNN(Opcode data)
        {
            pc = data.NNN;
        }

        // 2NNN
        // Calls subroutine at NNN
        void callSubroutineAtNNN(Opcode data)
        {
            push(pc);
            pc = data.NNN;
        }

        // 3XNN
        // Skips next opcode if VX equals to NN
        void skipIfVXEqualsToNN(Opcode data)
        {
            if (v[data.X] == data.NN)
            {
                // skip next opcode (2 bytes)
                pc += 2;
            }
        }

        // 4XNN
        // Skips next opcode if VX doesn't equals to NN
        void skipIfVXNotEqualsToNN(Opcode data)
        {
            if (v[data.X] != data.NN)
            {
                // skip next opcode (2 bytes)
                pc += 2;
            }
        }

        // 5XY0
        // Skips next opcode if VX equals VY
        void skipIfVXEqualsToVY(Opcode data)
        {
            if (v[data.X] == v[data.Y])
            {
                // skip next opcode (2 bytes)
                pc += 2;
            }
        }

        // 6XNN
        // Sets VX to NN
        void setVXToNN(Opcode data)
        {
            v[data.X] = data.NN;
        }

        // 7XNN
        // Adds NN to VX (Carray flag is not changed)
        void addNNToVX(Opcode data)
        {
            v[data.X] += data.NN;
        }

        // 8XY0
        // Sets VX to VY
        void setVXToVY(Opcode data)
        {
            v[data.X] = v[data.Y];
        }

        // 8XY1
        // Sets VX to VX|VY
        void setVXToVXOrVY(Opcode data)
        {
            v[data.X] |= v[data.Y];
        }

        // 8XY2
        // Sets VX to VX&VY
        void setVXToVXAndVY(Opcode data)
        {
            v[data.X] &= v[data.Y];
        }

        // 8XY3
        // Sets VX to VX^VY
        void setVXToVXXorVY(Opcode data)
        {
            v[data.X] ^= v[data.Y];
        }

        // 8XY4
        // Adds VY to VX, VF is set to 1 when there's a carry and to 0 when there isn't
        void addVYToVX(Opcode data)
        {
            v[0xF] = (byte)(v[data.X] + v[data.Y] > 0xFF ? 1 : 0);
            v[data.X] += v[data.Y];
        }

        // 8XY5
        // Subtracts VY from VX, VF is set to 0 when there's a borrow and to 0 when there isn't
        void subtractVYFromVX(Opcode data)
        {
            v[0xF] = (byte)(v[data.X] > v[data.Y] ? 1 : 0);
            v[data.X] -= v[data.Y];
        }

        void push(ushort value)
        {
            stack[sp++] = value;
        }

        ushort pop()
        {
            return stack[--sp];
        }

    }
}