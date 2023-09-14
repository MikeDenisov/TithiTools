using CoordinateSharp;

using System;
using System.Collections.Generic;
using System.Linq;

namespace TithiTools
{
    public static class TithiCalc
    {
        const int AngleStep = 12;
        const double Precision = 0.001d;

        /// <summary>
        /// Retrieves a list of Tithi for a specified date range.
        /// </summary>
        /// <param name="start">The start date of the range.</param>
        /// <param name="end">The end date of the range.</param>
        /// <param name="indexFilter">Optional set of Tithi indices to filter by.</param>
        /// <param name="precision">Optional precision to control accuracy of tithi time. Higher value will provide higher accuracy but takes more iterations</param>
        /// <returns>A list of Tithi objects within the specified date range.</returns>
        public static IList<Tithi> FindTithiInDateRange(DateTime start, DateTime end, ISet<int>? indexFilter = null, double precision = Precision)
        {
            if (start >= end)
            {
                throw new ArgumentOutOfRangeException(nameof(start), $"{nameof(start)} must be lower than {nameof(end)}");
            }

            if (precision <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), $"{nameof(precision)} must be > 0");
            }

            var result = new List<Tithi>();

            if (end < start)
            {
                return result;
            }

            var current = start.Date;

            var previousInd = 0;

            while (current <= end.Date)
            {
                var indayTithi = FindTithiByDay(current, precision);

                foreach (var tithiDateTime in indayTithi)
                {
                    var (ind, angle) = GetTithiIndexAndAngle(tithiDateTime);

                    if (previousInd == ind)
                    {
                        // Skip duplicate (happens when tithi starts very close to the midnight)
                        continue;
                    }

                    if (indexFilter == null || indexFilter.Contains(ind))
                    {
                        result.Add(new Tithi(ind, tithiDateTime, angle));
                    }

                    previousInd = ind;
                }

                current = current.AddDays(1);
            }

            return result;
        }

        /// <summary>
        /// Retrieves Tithi datetime that begins on a given day. May result in overlaps for consecutive days near midnight.
        /// </summary>
        /// <param name="dt">The date to find the Tithi for.</param>
        /// <returns>An array of Tithi datetimes that begin on the specified day.</returns>
        public static DateTime[] FindTithiByDay(DateTime dt, double precision)
        {
            if (precision <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), $"{nameof(precision)} must be > 0");
            }

            const int StepMinutes = 1;
            var dayStart = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
            var dayEnd = new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);


            // Determine the movement direction during the day
            var day_dir = GetMovementDirection(dayStart, dayEnd);
            var start_dir = GetMovementDirection(dayStart, dayStart.AddMinutes(StepMinutes));
            var end_dir = GetMovementDirection(dayEnd.AddMinutes(-StepMinutes), dayEnd);
            var isExtrema = !(day_dir == end_dir && day_dir == start_dir);

            if (isExtrema)
            {
                var extremaTime = GetExtremaTimeBinary(dayStart, dayEnd, start_dir > 0, precision);
                var result = new List<DateTime>(2);

                // Check if it's not a single tithi this day
                if ((extremaTime - dayStart).TotalHours > 12)
                {
                    var to_left = GetInRangeBinary(dayStart, extremaTime.AddHours(-12), start_dir, precision);
                    result.AddRange(to_left);
                }

                result.Add(extremaTime);

                if ((dayEnd - extremaTime).TotalHours > 12)
                {
                    var to_right = GetInRangeBinary(extremaTime.AddHours(12), dayEnd, end_dir, precision);
                    result.AddRange(to_right);
                }

                return result.ToArray();
            }
            else
            {
                return GetInRangeBinary(dayStart, dayEnd, day_dir, precision);
            }
        }

        /// <summary>
        /// Calculates the angle between the sun and moon on a given UTC datetime.
        /// The angle is in degrees and falls within the range of 0-180.
        /// </summary>
        /// <param name="dt">The UTC datetime for angle calculation.</param>
        /// <returns>The angle between the sun and moon in degrees.</returns>
        public static double GetAngle(DateTime dt)
        {
            var moon = Celestial.Get_Lunar_Coordinate(dt);
            var sun = Celestial.Get_Solar_Coordinate(dt);
            var moonLng = moon.Longitude;
            var sunLng = sun.Longitude;

            var res = Math.Abs(moonLng - sunLng);

            return res > 180 ? 360 - res : res;
        }

        /// <summary>
        /// Gets movement dynamics between two angles. Positive if movement goes from 0 to 180 degrees, Negative if reverse.
        /// </summary>
        /// <param name="start">The start datetime for angle calculation.</param>
        /// <param name="end">The end datetime for angle calculation.</param>
        /// <returns>The movement direction: 1 for positive (0 to 180), -1 for negative (reverse).</returns>
        private static int GetMovementDirection(DateTime start, DateTime end)
        {
            var start_angle = (Math.PI / 180) * GetAngle(start);
            var end_angle = (Math.PI / 180) * GetAngle(end);
            var dir = Math.Cos(start_angle) - Math.Cos(end_angle);

            return dir >= 0 ? 1 : -1;
        }

        /// <summary>
        /// Gets the Tithi index and angle for a given datetime.
        /// </summary>
        /// <param name="tithiTime">The datetime for which to calculate the Tithi index and angle.</param>
        /// <returns>A tuple containing the Tithi index and angle.</returns>
        private static (byte, short) GetTithiIndexAndAngle(DateTime tithiTime)
        {
            var angle = (short)Math.Round(GetAngle(tithiTime));
            var direction = GetMovementDirection(tithiTime, tithiTime.AddHours(1));
            if (direction < 0)
            {
                angle = (short)(180 + (180 - angle));
            }
            if (angle == 360)
            {
                angle = 0;
            }

            var ind = (byte)((angle / 12) + 1);
            return (ind, angle);
        }

        /// <summary>
        /// Gets the Tithi datetimes within a specified range using binary search.
        /// </summary>
        /// <param name="start">The start datetime of the range.</param>
        /// <param name="end">The end datetime of the range.</param>
        /// <param name="direction">The movement direction: 1 for positive (0 to 180), -1 for negative (reverse).</param>
        /// <param name="precision">The precision to control the accuracy of tithi time calculation.</param>
        /// <returns>An array of Tithi datetimes within the specified range.</returns>
        private static DateTime[] GetInRangeBinary(DateTime start, DateTime end, int direction, double precision)
        {

            var result = new List<DateTime>(2);
            var mid = start + (end - start) / 2;
            var reverse = direction < 0;

            if (TryGetTithi(start, mid, reverse, precision, out var morningTithiTime))
            {
                result.Add(morningTithiTime);
            }

            if (result.Any())
            {
                if (TryGetTithi(morningTithiTime.AddHours(12), end, reverse, precision, out var eveningTithiTime))
                {
                    result.Add(eveningTithiTime);
                }
            }
            else
            {
                if (TryGetTithi(mid, end, reverse, precision, out var eveningTithiTime))
                {
                    result.Add(eveningTithiTime);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Tries to calculate a Tithi datetime within a specified range.
        /// </summary>
        /// <param name="start">The start datetime of the range.</param>
        /// <param name="end">The end datetime of the range.</param>
        /// <param name="reverse">Indicates whether the movement is in reverse direction.</param>
        /// <param name="precision">The precision to control the accuracy of tithi time calculation.</param>
        /// <param name="result">The calculated Tithi datetime if successful.</param>
        /// <returns>True if a Tithi datetime was successfully calculated, otherwise false.</returns>
        private static bool TryGetTithi(DateTime start, DateTime end, bool reverse, double precision, out DateTime result)
        {

            var mid = start + (end - start) / 2;

            var start_angle = GetNormalizedAngle(start, reverse);
            var end_angle = GetNormalizedAngle(end, reverse);
            var mid_angle = GetNormalizedAngle(mid, reverse);

            if (start_angle <= precision || start_angle >= AngleStep - precision)
            {
                result = start;
                return true;
            }

            if (end_angle <= precision || end_angle >= AngleStep - precision)
            {
                result = end;
                return true;
            }

            if (mid_angle < start_angle && mid_angle < end_angle)
            {
                result = GetTithiBinary(start, mid, reverse, precision);
                return true;
            }
            if (mid_angle > start_angle && mid_angle > end_angle)
            {
                result = GetTithiBinary(mid, end, reverse, precision);
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Calculates a Tithi datetime within a specified range using binary search.
        /// </summary>
        /// <param name="start">The start datetime of the range.</param>
        /// <param name="end">The end datetime of the range.</param>
        /// <param name="reverse">Indicates whether the movement is in reverse direction.</param>
        /// <param name="precision">The precision to control the accuracy of tithi time calculation.</param>
        /// <returns>The calculated Tithi datetime.</returns>
        private static DateTime GetTithiBinary(DateTime start, DateTime end, bool reverse, double precision)
        {
            var mid = start + (end - start) / 2;

            var start_angle = GetNormalizedAngle(start, reverse, AngleStep);
            var end_angle = GetNormalizedAngle(end, reverse);
            var mid_angle = GetNormalizedAngle(mid, reverse, AngleStep);

            if (start_angle <= precision || start_angle >= AngleStep - precision)
            {
                return start;
            }

            if (end_angle <= precision || end_angle >= AngleStep - precision)
            {
                return end;
            }

            while (!(mid_angle <= precision || mid_angle >= AngleStep - precision))
            {
                if (start_angle < mid_angle)
                {
                    start = mid;
                    start_angle = mid_angle;
                }
                else
                {
                    end = mid;
                }

                mid = start + (end - start) / 2;

                mid_angle = GetNormalizedAngle(mid, reverse, AngleStep);
            }

            return mid;
        }

        /// <summary>
        /// Calculates the extrema time within a specified range using binary search.
        /// </summary>
        /// <param name="start">The start datetime of the range.</param>
        /// <param name="end">The end datetime of the range.</param>
        /// <param name="reverse">Indicates whether the movement is in reverse direction.</param>
        /// <param name="precision">The precision to control the accuracy of tithi time calculation.</param>
        /// <returns>The calculated extrema time.</returns>
        private static DateTime GetExtremaTimeBinary(DateTime start, DateTime end, bool reverse, double precision)
        {
            const double StepMinutes = 1;

            var mid = start + (end - start) / 2;
            var start_angle = GetNormalizedAngle(start, reverse);
            var end_angle = GetNormalizedAngle(end, reverse);
            var mid_angle = GetNormalizedAngle(mid, reverse);

            if (start_angle <= precision || start_angle >= AngleStep - precision)
            {
                return start;
            }

            if (end_angle <= precision || end_angle >= AngleStep - precision)
            {
                return end;
            }

            while (!(mid_angle <= precision || mid_angle >= AngleStep - precision))
            {
                if ((end - mid).TotalMinutes < StepMinutes)
                {
                    return mid;
                }

                var dir = GetNormalizedAngle(mid.AddMinutes(StepMinutes), reverse);
                if (dir > mid_angle)
                {
                    end = mid;
                }
                else
                {
                    start = mid;
                }

                mid = start + (end - start) / 2;
                mid_angle = GetNormalizedAngle(mid, reverse);
            }

            return mid;
        }

        private static double GetNormalizedAngle(DateTime dt, bool reverse, int range = AngleStep)
        {
            return reverse ? range - NormalizeAngle(GetAngle(dt), range) : NormalizeAngle(GetAngle(dt), range);
        }

        private static double NormalizeAngle(double angle, int range = AngleStep)
        {
            return angle - ((int)angle / range) * range;
        }
    }
}
