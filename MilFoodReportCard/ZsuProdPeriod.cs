using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilFoodReportCard
{
    public class ZsuProdPeriod
    {
        public int Year { get; private set; }
        public int Period { get; private set; }
        public int Week { get; private set; }

        private ZsuProdPeriod() { }

        public static ZsuProdPeriod GetZsuProdPeriod(DateTime date)
        {
            var rv = new ZsuProdPeriod();

            var yearFirst = new DateTime(date.Year, 1, 1);
            var yearNext = new DateTime(date.Year + 1, 1, 1);

            var offset = date.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int)date.DayOfWeek) - 1;
            var periodStart = date.AddDays(offset * -1);
            //Console.WriteLine("This period start: " + periodStart.ToShortDateString());
            offset = yearFirst.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int)yearFirst.DayOfWeek) - 1;
            var tyStart = yearFirst.AddDays(offset * -1);
            //Console.WriteLine("First this year period start: " + tyStart.ToShortDateString());
            offset = yearNext.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int)yearNext.DayOfWeek) - 1;
            var nyStart = yearNext.AddDays(offset * -1);
            //Console.WriteLine("First next year period start: " + nyStart.ToShortDateString());
            if (periodStart.CompareTo(nyStart) >= 0)
            {
                rv.Year = yearNext.Year;
                rv.Period = 1;
                rv.Week = 1;
            }
            else
            {
                var diffDays = periodStart.Subtract(tyStart).Days;
                rv.Year = date.Year;
                rv.Period = (diffDays / (7 * 4)) + 1;
                rv.Week = ((diffDays / 7) % 4) + 1;
            }

            return rv;
        }

        public ZsuProdPeriod(DateTime date)
        {
            var zpp = GetZsuProdPeriod(date);
            Year = zpp.Year;
            Period = zpp.Period;
            Week = zpp.Week;
        }

        public DateTime GetStartDate()
        {
            var jan1 = new DateTime(Year, 1, 1);
            var offset = jan1.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int)jan1.DayOfWeek) - 1;
            var tyStart = jan1.AddDays(offset * -1);
            return tyStart.AddDays(((Period - 1) * 4 + (Week - 1)) * 7);
        }

        public DateTime GetEndDate()
        {
            return GetStartDate().AddDays(7).AddMilliseconds(-1);
        }
    }
}
