// ReporteViajeWindow.xaml.cs  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AplicacionDespacho.Models;

namespace AplicacionDespacho
{
    public partial class ReporteViajeWindow : Window
    {
        private Viaje _viaje;
        private List<InformacionPallet> _pallets;

        public ReporteViajeWindow(Viaje viaje, List<InformacionPallet> pallets)
        {
            InitializeComponent();
            _viaje = viaje;
            _pallets = pallets;
            CargarDatos();
        }

        private void CargarDatos()
        {
            // Cargar información del viaje  
            txtFecha.Text = _viaje.Fecha.ToString("dd/MM/yyyy");
            txtNumeroViaje.Text = _viaje.NumeroViaje.ToString();
            txtNumeroGuia.Text = _viaje.NumeroGuia;
            txtResponsable.Text = _viaje.Responsable;
            txtEmpresa.Text = _viaje.NombreEmpresa;
            txtConductor.Text = _viaje.NombreConductor;

            // Cargar pallets  
            dgPallets.ItemsSource = _pallets;

            // Calcular totales  
            txtTotalPallets.Text = _pallets.Count.ToString();
            txtTotalCajas.Text = _pallets.Sum(p => p.NumeroDeCajas).ToString();
            txtPesoTotal.Text = _pallets.Sum(p => p.PesoTotal).ToString("F3");
        }

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Implementar exportación a Excel o PDF  
                MessageBox.Show("Funcionalidad de exportación será implementada próximamente.",
                               "En Desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}