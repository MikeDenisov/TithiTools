using System;

namespace TithiTools
{
    public readonly struct Tithi: IEquatable<Tithi>
    {
        /// <summary>
        /// Datetime UTC when tithi happens
        /// </summary>
        public readonly DateTime DateTimeUTC { get; }

        /// <summary>
        /// Tithi index, starting from 1
        /// </summary>
        public readonly byte Index { get; }

        /// <summary>
        /// Angle between moon and sun
        /// </summary>
        public readonly short Angle { get; }

        public Tithi(byte index, DateTime dateTime, short angle)
        {
            Index = index;
            DateTimeUTC = dateTime;
            Angle = angle;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Tithi other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return DateTimeUTC.GetHashCode();
        }

        public static bool operator ==(Tithi left, Tithi right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Tithi left, Tithi right)
        {
            return !(left == right);
        }

        public bool Equals(Tithi other)
        {
            return DateTimeUTC == other.DateTimeUTC && Index == other.Index && Angle == other.Angle;
        }
    }
}
