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
        private Texture2D tiles;

        private bool pixelated;
        private MouseState mouseState;
        private KeyboardState keyboardState;

        private int drawScale;
        private (int x, int y) drawMargin;
        private Point mousePoint;
        private Point viewOffset;

        private MapChunk map;

        private ParticleSet particles;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 640;
            _graphics.PreferredBackBufferHeight = 640;
            _graphics.ApplyChanges();

            particles = new ParticleSet(1024, 1.0f);
            map = new MapChunk();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");
            renderTarget = new RenderTarget2D(GraphicsDevice, 64, 64);
            tiles = Content.Load<Texture2D>("SpaceTiles");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var mouseStateLast = mouseState;
            mouseState = Mouse.GetState();
            var keyboardStateLast = keyboardState;
            keyboardState = Keyboard.GetState();
            
            if (wasKeyJustPressed(Keys.W, keyboardStateLast))
            {
                viewOffset.Y--;
            }
            if (wasKeyJustPressed(Keys.A, keyboardStateLast))
            {
                viewOffset.X--;
            }
            if (wasKeyJustPressed(Keys.S, keyboardStateLast))
            {
                viewOffset.Y++;
            }
            if (wasKeyJustPressed(Keys.D, keyboardStateLast))
            {
                viewOffset.X++;
            }

            mousePoint = new Point((int)MathF.Floor((mouseState.X - drawMargin.x) / (float)drawScale / 8.0f) + viewOffset.X, (int)MathF.Floor((mouseState.Y - drawMargin.y) / (float)drawScale / 8.0f) + viewOffset.Y);

            if (mouseState.LeftButton == ButtonState.Pressed && mouseStateLast.LeftButton == ButtonState.Released)
            {
                if (mousePoint.X >= 0 && mousePoint.Y >= 0 && mousePoint.X <= 256 && mousePoint.Y <= 256)
                {
                    var tile = map[mousePoint.X, mousePoint.Y];
                    tile.Road = true;
                }
            }

            base.Update(gameTime);
        }

        private bool wasKeyJustPressed(Keys key, KeyboardState last)
        {
            return keyboardState.IsKeyDown(key) && last.IsKeyUp(key);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(samplerState: pixelated ? SamplerState.PointClamp : SamplerState.AnisotropicClamp);

            /*var now = (float)gameTime.TotalGameTime.TotalSeconds;
            particles.Scan(now, p =>
            {
                _spriteBatch.Draw(tiles, p.Position + p.Velocity * (now - p.Created), new Rectangle(1, 1, 1, 1), Color.Cyan);
            });

            _spriteBatch.DrawString(font, "LowRezJam2021!!!", new Vector2(5, 5), Color.Teal);
            */

            int tileSize = 8;
            int visibleTiles = 64 / tileSize;
            for (int dy = 0; dy < visibleTiles; dy++)
            {
                for (int dx = 0; dx < visibleTiles; dx++)
                {
                    int x = dx + viewOffset.X;
                    int y = dy + viewOffset.Y;

                    var tile = map[x, y];
                    if (tile == null)
                        continue;

                    var dest = new Rectangle(dx * tileSize, dy * tileSize, tileSize, tileSize);

                    _spriteBatch.Draw(tiles, dest, new Rectangle(0, 0, 8, 8), Color.Green);

                    if (tile.Road)
                    {
                        var tileIndex = 8;
                        for (int i = 0; i < 4; i++)
                        {
                            var pt = new Point(x + offset[i].X, y + offset[i].Y);
                            if (pt.X < 0 || pt.Y < 0 || pt.X >= 256 || pt.Y >=256)
                            {
                                continue;
                            }
                            var neighbor = map[pt.X, pt.Y];
                            if (neighbor.Road)
                            {
                                tileIndex += 1 << i;
                            }
                        }

                        var src = new Rectangle(tileIndex % 8 * 8, tileIndex / 8 * 8, 8, 8);
                        _spriteBatch.Draw(tiles, dest, src, Color.White);
                    }

                }
            }

            _spriteBatch.Draw(tiles, new Rectangle((mousePoint.X - viewOffset.X) * tileSize, (mousePoint.Y - viewOffset.Y) * tileSize, tileSize, tileSize), new Rectangle(16, 0, 8, 8), Color.Silver);

            _spriteBatch.End();

            // 
            drawScale = (int)Math.Floor(Math.Min(Window.ClientBounds.Width, Window.ClientBounds.Height) / 64.0f);
            var destSize = 64 * drawScale;
            drawMargin = (x: (Window.ClientBounds.Width - destSize) / 2, y: (Window.ClientBounds.Height - destSize) / 2);
            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(renderTarget, new Rectangle(drawMargin.x, drawMargin.y, destSize, destSize), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private Point[] offset = new Point[]
        {
            new Point(1,0),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(0, -1)
        };
    }

    public class MapChunk
    {
        MapTile[,] tiles;

        public MapTile this[int x, int y]
        {
            get
            {
                if (x < 0 || y < 0 || x >= 256 || y >= 256)
                    return null;

                return tiles[x, y];
            }
        }

        public MapChunk()
        {
            tiles = new MapTile[256, 256];

            for (int x =0; x < tiles.GetLength(0); x++)
            {
                for (int y =0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y] = new MapTile
                    {
                        Location = new Point(x, y)
                    };

                }
            }

        }


    }

    public class MapTile
    {
        public Point Location { get; set; }

        public bool Road { get; set; }
    }

}
