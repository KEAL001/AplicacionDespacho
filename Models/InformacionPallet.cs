using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Models/InformacionPallet.cs
namespace AplicacionDespacho.Models
{
    public class InformacionPallet
    {
        public string NumeroPallet { get; set; }
        public string Variedad { get; set; }
        public string Calibre { get; set; }
        public string Embalaje { get; set; }
        public int NumeroDeCajas { get; set; }
        public int ViajeId { get; set; }
    }
}