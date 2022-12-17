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

        int width = 1920;
        int height =1080;

        GameObject testobject = new Item("", new Rectangle(700, 500, 50, 50), 1, 30, 0);
        GameObject testobject2 = new Item("", new Rectangle(100, 500, 50, 50), 1, 30, 1);
        PhysicsManager physicsManager= new PhysicsManager();
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
            if(Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                testobject.AddForce(new Vector2(100, 0));
                testobject2.AddForce(new Vector2(-100, 0));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                testobject.AddForce(new Vector2(-100, 0));
                testobject2.AddForce(new Vector2(-100, 0));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                testobject.AddForce(new Vector2(100, 0));
                testobject2.AddForce(new Vector2(100, 0));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                testobject.AddForce(new Vector2(0, -100));
                testobject2.AddForce(new Vector2(0, -100));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                testobject.AddForce(new Vector2(0, 100));
                testobject2.AddForce(new Vector2(0, 100));
            }
            physicsManager.GJK(testobject, testobject2);
            physicsManager.SequentialImpulse((float)gameTime.ElapsedGameTime.TotalSeconds, testobject, testobject2);
            System.Diagnostics.Debug.WriteLine(testobject.getOBB().TopLeft.ToString());
            testobject.Update(gameTime);
            testobject2.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(tex, new Rectangle(testobject.getOBB().TopLeft.ToPoint(), new Point(testobject.GetRectangle().Width,
                testobject2.GetRectangle().Height)
                ),null, Color.White, testobject.GetOrientation(), testobject.getOBB().center, SpriteEffects.None, 0);

            spriteBatch.Draw(tex, new Rectangle(testobject2.getOBB().TopLeft.ToPoint(), new Point(testobject2.GetRectangle().Width,
                testobject2.GetRectangle().Height)
                ), null, Color.White, testobject2.GetOrientation(), testobject2.getOBB().center, SpriteEffects.None, 0);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}