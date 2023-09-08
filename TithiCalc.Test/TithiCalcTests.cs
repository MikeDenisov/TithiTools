using System.Globalization;

namespace TithiCalc.Test
{
    [TestFixture]
    public class TithiCalcTests
    {
        [TestCase("01.01.2020", "31.01.2025")]
        [TestCase("01.01.2000", "31.01.2025")]
        [TestCase("15.03.2030", "17.11.2050")]
        [TestCase("31.12.2002", "31.01.2015")]
        public void GetTithiInRange_ValidInput_ReturnsContiniousSequence(string start, string end)
        {
            var startDate = DateTime.ParseExact(start, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(end, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var result = TithiCalc.GetTithiInRange(startDate, endDate);

            Assert.IsNotEmpty(result);

            for (int i = 1; i < result.Count; i++)
            {
                DateTime date = result[i].DateTimeUTC;

                var angle = Math.Abs((int)Math.Round(TithiCalc.GetAngle(date)) - (int)Math.Round(TithiCalc.GetAngle(result[i - 1].DateTimeUTC)));

                Assert.IsTrue(angle == 12, "Should have 12 degrees angle step between neighbouring tithi");;
            }
        }

        [TestCase("01.01.2020", "31.01.2025", new[] { 1, 2 })]
        [TestCase("01.01.2000", "31.01.2025", new[] { 11, 26 })]
        [TestCase("15.03.2030", "17.11.2050", new[] { 4, 9 })]
        [TestCase("31.12.2002", "31.01.2015", new[] { 3, 30 })]
        public void GetTithiInRange_ValidInput_ReturnsFilteredResult(string start, string end, int[] filter)
        {
            var startDate = DateTime.ParseExact(start, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(end, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var filterSet = new HashSet<int>(filter);
            var result = TithiCalc.GetTithiInRange(startDate, endDate, filterSet);

            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.All(tithi => filterSet.Contains(tithi.Index)));
        }

        [Test]
        public void GetTithiInRange_StartDateGreaterThanEndDate_ThrowsException()
        {
            var startDate = new DateTime(2023, 9, 7);
            var endDate = new DateTime(2023, 9, 6);

            Assert.Throws<ArgumentOutOfRangeException>(() => TithiCalc.GetTithiInRange(startDate, endDate));
        }

        [Test]
        public void GetTithiInRange_InvalidPrecision_ThrowsException()
        {
            var startDate = new DateTime(2023, 9, 6);
            var endDate = new DateTime(2023, 9, 7);

            Assert.Throws<ArgumentOutOfRangeException>(() => TithiCalc.GetTithiInRange(startDate, endDate, precision: -0.001d));
        }

        [Test]
        public void GetTithiInRange_ValidInput_ReturnsTithiList()
        {
            var startDate = new DateTime(2023, 9, 6);
            var endDate = new DateTime(2023, 9, 7);

            var tithiList = TithiCalc.GetTithiInRange(startDate, endDate);

            Assert.IsNotNull(tithiList);
            Assert.IsNotEmpty(tithiList);
        }

        [Test]
        public void GetAngle_ValidDateTime_ReturnsAngle()
        {
            var dateTime = new DateTime(2023, 9, 7);

            var angle = TithiCalc.GetAngle(dateTime);

            Assert.GreaterOrEqual(angle, 0);
            Assert.LessOrEqual(angle, 180);
        }

        [Test]
        public void GetTithiByDay_ValidInput_ReturnsTithiArray()
        {
            var dateTime = new DateTime(2023, 9, 7);
            var precision = 0.001d;

            var tithiArray = TithiCalc.GetTithiByDay(dateTime, precision);

            Assert.IsNotNull(tithiArray);
            Assert.IsInstanceOf<DateTime[]>(tithiArray);
        }
    }
}