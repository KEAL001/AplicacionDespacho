using System;
using System.Windows;
using System.Windows.Controls;
using AplicacionDespacho.Models;
using AplicacionDespacho.Services.DataAccess;
using AplicacionDespacho.ViewModels;

namespace AplicacionDespacho
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var accesoDatosPallet = new AccesoDatosPallet();
            this.DataContext = new ViewModelPrincipal(accesoDatosPallet);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ya no necesitamos cargar datos iniciales aquí      
            // porque se manejan en las ventanas modales      
        }

        private void btnAbrirAdmin_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            adminWindow.ShowDialog();
        }

        private void btnConsultasReportes_Click(object sender, RoutedEventArgs e)
        {
            var ventanaConsultas = new ConsultasReportesWindow();
            ventanaConsultas.ShowDialog();
        }

        private void btnGestionPesos_Click(object sender, RoutedEventArgs e)
        {
            var ventanaPesos = new PesosPorEmbalajeWindow();
            ventanaPesos.ShowDialog();
        }
        
        private void btnRevertir_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ViewModelPrincipal;
            if (viewModel?.ComandoRevertirCambios.CanExecute(null) == true)
            {
                viewModel.ComandoRevertirCambios.Execute(null);
            }
            else
            {
                MessageBox.Show("No hay pallet seleccionado para revertir.", "Advertencia",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Método para manejo de selección de pallets  
        private void dgPallets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as ViewModelPrincipal;
            if (viewModel?.PalletSeleccionado != null)
            {
                System.Diagnostics.Debug.WriteLine($"Pallet seleccionado: {viewModel.PalletSeleccionado.NumeroPallet}");
            }
        }
    }
}