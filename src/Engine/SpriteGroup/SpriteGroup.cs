using Engine.Common;

namespace Engine
{
    public class SpriteGroup : IRenderable
    {

        private Level _lvl;

        public SpriteGroup(TextureAtlas textureAtlas)
        {
            _lvl = new(textureAtlas);
        }

        public void AddTile(Tile t)
        {
            _lvl.tiles.Add(t);
        }

        public void Update(Camera camera)
        {
            _lvl.Renderer.UpdateVerts(camera);
            _lvl.Renderer.UpdateBuffers();
        }

        public List<Tile> GetList()
        {
            return _lvl.tiles;
        }

        public void Render(Camera camera)
        {
            _lvl.Renderer.Shader.Use();
            _lvl.Renderer.Shader.SetVector2("position", new OpenTK.Mathematics.Vector2((camera.Position.X / camera.Zoom), (camera.Position.Y / camera.Zoom)));
            _lvl.Render(camera);
        }
    }
}