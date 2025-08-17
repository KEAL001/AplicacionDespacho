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

        // Métodos para gestión de edición de pallets - IMPLEMENTADOS CORRECTAMENTE  
        private void btnAplicarCambios_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ViewModelPrincipal;
            if (viewModel?.UltimoPalletEscaneado != null)
            {
                // Obtener los valores actuales de los campos de edición  
                var palletEditado = viewModel.UltimoPalletEscaneado;

                // Verificar si hay un pallet seleccionado para actualizar  
                if (viewModel.PalletSeleccionado != null)
                {
                    // Verificar si hubo cambios comparando con valores originales  
                    bool huboModificacion =
                        palletEditado.Variedad != palletEditado.VariedadOriginal ||
                        palletEditado.Calibre != palletEditado.CalibreOriginal ||
                        palletEditado.Embalaje != palletEditado.EmbalajeOriginal ||
                        palletEditado.NumeroDeCajas != palletEditado.NumeroDeCajasOriginal;

                    // Actualizar el pallet en la colección  
                    var index = viewModel.PalletsEscaneados.IndexOf(viewModel.PalletSeleccionado);
                    if (index >= 0)
                    {
                        palletEditado.Modificado = huboModificacion;
                        palletEditado.FechaModificacion = huboModificacion ? DateTime.Now : viewModel.PalletSeleccionado.FechaModificacion;

                        // Recalcular peso si cambió el embalaje o número de cajas  
                        if (huboModificacion && (palletEditado.Embalaje != palletEditado.EmbalajeOriginal ||
                            palletEditado.NumeroDeCajas != palletEditado.NumeroDeCajasOriginal))
                        {
                            var accesoDatosViajes = new AccesoDatosViajes();
                            var pesoEmbalaje = accesoDatosViajes.ObtenerPesoEmbalaje(palletEditado.Embalaje);
                            if (pesoEmbalaje != null)
                            {
                                palletEditado.PesoUnitario = pesoEmbalaje.PesoUnitario;
                                palletEditado.PesoTotal = palletEditado.NumeroDeCajas * pesoEmbalaje.PesoUnitario;
                            }
                        }

                        viewModel.PalletsEscaneados[index] = palletEditado;
                        viewModel.PalletSeleccionado = palletEditado;

                        // Actualizar totales en el ViewModel  
                        viewModel.ActualizarTotales();

                        MessageBox.Show(huboModificacion ? "Cambios aplicados. Pallet marcado como modificado." : "Cambios aplicados.",
                                       "Edición", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("No hay pallet seleccionado para aplicar cambios.", "Advertencia",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
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
            // El binding automático ya maneja la selección  
            var viewModel = this.DataContext as ViewModelPrincipal;
            if (viewModel?.PalletSeleccionado != null)
            {
                System.Diagnostics.Debug.WriteLine($"Pallet seleccionado: {viewModel.PalletSeleccionado.NumeroPallet}");
            }
        }
    }
}