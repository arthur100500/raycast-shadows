namespace Engine
{
    public class Line
    {
        public Pos First;
        public Pos Second;

        public Line(Pos fst, Pos snd)
        {
            First = fst;
            Second = snd;
        }

        public static Line LineCP(Pos fst, Pos snd)
        {
            return new Line(fst.Copy(), snd.Copy());
        }

        public override string ToString()
        {
            return "Segment: A(" + First.ToString() + "), B(" + Second.ToString() + ")";
        }

        public Pos? GetIntersection(Line other)
        {
            double deltaACy = this.First.Y - other.First.Y;
            double deltaDCx = other.Second.X - other.First.X;
            double deltaACx = this.First.X - other.First.X;
            double deltaDCy = other.Second.Y - other.First.Y;
            double deltaBAx = this.Second.X - this.First.X;
            double deltaBAy = this.Second.Y - this.First.Y;

            double denominator = deltaBAx * deltaDCy - deltaBAy * deltaDCx;
            double numerator = deltaACy * deltaDCx - deltaACx * deltaDCy;

            if (denominator == 0)
            {
                if (numerator == 0)
                {
                    // collinear. Potentially infinite intersection points.
                    // Check and return one of them.
                    if (this.First.X >= other.First.X && this.First.X <= other.Second.X)
                    {
                        return this.First;
                    }
                    else if (other.First.X >= this.First.X && other.First.X <= this.Second.X)
                    {
                        return other.First;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                { // parallel
                    return null;
                }
            }

            double r = numerator / denominator;
            if (r < 0 || r > 1)
            {
                return null;
            }

            double s = (deltaACy * deltaBAx - deltaACx * deltaBAy) / denominator;
            if (s < 0 || s > 1)
            {
                return null;
            }

            return new Pos((float)(this.First.X + r * deltaBAx), (float)(this.First.Y + r * deltaBAy));
        }
    }
}