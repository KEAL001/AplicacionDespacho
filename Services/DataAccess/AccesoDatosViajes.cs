// Services/DataAccess/AccesoDatosViajes.cs  
using System;
using System.Collections.Generic;
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
                INSERT INTO VIAJES (Fecha, NumeroViaje, Responsable, NumeroGuia, PuntoPartida, PuntoLlegada, VehiculoId, ConductorId, Estado, FechaCreacion, UsuarioCreacion)  
                VALUES (@Fecha, @NumeroViaje, @Responsable, @NumeroGuia, @PuntoPartida, @PuntoLlegada, @VehiculoId, @ConductorId, @Estado, @FechaCreacion, @UsuarioCreacion);  
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@Fecha", nuevoViaje.Fecha);
                    comando.Parameters.AddWithValue("@NumeroViaje", nuevoViaje.NumeroViaje);
                    comando.Parameters.AddWithValue("@Responsable", nuevoViaje.Responsable);
                    comando.Parameters.AddWithValue("@NumeroGuia", nuevoViaje.NumeroGuia);
                    comando.Parameters.AddWithValue("@PuntoPartida", nuevoViaje.PuntoPartida ?? "");
                    comando.Parameters.AddWithValue("@PuntoLlegada", nuevoViaje.PuntoLlegada ?? "");
                    comando.Parameters.AddWithValue("@VehiculoId", nuevoViaje.VehiculoId);
                    comando.Parameters.AddWithValue("@ConductorId", nuevoViaje.ConductorId);
                    comando.Parameters.AddWithValue("@Estado", nuevoViaje.Estado ?? "Activo");
                    comando.Parameters.AddWithValue("@FechaCreacion", nuevoViaje.FechaCreacion);
                    comando.Parameters.AddWithValue("@UsuarioCreacion", nuevoViaje.UsuarioCreacion ?? "");

                    try
                    {
                        conexion.Open();
                        var viajeId = comando.ExecuteScalar();
                        nuevoViaje.ViajeId = Convert.ToInt32(viajeId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al guardar el viaje: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // MÉTODOS DE VALIDACIÓN PARA LAS NUEVAS REGLAS DE NEGOCIO  
        public bool ExisteNumeroViajePorFecha(int numeroViaje, DateTime fecha, int? viajeIdExcluir = null)
        {
            string consulta = @"  
                SELECT COUNT(*) FROM VIAJES   
                WHERE NumeroViaje = @NumeroViaje   
                AND CAST(Fecha AS DATE) = CAST(@Fecha AS DATE)";

            if (viajeIdExcluir.HasValue)
            {
                consulta += " AND ViajeId != @ViajeIdExcluir";
            }

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NumeroViaje", numeroViaje);
                    comando.Parameters.AddWithValue("@Fecha", fecha.Date);
                    if (viajeIdExcluir.HasValue)
                    {
                        comando.Parameters.AddWithValue("@ViajeIdExcluir", viajeIdExcluir.Value);
                    }

                    try
                    {
                        conexion.Open();
                        int count = (int)comando.ExecuteScalar();
                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al verificar número de viaje: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public bool ExisteNumeroGuia(string numeroGuia, int? viajeIdExcluir = null)
        {
            string consulta = "SELECT COUNT(*) FROM VIAJES WHERE NumeroGuia = @NumeroGuia";

            if (viajeIdExcluir.HasValue)
            {
                consulta += " AND ViajeId != @ViajeIdExcluir";
            }

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NumeroGuia", numeroGuia);
                    if (viajeIdExcluir.HasValue)
                    {
                        comando.Parameters.AddWithValue("@ViajeIdExcluir", viajeIdExcluir.Value);
                    }

                    try
                    {
                        conexion.Open();
                        int count = (int)comando.ExecuteScalar();
                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al verificar número de guía: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public bool PalletYaFueEnviado(string numeroPallet)
        {
            string consulta = @"  
                SELECT COUNT(*) FROM PALLETS_VIAJE pv  
                INNER JOIN VIAJES v ON pv.ViajeId = v.ViajeId  
                WHERE pv.NumeroPallet = @NumeroPallet   
                AND v.Estado = 'Finalizado'";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NumeroPallet", numeroPallet);

                    try
                    {
                        conexion.Open();
                        int count = (int)comando.ExecuteScalar();
                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al verificar pallet enviado: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public void ActualizarViaje(Viaje viaje)
        {
            string consulta = @"  
                UPDATE VIAJES SET   
                    Fecha = @Fecha,  
                    NumeroViaje = @NumeroViaje,  
                    Responsable = @Responsable,  
                    NumeroGuia = @NumeroGuia,  
                    PuntoPartida = @PuntoPartida,  
                    PuntoLlegada = @PuntoLlegada,  
                    VehiculoId = @VehiculoId,  
                    ConductorId = @ConductorId,  
                    Estado = @Estado,  
                    FechaModificacion = @FechaModificacion,  
                    UsuarioModificacion = @UsuarioModificacion  
                WHERE ViajeId = @ViajeId";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@ViajeId", viaje.ViajeId);
                    comando.Parameters.AddWithValue("@Fecha", viaje.Fecha);
                    comando.Parameters.AddWithValue("@NumeroViaje", viaje.NumeroViaje);
                    comando.Parameters.AddWithValue("@Responsable", viaje.Responsable);
                    comando.Parameters.AddWithValue("@NumeroGuia", viaje.NumeroGuia);
                    comando.Parameters.AddWithValue("@PuntoPartida", viaje.PuntoPartida ?? "");
                    comando.Parameters.AddWithValue("@PuntoLlegada", viaje.PuntoLlegada ?? "");
                    comando.Parameters.AddWithValue("@VehiculoId", viaje.VehiculoId);
                    comando.Parameters.AddWithValue("@ConductorId", viaje.ConductorId);
                    comando.Parameters.AddWithValue("@Estado", viaje.Estado);
                    comando.Parameters.AddWithValue("@FechaModificacion", DateTime.Now);
                    comando.Parameters.AddWithValue("@UsuarioModificacion", Environment.UserName);

                    try
                    {
                        conexion.Open();
                        comando.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al actualizar viaje: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public Vehiculo ObtenerVehiculoPorId(int vehiculoId)
        {
            Vehiculo vehiculo = null;
            string consulta = @"  
                SELECT v.VehiculoId, v.Placa, v.EmpresaId  
                FROM VEHICULOS v  
                WHERE v.VehiculoId = @VehiculoId";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@VehiculoId", vehiculoId);

                    try
                    {
                        conexion.Open();
                        SqlDataReader lector = comando.ExecuteReader();

                        if (lector.Read())
                        {
                            vehiculo = new Vehiculo
                            {
                                VehiculoId = (int)lector["VehiculoId"],
                                Placa = lector["Placa"].ToString(),
                                EmpresaId = (int)lector["EmpresaId"]
                            };
                        }
                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al obtener vehículo: {ex.Message}");
                    }
                }
            }

            return vehiculo;
        }

        public void GuardarPalletViaje(InformacionPallet pallet, int viajeId)
        {
            string consulta = @"  
        INSERT INTO PALLETS_VIAJE (NumeroPallet, Variedad, Calibre, Embalaje, NumeroDeCajas,   
                                   ViajeId, PesoUnitario, PesoTotal, VariedadOriginal,   
                                   CalibreOriginal, EmbalajeOriginal, NumeroDeCajasOriginal,   
                                   FechaEscaneo, Modificado, FechaModificacion)  
        VALUES (@NumeroPallet, @Variedad, @Calibre, @Embalaje, @NumeroDeCajas,   
                @ViajeId, @PesoUnitario, @PesoTotal, @VariedadOriginal,   
                @CalibreOriginal, @EmbalajeOriginal, @NumeroDeCajasOriginal,   
                @FechaEscaneo, @Modificado, @FechaModificacion)";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NumeroPallet", pallet.NumeroPallet);
                    comando.Parameters.AddWithValue("@Variedad", pallet.Variedad ?? "");
                    comando.Parameters.AddWithValue("@Calibre", pallet.Calibre ?? "");
                    comando.Parameters.AddWithValue("@Embalaje", pallet.Embalaje ?? "");
                    comando.Parameters.AddWithValue("@NumeroDeCajas", pallet.NumeroDeCajas);
                    comando.Parameters.AddWithValue("@ViajeId", viajeId);
                    comando.Parameters.AddWithValue("@PesoUnitario", pallet.PesoUnitario);
                    comando.Parameters.AddWithValue("@PesoTotal", pallet.PesoTotal);
                    comando.Parameters.AddWithValue("@VariedadOriginal", pallet.VariedadOriginal ?? "");
                    comando.Parameters.AddWithValue("@CalibreOriginal", pallet.CalibreOriginal ?? "");
                    comando.Parameters.AddWithValue("@EmbalajeOriginal", pallet.EmbalajeOriginal ?? "");
                    comando.Parameters.AddWithValue("@NumeroDeCajasOriginal", pallet.NumeroDeCajasOriginal);
                    comando.Parameters.AddWithValue("@FechaEscaneo", DateTime.Now);
                    comando.Parameters.AddWithValue("@Modificado", pallet.Modificado);
                    // ESTE ES EL PARÁMETRO QUE FALTA:  
                    comando.Parameters.AddWithValue("@FechaModificacion", pallet.FechaModificacion ?? DateTime.Now);

                    try
                    {
                        conexion.Open();
                        comando.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al guardar pallet en viaje: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public PesoEmbalaje ObtenerPesoEmbalaje(string nombreEmbalaje)
        {
            PesoEmbalaje pesoEmbalaje = null;
            string consulta = @"  
        SELECT PesoEmbalajeId, NombreEmbalaje, PesoUnitario, FechaCreacion, FechaModificacion, Activo  
        FROM PESOS_EMBALAJE   
        WHERE NombreEmbalaje = @NombreEmbalaje AND Activo = 1";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NombreEmbalaje", nombreEmbalaje);

                    try
                    {
                        conexion.Open();
                        SqlDataReader lector = comando.ExecuteReader();

                        if (lector.Read())
                        {
                            pesoEmbalaje = new PesoEmbalaje
                            {
                                PesoEmbalajeId = (int)lector["PesoEmbalajeId"],
                                NombreEmbalaje = lector["NombreEmbalaje"].ToString(),
                                PesoUnitario = (decimal)lector["PesoUnitario"],
                                FechaCreacion = (DateTime)lector["FechaCreacion"],
                                FechaModificacion = lector["FechaModificacion"] as DateTime?,
                                Activo = (bool)lector["Activo"]
                            };
                        }
                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al obtener peso de embalaje: {ex.Message}");
                    }
                }
            }

            return pesoEmbalaje;
        }
        public List<Viaje> BuscarViajesConFiltros(string numeroGuia, int? empresaId, int? conductorId, DateTime? fechaDesde, DateTime? fechaHasta, string numeroPallet)
        {
            var viajes = new List<Viaje>();
            string consulta = @"    
        SELECT v.ViajeId, v.Fecha, v.NumeroViaje, v.Responsable, v.NumeroGuia,     
               v.PuntoPartida, v.PuntoLlegada, v.VehiculoId, v.ConductorId, v.Estado,    
               e.NombreEmpresa, c.NombreConductor, vh.Placa    
        FROM VIAJES v    
        INNER JOIN VEHICULOS vh ON v.VehiculoId = vh.VehiculoId    
        INNER JOIN EMPRESAS_TRANSPORTE e ON vh.EmpresaId = e.EmpresaId    
        INNER JOIN CONDUCTORES c ON v.ConductorId = c.ConductorId";

            // NUEVO: Agregar LEFT JOIN para búsqueda por pallet  
            if (!string.IsNullOrEmpty(numeroPallet))
            {
                consulta += " INNER JOIN PALLETS_VIAJE pv ON v.ViajeId = pv.ViajeId";
            }

            consulta += " WHERE 1=1";

            var parametros = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(numeroGuia))
            {
                consulta += " AND v.NumeroGuia LIKE @NumeroGuia";
                parametros.Add(new SqlParameter("@NumeroGuia", $"%{numeroGuia}%"));
            }

            // NUEVO: Filtro por número de pallet  
            if (!string.IsNullOrEmpty(numeroPallet))
            {
                consulta += " AND pv.NumeroPallet = @NumeroPallet";
                parametros.Add(new SqlParameter("@NumeroPallet", numeroPallet));
            }

            if (empresaId.HasValue)
            {
                consulta += " AND e.EmpresaId = @EmpresaId";
                parametros.Add(new SqlParameter("@EmpresaId", empresaId.Value));
            }

            if (conductorId.HasValue)
            {
                consulta += " AND c.ConductorId = @ConductorId";
                parametros.Add(new SqlParameter("@ConductorId", conductorId.Value));
            }

            if (fechaDesde.HasValue)
            {
                consulta += " AND v.Fecha >= @FechaDesde";
                parametros.Add(new SqlParameter("@FechaDesde", fechaDesde.Value.Date));
            }

            if (fechaHasta.HasValue)
            {
                consulta += " AND v.Fecha <= @FechaHasta";
                parametros.Add(new SqlParameter("@FechaHasta", fechaHasta.Value.Date));
            }

            // NUEVO: Agregar GROUP BY cuando se busca por pallet para evitar duplicados  
            if (!string.IsNullOrEmpty(numeroPallet))
            {
                consulta += @" GROUP BY v.ViajeId, v.Fecha, v.NumeroViaje, v.Responsable, v.NumeroGuia,     
                              v.PuntoPartida, v.PuntoLlegada, v.VehiculoId, v.ConductorId, v.Estado,    
                              e.NombreEmpresa, c.NombreConductor, vh.Placa";
            }

            consulta += " ORDER BY v.Fecha DESC, v.NumeroViaje DESC";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddRange(parametros.ToArray());

                    try
                    {
                        conexion.Open();
                        SqlDataReader lector = comando.ExecuteReader();
                        while (lector.Read())
                        {
                            viajes.Add(new Viaje
                            {
                                ViajeId = (int)lector["ViajeId"],
                                Fecha = (DateTime)lector["Fecha"],
                                NumeroViaje = (int)lector["NumeroViaje"],
                                Responsable = lector["Responsable"].ToString(),
                                NumeroGuia = lector["NumeroGuia"].ToString(),
                                PuntoPartida = lector["PuntoPartida"].ToString(),
                                PuntoLlegada = lector["PuntoLlegada"].ToString(),
                                VehiculoId = (int)lector["VehiculoId"],
                                ConductorId = (int)lector["ConductorId"],
                                Estado = lector["Estado"].ToString(),
                                NombreEmpresa = lector["NombreEmpresa"].ToString(),
                                NombreConductor = lector["NombreConductor"].ToString(),
                                PlacaVehiculo = lector["Placa"].ToString()
                            });
                        }
                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al buscar viajes: {ex.Message}");
                    }
                }
            }

            return viajes;
        }

        public List<InformacionPallet> ObtenerPalletsDeViaje(int viajeId)
        {
            var pallets = new List<InformacionPallet>();
            string consulta = @"  
        SELECT NumeroPallet, Variedad, Calibre, Embalaje, NumeroDeCajas,   
               PesoUnitario, PesoTotal, VariedadOriginal, CalibreOriginal,   
               EmbalajeOriginal, NumeroDeCajasOriginal, FechaEscaneo,   
               Modificado, FechaModificacion  
        FROM PALLETS_VIAJE   
        WHERE ViajeId = @ViajeId  
        ORDER BY FechaEscaneo";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@ViajeId", viajeId);

                    try
                    {
                        conexion.Open();
                        SqlDataReader lector = comando.ExecuteReader();
                        while (lector.Read())
                        {
                            pallets.Add(new InformacionPallet
                            {
                                NumeroPallet = lector["NumeroPallet"].ToString(),
                                Variedad = lector["Variedad"].ToString(),
                                Calibre = lector["Calibre"].ToString(),
                                Embalaje = lector["Embalaje"].ToString(),
                                NumeroDeCajas = (int)lector["NumeroDeCajas"],
                                PesoUnitario = (decimal)lector["PesoUnitario"],
                                PesoTotal = (decimal)lector["PesoTotal"],
                                VariedadOriginal = lector["VariedadOriginal"].ToString(),
                                CalibreOriginal = lector["CalibreOriginal"].ToString(),
                                EmbalajeOriginal = lector["EmbalajeOriginal"].ToString(),
                                NumeroDeCajasOriginal = (int)lector["NumeroDeCajasOriginal"],
                                Modificado = (bool)lector["Modificado"],
                                FechaModificacion = lector["FechaModificacion"] as DateTime?
                            });
                        }
                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al obtener pallets del viaje: {ex.Message}");
                    }
                }
            }

            return pallets;
        }

        public List<PesoEmbalaje> ObtenerTodosPesosEmbalaje()
        {
            var pesos = new List<PesoEmbalaje>();
            string consulta = @"  
        SELECT PesoEmbalajeId, NombreEmbalaje, PesoUnitario, FechaCreacion,   
               FechaModificacion, Activo  
        FROM PESOS_EMBALAJE   
        ORDER BY NombreEmbalaje";

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
                            pesos.Add(new PesoEmbalaje
                            {
                                PesoEmbalajeId = (int)lector["PesoEmbalajeId"],
                                NombreEmbalaje = lector["NombreEmbalaje"].ToString(),
                                PesoUnitario = (decimal)lector["PesoUnitario"],
                                FechaCreacion = (DateTime)lector["FechaCreacion"],
                                FechaModificacion = lector["FechaModificacion"] as DateTime?,
                                Activo = (bool)lector["Activo"]
                            });
                        }
                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al obtener pesos de embalaje: {ex.Message}");
                    }
                }
            }

            return pesos;
        }

        public void GuardarPesoEmbalaje(PesoEmbalaje nuevoPeso)
        {
            string consulta = @"    
        INSERT INTO PESOS_EMBALAJE (NombreEmbalaje, PesoUnitario, TotalCajasFichaTecnica, FechaCreacion, Activo)    
        VALUES (@NombreEmbalaje, @PesoUnitario, @TotalCajasFichaTecnica, @FechaCreacion, @Activo)";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@NombreEmbalaje", nuevoPeso.NombreEmbalaje);
                    comando.Parameters.AddWithValue("@PesoUnitario", nuevoPeso.PesoUnitario);
                    comando.Parameters.AddWithValue("@TotalCajasFichaTecnica",
                        nuevoPeso.TotalCajasFichaTecnica.HasValue ? (object)nuevoPeso.TotalCajasFichaTecnica.Value : DBNull.Value);
                    comando.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);
                    comando.Parameters.AddWithValue("@Activo", true);

                    try
                    {
                        conexion.Open();
                        comando.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al guardar peso de embalaje: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public void ActualizarPesoEmbalaje(PesoEmbalaje peso)
        {
            string consulta = @"  
        UPDATE PESOS_EMBALAJE SET   
            NombreEmbalaje = @NombreEmbalaje,  
            PesoUnitario = @PesoUnitario,  
            FechaModificacion = @FechaModificacion,  
            Activo = @Activo  
        WHERE PesoEmbalajeId = @PesoEmbalajeId";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@PesoEmbalajeId", peso.PesoEmbalajeId);
                    comando.Parameters.AddWithValue("@NombreEmbalaje", peso.NombreEmbalaje);
                    comando.Parameters.AddWithValue("@PesoUnitario", peso.PesoUnitario);
                    comando.Parameters.AddWithValue("@FechaModificacion", DateTime.Now);
                    comando.Parameters.AddWithValue("@Activo", peso.Activo);

                    try
                    {
                        conexion.Open();
                        comando.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al actualizar peso de embalaje: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // MÉTODOS EXISTENTES (mantener todos los que ya tienes)  
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
        public List<string> ObtenerTodosLosEmbalajes()
        {
            var embalajes = new List<string>();
            // Conectar a la base de datos Packing_SJP para obtener embalajes únicos  
            string cadenaConexionPacking = "Data Source=DESKTOP-18ERN8F\\SQLEXPRESS;Initial Catalog=Packing_SJP;Integrated Security=True;";
            string consulta = "SELECT DISTINCT DESCRIPCION FROM EMBALAJE WHERE DESCRIPCION IS NOT NULL AND DESCRIPCION != '' ORDER BY DESCRIPCION";

            using (SqlConnection conexion = new SqlConnection(cadenaConexionPacking))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    try
                    {
                        conexion.Open();
                        SqlDataReader lector = comando.ExecuteReader();
                        while (lector.Read())
                        {
                            string descripcion = lector["DESCRIPCION"].ToString().Trim();
                            if (!string.IsNullOrEmpty(descripcion))
                            {
                                embalajes.Add(descripcion);
                            }
                        }
                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al obtener embalajes: {ex.Message}");
                    }
                }
            }

            return embalajes;
        }
    }
}