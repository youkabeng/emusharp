using System;
using System.Collections.Generic;
using System.Linq;

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
        private ushort PC = 0x200;

        // The stack pointer is used to point to the topmost level of the stack
        private byte SP;

        // The stack is used to store the address that the interpreter should return to when finished with a subroutine
        private ushort[] STACK = new ushort[0x0F];

        // Monochrome display
        private const byte DISPLAY_WIDTH = 64;
        private const byte DISPLAY_HEIGHT = 32;
        private bool[,] DISPLAY = new bool[DISPLAY_HEIGHT, DISPLAY_WIDTH];
        private bool needRedraw = false;

        // Time register
        private byte DT;

        // Sound register
        private byte ST;

        // Random
        Random random = new Random();

        // Keys that are currently pressed
        HashSet<byte> pressedKeys = new HashSet<byte>();

        // Opcode to method
        Dictionary<byte, Action<Opcode>> opMappings;

        public Chip8()
        {
            opMappings = new Dictionary<byte, Action<Opcode>>()
            {
                {0x0, executeOpcode0},
                {0x1, jumpToNNN},
                {0x2, callSubroutineAtNNN},
                {0x3, skipIfVXEqualsToNN},
                {0x4, skipIfVXNotEqualsToNN},
                {0x5, skipIfVXEqualsToVY},
                {0x6, setVXToNN},
                {0x7, addNNToVX},
                {0x8, executeOpcode8},
                {0x9, skipIfVXNotEqualsToVY},
                {(byte)0xA, setIToNNN},
                {(byte)0xB, jumpToNNNPlusV0},
                {(byte)0xC, setVXToRandAndNN},
                {(byte)0xD, drawSpriteAtVXVY},
                {(byte)0xE, waitForKey},
                {(byte)0xF, executeOpcodeF},
            };
        }

        public void LoadProgram(byte[] data) => Array.Copy(data, 0, RAM, 0x200, data.Length);

        public void Tick()
        {
            // Fetch Opcode
            // Read the two bytes of opcode (big endian)
            var opcode = (ushort)(RAM[PC++] << 8 | RAM[PC++]);
            // Decode Opcode
            var op = new Opcode()
            {
                code = opcode,
                NNN = (ushort)(opcode & 0x0FFF),
                NN = (byte)(opcode & 0x00FF),
                N = (byte)(opcode & 0x000F),
                X = (byte)((opcode & 0x0F00) >> 8),
                Y = (byte)((opcode & 0x00F0) >> 4),
            };
        }

        void execute(Opcode data)
        {
            var n = (byte)(data.code >> 12);
            opMappings[n](data);
        }

        void executeOpcode0(Opcode data)
        {
            if (data.code == 0xE0)
            {
                clearScreen();
            }
            else if (data.code == 0xEE)
            {
                ret();
            }
            else
            {
                callMachineRoutineAtNNN(data.NNN);
            }
        }

        void executeOpcode8(Opcode data)
        {
            if (data.N == 0x0)
            {
                setVXToVY(data);
            }
            else if (data.N == 0x1)
            {
                setVXToVXOrVY(data);
            }
            else if (data.N == 0x2)
            {
                setVXToVXAndVY(data);
            }
            else if (data.N == 0x3)
            {
                setVXToVXXorVY(data);
            }
            else if (data.N == 0x4)
            {
                addVYToVX(data);
            }
            else if (data.N == 0x5)
            {
                subtractVYFromVX(data);
            }
            else if (data.N == 0x6)
            {
                rightShiftVX(data);
            }
            else if (data.N == 0x7)
            {
                setVXToVYSubtractVX(data);
            }
            else if (data.N == 0xE)
            {
                leftShiftVX(data);
            }
        }

        void executeOpcodeF(Opcode data)
        {
            if (data.NN == 0x29)
            {
                setIToCharacterVX(data);
            }
            else if (data.NN == 0x33)
            {
                storeBinaryCodedDecimal(data);
            }
            else if (data.NN == 0x55)
            {
                saveVXToRAM(data);
            }
            else if (data.NN == 0x65)
            {
                loadVXFromRAM(data);
            }
        }

        // 00E0
        // Clears the screen
        void clearScreen()
        {
            for (var i = 0; i < DISPLAY_HEIGHT; i++)
            {
                for (var j = 0; j < DISPLAY_WIDTH; j++)
                {
                    DISPLAY[i, j] = false;
                }
            }
        }

        // 00EE
        // Returns from a subroutine
        void ret()
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
        // Sets VX to VY subtract VX, VF is set to 0 when there's a borrow, and 1 when there isn't
        void setVXToVYSubtractVX(Opcode data)
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
            if (V[data.X] != V[data.Y])
            {
                PC += 2;
            }
        }

        // ANNN
        // Sets I to the address NNN
        void setIToNNN(Opcode data)
        {
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

        // DXYN
        // Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N+1 pixels.
        // Each row of 8 pixels is read as bit-coded starting from memory location I; I value does not change after the execution of this instruction.
        // As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that does not happen
        void drawSpriteAtVXVY(Opcode data)
        {
            var startX = V[data.X];
            var startY = V[data.Y];
            V[0xF] = 0;
            for (var i = 0; i < data.N; i++)
            {
                var spriteLine = RAM[I + i];
                for (var bit = 0; bit < 8; bit++)
                {
                    var x = (startX + bit) % DISPLAY_WIDTH;
                    var y = (startY + i) % DISPLAY_HEIGHT;

                    var spriteBit = ((spriteLine >> (7 - bit)) & 1);
                    var oldBit = DISPLAY[x, y] ? 1 : 0;
                    if (spriteBit != oldBit)
                        needRedraw = true;

                    var newBit = oldBit ^ spriteBit;

                    DISPLAY[x, y] = newBit != 0;

                    if (oldBit != 0 && newBit == 0)
                        V[0xF] = 1;
                }
            }
        }

        // EX9E
        // Skips the next instruction if the key stored in VX is pressed.
        void skipIfKeyIsPressed(Opcode data)
        {
            if (pressedKeys.Contains(V[data.X]))
                PC += 2;
        }

        // EXA1
        // Skips the next instruction if the key stored in VX is not pressed.
        void skipIfKeyIsNotPressed(Opcode data)
        {
            if (!pressedKeys.Contains(V[data.X]))
                PC += 2;
        }

        // FX07
        // Sets VX to the value of delay timer
        void setVXToDelayTimer(Opcode data)
        {
            V[data.X] = DT;
        }

        // FX0A
        // A key press is awaited, and then stores in VX. (Blocking Operation. All instruction halted until next key event)
        void waitForKey(Opcode data)
        {
            if (pressedKeys.Count != 0)
            {
                V[data.X] = pressedKeys.First();
            }
            else
            {
                // if not meet the condition, go back to last instruction and wait again.
                PC -= 2;
            }
        }

        // FX15
        // Sets the delay timer to VX.
        void setDelayTimerToVX(Opcode data)
        {
            DT = V[data.X];
        }

        // FX18
        // Sets the sound timer to VX.
        void setSoundTimerToVX(Opcode data)
        {
            ST = V[data.X];
        }

        // FX1E
        // Adds VX to I. VF is not affected
        void addVXToI(Opcode data)
        {
            I += V[data.X];
        }

        // FX29
        // Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
        void setIToCharacterVX(Opcode data)
        {
            I = (ushort)(V[data.X] * 5);
        }

        // FX33
        // Stores the binary-coded decimal representation of VX
        void storeBinaryCodedDecimal(Opcode data)
        {
            RAM[I + 0] = (byte)((V[data.X] / 100) % 10);
            RAM[I + 1] = (byte)((V[data.X] / 10) % 10);
            RAM[I + 2] = (byte)(V[data.X] % 10);
        }

        // FX55
        // Stores V0 to VX (including VX) in memory starting at address I. The offset from I is increased by 1 for each value written, but I itself is left unmodified
        void saveVXToRAM(Opcode data)
        {
            for (var i = 0; i <= data.X; i++)
                RAM[I + i] = V[i];
        }

        // FX65
        // Fills V0 to VX (including VX) with values from memory starting at address I. The offset from I is increased by 1 for each value written, but I itself is left unmodified
        void loadVXFromRAM(Opcode data)
        {
            for (var i = 0; i <= data.X; i++)
                V[i] = RAM[I + i];
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