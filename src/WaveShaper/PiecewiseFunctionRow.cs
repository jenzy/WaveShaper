using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using WaveShaper.Annotations;

namespace WaveShaper
{
    public enum Operator
    {
        [Description("<")] LessThan,

        [Description("<=")] LessOrEqualThan
    }

    public class PiecewiseFunctionRow : INotifyPropertyChanged
    {
        private Operator toOperator;
        private double? to;
        private Operator fromOperator;
        private double? from;
        private string expression;

        public PiecewiseFunctionRow() : this(ProcessingType.PiecewiseFunction)
        {
        }

        public PiecewiseFunctionRow(ProcessingType mode = ProcessingType.PiecewiseFunction)
        {
            Mode = mode;
        }

        public ProcessingType Mode { get; private set; }

        public double? From
        {
            get { return from; }
            set
            {
                if (value == from) return;
                from = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FromDisplay));
            }
        }

        public Operator FromOperator
        {
            get { return fromOperator; }
            set
            {
                if (value == fromOperator) return;
                fromOperator = value;
                OnPropertyChanged();
            }
        }

        public double? To
        {
            get { return to; }
            set
            {
                if (value == to) return;
                to = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ToDisplay));
            }
        }

        public Operator ToOperator
        {
            get { return toOperator; }
            set
            {
                if (value == toOperator) return;
                toOperator = value;
                OnPropertyChanged();
            }
        }

        public string Expression
        {
            get { return expression; }
            set
            {
                if (value == expression) return;
                expression = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExpressionDisplay));
            }
        }

        public string VariableDisplay => "x";

        public string FromDisplay => From?.ToString() ?? "-∞";

        public string ToDisplay => To?.ToString() ?? "∞";

        public string ExpressionDisplay => Mode == ProcessingType.PiecewiseFunction ? Expression : PolynomialExpressionToDisplayString(Expression);

        public Predicate<double> GetCondition()
        {
            return (x) =>
            {
                bool r = true;

                if (From != null)
                {
                    if (FromOperator == Operator.LessOrEqualThan)
                        r &= From.Value <= x;
                    else if (FromOperator == Operator.LessThan)
                        r &= From.Value < x;
                }

                if (To != null)
                {
                    if (ToOperator == Operator.LessOrEqualThan)
                        r &= x <= To.Value;
                    else if (ToOperator == Operator.LessThan)
                        r &= x < To.Value;
                }

                return r;
            };
        }

        public Func<double, double> GetPolynomialFunction()
        {
            if (Mode != ProcessingType.PiecewisePolynomial)
                throw new InvalidOperationException();

            try
            {
                var coefficients = expression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(double.Parse)
                                             .ToList();

                return x => coefficients.Select((c, i) => c * Math.Pow(x, i)).Sum();
            }
            catch (Exception e)
            {
                throw new ArgumentException("Error in polynomial: " + e.Message, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string PolynomialExpressionToDisplayString(string expression)
        {
            try
            {
                var coefficients = expression.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(decimal.Parse)
                                             .ToList();
               
                var sb = new StringBuilder();
                for (int i = 0; i < coefficients.Count; i++)
                {
                    if (coefficients[i] == 0)
                        continue;

                    if (i == 0)
                    {
                        sb.Append(coefficients[i]);
                    }
                    else
                    {
                        sb.AppendFormat(" {0} ", Math.Sign(coefficients[i]) >= 0 ? '+' : '-');
                        if (coefficients[i] != 1)
                            sb.AppendFormat("{0}⋅", Math.Abs(coefficients[i]));
                        if (i == 1)
                            sb.Append("x");
                        else
                            sb.AppendFormat("x{0}", IntToSuperscript(i));
                    }
                }

                return sb.ToString().TrimStart(' ', '+');
            }
            catch
            {
                return expression;
            }
        }

        private static string IntToSuperscript(int x) => new string(x.ToString().Select(CharToSuperscript).ToArray());

        private static char CharToSuperscript(char c)
        {
            switch (c)
            {
                case '0': return '\x2070';
                case '1': return '\xB9';
                case '2': return '\xB2';
                case '3': return '\xB3';
                case '4': return '\x2074';
                case '5': return '\x2075';
                case '6': return '\x2076';
                case '7': return '\x2077';
                case '8': return '\x2078';
                case '9': return '\x2079';
            }

            throw new ArgumentOutOfRangeException(nameof(c), c, null);
        }
    }
}
