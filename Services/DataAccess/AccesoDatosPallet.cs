// Services/DataAccess/AccesoDatosPallet.cs
using AplicacionDespacho.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace AplicacionDespacho.Services.DataAccess
{
    public class AccesoDatosPallet : IAccesoDatosPallet
    {
        private readonly string _cadenaConexion;
        private readonly string _cadenaConexionPallet;
        private readonly string _cadenaConexionViajes;

        public AccesoDatosPallet()
        {
            _cadenaConexion = "Data Source=DESKTOP-18ERN8F\\SQLEXPRESS;Initial Catalog=Packing_SJP;Integrated Security=True;";
            _cadenaConexionPallet = "Data Source=DESKTOP-18ERN8F\\SQLEXPRESS;Initial Catalog=Packing_SJP;Integrated Security=True;";
            _cadenaConexionViajes = "Data Source=DESKTOP-18ERN8F\\SQLEXPRESS;Initial Catalog=Despachos_SJP;Integrated Security=True;";

        }

        public InformacionPallet ObtenerDatosPallet(string numeroPallet)
        {
            InformacionPallet informacionPallet = null;
            string numeroPalletLimpio = numeroPallet.Trim();

            // Verificar si es pallet completo (contiene "PC")  
            bool esPalletCompleto = numeroPalletLimpio.Contains("PC");

            string consulta = @"  
        SELECT   
            p.NUMERO_DEL_PALLETS AS Pallet,  
            p.CANTIDAD_DE_CAJAS AS CantidadCajas,  
            t.DESCRIPCION AS Calibre,  
            e.DESCRIPCION AS Embalaje,  
            r.Texto_Royalty AS Variedad,  
            pe.TotalCajasFichaTecnica  
        FROM   
            PALLETIZADOR p  
        LEFT JOIN   
            TIPO t ON p.CALIBRE = t.CODIGO  
        LEFT JOIN   
            EMBALAJE e ON p.EMBALAJE = e.CODIGO  
        LEFT JOIN  
            Royalty r ON e.CODIGO_VARIEDAD = r.Cod_Variedad  
        LEFT JOIN  
            [Despachos_SJP].[dbo].[PESOS_EMBALAJE] pe ON e.DESCRIPCION = pe.NombreEmbalaje AND pe.Activo = 1  
        WHERE   
            p.NUMERO_DEL_PALLETS = @NumeroPallet;";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NumeroPallet", numeroPalletLimpio);

                    try
                    {
                        conexion.Open();
                        SqlDataReader lector = comando.ExecuteReader();

                        if (lector.Read())
                        {
                            // Determinar el número de cajas según el tipo de pallet  
                            int numeroDeCajas;
                            if (esPalletCompleto && !lector.IsDBNull("TotalCajasFichaTecnica"))
                            {
                                // Para pallets completos, usar TotalCajasFichaTecnica  
                                numeroDeCajas = lector.GetInt32("TotalCajasFichaTecnica");
                            }
                            else
                            {
                                // Para pallets incompletos, usar CANTIDAD_DE_CAJAS de PALLETIZADOR  
                                numeroDeCajas = lector.GetInt32("CantidadCajas");
                            }

                            informacionPallet = new InformacionPallet
                            {
                                NumeroPallet = lector["Pallet"].ToString(),
                                Variedad = lector["Variedad"].ToString(),
                                Calibre = lector["Calibre"].ToString(),
                                Embalaje = lector["Embalaje"].ToString(),
                                NumeroDeCajas = numeroDeCajas
                            };
                        }
                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al obtener datos del pallet: {ex.Message}");
                    }
                }
            }
            return informacionPallet;
        }
    }
}