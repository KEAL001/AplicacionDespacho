using System;

namespace AplicacionDespacho.Models
{
    public class Viaje
    {
        public int ViajeId { get; set; }
        public DateTime Fecha { get; set; }
        public int NumeroViaje { get; set; }
        public string Responsable { get; set; }
        public string NumeroGuia { get; set; }
        public string PuntoPartida { get; set; }
        public string PuntoLlegada { get; set; }
        public int VehiculoId { get; set; }
        public int ConductorId { get; set; }

        // Nuevos campos agregados  
        public string Estado { get; set; } = "Activo";
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public string UsuarioModificacion { get; set; }

        // Propiedades de navegación para mostrar información relacionada  
        public string NombreEmpresa { get; set; }
        public string NombreConductor { get; set; }
        public string PlacaVehiculo { get; set; }
    }
}