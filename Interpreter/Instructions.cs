using System;
using System.Diagnostics;


namespace CPU
{


    /// <summary>
    /// Function that visualizes the display in the output. Useful for debugging.
    /// </summary>
    public class BasicVisualizer
    {
        public static void Visualize(int[] arr)
        {
            
            for (int y = 0; y<32; y++)
            {
                string rowString = "";

                for (int x = 0; x<64; x++)
                {
                    if (arr[y * 64 + x] == 1)
                    {
                        rowString += 'O';
                    }
                    else 
                    {
                        rowString += '.';
                    }
                }
                Console.WriteLine(rowString);
            }

            Console.WriteLine("\n\n");
            return;
        }
    }

    /// <summary>
    /// Important class that stores all the instructions that the interpreter calls
    /// </summary>
    class Instructions
    {
        //00E0
        public static void clr(Interpreter intr)
        {
            intr.drawFlag = true;
            intr.display = new int[64*32];
            intr.pc += 2;
        }


        //00EE
        public static void ret(Interpreter intr)
        {
            //pop top item of stack
            intr.pc = intr.stack.Pop();
           
            //Add pc by one, as the item at the top of the stack is the call itself
            // which we want to skip to avoid an infinite loop
            
            intr.pc += 2;
        }


        //1NNN
        public static void jmp(Interpreter intr)
        {
            //set pc to nnn
            intr.pc = intr.nnn;
            Debug.WriteLine($"pc = {intr.pc}");
        }


        //2NNN
        public static void call(Interpreter intr)
        {
            //add current program counter to top of stack
            intr.stack.Push(intr.pc);

            //jump to nnn
            intr.pc = intr.nnn;
        }


        //3XKK
        public static void skp_if_kk(Interpreter intr)
        {
            //jump one if vX is kk
            if (intr.registers[intr.x] == intr.kk)
            {
                intr.pc += 4;
                return;
            }
            intr.pc += 2;

        }


        //4XKK
        public static void skp_not_kk(Interpreter intr)
        {
            //jump one if vX is not kk
            if (intr.registers[intr.x] != intr.kk)
            {
                intr.pc += 4;
                return;
            }
            intr.pc += 2;
        }


        //5XY0
        public static void skp_if_x_y(Interpreter intr)
        {
            //jump one if vX is vY;
            if (intr.registers[intr.x] == intr.registers[intr.y])
            {
                intr.pc += 4;
                return;
            }

            intr.pc += 2;
        }


        //6XKK
        public static void ld_vx_kk(Interpreter intr)
        {
            intr.registers[intr.x] = (byte)intr.kk;

            intr.pc += 2;
        }


        //7XKK
        public static void add_vx_kk(Interpreter intr)
        {
            intr.registers[intr.x] += (byte)intr.kk;

            intr.pc += 2;

        }


        //8XY0
        public static void ld_vx_vy(Interpreter intr)
        {
            intr.registers[intr.x] = intr.registers[intr.y];

            intr.pc += 2;
        }


        //8XY1
        public static void or_vx_vy(Interpreter intr)
        {
            intr.registers[intr.x] = (byte)(intr.registers[intr.x] | intr.registers[intr.y]);

            intr.pc += 2;
        }


        //8XY2
        public static void and_vx_vy(Interpreter intr)
        {
            intr.registers[intr.x] = (byte)(intr.registers[intr.x] & intr.registers[intr.y]);

            intr.pc += 2;
        }


        //8XY3
        public static void xor_vx_vy(Interpreter intr)
        {
            intr.registers[intr.x] = (byte)(intr.registers[intr.x] ^ intr.registers[intr.y]);

            intr.pc += 2;
        }


        //8XY4
        public static void add_vx_vy(Interpreter intr)
        {

            if (intr.registers[intr.x] + intr.registers[intr.y] > 255)
            {
                //registers[x] = (byte)(registers[x] & 255);
                intr.registers[15] = 1;
            }
            else
            {
                intr.registers[15] = 0;
            }

            intr.registers[intr.x] = (byte)((intr.registers[intr.x] + intr.registers[intr.y]) & 255);

            intr.pc += 2;
        }


        //8XY5
        public static void sub_vx_vy(Interpreter intr)
        {
            intr.registers[15] = (byte)(intr.registers[intr.x] > intr.registers[intr.y] ? 1 : 0);

            intr.registers[intr.x] -= intr.registers[intr.y];


            intr.pc += 2;

        }


        //8XY6
        public static void shr_vx_vy(Interpreter intr)
        {
            //BEWARE: Confusion ahead!

            //Most modern programs run with the wrong specification, to 
            //use the original spec for older programs enable legacy mode

            if (!intr.legacyMode)
            {
                //Store the least significant digit of Vx in VF, shift Vx right by 1
                intr.registers[15] = (byte)(intr.registers[intr.x] & 1);
                intr.registers[intr.x] >>= 1;
            }

            else
            {
                //NOTE: This might also be wrong, beware when trying out legacy mode!

                //Same as above
                intr.registers[15] = (byte)(intr.registers[intr.x] & 1);

                //Shift Vy by one, load to Vx
                intr.registers[intr.x] = intr.registers[intr.y >> 1];
            }

            intr.pc += 2;

        }


        //8XY7
        public static void subn_vy_vx(Interpreter intr)
        {
            //If Vy is greater than Vx, set Vx to 1, else 0
            intr.registers[15] = (byte)(intr.registers[intr.y] > intr.registers[intr.x] ? 1 : 0);

            //Set Vx to Vy - Vx;
            intr.registers[intr.x] = (byte)(intr.registers[intr.y] - intr.registers[intr.x]);

            intr.pc += 2;
        }


        //8XYE
        public static void shl_vx_vy(Interpreter intr)
        {

            //Use bitmask with AND to check if the MSB is 1

            if ((0x80 & intr.registers[intr.x]) == 0x80)
            {
                intr.registers[15] = 1;
            }
            else
            {
                intr.registers[15] = 0;
            }


            //We need two implementations here, 'modern' and 'legacy'

            //Modern (default)
            if (!intr.legacyMode)
            {
                //Left shift registers[x] by one
                intr.registers[intr.x] <<= 1;
            }

            //Legacy (probably broken but low priority)
            else
            {
                //Store value of Vy << 1 in Vx
                intr.registers[intr.y] <<= 1;
                intr.registers[intr.x] = intr.registers[intr.y];
            }

            //R E M E M B E R  to increase pc
            intr.pc += 2;
            
        }

        //9XY0
        public static void skp_not_equal(Interpreter intr)
        {
            if (intr.registers[intr.x] != intr.registers[intr.y])
            {
                intr.pc += 4;
                return;
            }
            intr.pc += 2;
        }

        //ANNN
        public static void ld_i_nnn(Interpreter intr)
        {
            intr.i = intr.nnn;
            intr.pc += 2;
        }

        //BNNN
        public static void jmp_v0_nnn(Interpreter intr)
        {
            intr.pc = (short)(intr.nnn + intr.registers[0]);
        }

        //CXKK
        public static void ld_vx_rand(Interpreter intr)
        {
            intr.registers[intr.x] = (byte)(intr.random.Next(255) & intr.kk);
            intr.pc += 2;
        }

        //DXYN
        public static void drw(Interpreter intr)
        {
            //DONT DELETE YET
            #region deepSigh
            //make temporary copies of the values in vy and vx so we can edit them without disrupting the actual registers
            intr.spriteCoordinateY = intr.registers[intr.y];
            intr.spriteCoordinateX = intr.registers[intr.x];

            //oletusarvo on ettei collisionia tapahdu, jolloin Vx = 0
            intr.registers[15] = 0;

            //initialisoidaan byte array joka sisältää piirrettävät arvot
            byte[] drawBuffer = new byte[intr.n];


            //täytetään toDraw (mahdollista ohittaa tämä vaihe?)
            for (int verticalRow = 0; verticalRow < intr.n; verticalRow++)
            {
                drawBuffer[verticalRow] = intr.memory[intr.i + verticalRow];
            }




            // jokaista riviä y + yOffSet kohden
            for (int yOffSet = 0; yOffSet < intr.n; yOffSet++)
            {

                //muunnetaan rivi binaariksi ja otetaan sen string representaatio
                string stringRepr = Convert.ToString(drawBuffer[yOffSet], 2).PadLeft(8, '0');


                //jokaista riviä x + xOffSet kohden
                for (int xOffSet = 0; xOffSet < 8; xOffSet++)
                {

                    //Screenwrap code fubar
                    if (intr.spriteCoordinateX + xOffSet > 63 || intr.spriteCoordinateX + xOffSet < 0)
                    {
                        continue;
                    }

                    if (intr.spriteCoordinateY + yOffSet > 31)
                    {
                        intr.spriteCoordinateY = 0 - yOffSet;
                        Console.WriteLine("Went above the top of the screen, wrapping...");
                    }
                    
                    if (intr.spriteCoordinateY + yOffSet < 0 )
                    {
                        intr.spriteCoordinateY = 31 - yOffSet;
                        Console.WriteLine("Went below the bottom of the screen, wrapping...");
                    }


                    //!!
                    if (intr.spriteCoordinateX + xOffSet > 63)

                    {
                        Console.WriteLine("Went past the right edge, wrapping...");

                        intr.spriteCoordinateX = 0 - xOffSet;
                    }

                    
                    if (intr.spriteCoordinateX + xOffSet < 0)
                    {
                        Console.WriteLine("Went past the left edge, wrapping...");

                        intr.spriteCoordinateX = 63 - xOffSet;
                    }
                    

                    if (intr.display[(intr.spriteCoordinateY + yOffSet) * 64  + intr.spriteCoordinateX + xOffSet] == 1 && stringRepr[xOffSet] == '1')
                    {
                        // katsotaan tapahtuuko kolliisiota, jos näin niin Vx on 1;
                        intr.registers[15] = 1;
                    }

                    //kirjoitetaan data display-arrayhyn.
                    intr.display[(intr.spriteCoordinateY + yOffSet) * 64 + intr.spriteCoordinateX + xOffSet] ^= (int) Char.GetNumericValue(stringRepr[xOffSet]);



                    Console.WriteLine($"spriteCoordinateX={intr.spriteCoordinateX}");
                    Console.WriteLine($"spriteCoordinateY={intr.spriteCoordinateY}");

                    Console.WriteLine($"xOffSet={xOffSet}");
                    Console.WriteLine($"yOffSet={yOffSet}\n");



                    //BasicVisualizer.Visualize(intr.display);

                }


            }

            #endregion

            #region dumbNewFunction
            /*
            intr.spriteCoordinateX = intr.registers[intr.x];
            intr.spriteCoordinateY = intr.registers[intr.y];

            for (int yRow = 0; yRow<intr.n; yRow++)
            {
                intr.rowBuffer = intr.memory[intr.i + yRow];

                //is MSB set?
                if (intr.rowBuffer >= 128)
                {
                    intr.display[yRow * 64 + intr.x] ^= 1;
                }

                // is second most significant bit set?
                if (intr.rowBuffer >= 64)
                {
                    intr.display[yRow * 64 + intr.x + 1] ^= 1;
                }

                //etc, etc...
                if (intr.rowBuffer >= 32)
                {
                    intr.display[yRow * 64 + intr.x + 2] ^= 1;
                }

                if (intr.rowBuffer >= 16)
                {
                    intr.display[yRow * 64 + intr.x + 3] ^= 1;
                }

                if (intr.rowBuffer >= 8)
                {
                    intr.display[yRow * 64 + intr.x + 4] ^= 1;
                }

                if (intr.rowBuffer >= 4)
                {
                    intr.display[yRow * 64 + intr.x + 5] ^= 1;
                }

                if (intr.rowBuffer >= 2)
                {
                    intr.display[yRow * 64 + intr.x + 6] ^= 1;
                }

                if (intr.rowBuffer >= 1)
                {
                    intr.display[yRow * 64 + intr.x + 7] ^= 1;
                }



            }*/
            #endregion





            intr.drawFlag = true;
            intr.pc += 2;

        }

        //EX9E
        public static void skp_vx(Interpreter intr)
        {

            //If nothing/the wrong key is pressed at the moment, we just continue on            
            if (intr.keyPress != intr.registers[intr.x])
            {
                intr.pc += 2;
                return;
            }

            //If the key being pressed is in the register vX
            else
            {
                intr.pc += 4;
       
                return;
            }

            

            
        }

        //EXA1
        public static void sknp_vx(Interpreter intr)
        {

            if (intr.keyPress != intr.registers[intr.x])
            {
                intr.pc += 4;
                return;
            }
            
            
            intr.pc += 2;
        }

        //FX07
        public static void ld_vx_dt(Interpreter intr)
        {
            intr.registers[intr.x] = (byte)Convert.ToInt32(intr.delayTimer);
            intr.pc += 2;
        }

        //FX0A
        public static void ld_vx_key(Interpreter intr)
        {
            
            //if no key was released, ignore it *without* incrementing the pc so the operation will stall here until a key is pressed
            if (intr.keyRelease == -1)
            {
                return;
            }

            
            intr.registers[intr.x] = (byte)intr.keyRelease;
            Debug.WriteLine($"The key { (char)intr.keyRelease} has been released and will be registered (from FX0A)");
           
            

            intr.pc += 2;

        }

        //FX15
        public static void ld_dt_vx(Interpreter intr)
        {
            intr.delayTimer = intr.registers[intr.x];
            intr.pc += 2;
        }

        //FX18
        public static void ld_st_vx(Interpreter intr)
        {
            intr.soundTimer = intr.registers[intr.x];
            intr.pc += 2;
        }

        //FX1E
        public static void add_i_vx(Interpreter intr)
        {
            intr.i += intr.registers[intr.x];
            intr.pc += 2;
        }

        //FX29
        public static void ld_f_vx(Interpreter intr)
        {
            intr.i = (short)(intr.registers[intr.x] * 5);
            intr.pc += 2;
        }

        //FX33
        public static void ld_bcd(Interpreter intr)
        {
            //The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.

            intr.memory[intr.i + 2] = (byte)(intr.registers[intr.x] % 10);
            intr.memory[intr.i + 1] = (byte)((intr.registers[intr.x] % 100 - intr.memory[intr.i + 2]) / 10);
            intr.memory[intr.i] = (byte)((intr.registers[intr.x] - intr.memory[intr.i + 2] - intr.memory[intr.i + 1] * 10) / 100);

            intr.pc += 2;
        }

        //FX55
        public static void ld_i_vx(Interpreter intr)
        {
            //siirrä rekisterien arvot muistiin alkaen osoitteesta i

            for (int iter = 0; iter <= intr.x; iter++)
            {
                intr.memory[intr.i + iter] = intr.registers[iter];
            }

            intr.pc += 2;
        }

        //FX65
        public static void ld_vx_i(Interpreter intr)
        {
            //talleta rekistereihin arvot alkaen muistiosoitteesta i

            for (int iter = 0; iter <= intr.x; iter++)
            {
                intr.registers[iter] = intr.memory[intr.i + iter];
            }

            intr.pc += 2;
        }
    }
}