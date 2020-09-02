using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using static RegexDefinitions.RegexDefine;


namespace CPU
{

    #region TextInterpreter

    /// <summary>
    /// Serves as main if execution starts from program.cs
    /// mostly useful for debugging purposes, as it prints out 
    /// a fair bit of data about whatever is going on 
    /// </summary>

    public class TextBasedInterpreter
    {
        static void Main(string[] args)
        {
            Interpreter intr;

            if (args.Length != 0)
            {
                intr = new Interpreter(args[0]);
            }
            else
            {
                intr = new Interpreter(@"C:\Users\elias\Desktop\c8_test.ch8");
            }
            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                stopwatch.Start();
                intr.Advance();
                stopwatch.Stop();

                Console.WriteLine($"execution time in ms = {stopwatch.ElapsedMilliseconds}");

                stopwatch.Reset();
            }

        }
    }
    #endregion



    /// <summary>
    /// Most important class of the program, this is the interpreter itself and it's responsible for the parsing
    /// and evaluation of code
    /// </summary>
    public class Interpreter
    {

        public float delayTimer;
        public float soundTimer;
        public Stopwatch stopwatch;
        public byte[] memory;
        public int[] display;
        public byte[] registers;
        public short i;
        public short pc;
        public Stack<short> stack;
        public bool drawFlag;
        public Random random;
        public short x;
        public short y;
        public short n;
        public short kk;
        public string rom;
        public short nnn;
        public string currentInstruction;
        public int spriteCoordinateX;
        public int spriteCoordinateY;
        public int keyPress;
        public int keyRelease;
        public int prevKey;
        public bool debugDraw;
        public int rowBuffer;


        /// <summary>
        /// usedInstructions stores a list of all instructions used in the current program, useful for debugging
        /// </summary>
        public List<string> usedInstructions = new List<string>();

        /// <summary>
        /// buttonMap provides a mapping between the raw ASCII for the key pressed 
        /// and the hexadecimal input value the emulator requires. It can be adjusted to the user's preferences.
        /// </summary>
        
        public Dictionary<int, int> buttonMap = new Dictionary<int, int>
        {
            {-1,-1 },
            { 48,0 },
            { 49,1 },
            { 50,2 },
            { 51,3 },
            { 52,4 },
            { 53,5 },
            { 54,6 },
            { 55,7 },
            { 56,8 },
            { 57,9 },
            { 65,0x0A },
            { 66,0x0B },
            { 67,0x0C },
            { 68,0x0D },
            { 69,0x0E },
            { 70, 0x0F }

        };

        public int vxIndex;
        public bool legacyMode = false;
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

        public Interpreter(string rom= @"C:\Users\elias\Desktop\Clock Program [Bill Fisher, 1981].ch8", bool debugDraw=false)
        {
            this.rom = rom;

            this.debugDraw = debugDraw;

            //define everything
            random = new Random();
            memory = LoadData();

            stopwatch = new Stopwatch();
            
            // remember, height x width;
            display = new int[32 * 64];


            registers = new byte[16];

            i = 0;

            /// <summary>
            /// Store the memory address of the current location of the program
            /// </summary>
            pc = 512;

            /// <summary>
            /// Stores the full call stack, used for calling and returning.
            /// </summary>
            stack = new Stack<short>(16);
            
            Debug.WriteLine($"\nNow running: {rom}");


        }

        public byte[] LoadData()
        {
            //Initialize memory
            byte[] memory = new byte[4096];

            for (int i = 0; i<80; i++)
            {
                memory[i] = fontData[i];
            }


            //Start filestream for the ROM to load
            FileStream fs = new FileStream(rom, FileMode.Open);


            //Load the ROM to memory through the file
            int hexIn;

            for (int i = 0; (hexIn = fs.ReadByte()) != -1; i++)
            {
                memory[i + 512] = (byte)hexIn;
            }

            return memory;


        }

        /// <summary>
        /// This gets called whenever a key is pressed/released and it sets the keyPress/keyRelease variable to the ASCII code of the key pressed/released
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        public void GetKey(int key, bool state)
        {
            Debug.WriteLine($"GetKey called with parameters key={key} and state={state}");

            
            //if pressed key not in keymap, ignore it.
            if (!buttonMap.ContainsKey(key))
            {
                Debug.WriteLine("Keypress not found in keymap, ignoring...");
                return;
            }




            //If the keypress that game1.cs sent is -1, we set keyPress to -1 to indicate that nothing is being pressed right now
            if (key == -1 && state)
            {
                keyPress = -1;
                Debug.WriteLine($"keyPress is now {keyPress}\nkeyRelease is now {keyRelease}");
                return;
            }

            //If the keyrelease that game1.cs sent is -1, we set keyRelease to -1 to indicate that nothing is being released right now
            if (key == -1 && !state)
            {
                keyRelease = -1;
                Debug.WriteLine($"keyPress is now {keyPress}\nkeyRelease is now {keyRelease}");
                return;
            }


            //if keystate is down

            if (state)
            {
                Debug.WriteLine($"Key {key} has been pressed [from Interpreter.GetKey()]!");

                //if the pressed key is in the keymap we set keyPress to it
                if (buttonMap.ContainsKey(key))
                {
                    keyPress = buttonMap[key];
                }

            }

            //if keystate is up
            else
            {
                Debug.WriteLine($"Key {key} has been released [from Interpreter.GetKey()]!");
                
                //if the released key is in the keymap we set keyRelease to it
                if (buttonMap.ContainsKey(key))
                {
                    keyRelease = buttonMap[key];
                }
            }


            Debug.WriteLine($"keyPress is now {keyPress}\nkeyRelease is now {keyRelease}");

        }


        public void Advance()
        {
            //advance the program by one cycle


            //check if reached end of memory;
            if (pc >= 4095)
            {
                Console.WriteLine("ERR: reached end of memory");
            }
            else if (currentInstruction == "0000")
            {
                Console.WriteLine("ERR: empty opcode");
            }


            //parse current instruction and set it to a variable
            currentInstruction = String.Format("{0:X2}", memory[pc]) + String.Format("{0:X2}", memory[pc + 1]);


            //parse x
            x = Convert.ToInt16(Convert.ToString(currentInstruction[1]),16);

            //parsy y
            y = Convert.ToInt16(Convert.ToString(currentInstruction[2]), 16);

            //parse n
            n = Convert.ToInt16(Convert.ToString(currentInstruction[3]), 16);

            //parse kk
            kk = Convert.ToInt16(currentInstruction.Substring(2), 16);

            //parse nnn
            nnn = Convert.ToInt16(currentInstruction.Substring(1), 16);


            Console.WriteLine($"\npc={pc}");
            Console.WriteLine($"The current instruction is:{currentInstruction} ");



            //set drawflag to false by default
            drawFlag = false;

            //switch for all possible instructions
            //TODO: replace this
            switch (currentInstruction)
            {
                case "00E0":
                    Console.WriteLine("00E0");

                    if(!usedInstructions.Contains("00E0"))
                    {
                        usedInstructions.Add("00E0");
                    }

                    Instructions.clr(this);
                    return;


                case "00EE":

                    Console.WriteLine("00EE");

                    if (!usedInstructions.Contains("00EE"))
                    {
                        usedInstructions.Add("00EE");
                    }


                    Instructions.ret(this);
                    return;


                case var dummy when One_addr.IsMatch(dummy):
                    Console.WriteLine("1nnn");
                    if (!usedInstructions.Contains("1nnn"))
                    {
                        usedInstructions.Add("1nnn");
                    }


                    Instructions.jmp(this);
                   
                    return;


                case var dummy when Two_addr.IsMatch(dummy):
                    Console.WriteLine("2nnn");

                    if (!usedInstructions.Contains("2nnn"))
                    {
                        usedInstructions.Add("2nnn");
                    }

                    Instructions.call(this);

                    return;


                case var dummy when Three.IsMatch(dummy):
                    Console.WriteLine("3xkk");

                    if (!usedInstructions.Contains("3xkk"))
                    {
                        usedInstructions.Add("3xkk");
                    }


                    Instructions.skp_if_kk(this);
                    
                    return;


                case var dummy when Four.IsMatch(dummy):
                    Console.WriteLine("4xkk");

                    if (!usedInstructions.Contains("4xkk"))
                    {
                        usedInstructions.Add("4xkk");
                    }


                    Instructions.skp_not_kk(this);
                    
                    return;


                case var dummy when Five.IsMatch(dummy):
                    Console.WriteLine("5xy0");

                    if (!usedInstructions.Contains("5xy0"))
                    {
                        usedInstructions.Add("5xy0");
                    }


                    Instructions.skp_if_x_y(this);

                    return;


                case var dummy when Six.IsMatch(dummy):
                    Console.WriteLine("6xkk");

                    if (!usedInstructions.Contains("6xkk"))
                    {
                        usedInstructions.Add("6xkk");
                    }


                    Instructions.ld_vx_kk(this);
                    
                    return;


                case var dummy when Seven.IsMatch(dummy):
                    Console.WriteLine("7xkk");

                    if (!usedInstructions.Contains("7xkk"))
                    {
                        usedInstructions.Add("7xkk");
                    }


                    Instructions.add_vx_kk(this);
                    
                    return;


                case var dummy when Eight_load.IsMatch(dummy):
                    Console.WriteLine("8xy0");

                    if (!usedInstructions.Contains("8xy0"))
                    {
                        usedInstructions.Add("8xy0");
                    }


                    Instructions.ld_vx_vy(this);

                    return;


                case var dummy when Eight_or.IsMatch(dummy):
                    Console.WriteLine("8xy1");

                    if (!usedInstructions.Contains("8xy1"))
                    {
                        usedInstructions.Add("8xy1");
                    }



                    Instructions.or_vx_vy(this);
                    
                    return;


                case var dummy when Eight_and.IsMatch(dummy):
                    Console.WriteLine("8xy2");

                    if (!usedInstructions.Contains("8xy2"))
                    {
                        usedInstructions.Add("8xy2");
                    }



                    Instructions.and_vx_vy(this);

                    return;


                case var dummy when Eight_xor.IsMatch(dummy):
                    Console.WriteLine("8xy3");

                    if (!usedInstructions.Contains("8xy3"))
                    {
                        usedInstructions.Add("8xy3");
                    }



                    Instructions.xor_vx_vy(this);

                    return;


                case var dummy when Eight_add.IsMatch(dummy):
                    Console.WriteLine("8xy4");

                    if (!usedInstructions.Contains("8xy4"))
                    {
                        usedInstructions.Add("8xy4");
                    }



                    Instructions.add_vx_vy(this);

                    return;


                case var dummy when Eight_sub.IsMatch(dummy):
                    Console.WriteLine("8xy5");

                    if (!usedInstructions.Contains("8xy5"))
                    {
                        usedInstructions.Add("8xy5");
                    }



                    Instructions.sub_vx_vy(this);

                    return;


                case var dummy when Eight_shr.IsMatch(dummy):
                    Console.WriteLine("8xy6");


                    if (!usedInstructions.Contains("8xy6"))
                    {
                        usedInstructions.Add("8xy6");
                    }



                    Instructions.shr_vx_vy(this);
                
                    return;


                case var dummy when Eight_subn.IsMatch(dummy):
                    Console.WriteLine("8xy7");
                    Instructions.subn_vy_vx(this);
                    
                    return;


                case var dummy when Eight_shl.IsMatch(dummy):
                    Console.WriteLine("8xyE");

                    if (!usedInstructions.Contains("8xyE"))
                    {
                        usedInstructions.Add("8xyE");
                    }



                    Instructions.shl_vx_vy(this);
                    
                    return;


                case var dummy when Nine.IsMatch(dummy):
                    Console.WriteLine("9xy0");

                    if (!usedInstructions.Contains("9xy0"))
                    {
                        usedInstructions.Add("9xy0");
                    }



                    Instructions.skp_not_equal(this);
                    
                    return;


                case var dummy when A_addr.IsMatch(dummy):

                    if (!usedInstructions.Contains("Annn"))
                    {
                        usedInstructions.Add("Annn");
                    }



                    Console.WriteLine($"Annn where nnn = {currentInstruction.Substring(1, 3)}");
                    Instructions.ld_i_nnn(this);

                    return;


                case var dummy when B_addr.IsMatch(dummy):
                    Console.WriteLine("Bnnn");

                    if (!usedInstructions.Contains("Bnnn"))
                    {
                        usedInstructions.Add("Bnnn");
                    }


                    Instructions.jmp_v0_nnn(this);
                    
                    return;


                case var dummy when C_addr.IsMatch(dummy):
                    Console.WriteLine("Cxkk");

                    if (!usedInstructions.Contains("Cxkk"))
                    {
                        usedInstructions.Add("Cxkk");
                    }


                    Instructions.ld_vx_rand(this);
                    
                    return;


                #region draw_func

                //huomionarvoista:
                // optimoinnin vuoksi voisi olla fiksua keksiä tapa vähentää type conversioneita
                // huom. mahdolliset bugit jotka mainittu edellisissä kommenteissa

                case var dummy when D_addr.IsMatch(dummy):

                    stopwatch.Start();

                    Console.WriteLine("Dxyn");

                    if (!usedInstructions.Contains("Dxyn"))
                    {
                        usedInstructions.Add("Dxyn");
                    }


                    Instructions.drw(this);

                    stopwatch.Stop();
                    Console.WriteLine($"draw function elapsed ms = {stopwatch.ElapsedMilliseconds}");
                    stopwatch.Reset();


                    break;
                #endregion

                case var dummy when E_skp.IsMatch(dummy):
                    Console.WriteLine("Ex9E");

                    if (!usedInstructions.Contains("Ex9E"))
                    {
                        usedInstructions.Add("Ex9E");
                    }


                    Instructions.skp_vx(this);

                    return;


                case var dummy when E_sknp.IsMatch(dummy):
                    Console.WriteLine("ExA1");

                    if (!usedInstructions.Contains("ExA1"))
                    {
                        usedInstructions.Add("ExA1");
                    }



                    Instructions.sknp_vx(this);

                    return;


                case var dummy when F_load_from_dt.IsMatch(dummy):
                    Console.WriteLine("Fx07");

                    if (!usedInstructions.Contains("Fx07"))
                    {
                        usedInstructions.Add("Fx07");
                    }


                    Instructions.ld_vx_dt(this);
                    
                    Debug.WriteLine($"registers[{x}] = {registers[x]}");
                    
                    return;


                case var dummy when F_load_key.IsMatch(dummy):
                    Console.WriteLine("Fx0A");

                    if (!usedInstructions.Contains("Fx0A"))
                    {
                        usedInstructions.Add("Fx0");
                    }


                    Instructions.ld_vx_key(this);

                    return;


                case var dummy when F_load_to_dt.IsMatch(dummy):
                    Console.WriteLine("Fx15");

                    if (!usedInstructions.Contains("Fx15"))
                    {
                        usedInstructions.Add("Fx15");
                    }


                    Instructions.ld_dt_vx(this);

                    Debug.WriteLine($"delayTimer has been set to {delayTimer}");

                    return;


                case var dummy when F_load_to_st.IsMatch(dummy):
                    Console.WriteLine("Fx18");

                    if (!usedInstructions.Contains("Annn"))
                    {
                        usedInstructions.Add("Annn");
                    }


                    Instructions.ld_st_vx(this);

                    return;

                case var dummy when Add_i_vx.IsMatch(dummy):
                    Console.WriteLine("Fx1E");

                    if (!usedInstructions.Contains("Fx1E"))
                    {
                        usedInstructions.Add("Fx1E");
                    }


                    Instructions.add_i_vx(this);

                    return;


                case var dummy when Load_f_vx.IsMatch(dummy):
                    Console.WriteLine("Fx29");

                    if (!usedInstructions.Contains("Fx29"))
                    {
                        usedInstructions.Add("Fx29");
                    }


                    Instructions.ld_f_vx(this);
          
                    return;


                case var dummy when Load_b_vx.IsMatch(dummy):
                    Console.WriteLine("Fx33");

                    if (!usedInstructions.Contains("Fx33"))
                    {
                        usedInstructions.Add("Fx33");
                    }


                    Instructions.ld_bcd(this);

                    return;

                case var dummy when Load_i_vx.IsMatch(dummy):
                    Console.WriteLine("Fx55");

                    if (!usedInstructions.Contains("Fx55"))
                    {
                        usedInstructions.Add("Fx55");
                    }


                    Instructions.ld_i_vx(this);

                    return;

                case var dummy when Load_vx_i.IsMatch(dummy):
                    Console.WriteLine("Fx65");

                    if (!usedInstructions.Contains("Fx65"))
                    {
                        usedInstructions.Add("Fx65");
                    }



                    Instructions.ld_vx_i(this);

                    return;


                default:
                    Console.WriteLine("Unknown instruction");
                    pc += 2;
                    return;


            }
        }
    }        
}


