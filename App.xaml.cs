﻿// App.xaml.cs - VERSIÓN ACTUALIZADA  
using System;
using System.Configuration;
using System.Windows;
using AplicacionDespacho.Configuration;

namespace AplicacionDespacho
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Validar configuración al inicio  
                AppConfig.ValidateConfiguration();

                base.OnStartup(e);
            }
            catch (ConfigurationErrorsException ex)
            {
                MessageBox.Show($"Error de configuración: {ex.Message}\n\nLa aplicación se cerrará.",
                               "Error de Configuración",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                Shutdown(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al iniciar: {ex.Message}\n\nLa aplicación se cerrará.",
                               "Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                Shutdown(1);
            }
        }
    }
}