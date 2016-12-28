using System;
using System.Collections;
using System.Windows.Input;

namespace WaveShaper.Bezier
{
    public class UndoCommand : ICommand
    {
        private readonly Action restore;
        private readonly ICollection stack;


        public UndoCommand(Action restore, ICollection stack)
        {
            this.restore = restore;
            this.stack = stack;
        }

        public bool CanExecute(object parameter)
        {
            return stack.Count > 0;
        }

        public void Execute(object parameter)
        {
            restore();
            OnCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged;

        public virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
