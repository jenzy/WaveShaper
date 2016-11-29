using System.ComponentModel;

namespace WaveShaper
{
    public enum ProcessingType
    {
        [Description("No processing")]
        NoProcessing,

        [Description("Piecewise function")]
        PiecewiseFunction
    }
}
