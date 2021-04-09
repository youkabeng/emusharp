using System;

namespace EmuSharp.Chip8
{
    class Chip8
    {
        // Chip-8 system has 4k memory
        private byte[] RAM = new byte[0x1000];

        // 16 general purpose 8-bit registers
        private byte[] V = new byte[0x0F];

        // 16-bit register to store memory addresses, only the lowest (rightmost) 12 bits are usually used
        private ushort I;

        // The program counter is used to store the currently executing address
        private ushort PC;

        // The stack pointer is used to point to the topmost level of the stack
        private byte SP;

        // The stack is used to store the address that the interpreter should return to when finished with a subroutine
        private ushort[] STACK = new ushort[0x0F];

        // Monochrome display
        private const byte displayWidth = 64;
        private const byte displayHeight = 32;
        private bool[,] DISPLAY = new bool[displayHeight, displayWidth];

        // Time register
        private byte DT;

        // Sound register
        private byte ST;

        // Random
        Random random = new Random();

        public Chip8()
        {
            PC = 0x200;
        }

        public void LoadProgram(byte[] data) => Array.Copy(data, 0, RAM, 0x200, data.Length);

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
                    DISPLAY[i, j] = false;
                }
            }
        }

        // 00EE
        // Returns from a subroutine
        void RET()
        {
            PC = pop();
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
            PC = data.NNN;
        }

        // 2NNN
        // Calls subroutine at NNN
        void callSubroutineAtNNN(Opcode data)
        {
            push(PC);
            PC = data.NNN;
        }

        // 3XNN
        // Skips next opcode if VX equals to NN
        void skipIfVXEqualsToNN(Opcode data)
        {
            if (V[data.X] == data.NN)
            {
                // skip next opcode (2 bytes)
                PC += 2;
            }
        }

        // 4XNN
        // Skips next opcode if VX doesn't equals to NN
        void skipIfVXNotEqualsToNN(Opcode data)
        {
            if (V[data.X] != data.NN)
            {
                // skip next opcode (2 bytes)
                PC += 2;
            }
        }

        // 5XY0
        // Skips next opcode if VX equals VY
        void skipIfVXEqualsToVY(Opcode data)
        {
            if (V[data.X] == V[data.Y])
            {
                // skip next opcode (2 bytes)
                PC += 2;
            }
        }

        // 6XNN
        // Sets VX to NN
        void setVXToNN(Opcode data)
        {
            V[data.X] = data.NN;
        }

        // 7XNN
        // Adds NN to VX (Carray flag is not changed)
        void addNNToVX(Opcode data)
        {
            V[data.X] += data.NN;
        }

        // 8XY0
        // Sets VX to VY
        void setVXToVY(Opcode data)
        {
            V[data.X] = V[data.Y];
        }

        // 8XY1
        // Sets VX to VX|VY
        void setVXToVXOrVY(Opcode data)
        {
            V[data.X] |= V[data.Y];
        }

        // 8XY2
        // Sets VX to VX&VY
        void setVXToVXAndVY(Opcode data)
        {
            V[data.X] &= V[data.Y];
        }

        // 8XY3
        // Sets VX to VX^VY
        void setVXToVXXorVY(Opcode data)
        {
            V[data.X] ^= V[data.Y];
        }

        // 8XY4
        // Adds VY to VX, VF is set to 1 when there's a carry and to 0 when there isn't
        void addVYToVX(Opcode data)
        {
            V[0xF] = (byte)(V[data.X] + V[data.Y] > 0xFF ? 1 : 0);
            V[data.X] += V[data.Y];
        }

        // 8XY5
        // Subtracts VY from VX, VF is set to 0 when there's a borrow and to 0 when there isn't
        void subtractVYFromVX(Opcode data)
        {
            V[0xF] = (byte)(V[data.X] > V[data.Y] ? 1 : 0);
            V[data.X] -= V[data.Y];
        }

        // 8XY6
        // If the least-significant bit of VX is 1, then VF is set to 1, otherwise 0. Then VX is divided by 2
        void rightShiftVX(Opcode data)
        {
            V[0xF] = (byte)((V[data.X] & 0x1) != 0 ? 1 : 0);
            V[data.X] >>= 1;
        } 

        // 8XY7
        // Sets VX to VY minus VX, VF is set to 0 when there's a borrow, and 1 when there isn't
        void setVXToVYMinusVX(Opcode data)
        {
            V[0xF] = (byte)(V[data.Y] > V[data.X] ? 1 : 0);
            V[data.X] = (byte)(V[data.Y] - V[data.X]);
        }

        // 8XYE
        // If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2
        void leftShiftVX(Opcode data)
        {
            V[0xF] = (byte)((V[data.X] & 0x1) != 0 ? 1 : 0);
            V[data.X] <<= 1;
        }

        // 9XY0
        // Skip next instruction if VX != VY
        void skipIfVXNotEqualsToVY(Opcode data)
        {
            if(V[data.X] != V[data.Y]) {
                PC += 2;
            }
        }

        // ANNN
        // Sets I to the address NNN
        void setIToNNN(Opcode data) {
            I = data.NNN;
        }

        // BNNN
        // Jumps to the address NNN plus V0
        void jumpToNNNPlusV0(Opcode data) 
        {
            PC = (ushort)(data.NNN + V[0]);
        }

        // CXNN
        // Sets VX to the result of a bitwise and operation on a random number (0-255) and NN
        void setVXToRandAndNN(Opcode data)
        {
            V[data.X] = (byte)(random.Next(0, 256) & data.NN);
        }

        void push(ushort value)
        {
            STACK[SP++] = value;
        }

        ushort pop()
        {
            return STACK[--SP];
        }

    }
}