using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using Engine;
using Engine.Common;
using Engine.Light;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Scene
{
    public class GameScene : IScene
    {
        private Window _window;
        private SpriteGroup sg;
        private SpriteGroup bg;
        private TextureAtlas _atlas;
        private TextureAtlas _bgatlas;
        private Camera _cam;
        private Random _random;
        private Lightmap lm;
        private FrameBuffer _worldFBO;
        private FrameBuffer _worldFG;
        public string GetName()
        {
            return "A Game";
        }

        public GameScene(Window window)
        {
            _window = window;
            _cam = new();
            _atlas = new(Image.Load<Rgba32>("GameData/Textures/sprites.png"), 32, 32);
            _bgatlas = new(Image.Load<Rgba32>("GameData/Textures/white.png"), 32, 32);
            _random = new();
            _worldFBO = new();
            _worldFG = new();
        }

        public void Load()
        {
            _worldFBO.Load(new(_window.Size.X, _window.Size.Y));
            _worldFG.Load(new(_window.Size.X, _window.Size.Y));
            bg = new(_bgatlas);
            sg = new(_atlas);
            lm = new(Texture.LoadFromFile("GameData/Textures/light.png"), _worldFBO, _worldFG);

            bg.AddTile(new Tile(new Pos(0, 0), _atlas, new iPos(6, 0)));
            bg.GetList().Last().Width = 1000;
            bg.GetList().Last().Height = 1000;

            for (int i = 0; i < 20; i++)
            {
                sg.AddTile(new Tile(
                    new Pos(_random.NextSingle() * 100 - 50, _random.NextSingle() * 100 - 50),
                    _atlas,
                    new iPos(_random.Next(5), _random.Next(0))
                ));

                sg.GetList().Last().Width = _random.NextSingle() * 20;
                sg.GetList().Last().Height = _random.NextSingle() * 20;
                //sg.GetList().Last().Rotation = _random.NextSingle() * 10;
            }

            lm.Lights.Add(new Spotlight(new Pos(0, 0)));

            var shadowcasters = new List<Line>();

            foreach (var tile in sg.GetList())
            {
                var p1 = new Pos(tile.Position.X + tile.Width / 2, tile.Position.Y + tile.Height / 2) * (1f / _cam.Zoom);
                var p2 = new Pos(tile.Position.X + tile.Width / 2, tile.Position.Y - tile.Height / 2) * (1f / _cam.Zoom);
                var p3 = new Pos(tile.Position.X - tile.Width / 2, tile.Position.Y + tile.Height / 2) * (1f / _cam.Zoom);
                var p4 = new Pos(tile.Position.X - tile.Width / 2, tile.Position.Y - tile.Height / 2) * (1f / _cam.Zoom);

                shadowcasters.Add(new(p1, p2));
                shadowcasters.Add(new(p1, p3));
                shadowcasters.Add(new(p4, p2));
                shadowcasters.Add(new(p4, p3));
            }

            lm.ShadowCasters = shadowcasters;
            bg.Update(_cam);
            sg.Update(_cam);
        }

        public void Render()
        {
            _worldFG.Start();
            sg.Render(_cam);
            _worldFG.Stop();

            _worldFBO.Start();
            bg.Render(_cam);
            _worldFG.Render();
            _worldFBO.Stop();

            lm.Render(_cam);

        }

        public void Update()
        {

        }

        public void FixedUpdate()
        {
            _cam.Position += Controls.control_direction_f * 0.1f;
            lm.Lights[0].Position = (_cam.Position) * (1f / _cam.Zoom)
            + new Pos(Controls.mouse.Position.X / (float)_window.Size.X, -Controls.mouse.Position.Y / (float)_window.Size.Y) * 2 - new Pos(1, -1);

            if (Controls.MouseButtonPressedOnce(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
            {
                var l = new Spotlight(lm.Lights[0].Position.Copy());
                l.Color = new Vector4(_random.NextSingle(), _random.NextSingle(), _random.NextSingle(), 1.0f);
                l.Width = l.Height = _random.NextSingle() * 10;
                lm.Lights.Add(l);
            }

        }

        public void Resize()
        {

        }
    }
}