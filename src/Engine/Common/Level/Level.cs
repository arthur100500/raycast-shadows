using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using Engine;

namespace Engine.Common
{
    public class Level
    {
        public LevelRenderer Renderer;
        public TextureAtlas textureAtlas;
        public List<Tile> tiles;

        public Level(TextureAtlas textureAtlas)
        {
            this.textureAtlas = textureAtlas;
            tiles = new List<Tile>();
            Renderer = new LevelRenderer(textureAtlas, this);
        }

        public void Render(Camera cam)
        {
            Renderer.Render(cam);
        }

        public static Level FromFile(string fileName, TextureAtlas atlas)
        {
            var levelData = File.ReadAllText(fileName);
            var result = new Level(atlas);

            foreach (var line in levelData.Split("\n"))
            {
                // Tile
                if (line.StartsWith("|"))
                {
                    string[] messages = line.Replace("|", "").Split(":");
                    Tile t = new Tile(new Pos(float.Parse(messages[2]), float.Parse(messages[3])),
                        result.textureAtlas,
                        new iPos(int.Parse(messages[0]), int.Parse(messages[1])));
                    t.Rotation = float.Parse(messages[4]);
                    t.Group = int.Parse(messages[5]);
                    t.Width = float.Parse(messages[6]);
                    t.Height = float.Parse(messages[6]);
                    result.tiles.Add(t);
                }
            }

            return result;
        }

        public void SaveToFile(string fileName)
        {
            var levelData = "# Level Format 1.0\n";

            foreach (var tile in tiles)
            {
                levelData += "|" + tile.TextureInAtlas.X + ":" + tile.TextureInAtlas.Y + ":" + tile.Position.X + ":" +
                             tile.Position.Y + ":" + tile.Rotation + ":" + tile.Group +
                             ":" + tile.Width + ":" + tile.Height + "\n";
            }

            File.WriteAllText(fileName, levelData);
        }
    }
}