// ViewModels/ViewModelPrincipal.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AplicacionDespacho.Models;
using AplicacionDespacho.Services.DataAccess;

namespace AplicacionDespacho.ViewModels
{
    public class ViewModelPrincipal : INotifyPropertyChanged
    {
        private readonly IAccesoDatosPallet _accesoDatos;

        private string _entradaNumeroPallet;
        public string EntradaNumeroPallet
        {
            get => _entradaNumeroPallet;
            set
            {
                _entradaNumeroPallet = value;
                OnPropertyChanged(nameof(EntradaNumeroPallet));
            }
        }

        public ObservableCollection<InformacionPallet> PalletsEscaneados { get; } = new ObservableCollection<InformacionPallet>();

        private InformacionPallet _ultimoPalletEscaneado;
        public InformacionPallet UltimoPalletEscaneado
        {
            get => _ultimoPalletEscaneado;
            set
            {
                _ultimoPalletEscaneado = value;
                OnPropertyChanged(nameof(UltimoPalletEscaneado));
            }
        }

        public ICommand ComandoEscanear { get; }
        public ICommand ComandoFinalizarDespacho { get; }

        public ViewModelPrincipal(IAccesoDatosPallet accesoDatos)
        {
            _accesoDatos = accesoDatos;
            ComandoEscanear = new ComandoRelevo(EscanearPallet, PuedeEscanearPallet);
            ComandoFinalizarDespacho = new ComandoRelevo(FinalizarDespacho, PuedeFinalizarDespacho);
        }

        private void EscanearPallet(object parameter)
        {
            InformacionPallet pallet = _accesoDatos.ObtenerDatosPallet(EntradaNumeroPallet);

            if (pallet != null)
            {
                PalletsEscaneados.Add(pallet);
                UltimoPalletEscaneado = pallet;
                EntradaNumeroPallet = string.Empty;
            }
            else
            {
                MessageBox.Show("El número de pallet no se encontró en la base de datos.", "Error de Búsqueda", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool PuedeEscanearPallet(object parameter)
        {
            return PalletsEscaneados.Count < 50;
        }

        private void FinalizarDespacho(object parameter)
        {
            if (PalletsEscaneados.Count > 0)
            {
                MessageBox.Show($"Despacho finalizado con éxito. Se registraron {PalletsEscaneados.Count} pallets.", "Despacho Completo", MessageBoxButton.OK, MessageBoxImage.Information);
                PalletsEscaneados.Clear();
                UltimoPalletEscaneado = null;
            }
            else
            {
                MessageBox.Show("No hay pallets registrados para finalizar el despacho.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool PuedeFinalizarDespacho(object parameter)
        {
            return PalletsEscaneados.Count > 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string nombrePropiedad)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}