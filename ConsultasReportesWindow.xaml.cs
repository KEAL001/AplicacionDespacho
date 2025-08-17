// ConsultasReportesWindow.xaml.cs - CÓDIGO COMPLETO ACTUALIZADO  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AplicacionDespacho.Models;
using AplicacionDespacho.Services.DataAccess;

namespace AplicacionDespacho
{
    public partial class ConsultasReportesWindow : Window
    {
        private AccesoDatosViajes _accesoDatosViajes;
        private List<EmpresaTransporte> _listaEmpresas;
        private List<Conductor> _listaConductores;
        private List<Viaje> _resultadosBusqueda;

        public ConsultasReportesWindow()
        {
            InitializeComponent();
            _accesoDatosViajes = new AccesoDatosViajes();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatosIniciales();
        }

        private void CargarDatosIniciales()
        {
            try
            {
                // Cargar empresas    
                _listaEmpresas = _accesoDatosViajes.ObtenerEmpresas();
                cmbEmpresa.ItemsSource = _listaEmpresas;
                cmbEmpresa.DisplayMemberPath = "NombreEmpresa";
                cmbEmpresa.SelectedValuePath = "EmpresaId";

                // Agregar opción "Todas" al inicio    
                var empresasConTodas = new List<EmpresaTransporte>
                {
                    new EmpresaTransporte { EmpresaId = -1, NombreEmpresa = "-- Todas las Empresas --" }
                };
                empresasConTodas.AddRange(_listaEmpresas);
                cmbEmpresa.ItemsSource = empresasConTodas;

                // Cargar todos los viajes inicialmente    
                BuscarViajes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos iniciales: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbEmpresa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEmpresa.SelectedItem is EmpresaTransporte empresaSeleccionada && empresaSeleccionada.EmpresaId != -1)
            {
                try
                {
                    _listaConductores = _accesoDatosViajes.ObtenerConductoresPorEmpresa(empresaSeleccionada.EmpresaId);

                    var conductoresConTodos = new List<Conductor>
                    {
                        new Conductor { ConductorId = -1, NombreConductor = "-- Todos los Conductores --" }
                    };
                    conductoresConTodos.AddRange(_listaConductores);

                    cmbConductor.ItemsSource = conductoresConTodos;
                    cmbConductor.DisplayMemberPath = "NombreConductor";
                    cmbConductor.SelectedValuePath = "ConductorId";
                    cmbConductor.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar conductores: {ex.Message}", "Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                cmbConductor.ItemsSource = new List<Conductor>
                {
                    new Conductor { ConductorId = -1, NombreConductor = "-- Todos los Conductores --" }
                };
                cmbConductor.SelectedIndex = 0;
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarViajes();
        }

        // MÉTODO ACTUALIZADO CON BÚSQUEDA POR PALLET  
        private void BuscarViajes()
        {
            try
            {
                string numeroGuia = string.IsNullOrWhiteSpace(txtNumeroGuia.Text) ? null : txtNumeroGuia.Text.Trim();
                string numeroPallet = string.IsNullOrWhiteSpace(txtNumeroPallet.Text) ? null : txtNumeroPallet.Text.Trim().ToUpper();
                int? empresaId = (cmbEmpresa.SelectedValue != null && (int)cmbEmpresa.SelectedValue != -1) ? (int)cmbEmpresa.SelectedValue : null;
                int? conductorId = (cmbConductor.SelectedValue != null && (int)cmbConductor.SelectedValue != -1) ? (int)cmbConductor.SelectedValue : null;
                DateTime? fechaDesde = dpFechaDesde.SelectedDate;
                DateTime? fechaHasta = dpFechaHasta.SelectedDate;

                _resultadosBusqueda = _accesoDatosViajes.BuscarViajesConFiltros(numeroGuia, empresaId, conductorId, fechaDesde, fechaHasta, numeroPallet);
                dgResultados.ItemsSource = _resultadosBusqueda;

                // Mostrar mensaje si no hay resultados    
                if (_resultadosBusqueda.Count == 0)
                {
                    MessageBox.Show("No se encontraron viajes que coincidan con los criterios de búsqueda.",
                                   "Sin Resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar viajes: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // MÉTODO ACTUALIZADO CON LIMPIEZA DEL CAMPO PALLET  
        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtNumeroGuia.Text = string.Empty;
            txtNumeroPallet.Text = string.Empty; // NUEVO: Limpiar campo de pallet  
            cmbEmpresa.SelectedIndex = 0;
            cmbConductor.SelectedIndex = 0;
            dpFechaDesde.SelectedDate = null;
            dpFechaHasta.SelectedDate = null;

            // Recargar todos los viajes    
            BuscarViajes();
        }

        private void dgResultados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Habilitar botones según la selección    
            bool haySeleccion = dgResultados.SelectedItem != null;
            btnGenerarReporte.IsEnabled = haySeleccion;
            btnImprimirDespacho.IsEnabled = haySeleccion;
        }

        private void btnGenerarReporte_Click(object sender, RoutedEventArgs e)
        {
            if (dgResultados.SelectedItem is Viaje viajeSeleccionado)
            {
                try
                {
                    // Obtener pallets del viaje    
                    var pallets = _accesoDatosViajes.ObtenerPalletsDeViaje(viajeSeleccionado.ViajeId);

                    // Crear ventana de reporte    
                    var ventanaReporte = new ReporteViajeWindow(viajeSeleccionado, pallets);
                    ventanaReporte.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Seleccione un viaje para generar el reporte.", "Selección Requerida",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnImprimirDespacho_Click(object sender, RoutedEventArgs e)
        {
            if (dgResultados.SelectedItem is Viaje viajeSeleccionado)
            {
                try
                {
                    // Obtener pallets del viaje    
                    var pallets = _accesoDatosViajes.ObtenerPalletsDeViaje(viajeSeleccionado.ViajeId);

                    // Crear ventana de impresión    
                    var ventanaImpresion = new ImpresionDespachoWindow(viajeSeleccionado, pallets);
                    ventanaImpresion.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al preparar impresión: {ex.Message}", "Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Seleccione un viaje para imprimir el despacho.", "Selección Requerida",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}