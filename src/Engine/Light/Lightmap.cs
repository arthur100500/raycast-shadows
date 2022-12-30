using Engine.Common;
using OpenTK.Graphics.OpenGL4;

namespace Engine.Light
{
    public class Lightmap : IRenderable
    {
        public List<Spotlight> Lights;
        public List<Line> ShadowCasters;
        private List<float> _verticies;
        private int[] _indices;
        protected readonly int _ebo;
        public Shader Shader;
        protected readonly int _vao;
        protected readonly int _vbo;
        private Texture _texture;
        public FrameBuffer fbo;
        public FrameBuffer worldfbo;
        public FrameBuffer worldfboMask;
        protected static readonly string standart_vert = @"#version 330 core
                                        layout(location = 0) in vec3 aLPosition;
                                        layout(location = 1) in vec2 aLTexCoord;
                                        layout(location = 2) in vec4 aLColor;

										uniform vec2 position;
                                        out vec2 texCoord;
                                        out vec4 lightColor;

                                        void main(void)
                                        {
                                            texCoord = aLTexCoord;
                                            lightColor = aLColor;

                                            gl_Position = vec4(aLPosition.xy - position.xy, aLPosition.z + 0.1, 1.0);
                                        }";

        public static readonly string standart_frag = @"#version 330
                                        out vec4 outputColor;
                                        in vec2 texCoord;
                                        in vec4 lightColor;
                                        uniform sampler2D texture0;

                                        void main()
                                        {
                                            outputColor = texture(texture0, texCoord).a * 2 * lightColor;
                                        }";

        public static readonly string fbo_frag = @"#version 330
                                        out vec4 outputColor;
                                        in vec2 texCoord;
                                        uniform sampler2D texture0;
                                        uniform sampler2D texture1;

                                        void main()
                                        {
                                            float Pi = 6.28318530718; // Pi*2
                                            float Directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
                                            float Quality = 8.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
                                            float Size = texture(texture1, texCoord).a > 0.01 ? 220. : 10.; // BLUR SIZE (Radius)
                                            // GAUSSIAN BLUR SETTINGS }}}
                                        
                                            vec2 Radius = Size / vec2(800, 800);
                                            
                                            // Pixel colour
                                            vec4 Color = texture(texture0, texCoord);
                                            
                                            // Blur calculations
                                            vec2 offset;
                                            for(float d = 0.0; d < Pi; d += Pi / Directions)
                                            {
                                                for(float i=1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
                                                {
                                                    offset = texCoord + vec2(cos(d),sin(d))*Radius*i;
                                                    offset.x = max(0, min(0.9999, offset.x));
                                                    offset.y = max(0, min(0.9999, offset.y));
                                                    Color += texture(texture0, offset);
                                                }
                                            }
                                            
                                            // Output to screen
                                            Color /= Quality * Directions - 15.0;

                                            float innerLight = texture(texture1, texCoord).a > 0.01 ? 2 : 1;

                                            outputColor = Color * innerLight;
                                        }";
        public Lightmap(Texture texture, FrameBuffer worldfbo, FrameBuffer worldfboMask)
        {
            this.worldfbo = worldfbo;
            this.worldfboMask = worldfboMask;
            ShadowCasters = new();
            Lights = new();
            _texture = texture;
            _verticies = new List<float>();
            _indices = new int[0];
            fbo = new();
            fbo.Load(new iPos(800, 800));
            Shader = new(standart_vert, standart_frag, 0);
            var _fbo_sh = new Shader(standart_vert, fbo_frag, 0);
            _fbo_sh.SetInt("texture1", 1);
            fbo.Sprite?.SetShader(_fbo_sh);

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _verticies.Count * sizeof(float), _verticies.ToArray(),
                BufferUsageHint.DynamicDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices,
                BufferUsageHint.StaticDraw);

            SetAttribs();
        }

        private void SetAttribs()
        {
            Shader.Use();

            GL.BindVertexArray(_vao);

            var vertexLocation = Shader.GetAttribLocation("aLPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);

            var texCoordLocation = Shader.GetAttribLocation("aLTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));

            var colorLocation = Shader.GetAttribLocation("aLColor");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, 9 * sizeof(float), 5 * sizeof(float));
        }

        public void UpdateVerts()
        {
            _verticies.Clear();
            foreach (var light in Lights)
            {
                light.CreateMesh(ShadowCasters.Where(
                    line => Misc.Len(line.First, light.Position) < light.Width * 1.5f ||
                    Misc.Len(line.Second, light.Position) < light.Width * 1.5f).ToList()
                );
                _verticies.AddRange(light.MeshVerticies);
            }

            _indices = new int[_verticies.Count() / 9];
            for (int i = 0; i < _indices.Length; i++)
                _indices[i] = i;
        }

        public virtual void UpdateBuffers()
        {
            //SetAttribs();
            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _verticies.Count * sizeof(float), _verticies.ToArray(), BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
        }

        public void Render(Camera camera)
        {
            fbo.Start();
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

            GL.Viewport(0, 0, 800, 800);
            Shader.SetVector2("position", new OpenTK.Mathematics.Vector2((camera.Position.X / camera.Zoom), (camera.Position.Y / camera.Zoom)));

            UpdateVerts();
            UpdateBuffers();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            _texture.Use(TextureUnit.Texture0);
            Shader.Use();

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            fbo.Stop();
            GL.Viewport(0, 0, 800, 800);

            worldfboMask.Texture?.Use(TextureUnit.Texture1);

            worldfbo.Render();
            GL.BlendFunc(BlendingFactor.DstColor, BlendingFactor.Zero);


            fbo.Render();
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
    }
}