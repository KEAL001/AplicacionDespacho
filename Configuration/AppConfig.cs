// Configuration/AppConfig.cs  
using System;
using System.Configuration;

namespace AplicacionDespacho.Configuration
{
    public static class AppConfig
    {
        // Cadenas de conexión  
        public static string PackingSJPConnectionString =>
            ConfigurationManager.ConnectionStrings["PackingSJP"]?.ConnectionString
            ?? throw new InvalidOperationException("Cadena de conexión PackingSJP no encontrada");

        public static string DespachosSJPConnectionString =>
            ConfigurationManager.ConnectionStrings["DespachosSJP"]?.ConnectionString
            ?? throw new InvalidOperationException("Cadena de conexión DespachosSJP no encontrada");

        // Configuraciones de negocio  
        public static int MaxPalletsPerTrip =>
            int.TryParse(ConfigurationManager.AppSettings["MaxPalletsPerTrip"], out int value) ? value : 50;

        public static string DefaultGuidePrefix =>
            ConfigurationManager.AppSettings["DefaultGuidePrefix"] ?? "T004-";

        public static string DefaultResponsible =>
            ConfigurationManager.AppSettings["DefaultResponsible"] ?? "MIRTHA CASTRO";

        public static string DefaultDeparturePoint =>
            ConfigurationManager.AppSettings["DefaultDeparturePoint"] ?? "PIURA";

        public static string DefaultArrivalPoint =>
            ConfigurationManager.AppSettings["DefaultArrivalPoint"] ?? "SULLANA";

        // Configuraciones de logging  
        public static string LogLevel =>
            ConfigurationManager.AppSettings["LogLevel"] ?? "Info";

        public static string LogFilePath =>
            ConfigurationManager.AppSettings["LogFilePath"] ?? "Logs\\AplicacionDespacho.log";

        public static bool EnableFileLogging =>
            bool.TryParse(ConfigurationManager.AppSettings["EnableFileLogging"], out bool value) && value;

        // Configuraciones de UI  
        public static int DataGridPageSize =>
            int.TryParse(ConfigurationManager.AppSettings["DataGridPageSize"], out int value) ? value : 100;

        public static int AutoSaveInterval =>
            int.TryParse(ConfigurationManager.AppSettings["AutoSaveInterval"], out int value) ? value : 300;

        public static bool ShowDebugInfo =>
            bool.TryParse(ConfigurationManager.AppSettings["ShowDebugInfo"], out bool value) && value;

        // Método para validar configuración al inicio  
        public static void ValidateConfiguration()
        {
            try
            {
                var _ = PackingSJPConnectionString;
                var __ = DespachosSJPConnectionString;
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException($"Error en configuración: {ex.Message}", ex);
            }
        }
    }
}