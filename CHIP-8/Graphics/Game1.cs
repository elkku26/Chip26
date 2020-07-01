using FrameworkInterpreter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Graphics
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 
    public class Game1 : Game
    {
        const float hertz60 = 0.01666666666F;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D texture;
        Vector2 position;
        Vector2 scale;
        Interpreter inter;
        Stopwatch updateLoopTimer;
        Stopwatch stopwatch;
        Keys[] keys;
         //Keys prevKeyPress;
        Color[] colorData;
        Stopwatch spritebatchSW;
        Dictionary<int, Color> colorLookup;
        Dictionary<Color, int> reverseColorLookup;


        public delegate void KeyPressedEventHandler(int key);
        public event KeyPressedEventHandler KeyPressed;
        
        

        public Game1(string[] args)
        {
            if (args.Length != 0)
            {
                inter = new Interpreter(args[0]);
            }
            else
            {
                inter = new Interpreter(@"C:\Users\elias\Desktop\Space Invaders.ch8");
            }

            KeyPressed += inter.GetKey;

            graphics = new GraphicsDeviceManager(this);

            updateLoopTimer = new Stopwatch();

            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 320;

            //disable vsync
            graphics.SynchronizeWithVerticalRetrace = false;

            //enable variable time step
            IsFixedTimeStep = false;

            Content.RootDirectory = "Content";
            position = new Vector2(0, 0);
            scale = new Vector2(10, 10);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //initialise color lookup tables, these are used to set the colors used by the emulator
            colorLookup = new Dictionary<int, Color>
            {   // 0 = black
                {0, Color.Black},
                //1 = white
                {1, Color.White},
        };
            reverseColorLookup = new Dictionary<Color, int>
            {   // black = 0
                {Color.Black,0},
                //white = 1
                {Color.White,1},
        };

            //create the main texture onto which the display is drawn
            texture = new Texture2D(GraphicsDevice, 64, 32);

            //this stopwatch is used both for timers and performance measuring
            stopwatch = new Stopwatch();

            //set the title of the window
            Window.Title = "CHIP-8";

            spritebatchSW = new Stopwatch();
            
            //create the color array, the texture is manipulated by changing the colors of this array
            colorData = new Color[64 * 32];

            //draw it white by default, useful in debugging purposes as if the screen is entirely white, 
            //you know no draw calls have been issued
            for (int i = 0; i < 64*32; i++)
            {
                colorData[i] = Color.White;
            }


            

            //set the colordata to the texture
            texture.SetData(colorData);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            

            // TODO: use this.Content to load your game content here
        }
        

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
            // TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// CheckForKeyPress will be called once every frame and it notifies the interpreter about any
        /// possible keys that have been pressed
        /// </summary>
        protected void CheckForKeyPress()
        {
            keys = Keyboard.GetState().GetPressedKeys();

            //if any keys have been pressed
            if (keys.Length > 0)
            {
                //if the key is not the same as the previous key
                //if (keys[0] != prevKeyPress)
                //{
                    //prevKeyPress = keys[0];
                    Debug.WriteLine($"{keys[0]} registered in game1.cs");
                    OnKeyPressed(Convert.ToInt32(keys[0]));
                }
                //else
                //{
                  //  OnKeyPressed(0);
                //}
            //}
            //else
            //{
            //    prevKeyPress = 0;
            //}
        }

        /// <summary>
        ///  DrawTextureArray draws the wanted content to the texture as colorData
        /// </summary>
        protected void DrawTextureArray()
        {
            //instead of clearing the colorData everytime, iterate through the old colorData, 
            //changing what needs to be changed

            //note: probably still has much room for improvement

            stopwatch.Start();

            for (int i = 0; i < 2048; i++)
            {
                if (inter.display[i] != reverseColorLookup[colorData[i]])
                {
                    colorData[i] = colorLookup[inter.display[i]];
                }

            }
            stopwatch.Stop();

            Debug.WriteLine($"Populating the texture took: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Reset();

            stopwatch.Start();

            texture.SetData(colorData);

            stopwatch.Stop();

            Console.WriteLine($"texture.SetData took {stopwatch.Elapsed.Milliseconds}");



        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //timer for measuring performance & implementing the clocks
            updateLoopTimer.Start();

            if (IsActive)
            {
                //start stopwatch to measure performance of the interpreter step method
                stopwatch.Start();

                //step the interpreter
                inter.Advance();

                stopwatch.Stop();
                
                Debug.WriteLine($"Interpreter advance execution time {stopwatch.ElapsedMilliseconds}ms");

                stopwatch.Reset();

                //check for any keys being pressed
                CheckForKeyPress();


                //if the drawglag is set, 
                if (inter.drawFlag)
                {
                    stopwatch.Start();

                    DrawTextureArray();



                }

                //decrement delay timer

                //sometimes the program may be stuck in an infinite loop of instructions that take
                //less than 1ms to execute, thus making the delaytimer unable to decrement
                //and causing issues

                //if (updateLoopTimer.Elapsed.TotalMilliseconds == 0)
                //{
                //if the timer rounds the stopwatch time to zero, decrease the 
                //timer by 0.1 ms to avoid infinite loops
                //    inter.delayTimer -= (float)0.5;
                //}

                inter.delayTimer -= (float)(0.134 * updateLoopTimer.Elapsed.TotalMilliseconds);
                //inter.delayTimer -= 0.0024f;

                if (inter.delayTimer < 0)
                {
                    inter.delayTimer = 0;
                }

                Console.WriteLine($"updateLoopTimer.ElapsedMilliseconds: {updateLoopTimer.ElapsedMilliseconds}");

                Console.WriteLine($"delaytimer={inter.delayTimer}");

                //If user presses Esc, exit
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                  ButtonState.Pressed || Keyboard.GetState().IsKeyDown(
                  Keys.Escape))
                    Exit();

                //measure amt of time taken by base.Update 
                stopwatch.Start();

                base.Update(gameTime);
                
                stopwatch.Stop();
                
                Console.WriteLine($"base.Update(gametime) took {stopwatch.ElapsedMilliseconds} ms");
                
                stopwatch.Reset();

                updateLoopTimer.Stop();
                Console.WriteLine($"updateLoopTimer.Elapsed.TotalMilliseconds: {updateLoopTimer.Elapsed.TotalMilliseconds}");
                updateLoopTimer.Reset();

            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            stopwatch.Start();

            spriteBatch.Begin(samplerState:SamplerState.PointClamp);
            
            spriteBatch.Draw(texture: texture, position: position, scale: scale);
            
            spriteBatch.End();

            stopwatch.Stop();
            Debug.WriteLine($"spritebatch.Begin() and spritebatch.Draw() took {stopwatch.Elapsed.TotalMilliseconds}");
            stopwatch.Reset();

            stopwatch.Start();           
            base.Draw(gameTime);
            stopwatch.Stop();
            Debug.WriteLine($"base.Draw(gameTime) took {stopwatch.Elapsed.TotalMilliseconds}ms to execute");

            //updateLoopTimer.Reset();

        }

        private void OnKeyPressed(int key)
        {
            if (KeyPressed != null)
            {
                KeyPressed(key);
            }
        }
    }
}
