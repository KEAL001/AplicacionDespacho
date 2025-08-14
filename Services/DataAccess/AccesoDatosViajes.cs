// Services/DataAccess/AccesoDatosViajes.cs
using System;
using System.Data.SqlClient;
using AplicacionDespacho.Models;

namespace AplicacionDespacho.Services.DataAccess
{
    public class AccesoDatosViajes
    {
        private readonly string _cadenaConexion;

        public AccesoDatosViajes()
        {
            _cadenaConexion = "Data Source=DESKTOP-18ERN8F\\SQLEXPRESS;Initial Catalog=Despachos_SJP;Integrated Security=True;";
        }

        public void GuardarViaje(Viaje nuevoViaje)
        {
            string consulta = @"
                INSERT INTO VIAJES (Fecha, NumeroViaje, Responsable, NumeroGuia, PuntoPartida, PuntoLlegada, VehiculoId, ConductorId)
                VALUES (@Fecha, @NumeroViaje, @Responsable, @NumeroGuia, @PuntoPartida, @PuntoLlegada, @VehiculoId, @ConductorId);";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@Fecha", nuevoViaje.Fecha);
                    comando.Parameters.AddWithValue("@NumeroViaje", nuevoViaje.NumeroViaje);
                    comando.Parameters.AddWithValue("@Responsable", nuevoViaje.Responsable);
                    comando.Parameters.AddWithValue("@NumeroGuia", nuevoViaje.NumeroGuia);
                    comando.Parameters.AddWithValue("@PuntoPartida", nuevoViaje.PuntoPartida);
                    comando.Parameters.AddWithValue("@PuntoLlegada", nuevoViaje.PuntoLlegada);
                    comando.Parameters.AddWithValue("@VehiculoId", nuevoViaje.VehiculoId);
                    comando.Parameters.AddWithValue("@ConductorId", nuevoViaje.ConductorId);

                    try
                    {
                        conexion.Open();
                        comando.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Aquí puedes manejar el error, por ejemplo, loguearlo o mostrar un mensaje.
                        System.Diagnostics.Debug.WriteLine($"Error al guardar el viaje: {ex.Message}");
                    }
                }
            }
        }
        public List<EmpresaTransporte> ObtenerEmpresas()
        {
            var empresas = new List<EmpresaTransporte>();
            string consulta = "SELECT EmpresaId, NombreEmpresa, RUC FROM EMPRESAS_TRANSPORTE";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    try
                    {
                        conexion.Open();
                        SqlDataReader lector = comando.ExecuteReader();
                        while (lector.Read())
                        {
                            empresas.Add(new EmpresaTransporte
                            {
                                EmpresaId = lector.GetInt32(lector.GetOrdinal("EmpresaId")),
                                NombreEmpresa = lector.GetString(lector.GetOrdinal("NombreEmpresa")),
                                RUC = lector.GetString(lector.GetOrdinal("RUC"))
                            });
                        }
                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al obtener empresas: {ex.Message}");
                    }
                }
            }
            return empresas;
        }

        public List<Vehiculo> ObtenerVehiculosPorEmpresa(int empresaId)
        {
            var vehiculos = new List<Vehiculo>();
            string consulta = "SELECT VehiculoId, Placa, EmpresaId FROM VEHICULOS WHERE EmpresaId = @EmpresaId";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@EmpresaId", empresaId);
                    try
                    {
                        conexion.Open();
                        SqlDataReader lector = comando.ExecuteReader();
                        while (lector.Read())
                        {
                            vehiculos.Add(new Vehiculo
                            {
                                VehiculoId = lector.GetInt32(lector.GetOrdinal("VehiculoId")),
                                Placa = lector.GetString(lector.GetOrdinal("Placa")),
                                EmpresaId = lector.GetInt32(lector.GetOrdinal("EmpresaId"))
                            });
                        }
                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al obtener vehículos: {ex.Message}");
                    }
                }
            }
            return vehiculos;
        }

        public List<Conductor> ObtenerConductoresPorEmpresa(int empresaId)
        {
            var conductores = new List<Conductor>();
            string consulta = "SELECT ConductorId, NombreConductor, EmpresaId FROM CONDUCTORES WHERE EmpresaId = @EmpresaId";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@EmpresaId", empresaId);
                    try
                    {
                        conexion.Open();
                        SqlDataReader lector = comando.ExecuteReader();
                        while (lector.Read())
                        {
                            conductores.Add(new Conductor
                            {
                                ConductorId = lector.GetInt32(lector.GetOrdinal("ConductorId")),
                                NombreConductor = lector.GetString(lector.GetOrdinal("NombreConductor")),
                                EmpresaId = lector.GetInt32(lector.GetOrdinal("EmpresaId"))
                            });
                        }
                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al obtener conductores: {ex.Message}");
                    }
                }
            }
            return conductores;
        }
        // Services/DataAccess/AccesoDatosViajes.cs

        // ... (código existente de la clase AccesoDatosViajes) ...

        public void GuardarEmpresa(EmpresaTransporte nuevaEmpresa)
        {
            string consulta = @"
        INSERT INTO EMPRESAS_TRANSPORTE (NombreEmpresa, RUC)
        VALUES (@NombreEmpresa, @RUC);";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NombreEmpresa", nuevaEmpresa.NombreEmpresa);
                    comando.Parameters.AddWithValue("@RUC", nuevaEmpresa.RUC);

                    try
                    {
                        conexion.Open();
                        comando.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al guardar la empresa: {ex.Message}");
                    }
                }
            }
        }

        public void GuardarConductor(Conductor nuevoConductor)
        {
            string consulta = @"
        INSERT INTO CONDUCTORES (NombreConductor, EmpresaId)
        VALUES (@NombreConductor, @EmpresaId);";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NombreConductor", nuevoConductor.NombreConductor);
                    comando.Parameters.AddWithValue("@EmpresaId", nuevoConductor.EmpresaId);

                    try
                    {
                        conexion.Open();
                        comando.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al guardar el conductor: {ex.Message}");
                    }
                }
            }
        }

        public void GuardarVehiculo(Vehiculo nuevoVehiculo)
        {
            string consulta = @"
        INSERT INTO VEHICULOS (Placa, EmpresaId)
        VALUES (@Placa, @EmpresaId);";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@Placa", nuevoVehiculo.Placa);
                    comando.Parameters.AddWithValue("@EmpresaId", nuevoVehiculo.EmpresaId);

                    try
                    {
                        conexion.Open();
                        comando.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al guardar el vehículo: {ex.Message}");
                    }
                }
            }
        }
    }
}