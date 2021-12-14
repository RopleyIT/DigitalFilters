using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalFilters
{
    public class IIRFilterStage
    {
        public double[] CoeffX { get; set; }
        public double[] CoeffY { get; set; }

        public IIRFilterStage(int order)
        {
            CoeffX = new double[order + 1];
            CoeffY = new double[order];
        }
    }
}
