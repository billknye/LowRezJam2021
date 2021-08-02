using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace LowRezJam2021
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D renderTarget;
        private SpriteFont font;
        private Texture2D spaceTiles;
        private float angle;
        private bool pixelated;
        private KeyboardState keyboardState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");
            renderTarget = new RenderTarget2D(GraphicsDevice, 64, 64);
            spaceTiles = Content.Load<Texture2D>("SpaceTiles");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var mouseState = Mouse.GetState();
            var keyboardStateLast = keyboardState;
            keyboardState = Keyboard.GetState();

            angle = MathF.Atan2(mouseState.Y - Window.ClientBounds.Height / 2, mouseState.X - Window.ClientBounds.Width / 2);

            if (keyboardState.IsKeyDown(Keys.F2) && keyboardStateLast.IsKeyUp(Keys.F2))
            {
                pixelated = !pixelated;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(samplerState: pixelated ? SamplerState.PointClamp : SamplerState.AnisotropicClamp);


            _spriteBatch.DrawString(font, "LowRezJam2021!!!", new Vector2(5, 5), Color.Teal);

            _spriteBatch.Draw(spaceTiles, new Vector2(32, 32), new Rectangle(0, 0, 8, 8), Color.White, angle, new Vector2(4, 4), 1.0f, SpriteEffects.None, 0f);

            _spriteBatch.End();

            int scale = (int)Math.Floor(Math.Min(Window.ClientBounds.Width, Window.ClientBounds.Height) / 64.0f);
            var dest = 64 * scale;
            var margin = (x: (Window.ClientBounds.Width - dest) / 2, y: (Window.ClientBounds.Height - dest) / 2);
            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(renderTarget, new Rectangle(margin.x, margin.y, dest, dest), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public class Particle
    {
        public Color StartColor;
        public Color EndColor;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Created;
    }

    public class RingBuffer<T> where T : new()
    {
        T[] items;
        int nextFree;

        public RingBuffer(int count)
        {
            items = new T[count];

            for (int i = 0; i < count; i++)
            {
                items[i] = new T();
            }
        }

        protected void Add(Action<T> change)
        {
            change(items[nextFree]);
            nextFree = (nextFree + 1) % items.Length;
        }
    }

    public class ParticleSet : RingBuffer<Particle>
    {
        int minScanIndex;
        float currentTime;
        float maxScanTime;

        public ParticleSet(int count, float lifeTime)
            : base(count)
        {
            LifeTime = lifeTime;
        }

        public float LifeTime { get; }

        public void Add(Action<Particle> change)
        {
            base.Add(n =>
            {
                change(n);
                n.Created = currentTime;
            });

            maxScanTime = currentTime + LifeTime;
        }

        public void Scan(float time)
        {
            currentTime = time;

            while (true)
            {

            }
        }
    }
}
