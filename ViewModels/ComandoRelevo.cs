// ViewModels/ComandoRelevo.cs
using System;
using System.Windows.Input;

namespace AplicacionDespacho.ViewModels
{
    public class ComandoRelevo : ICommand
    {
        private readonly Action<object> _ejecutar;
        private readonly Func<object, bool> _puedeEjecutar;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public ComandoRelevo(Action<object> ejecutar, Func<object, bool> puedeEjecutar = null)
        {
            _ejecutar = ejecutar ?? throw new ArgumentNullException(nameof(ejecutar));
            _puedeEjecutar = puedeEjecutar;
        }

        public bool CanExecute(object parameter)
        {
            return _puedeEjecutar == null || _puedeEjecutar(parameter);
        }

        public void Execute(object parameter)
        {
            _ejecutar(parameter);
        }
    }
}