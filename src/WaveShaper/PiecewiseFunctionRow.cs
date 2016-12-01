using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WaveShaper.Annotations;

namespace WaveShaper
{
    public enum Operator
    {
        [Description("<")]
        LessThan,

        [Description("<=")]
        LessOrEqualThan
    }

    public class PiecewiseFunctionRow : INotifyPropertyChanged
    {
        private Operator toOperator;
        private double? to;
        private Operator fromOperator;
        private double? from;
        private string expression;

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
            }
        }

        public string VariableDisplay => "x";

        public string FromDisplay => From?.ToString() ?? "-∞";

        public string ToDisplay => To?.ToString() ?? "∞";

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
