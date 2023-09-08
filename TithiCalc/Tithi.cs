namespace TithiCalc
{
    public struct Tithi
    {
        /// <summary>
        /// Datetime UTC when tithi happens
        /// </summary>
        public readonly DateTime DateTimeUTC;

        /// <summary>
        /// Tithi index, starting from 1
        /// </summary>
        public readonly byte Index;

        /// <summary>
        /// Angle between moon and sun
        /// </summary>
        public readonly short Angle;

        public Tithi(byte index, DateTime dateTime, short angle)
        {
            Index = index;
            DateTimeUTC = dateTime;
            Angle = angle;
        }
    }
}
