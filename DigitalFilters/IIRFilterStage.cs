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
