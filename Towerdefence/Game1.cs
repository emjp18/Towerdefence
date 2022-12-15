using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Spline;

using System.IO;

namespace Towerdefence
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        Texture2D tex;

        int width = 800;
        int height = 600;

     

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            
            graphics.PreferredBackBufferHeight = height;
            graphics.PreferredBackBufferWidth = width;
            graphics.ApplyChanges();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            tex = Content.Load<Texture2D>("ball");

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

          

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
           
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}