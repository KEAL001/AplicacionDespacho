// Views/MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AplicacionDespacho.Models;
using AplicacionDespacho.Services.DataAccess;
using AplicacionDespacho.ViewModels;

namespace AplicacionDespacho
{
    public partial class MainWindow : Window
    {
        private AccesoDatosViajes _accesoDatosViajes;
        private List<EmpresaTransporte> _listaEmpresas;
        private List<Vehiculo> _listaVehiculos;
        private List<Conductor> _listaConductores;

        public MainWindow()
        {
            InitializeComponent();

            var accesoDatosPallet = new AccesoDatosPallet();
            this.DataContext = new ViewModelPrincipal(accesoDatosPallet);
            _accesoDatosViajes = new AccesoDatosViajes();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatosIniciales();
        }

        private void CargarDatosIniciales()
        {
            _listaEmpresas = _accesoDatosViajes.ObtenerEmpresas();
            comboBoxEmpresa.ItemsSource = _listaEmpresas;
            comboBoxEmpresa.DisplayMemberPath = "NombreEmpresa";
            comboBoxEmpresa.SelectedValuePath = "EmpresaId";

            if (_listaEmpresas.Count > 0)
            {
                comboBoxEmpresa.SelectedIndex = 0;
            }
        }

        private void ComboBoxEmpresa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxEmpresa.SelectedItem is EmpresaTransporte empresaSeleccionada)
            {
                _listaVehiculos = _accesoDatosViajes.ObtenerVehiculosPorEmpresa(empresaSeleccionada.EmpresaId);
                comboBoxPlaca.ItemsSource = _listaVehiculos;
                comboBoxPlaca.DisplayMemberPath = "Placa";
                comboBoxPlaca.SelectedValuePath = "VehiculoId";

                _listaConductores = _accesoDatosViajes.ObtenerConductoresPorEmpresa(empresaSeleccionada.EmpresaId);
                comboBoxConductor.ItemsSource = _listaConductores;
                comboBoxConductor.DisplayMemberPath = "NombreConductor";
                comboBoxConductor.SelectedValuePath = "ConductorId";
            }
            else
            {
                comboBoxPlaca.ItemsSource = null;
                comboBoxConductor.ItemsSource = null;
            }
        }

        private void btnGuardarViaje_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBoxEmpresa.SelectedValue == null || comboBoxPlaca.SelectedValue == null || comboBoxConductor.SelectedValue == null ||
                    string.IsNullOrWhiteSpace(textBoxNumeroViaje.Text) || string.IsNullOrWhiteSpace(textBoxResponsable.Text) ||
                    string.IsNullOrWhiteSpace(textBoxNumeroGuia.Text) || datePickerFecha.SelectedDate == null)
                {
                    MessageBox.Show("Por favor, complete todos los campos y seleccione una empresa, vehículo y conductor.", "Error de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var nuevoViaje = new Viaje
                {
                    Fecha = datePickerFecha.SelectedDate.GetValueOrDefault(),
                    NumeroViaje = int.Parse(textBoxNumeroViaje.Text),
                    Responsable = textBoxResponsable.Text,
                    NumeroGuia = textBoxNumeroGuia.Text,
                    PuntoPartida = textBoxPuntoPartida.Text,
                    PuntoLlegada = textBoxPuntoLlegada.Text,
                    VehiculoId = (int)comboBoxPlaca.SelectedValue,
                    ConductorId = (int)comboBoxConductor.SelectedValue
                };

                _accesoDatosViajes.GuardarViaje(nuevoViaje);
                MessageBox.Show("¡Viaje registrado exitosamente!", "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FormatException)
            {
                MessageBox.Show("El campo 'N° Viaje' debe ser un número válido.", "Error de Formato", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar el viaje: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAbrirAdmin_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            adminWindow.ShowDialog();

            CargarDatosIniciales();
        }
    }
}