using FrameworkInterpreter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;


namespace Graphics
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 
    public class Game1 : Game
    {
        const float hertz60 = 0.016666F;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D texture;
        Vector2 position;
        Vector2 scale;
        Interpreter inter;
        Stopwatch updateLoop;
        Stopwatch stopwatch;
        Keys[] keys;
        Stopwatch spritebatchSW;

        public delegate void KeyPressedEventHandler(int key);
        public event KeyPressedEventHandler KeyPressed;
        
        

        public Game1()
        {
            this.Window.Title = "CHIP-8";
            inter = new Interpreter();

            KeyPressed += inter.GetKey;

            graphics = new GraphicsDeviceManager(this);

            updateLoop = new Stopwatch();

            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 320;
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
            texture = new Texture2D(this.GraphicsDevice, 64, 32);

            stopwatch = new Stopwatch();

            spritebatchSW = new Stopwatch();

            Color[] colorData = new Color[64 * 32];
            for (int i = 0; i < 64*32; i++)
            {
                colorData[i] = Color.Red;

            }
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
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            updateLoop.Start();

            keys = Keyboard.GetState().GetPressedKeys();

            if (keys.Length > 0 && Convert.ToInt32(keys[0]) != 0)
            {
                Debug.WriteLine(keys[0]);
                OnKeyPressed(Convert.ToInt32(keys[0]));
            }

            if (IsActive)
            {
                stopwatch.Start();

                inter.Advance();

                stopwatch.Stop();

                Debug.WriteLine($"interpreter advance execution time {stopwatch.ElapsedMilliseconds}");

                stopwatch.Reset();

    

                if (inter.drawFlag)
                {
                    stopwatch.Start();


                    Color[] colorData = new Color[64*32];
                    for (int i = 0; i < 64 * 32; i++)
                    {

                        if (inter.display[i / 64, i % 64] == 1)
                        {
                            colorData[i] = new Color(90,135,197);
                        }
                        else
                        {
                            colorData[i] = Color.Black;
                        }


                    }
                    texture.SetData(colorData);
                    stopwatch.Stop();
                    Debug.WriteLine($"populating the texture took: {stopwatch.ElapsedMilliseconds}ms");
                    stopwatch.Reset();

                }
       
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                  ButtonState.Pressed || Keyboard.GetState().IsKeyDown(
                  Keys.Escape))
                    Exit();


                base.Update(gameTime);

            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(samplerState:SamplerState.PointClamp);
            spritebatchSW.Start();
            spriteBatch.Draw(texture: texture, position: position, scale: scale);
            spritebatchSW.Stop();
            Debug.WriteLine($"spritebatch.Draw() took {spritebatchSW.ElapsedMilliseconds}");
            spritebatchSW.Reset();
            spriteBatch.End();
            base.Draw(gameTime);
            updateLoop.Stop();
            Console.WriteLine($"updateLoop.ElapsedMilliseconds: {updateLoop.ElapsedMilliseconds}");
            //delayTimer decrements 60 times per second
            inter.delayTimer -= 1000 * hertz60 - updateLoop.ElapsedMilliseconds;
            if (inter.delayTimer < 0)
            {
                inter.delayTimer = 0;
            }

            Console.WriteLine($"delaytimer={inter.delayTimer}");

            updateLoop.Reset();

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
