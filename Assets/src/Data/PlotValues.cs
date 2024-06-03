public class PlotValues
{
    public PlotValues(int[] xValues, float[] yValues, float mean, float standardDeviation, int minValue = 0, int maxValue = 100)
    {
        this.xValues = xValues;
        this.yValues = yValues;
        this.mean = mean;
        this.standardDeviation = standardDeviation;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

    private int[] xValues;
    public int[] XValues => xValues;
    private float[] yValues;
    public float[] YValues => yValues;
    private float mean;
    public float Mean => mean;
    private float standardDeviation;
    public float StandardDeviation => standardDeviation;
    private int minValue;
    public int MinValue => minValue;
    private int maxValue;
    public int MaxValue => maxValue;
}
