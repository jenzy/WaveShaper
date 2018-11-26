using System.ComponentModel;

namespace WaveShaper.Core
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
