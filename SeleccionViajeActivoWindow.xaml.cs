using System.Collections.Generic;
using System.Windows;
using AplicacionDespacho.Models;

namespace AplicacionDespacho
{
    public partial class SeleccionViajeActivoWindow : Window
    {
        public Viaje ViajeSeleccionado { get; private set; }

        public SeleccionViajeActivoWindow(List<Viaje> viajesActivos)
        {
            InitializeComponent();
            dgViajesActivos.ItemsSource = viajesActivos;
        }

        private void btnContinuar_Click(object sender, RoutedEventArgs e)
        {
            if (dgViajesActivos.SelectedItem is Viaje viajeSeleccionado)
            {
                ViajeSeleccionado = viajeSeleccionado;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un viaje para continuar.", "Selección Requerida",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}