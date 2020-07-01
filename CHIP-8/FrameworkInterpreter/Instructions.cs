using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace FrameworkInterpreter
{
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
            //set pc to top of stack
            intr.pc = intr.stack[intr.stack.Count - 1];
           
            //remove item at top of stack
            intr.stack.RemoveAt(intr.stack.Count - 1);
            
            //Add pc by one, as the item at the top of the stack is the call itself
            // which we want to skip to avoid an infinite loop
            
            intr.pc += 2;
        }


        //1NNN
        public static void jmp(Interpreter intr)
        {
            //set pc to nnn
            intr.pc = intr.nnn;
            Console.WriteLine($"pc = {intr.pc}");
        }


        //2NNN
        public static void call(Interpreter intr)
        {
            //add current program counter to top of stack
            intr.stack.Add(intr.pc);

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

            intr.vY_Snapshot = intr.registers[intr.y];
            intr.vX_Snapshot = intr.registers[intr.x];

            //oletusarvo on ettei collisionia tapahdu, jolloin Vx = 0
            intr.registers[15] = 0;

            //initialisoidaan byte array joka sisältää piirrettävät arvot
            byte[] toDraw = new byte[intr.n];


            //täytetään toDraw (mahdollista ohittaa tämä vaihe?)
            for (int verticalRow = 0; verticalRow < intr.n; verticalRow++)
            {
                toDraw[verticalRow] = intr.memory[intr.i + verticalRow];
            }


            //siirretään toDraw display arrayhin

            /*COPY OF DRAW FUNC IN CASE OF FUBAR
              
                

            // jokaista riviä y + yOffSet kohden
            for (int yOffset = 0; yOffset < toDraw.Length; yOffset++)
            {

                //muunnetaan rivi binaariksi ja otetaan sen string representaatio
                string stringRepr = Convert.ToString(toDraw[yOffset], 2).PadLeft(8, '0');


                //tarkistetaan, mennäänkö näytön reunan yli (mahd. väärin?)







                for (int xOffSet = 0; xOffSet < stringRepr.Length; xOffSet++)
                {
                    if (intr.vY_Snapshot + yOffset > 31)
                    {
                        intr.vY_Snapshot = 0 - yOffset;
                    }

                    if (intr.vY_Snapshot + yOffset < 0)
                    {
                        intr.vY_Snapshot = 31 - yOffset;
                    }


                    if (intr.vX_Snapshot + xOffSet > 63)
                    {
                        intr.vX_Snapshot = 0 - xOffSet;
                    }

                    if (intr.vX_Snapshot + xOffSet < 0)
                    {
                        intr.vX_Snapshot = 63;
                    }


                    if (intr.display[intr.vY_Snapshot + yOffset, intr.vX_Snapshot + xOffSet] == 1 && stringRepr[xOffSet] == '1')
                    {
                        // katsotaan tapahtuuko kolliisiota, jos näin niin Vx on 1;
                        intr.registers[15] = 1;
                    }

                    //kirjoitetaan data display-arrayhyn.
                    intr.display[intr.vY_Snapshot + yOffset, intr.vX_Snapshot + xOffSet] ^= (int)Char.GetNumericValue(stringRepr[xOffSet]);
                }
            }
                */




            // jokaista riviä y + yOffSet kohden
            for (int yOffset = 0; yOffset < toDraw.Length; yOffset++)
            {

                //muunnetaan rivi binaariksi ja otetaan sen string representaatio
                string stringRepr = Convert.ToString(toDraw[yOffset], 2).PadLeft(8, '0');


                //tarkistetaan, mennäänkö näytön reunan yli (mahd. väärin?)







                for (int xOffSet = 0; xOffSet < stringRepr.Length; xOffSet++)
                {
                    if (intr.vY_Snapshot + yOffset > 31)
                    {
                        intr.vY_Snapshot = 0 - yOffset;
                    }

                    if (intr.vY_Snapshot + yOffset < 0)
                    {
                        intr.vY_Snapshot = 31 - yOffset;
                    }


                    if (intr.vX_Snapshot + xOffSet > 63)
                    {
                        intr.vX_Snapshot = 0 - xOffSet;
                    }

                    if (intr.vX_Snapshot + xOffSet < 0)
                    {
                        intr.vX_Snapshot = 63;
                    }


                    if (intr.display[(intr.vY_Snapshot + yOffset ) * 64  + intr.vX_Snapshot + xOffSet] == 1 && stringRepr[xOffSet] == '1')
                    {
                        // katsotaan tapahtuuko kolliisiota, jos näin niin Vx on 1;
                        intr.registers[15] = 1;
                    }

                    //kirjoitetaan data display-arrayhyn.
                    intr.display[(intr.vY_Snapshot + yOffset) * 64 + intr.vX_Snapshot + xOffSet] ^= (int)Char.GetNumericValue(stringRepr[xOffSet]);
                }
            }



            intr.drawFlag = true;
            intr.pc += 2;

        }

        //EX9E
        public static void skp_vx(Interpreter intr)
        {
            if (intr.keyPress == 0)
            {
                intr.pc += 2;
                return;
            }

            //NOTE: Too many conversions, very stupid
            if (Convert.ToByte(Convert.ToInt16(Convert.ToString(Convert.ToChar(intr.keyPress)), 16)) == intr.registers[intr.x])
            {
                intr.pc += 4;
                intr.keyPress = 0;
                return;
            }

            intr.pc += 2;
            intr.keyPress = 0;
        }

        //EXA1
        public static void sknp_vx(Interpreter intr)
        {
            if (intr.keyPress == 0)
            {
                intr.pc += 2;
                return;
            }

            //NOTE: Too many conversions, very stupid
            if (Convert.ToByte(Convert.ToInt16(Convert.ToString(Convert.ToChar(intr.keyPress)), 16)) != intr.registers[intr.x])
            {
                intr.pc += 4;
                intr.keyPress = 0;
                return;
            }

            intr.pc += 2;
            intr.keyPress = 0;
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
            if (intr.keyPress == 0)
            {
                return;
            }

            //NOTE: Also too many conversions, this is dumb
            intr.registers[intr.x] = (byte)Convert.ToInt16(Convert.ToString((char)intr.keyPress), 16);

            intr.keyPress = 0;

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