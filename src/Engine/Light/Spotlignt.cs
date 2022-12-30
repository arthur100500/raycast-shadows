using Engine.Common;
using OpenTK.Mathematics;

namespace Engine.Light
{
    public class Spotlight
    {
        public float Width;
        public float Height;
        public Pos Position;
        public List<float> MeshVerticies;
        private float _possum = 0;
        public Vector4 Color;
        public Spotlight(Pos position)
        {
            Width = 5f;
            Height = 5f;
            MeshVerticies = new();
            Position = position;
            Color = new Vector4(1f);
        }
        public void CreateMesh(List<Line> shadowCasters)
        {

            float _npossum = Position.X + Position.Y;
            // Avoid recalculating something twice
            foreach (var line in shadowCasters)
                _npossum += line.First.X + line.First.Y;
            if (_possum == _npossum)
                return;
            _possum = _npossum;


            Pos[] shadowCasterPointArray = new Pos[shadowCasters.Count() * 2 + 4];
            int lastAddedPoint = 0;

            shadowCasterPointArray[shadowCasters.Count() * 2 + 0] = Position + new Pos(Width / 2, Height / 2);
            shadowCasterPointArray[shadowCasters.Count() * 2 + 1] = Position + new Pos(Width / 2, -Height / 2);
            shadowCasterPointArray[shadowCasters.Count() * 2 + 2] = Position + new Pos(-Width / 2, Height / 2);
            shadowCasterPointArray[shadowCasters.Count() * 2 + 3] = Position + new Pos(-Width / 2, -Height / 2);

            shadowCasters.ForEach(elem =>
            {
                shadowCasterPointArray[lastAddedPoint++] = elem.First;
                shadowCasterPointArray[lastAddedPoint++] = elem.Second;
            });

            float[] rayCastAngles = new float[2 * shadowCasterPointArray.Length];

            float[] vertecies = new float[54 * shadowCasterPointArray.Length];

            for (int i = 0; i < rayCastAngles.Length; i += 2)
            {
                rayCastAngles[i] = (MathF.Atan2((shadowCasterPointArray[i / 2].Y - Position.Y), (shadowCasterPointArray[i / 2].X - Position.X))) - 0.01f;

                rayCastAngles[i + 1] = (MathF.Atan2((shadowCasterPointArray[i / 2].Y - Position.Y), (shadowCasterPointArray[i / 2].X - Position.X))) + 0.01f;
            }

            Array.Sort(rayCastAngles);
            List<Pos> intersections = new();
            Pos closest;
            float closestLen = 10000f;
            float currentLen;
            int lastVertexIndex = 0;
            Pos? prev = null;

            for (int i = 0; i < rayCastAngles.Length; i++)
            {
                // Create line to a distance
                Line distant = new(Position, new Pos(500f * MathF.Cos(rayCastAngles[i]), 500f * MathF.Sin(rayCastAngles[i])));

                // Find closest intersection
                intersections.Clear();
                closest = null;
                closestLen = 10000f;
                foreach (var elem in shadowCasters)
                {
                    var p = distant.GetIntersection(elem);
                    if (!(p is null))
                        intersections.Add(p);
                }

                // Check intersections with bounding box
                var boundingBoxIntersection = new Line(new Pos(Position.X + Width / 2, Position.Y + Height / 2),
                    new Pos(Position.X - Width / 2, Position.Y + Height / 2)
                ).GetIntersection(distant);
                if (!(boundingBoxIntersection is null))
                    intersections.Add(boundingBoxIntersection);
                boundingBoxIntersection = new Line(new Pos(Position.X + Width / 2, Position.Y + Height / 2),
                    new Pos(Position.X + Width / 2, Position.Y - Height / 2)
                ).GetIntersection(distant);
                if (!(boundingBoxIntersection is null))
                    intersections.Add(boundingBoxIntersection);
                boundingBoxIntersection = new Line(new Pos(Position.X - Width / 2, Position.Y - Height / 2),
                    new Pos(Position.X + Width / 2, Position.Y - Height / 2)
                ).GetIntersection(distant);
                if (!(boundingBoxIntersection is null))
                    intersections.Add(boundingBoxIntersection);
                boundingBoxIntersection = new Line(new Pos(Position.X - Width / 2, Position.Y - Height / 2),
                    new Pos(Position.X - Width / 2, Position.Y + Height / 2)
                ).GetIntersection(distant);
                if (!(boundingBoxIntersection is null))

                    intersections.Add(boundingBoxIntersection);

                foreach (var elem in intersections)
                {
                    var l = (elem - Position);
                    currentLen = Math.Abs(l.X) + Math.Abs(l.Y);
                    if (currentLen < closestLen)
                    {
                        closest = elem;
                        closestLen = currentLen;
                    }
                }

                // Add intersection to an array
                if (!(prev is null))
                {
                    // Center X Y tX tY
                    vertecies[lastVertexIndex++] = Position.X;
                    vertecies[lastVertexIndex++] = Position.Y;
                    vertecies[lastVertexIndex++] = 0;
                    vertecies[lastVertexIndex++] = 0.5f;
                    vertecies[lastVertexIndex++] = 0.5f;

                    for (int j = 0; j < 4; j++)
                        vertecies[lastVertexIndex++] = Color[j];

                    // Corner1 X Y tX tY
                    vertecies[lastVertexIndex++] = prev.X;
                    vertecies[lastVertexIndex++] = prev.Y;
                    vertecies[lastVertexIndex++] = 0;
                    vertecies[lastVertexIndex++] = -(prev.X - Position.X - Width / 2) / (Width);
                    vertecies[lastVertexIndex++] = -(prev.Y - Position.Y - Height / 2) / (Height);


                    for (int j = 0; j < 4; j++)
                        vertecies[lastVertexIndex++] = Color[j];

                    // Corner2 X Y tX tY
                    vertecies[lastVertexIndex++] = closest.X;
                    vertecies[lastVertexIndex++] = closest.Y;
                    vertecies[lastVertexIndex++] = 0;
                    vertecies[lastVertexIndex++] = -(closest.X - Position.X - Width / 2) / (Width);
                    vertecies[lastVertexIndex++] = -(closest.Y - Position.Y - Height / 2) / (Height);

                    for (int j = 0; j < 4; j++)
                        vertecies[lastVertexIndex++] = Color[j];
                }
                prev = closest;
            }

            lastVertexIndex -= 9;
            vertecies[lastVertexIndex++] = vertecies[9];
            vertecies[lastVertexIndex++] = vertecies[10];
            vertecies[lastVertexIndex++] = vertecies[11];
            vertecies[lastVertexIndex++] = vertecies[12];
            vertecies[lastVertexIndex++] = vertecies[13];

            for (int j = 0; j < 4; j++)
                vertecies[lastVertexIndex++] = Color[j];

            MeshVerticies = vertecies.ToList();
        }
    }
}