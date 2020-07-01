using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using static RegexDefinitions.RegexDefine;

namespace FrameworkInterpreter
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
        public List<short> stack;
        public bool drawFlag;
        public Random random;
        public short x;
        public short y;
        public short n;
        public short kk;
        public string rom;
        public short nnn;
        public string currentInstruction;
        public int vX_Snapshot;
        public int vY_Snapshot;
        public int keyPress;
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

        public Interpreter(string rom= @"C:\Users\elias\Desktop\Clock Program [Bill Fisher, 1981].ch8")
        {
            this.rom = rom;

            //define everything
            random = new Random();
            memory = LoadData();

            stopwatch = new Stopwatch();

            // height x width;
            display = new int[32 * 64];


            registers = new byte[16];

            i = 0;

            pc = 512;

            stack = new List<short>();

            drawFlag = false;


            //legacyMode = true;

        }

        public byte[] LoadData()
        {
            byte[] memory = new byte[4096];

            for (int i = 0; i<75; i++)
            {
                memory[i] = fontData[i];
            }

            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            FileStream fs = new FileStream(this.rom, FileMode.Open);

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
            if (key != 0)
            {
                Debug.WriteLine($"Key {key} has been pressed!");
                keyPress = key;
            }

        }


        public void Advance()
        {
            //advance the program by one cycle


            //check if reached end of memory;
            if (pc + 2 >= memory.Length)
            {
                Console.WriteLine("ERR: reached end of memory");
                return;
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

                    Instructions.clr(this);
                    return;


                case "00EE":

                    Console.WriteLine("00EE");
                    Instructions.ret(this);
                    return;


                case var dummy when One_addr.IsMatch(dummy):
                    Console.WriteLine("1nnn");
                    Instructions.jmp(this);
                   
                    return;


                case var dummy when Two_addr.IsMatch(dummy):
                    Console.WriteLine("2nnn");
                    Instructions.call(this);

                    return;


                case var dummy when Three.IsMatch(dummy):
                    Console.WriteLine("3xkk");
                    Instructions.skp_if_kk(this);
                    
                    return;


                case var dummy when Four.IsMatch(dummy):
                    Console.WriteLine("4xkk");
                    Instructions.skp_not_kk(this);
                    
                    return;


                case var dummy when Five.IsMatch(dummy):
                    Console.WriteLine("5xy0");
                    Instructions.skp_if_x_y(this);

                    return;


                case var dummy when Six.IsMatch(dummy):
                    Console.WriteLine("6xkk");
                    Instructions.ld_vx_kk(this);
                    
                    return;


                case var dummy when Seven.IsMatch(dummy):
                    Console.WriteLine("7xkk");
                    Instructions.add_vx_kk(this);
                    
                    return;


                case var dummy when Eight_load.IsMatch(dummy):
                    Console.WriteLine("8xy0");
                    Instructions.ld_vx_vy(this);

                    return;


                case var dummy when Eight_or.IsMatch(dummy):
                    Console.WriteLine("8xy1");
                    Instructions.or_vx_vy(this);
                    
                    return;


                case var dummy when Eight_and.IsMatch(dummy):
                    Console.WriteLine("8xy2");
                    Instructions.and_vx_vy(this);

                    return;


                case var dummy when Eight_xor.IsMatch(dummy):
                    Console.WriteLine("8xy3");
                    Instructions.xor_vx_vy(this);

                    return;


                case var dummy when Eight_add.IsMatch(dummy):
                    Console.WriteLine("8xy4");
                    Instructions.add_vx_vy(this);

                    return;


                case var dummy when Eight_sub.IsMatch(dummy):
                    Console.WriteLine("8xy5");
                    Instructions.sub_vx_vy(this);

                    return;


                case var dummy when Eight_shr.IsMatch(dummy):
                    Console.WriteLine("8xy6");
                    Instructions.shr_vx_vy(this);
                
                    return;


                case var dummy when Eight_subn.IsMatch(dummy):
                    Console.WriteLine("8xy7");
                    Instructions.subn_vy_vx(this);
                    
                    return;


                case var dummy when Eight_shl.IsMatch(dummy):
                    Console.WriteLine("8xyE");
                    Instructions.shl_vx_vy(this);
                    
                    return;


                case var dummy when Nine.IsMatch(dummy):
                    Console.WriteLine("9xy0");
                    Instructions.skp_not_equal(this);
                    
                    return;


                case var dummy when A_addr.IsMatch(dummy):
                    Console.WriteLine($"Annn where nnn = {currentInstruction.Substring(1, 3)}");
                    Instructions.ld_i_nnn(this);

                    return;


                case var dummy when B_addr.IsMatch(dummy):
                    Console.WriteLine("Bnnn");
                    Instructions.jmp_v0_nnn(this);
                    
                    return;


                case var dummy when C_addr.IsMatch(dummy):
                    Console.WriteLine("Cxkk");
                    Instructions.ld_vx_rand(this);
                    
                    return;


                #region draw_func

                //huomionarvoista:
                // optimoinnin vuoksi voisi olla fiksua keksiä tapa vähentää type conversioneita
                // huom. mahdolliset bugit jotka mainittu edellisissä kommenteissa

                case var dummy when D_addr.IsMatch(dummy):

                    stopwatch.Start();

                    Console.WriteLine("Dxyn");

                    Instructions.drw(this);

                    stopwatch.Stop();
                    Console.WriteLine($"draw function elapsed ms = {stopwatch.ElapsedMilliseconds}");
                    stopwatch.Reset();


                    break;
                #endregion

                case var dummy when E_skp.IsMatch(dummy):
                    Console.WriteLine("Ex9E");
                    Instructions.skp_vx(this);

                    return;


                case var dummy when E_sknp.IsMatch(dummy):
                    Console.WriteLine("ExA1");

                    Instructions.sknp_vx(this);

                    return;


                case var dummy when F_load_from_dt.IsMatch(dummy):
                    Console.WriteLine("Fx07");

                    Instructions.ld_vx_dt(this);
                    
                    Debug.WriteLine($"registers[{x}] = {registers[x]}");
                    
                    return;


                case var dummy when F_load_key.IsMatch(dummy):
                    Console.WriteLine("Fx0A");

                    Instructions.ld_vx_key(this);

                    return;


                case var dummy when F_load_to_dt.IsMatch(dummy):
                    Console.WriteLine("Fx15");

                    Instructions.ld_dt_vx(this);

                    Debug.WriteLine($"delayTimer has been set to {delayTimer}");

                    return;


                case var dummy when F_load_to_st.IsMatch(dummy):
                    Console.WriteLine("Fx18");

                    Instructions.ld_st_vx(this);

                    return;

                case var dummy when Add_i_vx.IsMatch(dummy):
                    Console.WriteLine("Fx1E");

                    Instructions.add_i_vx(this);

                    return;


                case var dummy when Load_f_vx.IsMatch(dummy):
                    Console.WriteLine("Fx29");
                    Instructions.ld_f_vx(this);
          
                    return;


                case var dummy when Load_b_vx.IsMatch(dummy):
                    Console.WriteLine("Fx33");

                    Instructions.ld_bcd(this);

                    return;

                case var dummy when Load_i_vx.IsMatch(dummy):
                    Console.WriteLine("Fx55");

                    Instructions.ld_i_vx(this);

                    return;

                case var dummy when Load_vx_i.IsMatch(dummy):
                    Console.WriteLine("Fx65");

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


