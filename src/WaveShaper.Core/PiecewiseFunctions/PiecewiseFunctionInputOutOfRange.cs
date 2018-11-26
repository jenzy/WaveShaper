using System;

namespace WaveShaper.Core.PiecewiseFunctions
{
    public class PiecewiseFunctionInputOutOfRange : ArgumentOutOfRangeException
    {
        public PiecewiseFunctionInputOutOfRange()
        {
        }

        public PiecewiseFunctionInputOutOfRange(string paramName) : base(paramName)
        {
        }

        public PiecewiseFunctionInputOutOfRange(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }
    }
}