using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplicacionDespacho.Models
{
    // Models/Viaje.cs
    

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
    }
}
