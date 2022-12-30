using System;

namespace Engine
{
    /// <summary>
    ///  Class for storing 2d integer position
    /// </summary>
    public class iPos
    {
        public int X;
        public int Y;

        public iPos(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public static bool operator ==(iPos obj1, iPos obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;
            return obj1.X == obj2.X && obj1.Y == obj2.Y;
        }

        public static bool operator !=(iPos obj1, iPos obj2)
        {
            return !(obj1 == obj2);
        }

        public static iPos operator +(iPos obj1, iPos obj2)
        {
            return new iPos(obj1.X + obj2.X, obj1.Y + obj2.Y);
        }

        public static iPos? operator *(iPos obj1, int another)
        {
            if (obj1 is null)
                return null;
            return new iPos(obj1.X * another, obj1.Y * another);
        }

        public override string ToString()
        {
            return Convert.ToString(X) + " " + Convert.ToString(Y);
        }

        public iPos Copy()
        {
            return new iPos(X, Y);
        }

        public void Add(iPos another)
        {
            X += another.X;
            Y += another.Y;
        }

        public static explicit operator iPos(Pos v)
        {
            return new iPos((int)Math.Floor(v.X), (int)Math.Floor(v.Y));
        }
    }

    /// <summary>
    ///  Class for storing 2d float position
    /// </summary>
    public class Pos
    {
        public float X;
        public float Y;

        public Pos(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Pos(iPos pos)
        {
            this.X = pos.X;
            this.Y = pos.Y;
        }

        public static bool operator ==(Pos obj1, Pos obj2)
        {
            return obj1.X == obj2.X && obj1.Y == obj2.Y;
        }

        public static bool operator !=(Pos obj1, Pos obj2)
        {
            return !(obj1 == obj2);
        }

        public static Pos operator +(Pos obj1, Pos obj2)
        {
            return new Pos(obj1.X + obj2.X, obj1.Y + obj2.Y);
        }

        public static Pos operator +(Pos obj1, iPos obj2)
        {
            return new Pos(obj1.X + obj2.X, obj1.Y + obj2.Y);
        }

        public static Pos operator +(iPos obj1, Pos obj2)
        {
            return new Pos(obj1.X + obj2.X, obj1.Y + obj2.Y);
        }

        public static Pos operator -(Pos obj1, Pos obj2)
        {
            return new Pos(obj1.X - obj2.X, obj1.Y - obj2.Y);
        }

        public static Pos operator *(Pos obj1, float another)
        {
            return new Pos(obj1.X * another, obj1.Y * another);
        }

        public static Pos operator *(float another, Pos obj1)
        {
            return new Pos(obj1.X * another, obj1.Y * another);
        }

        public override string ToString()
        {
            return Convert.ToString(X) + " " + Convert.ToString(Y);
        }

        public Pos Copy()
        {
            return new Pos(X, Y);
        }

        public void Add(Pos another)
        {
            X += another.X;
            Y += another.Y;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            throw new NotImplementedException();
        }
    }
}