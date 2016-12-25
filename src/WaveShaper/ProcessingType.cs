using System.ComponentModel;

namespace WaveShaper
{
    public enum ProcessingType
    {
        [Description("No processing")]
        NoProcessing,

        [Description("Piecewise polynomial")]
        PiecewisePolynomial,

        [Description("Piecewise function")]
        PiecewiseFunction,

        [Description("Bezier curve")]
        Bezier
    }
}
