﻿using CPU;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Graphics
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D texture;
        Vector2 position;
        Vector2 scale;
        Interpreter inter;
        Stopwatch updateLoopTimer;
        Stopwatch perfMeasureSW;
        Keys[] keys;
        Keys prevKeyPress;
        Color[] colorData;
        Dictionary<object, object> colorLookup;
        SoundEffect beep;
        SoundEffectInstance beepInstance;
        int screenScalar;
        int slowDownMultiplier;

        public delegate void KeyPressedEventHandler(int key, bool state);
        public event KeyPressedEventHandler KeyStateChanged;
        
        

        public Game1(string[] args)
        {
            if (args.Length != 0)
            {
                inter = new Interpreter(args[0]);
            }
            else
            {
                inter = new Interpreter(@"C:\Users\elias\Desktop\CHIP-26 Project\Test Programs\Space Invaders.ch8");
            }

            KeyStateChanged += inter.GetKey;
            
            graphics = new GraphicsDeviceManager(this);

            updateLoopTimer = new Stopwatch();

            screenScalar = 15;
            slowDownMultiplier = 2;

            graphics.PreferredBackBufferWidth = 64 * screenScalar;
            graphics.PreferredBackBufferHeight = 32 * screenScalar;

            //disable vsync
            graphics.SynchronizeWithVerticalRetrace = false;

            //enable variable time step
            IsFixedTimeStep = false;

            //create the sound instance for the beep
            beep = Content.Load<SoundEffect>("beep");
            beepInstance = beep.CreateInstance();

            Content.RootDirectory = "Content";
            position = new Vector2(0, 0);

            scale = new Vector2(screenScalar, screenScalar);




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
            colorLookup = new Dictionary<object, object>
            {   // 0 = black
                {0, Color.Black},
                //1 = white
                {1, Color.White},

                {Color.Black, 0},

                {Color.White, 1}
        };


            //create the main texture onto which the display is drawn
            texture = new Texture2D(GraphicsDevice, 64, 32);

            //this stopwatch is used both for timers and performance measuring
            perfMeasureSW = new Stopwatch();

            //set the title of the window
            Window.Title = "Chip26";
            
            //create the color array, the texture is manipulated by changing the colors of this array
            colorData = new Color[64 * 32];

            //draw it white by default, useful for debugging purposes as if the screen is entirely white, 
            //you know no draw calls have been issued
            //note: some (at least clock) don't clear the screen at the start, so it might be a good idea to remove 
            //this feature for consistency when the bugs have been ironed out
            
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


            //beep = Content.Load<SoundEffect>("Beep");
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
        /// possible keystate changes that have occurred
        /// </summary>
        protected void CheckForKeyPress()
        {
            keys = Keyboard.GetState().GetPressedKeys();

            //if any keys have been pressed
            if (keys.Length > 0)
            {
                //if the key is not the same as the previous key (aka the keypress changed)
                if (keys[0] != prevKeyPress)
                {
                    Debug.WriteLine($"{prevKeyPress} has been released in game1.cs");

                    //previous key press has been released
                    KeyStateChanged( (int) prevKeyPress, false );
                    
                    Debug.WriteLine($"{keys[0]} pressed in game1.cs");

                    /*Set the currently pressed key as the new previously pressed key for the next round
                     * If prevKeyPress is 0, it needs to be set to -1 instead to avoid a bug in the input processing system 
                     */
                    prevKeyPress = keys[0];

                    //let the cpu know that the current key has been pushed down
                    KeyStateChanged( (int) keys[0], true);
                }
                else
                {
                    Debug.WriteLine($"{prevKeyPress} is (still) being pressed");
                    //nothing has been released
                    KeyStateChanged(-1, false);
                    
                }
            }

            //nothing is being pressed
            else
            {
                Debug.WriteLine("No keys are currently being pressed." +
                    " Setting previous key as released and setting the prevKeyPress variable to 0.");

                //set the previous key as released and set prevKeyPress to 0
                //bonus note: the prevKeyPress refuses to store -1, but we need it later in the loop so if prevKeyPress is 0 we manually send in -1 instead

                if (prevKeyPress == 0)
                {
                    KeyStateChanged(-1, false);
                }
                else
                {
                    KeyStateChanged( (int) prevKeyPress, false);
                }
                /*
                let the cpu know that currently nothing is being pressed. the cpu assumes that the previous key is pressed as
                long as we don't tell it otherwise, which we do by saying that -1 (aka nothing in this case) is being pressed
                */

                KeyStateChanged(-1, true);
          
                /*
                 * Set prevKeyPress to 0 so the next time
                 * we check the inputs we assume that nothing was pressed the last time so that any potential new inputs can be accepted freely now.
                 */ 
                prevKeyPress = 0;
            
            }
        }

        /// <summary>
        ///  DrawTextureArray draws the requested content to the texture as colorData
        /// </summary>
        protected void DrawTextureArray()
        {
            //instead of clearing the colorData everytime, iterate through the old colorData, 
            //changing what needs to be changed

            
            perfMeasureSW.Start();

            for (int i = 0; i < 2048; i++)
            {
                if (inter.display[i] != (int)colorLookup[colorData[i]])
                {
                    colorData[i] = (Color)colorLookup[inter.display[i]];
                }

            }
            perfMeasureSW.Stop();

            Debug.WriteLine($"Populating the texture took: {perfMeasureSW.Elapsed.TotalMilliseconds}ms");

            perfMeasureSW.Reset();


            perfMeasureSW.Start();

            texture.SetData(colorData);

            perfMeasureSW.Stop();

            Console.WriteLine($"texture.SetData took {perfMeasureSW.Elapsed.TotalMilliseconds}ms");

            perfMeasureSW.Reset();
            
            //if the debug draw option is on, pause for 2,5 seconds before continuing
            if (inter.debugDraw)
            {
                Thread.Sleep(2500);
            }

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
            Thread.Sleep(slowDownMultiplier);

            if (IsActive)
            {
                //start perfMeasureSW to measure performance of the interpreter step method
                perfMeasureSW.Start();

                //step the interpreter
                inter.Advance();

                perfMeasureSW.Stop();
                
                Debug.WriteLine($"Interpreter advance execution time {perfMeasureSW.ElapsedMilliseconds}ms");

                perfMeasureSW.Reset();

                //check for any keys being pressed
                CheckForKeyPress();


                //if the drawglag is set, 
                if (inter.drawFlag)
                {
                    perfMeasureSW.Start();

                    DrawTextureArray();



                }

               

                //If user presses Esc, exit
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                  ButtonState.Pressed || Keyboard.GetState().IsKeyDown(
                  Keys.Escape))
                    Exit();

                //measure amt of time taken by base.Update 
                perfMeasureSW.Start();

                base.Update(gameTime);
                
                perfMeasureSW.Stop();
                
                Debug.WriteLine($"base.Update(gametime) took {perfMeasureSW.ElapsedMilliseconds} ms");
                
                perfMeasureSW.Reset();

            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            perfMeasureSW.Start();

            spriteBatch.Begin(samplerState:SamplerState.PointClamp);
            spriteBatch.Draw(texture: texture, position: position, scale: scale);
            spriteBatch.End();

            perfMeasureSW.Stop();
            Debug.WriteLine($"spritebatch.Begin() and spritebatch.Draw() took {perfMeasureSW.Elapsed.TotalMilliseconds}");
            perfMeasureSW.Reset();

            perfMeasureSW.Start();           
            base.Draw(gameTime);
            perfMeasureSW.Stop();

            Debug.WriteLine($"base.Draw(gameTime) took {perfMeasureSW.Elapsed.TotalMilliseconds}ms to execute");


            
            //decrement delay timer


            //this has to be put here, because otherwise the delay timer wouldn't account for the time spent here, which causes noticeable timing issue

            updateLoopTimer.Stop();
            Console.WriteLine($"updateLoopTimer.Elapsed.TotalMilliseconds: {updateLoopTimer.Elapsed.TotalMilliseconds}");



            inter.delayTimer -= (float)(0.06 * updateLoopTimer.Elapsed.TotalMilliseconds);
            
            inter.soundTimer -= (float)(0.06 * updateLoopTimer.Elapsed.TotalMilliseconds);

            if (inter.delayTimer < 0)
            {
                inter.delayTimer = 0;
            }


            if (inter.soundTimer <= 0)
            {
                inter.soundTimer = 0;
                beepInstance.Stop();
            }
            else
            {
                beepInstance.IsLooped = true;
                beepInstance.Play();
            }


            Debug.WriteLine($"updateLoopTimer.ElapsedMilliseconds: {updateLoopTimer.ElapsedMilliseconds}");
            updateLoopTimer.Reset();


            Debug.WriteLine($"delaytimer={inter.delayTimer}");

            Debug.WriteLine($"soundtimer={inter.soundTimer}");


        }

        private void OnKeyStateChanged(int key, bool state)
        {
            if (KeyStateChanged != null)
            {
                KeyStateChanged(key, state);
            }
        }

    }
}