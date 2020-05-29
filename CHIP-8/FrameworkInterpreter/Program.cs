using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using static RegexDefinitions.RegexDefine;


namespace FrameworkInterpreter
{   
    public class BasicVisualizer
    {
        public static void Visualize(int[,] arr)
        {
            int rowLength = arr.GetLength(0);
            int colLength = arr.GetLength(1);

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    if (arr[i,j] == 0)
                    {
                        Console.Write("  ");
                    }
                    else
                    {
                        Console.Write("■ ");
                    }
                    //Console.Write("{0}", arr[i, j]);
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        
        }
    }

        public class Ugh
    {
        static void Main()
        {
            Interpreter intr = new Interpreter();
            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                stopwatch.Start();
                intr.Advance();
                stopwatch.Stop();

                Console.WriteLine($"execution time in ms = {stopwatch.ElapsedMilliseconds}");
                //Thread.Sleep((int)stopwatch.ElapsedMilliseconds);
                stopwatch.Reset();
            }

        }
    }

    public class Interpreter
    {

        public float delayTimer;
        public int evenCounter;
        public float soundTimer;
        public Stopwatch stopwatch;
        public byte[] memory;
        public int[,] display;
        public byte[] registers;
        public short i;
        public short pc;
        public List<short> stack;
        public bool drawFlag;
        public Random random;
        public int x;
        public int y;
        int n;
        int vX_Snapshot;
        int vY_Snapshot;
        int keyPress;
        public int vxIndex;
        public int vyIndex;
        public byte[] fontData = new byte[] { 0xF0, 0x90, 0x90, 0x90, 0xF0,
                                              0x20, 0x60, 0x20, 0x20, 0x70,
                                              0xF0, 0x10, 0xF0, 0x80, 0xF0,
                                              0xF0, 0x10, 0xF0, 0x10, 0xF0,
                                              0x90, 0x90, 0xF0, 0x10, 0x10,
                                              0xF0, 0x80, 0xF0, 0x10, 0xF0,
                                              0xF0, 0x80, 0xF0, 0x90, 0xF0,
                                              0xF0, 0x10, 0x20, 0x40, 0x40,
                                              0xF0, 0x90, 0xF0, 0x90, 0xF0,
                                              0xF0, 0x90, 0xF0, 0x10, 0xF0,
                                              0xF0, 0x90, 0xF0, 0x90, 0x90,
                                              0xE0, 0x90, 0xE0, 0x90, 0xE0,
                                              0xF0, 0x80, 0x80, 0x80, 0xF0,
                                              0xE0, 0x90, 0x90, 0x90, 0xE0,
                                              0xF0, 0x80, 0xF0, 0x80, 0xF0,
                                              0xF0, 0x80, 0xF0, 0x80, 0x80 };

        public byte[] LoadData()
        {
            byte[] memory = new byte[4096];

            for (int i = 0; i<75; i++)
            {
                memory[i] = fontData[i];
            }

            FileStream fs = new FileStream(@"C:\Users\elias\Desktop\Space Invaders.ch8", FileMode.Open);

            int hexIn;

            for (int i = 0; (hexIn = fs.ReadByte()) != -1; i++)
            {
                memory[i + 512] = (byte)hexIn;
            }



            return memory;


        }

        //this gets called whenever a key is pressed and it sets the keyPress variable to the ascii code of the key pressed
        public void GetKey(int key)
        {
            Debug.WriteLine($"Key {key} has been pressed!");
            keyPress = key;
        }

        public Interpreter()
        {
            //define everything
            this.random = new Random();
            this.memory = this.LoadData();

            this.stopwatch = new Stopwatch();


            // height x width;
            this.display = new int[32, 64];

            this.registers = new byte[16];

            this.i = 0;

            this.pc = 512;

            this.stack = new List<short>();

            this.drawFlag = false;

        }
        public void Advance()
        {
            //advance the program by one cycle


            //check if reached end of memory;
            if (pc + 2 == memory.Length)
            {
                Console.WriteLine("ERR: reached end of memory");
                return;
            }

            //parse current instruction and set it to a variable
            string currentInstruction = String.Format("{0:X2}", memory[pc]) + String.Format("{0:X2}", memory[pc + 1]);

            //parse x
            x = Convert.ToInt32(Convert.ToString(currentInstruction[1]),16);

            //parsy y
            y = Convert.ToInt32(Convert.ToString(currentInstruction[2]), 16);

            //parse n
            n = Convert.ToInt32(Convert.ToString(currentInstruction[3]), 16);



            
            Console.WriteLine($"pc={pc}");
            Console.WriteLine($"The current instruction is:{currentInstruction} ");



            //set drawflag to false by default
            drawFlag = false;

            //switch for all possible instructions
            switch (currentInstruction)
            {
                case "00E0":
                    Console.WriteLine("00E0");
                    
                    //reset display
                    display = new int[32, 64];
                    
                    //set drawFlag so monogame knows to redraw the screen
                    drawFlag = true;
                    
                    //advance pc
                    this.pc += 2;
                    break;

                case "00EE":
                    //return

                    Console.WriteLine("00EE");
                    
                    //set pc to top of stack
                    this.pc = stack[stack.Count - 1];
                    //remove item at top of stack
                  
                    stack.RemoveAt(stack.Count-1);
                    this.pc += 2;
                    break;

                case var dummy when One_addr.IsMatch(dummy):
                    Console.WriteLine("1nnn");
                   
                    //set pc to nnn
                    pc = Convert.ToInt16(currentInstruction.Substring(1, 3), 16);
                    Console.WriteLine($"pc = {pc}");
                    break;

                case var dummy when Two_addr.IsMatch(dummy):
                    Console.WriteLine("2nnn");
                    
                    //add current instruction to top of stack
                    stack.Add(pc);

                    //jump to nnn
                    pc = Convert.ToInt16(currentInstruction.Substring(1, 3), 16);
                    break;

                case var dummy when Three.IsMatch(dummy):
                    Console.WriteLine("3xkk");
                    
                    //jump one if vX is kk
                    if (registers[x] == (byte)Convert.ToInt32(currentInstruction.Substring(2, 2), 16))
                    {
                        pc += 4;
                        break;
                    }
                    pc += 2;
                    break;

                case var dummy when Four.IsMatch(dummy):
                    Console.WriteLine("4xkk");
                    if ((byte)Convert.ToInt32(currentInstruction.Substring(2, 2), 16) != registers[x])
                    {
                        pc += 4;
                        break;
                    }


                    pc += 2;
                    break;

                case var dummy when Five.IsMatch(dummy):
                    Console.WriteLine("5xy0");

                    if (registers[x] == registers[y])
                    {
                        pc += 4;
                        break;
                    }

                    pc += 2;
                    break;

                case var dummy when Six.IsMatch(dummy):
                    Console.WriteLine("6xkk");
                    registers[x] = (byte)Convert.ToInt32(currentInstruction.Substring(2, 2), 16);

                    pc += 2;
                    break;


                case var dummy when Seven.IsMatch(dummy):
                    Console.WriteLine("7xkk");
                    registers[x] += (byte)Convert.ToInt32(currentInstruction.Substring(2, 2), 16);

                    pc += 2;
                    break;


                case var dummy when Eight_load.IsMatch(dummy):
                    Console.WriteLine("8xy0");
                    registers[x] = registers[y];


                    pc += 2;
                    break;

                case var dummy when Eight_or.IsMatch(dummy):
                    Console.WriteLine("8xy1");
                    registers[x] = (byte)(registers[x] | registers[y]);

                    pc += 2;
                    break;

                case var dummy when Eight_and.IsMatch(dummy):
                    Console.WriteLine("8xy2");
                    registers[x] = (byte)(registers[x] & registers[y]);


                    pc += 2;
                    break;

                case var dummy when Eight_xor.IsMatch(dummy):
                    Console.WriteLine("8xy3");
                    registers[x] = (byte)(registers[x] ^ registers[y]);


                    pc += 2;
                    break;

                case var dummy when Eight_add.IsMatch(dummy):
                    Console.WriteLine("8xy4");
                    
                    registers[x] = (byte)(registers[x] + registers[y]);
                    if (registers[x] > 255)
                    {
                        registers[x] = (byte)(registers[x] & 255);
                        registers[15] = 1;
                    }
                    else
                    {
                        registers[15] = 0;
                    }


                    pc += 2;
                    break;

                case var dummy when Eight_sub.IsMatch(dummy):
                    Console.WriteLine("8xy5");
                    
                    if (registers[x] > registers[y])
                    {
                        registers[15] = 1;
                    }
                    else
                    {
                        registers[15] = 0;
                    }
                    registers[x] = (byte)(registers[x] - registers[y]);




                    pc += 2;
                    break;

                case var dummy when Eight_shr.IsMatch(dummy):
                    Console.WriteLine("8xy6");
                    
                    if ((registers[x] & 0x0000FFFF) == 1)
                    {
                        registers[15] = 1;
                    }
                    else
                    {
                        registers[15] = 0;
                    }
                    registers[x] = (byte)(registers[x] >> 1);
                    pc += 2;
                    break;


                case var dummy when Eight_subn.IsMatch(dummy):
                    Console.WriteLine("8xy7");
                    
                    if (registers[y] > registers[x])
                    {
                        registers[15] = 1;
                    }
                    else
                    {
                        registers[15] = 0;
                    }

                    registers[x] = (byte)(registers[y] - registers[x]);


                    pc += 2;
                    break;

                case var dummy when Eight_shl.IsMatch(dummy):
                    Console.WriteLine("8xyE");
                    
                    if (registers[x] >> 7 == 1)
                    {
                        registers[15] = 1;
                    }
                    else
                    {
                        registers[15] = 0;
                    }
                    pc += 2;
                    registers[x] = (byte)(registers[x] >> 1);

                    break;


                case var dummy when Nine.IsMatch(dummy):
                    Console.WriteLine("9xy0");

                    if (registers[x] != registers[y])
                    {
                        pc += 4;
                        break;
                    }

                    pc += 2;
                    break;


                case var dummy when A_addr.IsMatch(dummy):
                    Console.WriteLine($"Annn where nnn = {currentInstruction.Substring(1, 3)}");
                    i = Convert.ToInt16(currentInstruction.Substring(1, 3), 16);
                    Console.WriteLine($"i = {i}");
                    pc += 2;
                    break;


                case var dummy when B_addr.IsMatch(dummy):
                    Console.WriteLine("Bnnn");
                    pc = (short)(Convert.ToInt16(currentInstruction.Substring(1, 3), 16) + registers[0]);
                    break;

                case var dummy when C_addr.IsMatch(dummy):
                    Console.WriteLine("Cxkk");
                    registers[x] = (byte) (random.Next(255) & Convert.ToInt32(currentInstruction.Substring(2, 2), 16));
                    pc += 2;
                    break;


                #region draw_func

                //huomionarvoista:
                // optimoinnin vuoksi voisi olla fiksua keksiä tapa vähentää type conversioneita
                // huom. mahdolliset bugit jotka mainittu edellisissä kommenteissa

                case var dummy when D_addr.IsMatch(dummy):

                    stopwatch.Start();

                    Console.WriteLine("Dxyn");

                    vY_Snapshot = registers[y];
                    vX_Snapshot = registers[x];

                    //oletusarvo on ettei collisionia tapahdu, jolloin Vx = 0
                    registers[15] = 0;

                    //initialisoidaan byte array joka sisältää piirrettävät arvot
                    byte[] toDraw = new byte[n];


                    //täytetään toDraw (mahdollista ohittaa tämä vaihe?)
                    for (int verticalRow = 0; verticalRow < n; verticalRow++)
                    {
                        toDraw[verticalRow] = memory[i + verticalRow];
                    }


                    //siirretään toDraw display arrayhin

                    // jokaista riviä y + yOffSet kohden
                    for (int yOffset = 0; yOffset < toDraw.Length; yOffset++)
                    {

                        //muunnetaan rivi binaariksi ja otetaan sen string representaatio
                        string stringRepr = Convert.ToString(toDraw[yOffset], 2).PadLeft(8, '0');
                        

                        //tarkistetaan, mennäänkö näytön reunan yli (mahd. väärin?)

                        for (int xOffSet = 0; xOffSet < stringRepr.Length; xOffSet++)
                        {
                            if (vY_Snapshot + yOffset > 31)
                            {
                                //registers[y]:n muuttaminen suoraan nollaksi saattaa olla tyhmää ja aiheuttaa bugeja,
                                //mielummin pitäisi ottaa registers[y]:n nykyinen versio erilliseen muuttujaan,
                                //jossa sitä voi muunnella ilman että vaikutetaan rekistereiden tilaan.

                                vY_Snapshot = 0;
                            }

                            if (vX_Snapshot + xOffSet > 63)
                            {
                                //sama kuin äsköinen
                                vX_Snapshot = 0;
                            }


                            if (display[vY_Snapshot + yOffset, vX_Snapshot + xOffSet] == 1 && stringRepr[xOffSet] == '1')
                            {
                                // katsotaan tapahtuuko kolliisiota, jos näin niin Vx on 1;
                                registers[15] = 1;
                            }

                            //kirjoitetaan data display-arrayyn.
                            display[vY_Snapshot + yOffset, vX_Snapshot + xOffSet] ^= (int)Char.GetNumericValue(stringRepr[xOffSet]);
                        }
                    }
                    stopwatch.Stop();
                    Console.WriteLine($"draw function elapsed ms = {stopwatch.ElapsedMilliseconds}");
                    stopwatch.Reset();

                    BasicVisualizer.Visualize(display);


                    drawFlag = true;
                    pc += 2;
                    break;
                #endregion

                case var dummy when E_skp.IsMatch(dummy):
                    Console.WriteLine("Ex9E");
                    if (keyPress == 0)
                    {
                        pc += 2;
                        break;
                    }

                    if (Convert.ToByte(Convert.ToInt16(Convert.ToString(Convert.ToChar(keyPress)), 16)) == registers[x])
                    {
                        pc += 4;
                        break;
                    }

                    pc += 2;
                    keyPress = 0;
                    break;


                case var dummy when E_sknp.IsMatch(dummy):
                    Console.WriteLine("ExA1");

                    if (keyPress == 0)
                    {
                        pc += 2;
                        break;
                    }

                    if (Convert.ToByte(Convert.ToInt16(Convert.ToString(Convert.ToChar(keyPress)), 16)) != registers[x])
                    {
                        pc += 4;
                        break;
                    }

                    pc += 2;
                    keyPress = 0;
                    break;


                case var dummy when F_load_from_dt.IsMatch(dummy):
                    Console.WriteLine("Fx07");
                    registers[x] = (byte)Convert.ToInt32(delayTimer);
                    Debug.WriteLine($"registers[{x}] = {registers[x]}");
                    pc += 2;
                    break;


                case var dummy when F_load_key.IsMatch(dummy):
                    Console.WriteLine("Fx0A");
                    
                    //maybe try while here?
                    if (keyPress == 0)
                    {
                        break;
                    }
                    Debug.WriteLine("Keypress: {0},  keypress type: {1}", keyPress, keyPress.GetType());
                    registers[x] = (byte) Convert.ToInt16(Convert.ToString( (char) keyPress),16);

                    Debug.WriteLine($"registers[x] = {registers[x]}");
                    pc += 2;

                    keyPress = 0;
                    break;


                case var dummy when F_load_to_dt.IsMatch(dummy):
                    Console.WriteLine("Fx15");
                    delayTimer = registers[x];
                    Debug.WriteLine($"delayTimer has been set to {delayTimer}");
                    pc += 2;
                    break;


                case var dummy when F_load_to_st.IsMatch(dummy):

                    Console.WriteLine("Fx18");
                    soundTimer = registers[x];
                    pc += 2;
                    break;

                case var dummy when Add_i_vx.IsMatch(dummy):
                    Console.WriteLine("Fx1E");
                    i += registers[x];
                    pc += 2;
                    break;


                case var dummy when Load_f_vx.IsMatch(dummy):
                    Console.WriteLine("Fx29");
                    i = (short)(registers[x] * 5);
                    pc += 2;
                    break;


                case var dummy when Load_b_vx.IsMatch(dummy):
                    Console.WriteLine("Fx33");

                    //The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.

                        memory[i + 2] = (byte)(registers[x] % 10);
                        memory[i + 1] = (byte)((registers[x] % 100 - memory[i + 2])/10);
                        memory[i] = (byte)((registers[x] - memory[i + 2] - memory[i + 1]*10) / 100);




                    pc += 2;
                    break;

                case var dummy when Load_i_vx.IsMatch(dummy):
                    Console.WriteLine("Fx55");
                    for (int iter = 0; iter <= x; iter++)
                    {
                        memory[i + iter] = registers[iter];
                    }
                    pc += 2;
                    break;

                case var dummy when Load_vx_i.IsMatch(dummy):
                    Console.WriteLine("Fx65");
                    //lataa muistiosoitteesta I alkaen arvot rekistereihin

                    for (int iter = 0; iter<=x; iter++)
                    {
                        registers[iter] = memory[i+iter];
                        evenCounter += 2;
                    }
                    

                    pc += 2;
                    break;


                default:
                    Console.WriteLine("Unknown instruction");
                    pc += 2;
                    break;


            }

        }

    }        
        }





