using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace AStarMonoGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        NodeManager manager;
        Random rng = new Random();


        int nrX = 80; //Antal rutor/nodes i x-led
        int nrY = 50; //Antal rutor/nodes i y-led
        int nrWalls = 100; //Antal hinder/väggar
        int sizeWall=4; //Storlek på hindren/väggarna

        Vector2 start, goal;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Slumpa start och mål
            start = new Vector2(rng.Next(nrX),rng.Next(nrY)); 
            goal = new Vector2(rng.Next(nrX), rng.Next(nrY));

            manager = new NodeManager(new Vector2(nrX, nrY),true);
            
            //VÄGGAR! Slumpa fram nrWalls stycken.
            List<Vector2> walls = new List<Vector2>();
            for (int i = 0; i < nrWalls; i++)
            {
                List<Vector2> wallPos = new List<Vector2>();
                int rndX = 0, rndY = 0;
                bool validWall = true;

                //Koordinater för övre vänstra hörnet i väggen
                rndX = rng.Next(0, nrX-sizeWall);
                rndY = rng.Next(0, nrY-sizeWall);
                 
                //Fyll på så att väggen blir större än en ruta; den ska vara en kvadrat med sidlängden sizeWall
                for (int x = 0; x < sizeWall; x++)
                {
                    for (int y = 0; y < sizeWall; y++)
                    {
                        Vector2 wallPart = new Vector2(rndX + x, rndY + y);

                        //Om väggen ligger på målet kan vi inte använda den
                        if (wallPart == start || wallPart == goal)
                            validWall = false;

                        wallPos.Add(wallPart);
                    }
                }
                //Lägg till väggen i listan om den är ok!
                if (validWall)
                    walls.AddRange(wallPos);
            }


            manager.Initialize(start, goal, walls);//Initiera manager med start, goal och listan med alla väggar

            //Grafikinställningar
            graphics.PreferredBackBufferHeight = 1000;
            graphics.PreferredBackBufferWidth = 1800;
            IsMouseVisible = true;
            graphics.ApplyChanges();
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

            manager.LoadContent(Content.Load<SpriteFont>("Fonten"));
        }


        double interval=0;
        KeyboardState newState, oldState = new KeyboardState();
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            newState = Keyboard.GetState();

            //Escape - börja om!
            if (newState.IsKeyDown(Keys.Escape) && oldState.IsKeyUp(Keys.Escape))
                Initialize();

            //Mellanslag - plocka fram vägen direkt!
            if (newState.IsKeyDown(Keys.Space) && oldState.IsKeyUp(Keys.Space))
                manager.AllSteps();

            interval += gameTime.ElapsedGameTime.TotalMilliseconds;

            //Ändra det här om du vill få långsammare progression!
            if (interval>1)
            {
                interval = 0;
               manager.Step();
            }

            oldState = newState;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            manager.Draw(spriteBatch);
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
