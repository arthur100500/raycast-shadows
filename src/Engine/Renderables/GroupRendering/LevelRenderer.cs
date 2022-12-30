using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using Engine.Common;

namespace Engine
{
    public class LevelRenderer
    {
        protected readonly TextureAtlas _atlas;
        protected readonly int _ebo;

        protected int[] _indices;

        public Shader Shader;

        protected readonly int _vao;
        protected readonly int _vbo;
        protected readonly List<float> _verts;
        public Level level;

        protected static readonly string standart_vert = @"#version 330 core
                                        layout(location = 0) in vec3 aPosition;
                                        layout(location = 1) in vec2 aTexCoord;

										uniform vec2 position;
                                        out vec2 texCoord;

                                        void main(void)
                                        {
                                            texCoord = aTexCoord;

                                            gl_Position = vec4(aPosition.xy - position.xy, aPosition.z + 0.1, 1.0);
                                        }";

        protected static readonly string standart_frag = StandartShaders.standart_frag;
        public LevelRenderer(TextureAtlas atlas, Level self)
        {
            level = self;
            _atlas = atlas;
            _verts = new List<float>();
            _indices = new int[0];
            Shader = new Shader(standart_vert, standart_frag, 0);


            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _verts.Count * sizeof(float), _verts.ToArray(),
                BufferUsageHint.DynamicDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices,
                BufferUsageHint.StaticDraw);

            Shader.Use();
            var vertexLocation = Shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = Shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float),
                3 * sizeof(float));
        }

        public virtual void UpdateBuffers()
        {
            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _verts.Count * sizeof(float), _verts.ToArray(),
                BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices,
                BufferUsageHint.StaticDraw);
        }

        public void Render(Camera camera, bool updateVerts = true)
        {
            if (updateVerts)
                UpdateVerts(camera);

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            _atlas.Use(TextureUnit.Texture0);
            Shader.Use();

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void UpdateVerts(Camera camera)
        {
            _verts.Clear();
            foreach (var tile in level.tiles) tile.AddToVerts(_verts, camera);

            var objectCount = _verts.Count / 20;
            _indices = new int[6 * objectCount];
            for (var i = 0; i < objectCount; i++)
            {
                _indices[6 * i + 0] = 4 * i;
                _indices[6 * i + 1] = 4 * i + 1;
                _indices[6 * i + 2] = 4 * i + 3;
                _indices[6 * i + 3] = 4 * i + 1;
                _indices[6 * i + 4] = 4 * i + 2;
                _indices[6 * i + 5] = 4 * i + 3;
            }
        }
    }
}