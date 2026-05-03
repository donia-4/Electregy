using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.Consumption
{
    public class ChartDataResponse
    {
        // الوقت بتنسيق "HH:mm" عشان يظهر على المحور الأفقي (X-axis) للرسم البياني
        public string Time { get; set; }

        // إجمالي الكيلو وات المستهلك في الساعة دي (Usage)
        public double Usage { get; set; }

        // التكلفة بالجنيه المصري للساعة دي (Cost)
        public double Cost { get; set; }
    }
}
