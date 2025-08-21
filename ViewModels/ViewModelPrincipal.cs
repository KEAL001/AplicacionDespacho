// ViewModels/ViewModelPrincipal.cs - PARTE 1: Declaraciones y Constructor  
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AplicacionDespacho.Models;
using AplicacionDespacho.Services.DataAccess;
using AplicacionDespacho.Configuration;
using AplicacionDespacho.Services.Logging;

namespace AplicacionDespacho.ViewModels
{
    public class ViewModelPrincipal : INotifyPropertyChanged
    {
        private readonly IAccesoDatosPallet _accesoDatos;
        private readonly AccesoDatosViajes _accesoDatosViajes;
        private readonly ILoggingService _logger;

        private string _entradaNumeroPallet;
        public string EntradaNumeroPallet
        {
            get => _entradaNumeroPallet;
            set
            {
                _entradaNumeroPallet = value?.ToUpper();
                OnPropertyChanged(nameof(EntradaNumeroPallet));
            }
        }

        public ObservableCollection<InformacionPallet> PalletsEscaneados { get; } = new ObservableCollection<InformacionPallet>();

        private InformacionPallet _ultimoPalletEscaneado;
        public InformacionPallet UltimoPalletEscaneado
        {
            get => _ultimoPalletEscaneado;
            set
            {
                _ultimoPalletEscaneado = value;
                OnPropertyChanged(nameof(UltimoPalletEscaneado));
            }
        }

        // NUEVAS PROPIEDADES PARA SELECCIÓN DE PALLETS    
        private InformacionPallet _palletSeleccionado;
        public InformacionPallet PalletSeleccionado
        {
            get => _palletSeleccionado;
            set
            {
                _palletSeleccionado = value;
                OnPropertyChanged(nameof(PalletSeleccionado));
                OnPropertyChanged(nameof(TienePalletSeleccionado));

                if (value != null && (UltimoPalletEscaneado == null || UltimoPalletEscaneado.NumeroPallet != value.NumeroPallet))
                {
                    _logger.LogDebug("Cargando pallet para edición: {NumeroPallet}", value.NumeroPallet);
                    CargarPalletParaEdicion(value);
                }
            }
        }

        public ViewModelPrincipal(IAccesoDatosPallet accesoDatos)
        {
            _accesoDatos = accesoDatos;
            _accesoDatosViajes = new AccesoDatosViajes();
            _logger = LoggingFactory.CreateLogger("ViewModelPrincipal");

            _logger.LogInfo("ViewModelPrincipal inicializado");

            ComandoEscanear = new ComandoRelevo(EscanearPallet, PuedeEscanearPallet);
            ComandoFinalizarDespacho = new ComandoRelevo(FinalizarDespacho, PuedeFinalizarDespacho);
            ComandoNuevoViaje = new ComandoRelevo(NuevoViaje, parameter => true);
            ComandoEditarViaje = new ComandoRelevo(EditarViaje, parameter => TieneViajeActivo);
            ComandoEliminarPallet = new ComandoRelevo(EliminarPallet, parameter => TienePalletSeleccionado);
            ComandoAplicarCambios = new ComandoRelevo(AplicarCambios, parameter => UltimoPalletEscaneado != null);
            ComandoRevertirCambios = new ComandoRelevo(RevertirCambios, parameter => UltimoPalletEscaneado != null);
            ComandoContinuarViaje = new ComandoRelevo(ContinuarViaje, parameter => !TieneViajeActivo);
        }
        // PARTE 2: Propiedades y Métodos Auxiliares  
        private bool PuedeEscanearPallet(object parameter)
        {
            return TieneViajeActivo && PalletsEscaneados.Count < AppConfig.MaxPalletsPerTrip;
        }

        public void RecalcularPesoPallet(InformacionPallet pallet)
        {
            _logger.LogDebug("Recalculando peso para pallet: {NumeroPallet}", pallet.NumeroPallet);

            var pesoEmbalaje = _accesoDatosViajes.ObtenerPesoEmbalaje(pallet.Embalaje);
            if (pesoEmbalaje != null)
            {
                pallet.PesoUnitario = pesoEmbalaje.PesoUnitario;
                pallet.PesoTotal = pallet.NumeroDeCajas * pesoEmbalaje.PesoUnitario;

                _logger.LogInfo("Peso recalculado para pallet {NumeroPallet}: {PesoTotal} kg",
                              pallet.NumeroPallet, pallet.PesoTotal);
            }
            else
            {
                pallet.PesoUnitario = 0;
                pallet.PesoTotal = 0;
                _logger.LogWarning("No se encontró configuración de peso para embalaje: {Embalaje}", pallet.Embalaje);
            }
        }

        public bool TienePalletSeleccionado => PalletSeleccionado != null;

        // NUEVAS PROPIEDADES PARA MANEJO DE VIAJES    
        private Viaje _viajeActivo;
        public Viaje ViajeActivo
        {
            get => _viajeActivo;
            set
            {
                _viajeActivo = value;
                OnPropertyChanged(nameof(ViajeActivo));
                OnPropertyChanged(nameof(TieneViajeActivo));
                OnPropertyChanged(nameof(InformacionViajeActivo));

                if (value != null)
                {
                    _logger.LogInfo("Viaje activo establecido: #{NumeroViaje} - {Responsable}",
                                  value.NumeroViaje, value.Responsable);
                }
                else
                {
                    _logger.LogInfo("Viaje activo limpiado");
                }
            }
        }

        public bool TieneViajeActivo => ViajeActivo != null;

        public string InformacionViajeActivo
        {
            get
            {
                if (ViajeActivo == null)
                    return "No hay viaje activo";

                return $"Viaje #{ViajeActivo.NumeroViaje} - {ViajeActivo.Fecha:dd/MM/yyyy} - {ViajeActivo.Responsable}";
            }
        }

        public int TotalCajas
        {
            get
            {
                return PalletsEscaneados.Sum(p => p.NumeroDeCajas);
            }
        }

        public decimal PesoTotalViaje
        {
            get
            {
                return PalletsEscaneados.Sum(p => p.PesoTotal);
            }
        }

        // COMANDOS    
        public ICommand ComandoEscanear { get; }
        public ICommand ComandoFinalizarDespacho { get; }
        public ICommand ComandoNuevoViaje { get; }
        public ICommand ComandoEditarViaje { get; }
        public ICommand ComandoContinuarViaje { get; }
        public ICommand ComandoEliminarPallet { get; }
        public ICommand ComandoAplicarCambios { get; }
        public ICommand ComandoRevertirCambios { get; }
        // PARTE 3: Método EscanearPallet  
        private void EscanearPallet(object parameter)
        {
            _logger.LogInfo("Iniciando escaneo de pallet: {NumeroPallet}", EntradaNumeroPallet);

            if (!TieneViajeActivo)
            {
                _logger.LogWarning("Intento de escaneo sin viaje activo");
                MessageBox.Show("Debe crear un viaje antes de escanear pallets.", "Viaje Requerido",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // VALIDACIÓN: Verificar que el pallet no esté duplicado en el viaje actual          
            if (PalletsEscaneados.Any(p => p.NumeroPallet == EntradaNumeroPallet))
            {
                _logger.LogWarning("Pallet duplicado detectado: {NumeroPallet}", EntradaNumeroPallet);
                MessageBox.Show($"El pallet {EntradaNumeroPallet} ya fue escaneado en este viaje.",
                               "Pallet Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                EntradaNumeroPallet = string.Empty;
                return;
            }

            // VALIDACIÓN: Verificar que el pallet no haya sido enviado en otro viaje          
            if (_accesoDatosViajes.PalletYaFueEnviado(EntradaNumeroPallet))
            {
                _logger.LogWarning("Pallet ya enviado en otro viaje: {NumeroPallet}", EntradaNumeroPallet);
                MessageBox.Show($"El pallet {EntradaNumeroPallet} ya fue enviado en otro viaje y no puede ser reutilizado.",
                               "Pallet Ya Enviado", MessageBoxButton.OK, MessageBoxImage.Error);
                EntradaNumeroPallet = string.Empty;
                return;
            }

            InformacionPallet pallet = _accesoDatos.ObtenerDatosPallet(EntradaNumeroPallet);

            if (pallet != null)
            {
                _logger.LogInfo("Pallet encontrado: {NumeroPallet} - {Variedad}", pallet.NumeroPallet, pallet.Variedad);

                // Guardar datos originales para tracking de modificaciones          
                pallet.VariedadOriginal = pallet.Variedad;
                pallet.CalibreOriginal = pallet.Calibre;
                pallet.EmbalajeOriginal = pallet.Embalaje;
                pallet.NumeroDeCajasOriginal = pallet.NumeroDeCajas;

                // ✅ NUEVA LÓGICA: Verificar si es pallet completo y usar TotalCajasFichaTecnica    
                var pesoEmbalaje = _accesoDatosViajes.ObtenerPesoEmbalaje(pallet.Embalaje);
                if (pesoEmbalaje != null)
                {
                    pallet.PesoUnitario = pesoEmbalaje.PesoUnitario;

                    // ✅ LÓGICA DE PALLET COMPLETO: Si el pallet termina en "PC" o contiene "PC"    
                    bool esPalletCompleto = pallet.NumeroPallet.ToUpper().EndsWith("PC") ||
                                           pallet.NumeroPallet.ToUpper().Contains("PC");

                    if (esPalletCompleto && pesoEmbalaje.TotalCajasFichaTecnica.HasValue)
                    {
                        _logger.LogInfo("Pallet completo detectado: {NumeroPallet}, usando {TotalCajas} cajas de ficha técnica",
                                      pallet.NumeroPallet, pesoEmbalaje.TotalCajasFichaTecnica.Value);

                        // Usar el valor de ficha técnica para pallets completos    
                        pallet.NumeroDeCajas = pesoEmbalaje.TotalCajasFichaTecnica.Value;
                        pallet.PesoTotal = pallet.NumeroDeCajas * pesoEmbalaje.PesoUnitario;
                    }
                    else
                    {
                        // Usar el valor escaneado para pallets normales    
                        pallet.PesoTotal = pallet.NumeroDeCajas * pesoEmbalaje.PesoUnitario;
                    }
                }
                else
                {
                    pallet.PesoUnitario = 0;
                    pallet.PesoTotal = 0;
                    _logger.LogWarning("No se encontró peso configurado para embalaje: {Embalaje}", pallet.Embalaje);
                    MessageBox.Show($"No se encontró peso configurado para el embalaje '{pallet.Embalaje}'.",
                                   "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Asegurar que FechaModificacion tenga un valor      
                if (pallet.FechaModificacion == null)
                {
                    pallet.FechaModificacion = DateTime.Now;
                }

                // ✅ GUARDADO AUTOMÁTICO: Guardar inmediatamente en BD      
                try
                {
                    _accesoDatosViajes.GuardarPalletViaje(pallet, ViajeActivo.ViajeId);
                    _logger.LogInfo("Pallet guardado exitosamente en BD: {NumeroPallet}", pallet.NumeroPallet);

                    // Solo agregar a la colección si se guardó exitosamente      
                    PalletsEscaneados.Add(pallet);
                    UltimoPalletEscaneado = pallet;
                    EntradaNumeroPallet = string.Empty;

                    // Notificar cambios en propiedades calculadas          
                    OnPropertyChanged(nameof(TotalCajas));
                    OnPropertyChanged(nameof(PesoTotalViaje));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar pallet {NumeroPallet}: {ErrorMessage}",
                                    pallet.NumeroPallet, ex.Message);
                    MessageBox.Show($"Error al guardar pallet en la base de datos: {ex.Message}",
                                   "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _logger.LogWarning("Pallet no encontrado: {NumeroPallet}", EntradaNumeroPallet);
                MessageBox.Show("El número de pallet no se encontró en la base de datos.", "Error de Búsqueda",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // PARTE 4: Métodos de Finalización y Gestión de Viajes  
        private void FinalizarDespacho(object parameter)
        {
            _logger.LogInfo("Iniciando finalización de despacho para viaje #{NumeroViaje}", ViajeActivo?.NumeroViaje);

            if (!TieneViajeActivo)
            {
                _logger.LogWarning("Intento de finalizar despacho sin viaje activo");
                MessageBox.Show("No hay viaje activo para finalizar.", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PalletsEscaneados.Count > 0)
            {
                try
                {
                    // Los pallets ya están guardados automáticamente, solo cambiar estado del viaje    
                    ViajeActivo.Estado = "Finalizado";
                    ViajeActivo.FechaModificacion = DateTime.Now;
                    _accesoDatosViajes.ActualizarViaje(ViajeActivo);

                    _logger.LogInfo("Despacho finalizado exitosamente - Viaje #{NumeroViaje}, {TotalPallets} pallets",
                                  ViajeActivo.NumeroViaje, PalletsEscaneados.Count);

                    MessageBox.Show($"Despacho finalizado con éxito. Se registraron {PalletsEscaneados.Count} pallets.\n" +
                                   $"Total de cajas: {TotalCajas}\n" +
                                   $"Peso total: {PesoTotalViaje:F3} kg",
                                   "Despacho Completo", MessageBoxButton.OK, MessageBoxImage.Information);

                    AbrirImpresionDespacho(ViajeActivo, new List<InformacionPallet>(PalletsEscaneados));

                    // Limpiar datos        
                    PalletsEscaneados.Clear();
                    UltimoPalletEscaneado = null;
                    PalletSeleccionado = null;
                    ViajeActivo = null;

                    OnPropertyChanged(nameof(TotalCajas));
                    OnPropertyChanged(nameof(PesoTotalViaje));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al finalizar despacho para viaje #{NumeroViaje}: {ErrorMessage}",
                                    ViajeActivo?.NumeroViaje, ex.Message);
                    MessageBox.Show($"Error al finalizar el viaje: {ex.Message}",
                                   "Error al Finalizar", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _logger.LogWarning("Intento de finalizar despacho sin pallets");
                MessageBox.Show("No hay pallets registrados para finalizar el despacho.", "Advertencia",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AbrirImpresionDespacho(Viaje viaje, List<InformacionPallet> pallets)
        {
            _logger.LogInfo("Abriendo ventana de impresión para viaje #{NumeroViaje}", viaje.NumeroViaje);

            try
            {
                var ventanaImpresion = new ImpresionDespachoWindow(viaje, pallets);
                ventanaImpresion.ShowDialog();

                _logger.LogInfo("Ventana de impresión cerrada para viaje #{NumeroViaje}", viaje.NumeroViaje);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al abrir ventana de impresión para viaje #{NumeroViaje}: {ErrorMessage}",
                               viaje.NumeroViaje, ex.Message, ex);
                MessageBox.Show($"Error al abrir la ventana de impresión: {ex.Message}",
                               "Error de Impresión", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ContinuarViaje(object parameter)
        {
            _logger.LogInfo("Iniciando continuación de viaje activo");

            var viajesActivos = _accesoDatosViajes.ObtenerViajesActivos();

            if (viajesActivos.Count == 0)
            {
                _logger.LogInfo("No hay viajes activos disponibles para continuar");
                MessageBox.Show("No hay viajes activos para continuar.", "Sin Viajes Activos",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Crear ventana de selección de viajes activos    
            var ventanaSeleccion = new SeleccionViajeActivoWindow(viajesActivos);
            if (ventanaSeleccion.ShowDialog() == true && ventanaSeleccion.ViajeSeleccionado != null)
            {
                ViajeActivo = ventanaSeleccion.ViajeSeleccionado;
                _logger.LogInfo("Viaje seleccionado para continuar: #{NumeroViaje}", ViajeActivo.NumeroViaje);

                // Cargar pallets existentes del viaje    
                var palletsExistentes = _accesoDatosViajes.ObtenerPalletsDeViaje(ViajeActivo.ViajeId);

                PalletsEscaneados.Clear();
                foreach (var pallet in palletsExistentes)
                {
                    PalletsEscaneados.Add(pallet);
                }

                UltimoPalletEscaneado = PalletsEscaneados.LastOrDefault();
                PalletSeleccionado = null;

                OnPropertyChanged(nameof(TotalCajas));
                OnPropertyChanged(nameof(PesoTotalViaje));

                _logger.LogInfo("Viaje #{NumeroViaje} cargado con {TotalPallets} pallets",
                              ViajeActivo.NumeroViaje, PalletsEscaneados.Count);
                MessageBox.Show($"Viaje #{ViajeActivo.NumeroViaje} cargado con {PalletsEscaneados.Count} pallets.",
                               "Viaje Continuado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool PuedeFinalizarDespacho(object parameter)
        {
            return TieneViajeActivo && PalletsEscaneados.Count > 0;
        }

        private void NuevoViaje(object parameter)
        {
            _logger.LogInfo("Iniciando creación de nuevo viaje");

            var ventanaRegistro = new RegistroViajeWindow();
            if (ventanaRegistro.ShowDialog() == true && ventanaRegistro.ViajeGuardado)
            {
                ViajeActivo = ventanaRegistro.ViajeCreado;
                PalletsEscaneados.Clear();
                UltimoPalletEscaneado = null;
                PalletSeleccionado = null;

                OnPropertyChanged(nameof(TotalCajas));
                OnPropertyChanged(nameof(PesoTotalViaje));

                _logger.LogInfo("Nuevo viaje creado: #{NumeroViaje}", ViajeActivo.NumeroViaje);
            }
        }

        private void EditarViaje(object parameter)
        {
            if (ViajeActivo != null)
            {
                _logger.LogInfo("Iniciando edición de viaje: #{NumeroViaje}", ViajeActivo.NumeroViaje);

                var ventanaRegistro = new RegistroViajeWindow(ViajeActivo);
                if (ventanaRegistro.ShowDialog() == true && ventanaRegistro.ViajeGuardado)
                {
                    ViajeActivo = ventanaRegistro.ViajeCreado;
                    _logger.LogInfo("Viaje editado exitosamente: #{NumeroViaje}", ViajeActivo.NumeroViaje);
                }
            }
        }
        // PARTE 5: Métodos de Edición de Pallets  
        private void CargarPalletParaEdicion(InformacionPallet pallet)
        {
            _logger.LogDebug("Cargando pallet para edición: {NumeroPallet}", pallet.NumeroPallet);

            // Crear una copia para edición sin modificar el original    
            UltimoPalletEscaneado = new InformacionPallet
            {
                NumeroPallet = pallet.NumeroPallet,
                Variedad = pallet.Variedad,
                Calibre = pallet.Calibre,
                Embalaje = pallet.Embalaje,
                NumeroDeCajas = pallet.NumeroDeCajas,
                PesoUnitario = pallet.PesoUnitario,
                PesoTotal = pallet.PesoTotal,
                // Preservar valores originales    
                VariedadOriginal = pallet.VariedadOriginal ?? pallet.Variedad,
                CalibreOriginal = pallet.CalibreOriginal ?? pallet.Calibre,
                EmbalajeOriginal = pallet.EmbalajeOriginal ?? pallet.Embalaje,
                NumeroDeCajasOriginal = pallet.NumeroDeCajasOriginal != 0 ? pallet.NumeroDeCajasOriginal : pallet.NumeroDeCajas,
                Modificado = pallet.Modificado,
                FechaModificacion = pallet.FechaModificacion
            };

            _logger.LogInfo("Pallet cargado para edición: {NumeroPallet}", pallet.NumeroPallet);
        }

        private void AplicarCambios(object parameter)
        {
            if (PalletSeleccionado != null && UltimoPalletEscaneado != null)
            {
                _logger.LogInfo("Aplicando cambios al pallet: {NumeroPallet}", UltimoPalletEscaneado.NumeroPallet);

                // ✅ CORRECTO: Comparar con valores originales para detectar modificaciones      
                bool huboModificacion =
                    UltimoPalletEscaneado.Variedad != UltimoPalletEscaneado.VariedadOriginal ||
                    UltimoPalletEscaneado.Calibre != UltimoPalletEscaneado.CalibreOriginal ||
                    UltimoPalletEscaneado.Embalaje != UltimoPalletEscaneado.EmbalajeOriginal ||
                    UltimoPalletEscaneado.NumeroDeCajas != UltimoPalletEscaneado.NumeroDeCajasOriginal;

                _logger.LogDebug("Modificación detectada: {HuboModificacion}", huboModificacion);

                // ✅ CORRECTO: Recalcular peso si cambió embalaje o número de cajas (comparar con originales)      
                if (UltimoPalletEscaneado.Embalaje != UltimoPalletEscaneado.EmbalajeOriginal ||
                    UltimoPalletEscaneado.NumeroDeCajas != UltimoPalletEscaneado.NumeroDeCajasOriginal)
                {
                    RecalcularPesoPallet(UltimoPalletEscaneado);
                }

                var index = PalletsEscaneados.IndexOf(PalletSeleccionado);
                if (index >= 0)
                {
                    // ✅ PRESERVAR: Estado de modificación correctamente      
                    UltimoPalletEscaneado.Modificado = huboModificacion;
                    UltimoPalletEscaneado.FechaModificacion = huboModificacion ? DateTime.Now : PalletSeleccionado.FechaModificacion;

                    // ✅ AGREGAR: Actualizar en BD después de modificar    
                    try
                    {
                        _accesoDatosViajes.ActualizarPalletViaje(UltimoPalletEscaneado, ViajeActivo.ViajeId);
                        _logger.LogInfo("Pallet actualizado exitosamente en BD: {NumeroPallet}", UltimoPalletEscaneado.NumeroPallet);

                        PalletsEscaneados[index] = UltimoPalletEscaneado;
                        PalletSeleccionado = UltimoPalletEscaneado;

                        OnPropertyChanged(nameof(TotalCajas));
                        OnPropertyChanged(nameof(PesoTotalViaje));

                        MessageBox.Show(huboModificacion ? "Cambios aplicados. Pallet marcado como modificado." : "Cambios aplicados.",
                                       "Edición", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error al actualizar pallet {NumeroPallet}: {ErrorMessage}",
                                       UltimoPalletEscaneado.NumeroPallet, ex.Message, ex);
                        MessageBox.Show($"Error al actualizar pallet: {ex.Message}", "Error",
                                       MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void RevertirCambios(object parameter)
        {
            if (PalletSeleccionado != null)
            {
                _logger.LogInfo("Iniciando reversión de cambios para pallet: {NumeroPallet}", PalletSeleccionado.NumeroPallet);

                // ✅ PASO 1: Mostrar primero los datos originales en la UI    
                UltimoPalletEscaneado = new InformacionPallet
                {
                    NumeroPallet = PalletSeleccionado.NumeroPallet,
                    Variedad = PalletSeleccionado.VariedadOriginal ?? PalletSeleccionado.Variedad,
                    Calibre = PalletSeleccionado.CalibreOriginal ?? PalletSeleccionado.Calibre,
                    Embalaje = PalletSeleccionado.EmbalajeOriginal ?? PalletSeleccionado.Embalaje,
                    NumeroDeCajas = PalletSeleccionado.NumeroDeCajasOriginal != 0 ?
                                   PalletSeleccionado.NumeroDeCajasOriginal : PalletSeleccionado.NumeroDeCajas,
                    // ✅ PRESERVAR: Referencias originales      
                    VariedadOriginal = PalletSeleccionado.VariedadOriginal,
                    CalibreOriginal = PalletSeleccionado.CalibreOriginal,
                    EmbalajeOriginal = PalletSeleccionado.EmbalajeOriginal,
                    NumeroDeCajasOriginal = PalletSeleccionado.NumeroDeCajasOriginal,
                    // ✅ RESETEAR: Estado de modificación a false al revertir      
                    Modificado = false,
                    FechaModificacion = PalletSeleccionado.FechaModificacion
                };

                // ✅ RECALCULAR: Peso con valores originales      
                RecalcularPesoPallet(UltimoPalletEscaneado);

                // ✅ NOTIFICAR: Actualizar la UI para mostrar los datos originales    
                OnPropertyChanged(nameof(UltimoPalletEscaneado));

                // ✅ PASO 2: Ahora pedir confirmación para guardar    
                var resultado = MessageBox.Show(
                    $"Se han mostrado los valores originales del pallet {PalletSeleccionado.NumeroPallet}.\n\n¿Desea guardar estos cambios en la base de datos?",
                    "Confirmar Guardado de Reversión",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    // ✅ PASO 3: Guardar en BD solo si confirma    
                    var index = PalletsEscaneados.IndexOf(PalletSeleccionado);
                    if (index >= 0)
                    {
                        try
                        {
                            _accesoDatosViajes.ActualizarPalletViaje(UltimoPalletEscaneado, ViajeActivo.ViajeId);
                            _logger.LogInfo("Cambios revertidos y guardados para pallet: {NumeroPallet}", UltimoPalletEscaneado.NumeroPallet);

                            PalletsEscaneados[index] = UltimoPalletEscaneado;
                            PalletSeleccionado = UltimoPalletEscaneado;

                            OnPropertyChanged(nameof(TotalCajas));
                            OnPropertyChanged(nameof(PesoTotalViaje));

                            MessageBox.Show("Cambios revertidos y guardados exitosamente.", "Reversión Completada",
                                           MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Error al guardar cambios revertidos para pallet {NumeroPallet}: {ErrorMessage}",
                                           UltimoPalletEscaneado.NumeroPallet, ex.Message, ex);
                            MessageBox.Show($"Error al guardar cambios revertidos: {ex.Message}", "Error",
                                           MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                else
                {
                    // ✅ PASO 4: Si cancela, restaurar los valores modificados    
                    CargarPalletParaEdicion(PalletSeleccionado);
                    _logger.LogInfo("Reversión cancelada para pallet: {NumeroPallet}", PalletSeleccionado.NumeroPallet);
                    MessageBox.Show("Reversión cancelada. Se mantienen los valores modificados.", "Operación Cancelada",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        // PARTE 6: Métodos de Eliminación y Finalización  
        private void EliminarPallet(object parameter)
        {
            if (PalletSeleccionado != null)
            {
                _logger.LogInfo("Iniciando eliminación de pallet: {NumeroPallet}", PalletSeleccionado.NumeroPallet);

                var resultado = MessageBox.Show($"¿Está seguro de eliminar el pallet {PalletSeleccionado.NumeroPallet}?",
                                               "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    // ✅ AGREGAR: Eliminar de BD primero    
                    try
                    {
                        _accesoDatosViajes.EliminarPalletViaje(PalletSeleccionado.NumeroPallet, ViajeActivo.ViajeId);
                        _logger.LogInfo("Pallet eliminado exitosamente de BD: {NumeroPallet}", PalletSeleccionado.NumeroPallet);

                        // Luego eliminar de la colección    
                        PalletsEscaneados.Remove(PalletSeleccionado);
                        PalletSeleccionado = null;
                        UltimoPalletEscaneado = null;

                        // Actualizar totales      
                        OnPropertyChanged(nameof(TotalCajas));
                        OnPropertyChanged(nameof(PesoTotalViaje));

                        MessageBox.Show("Pallet eliminado exitosamente.", "Eliminación",
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error al eliminar pallet {NumeroPallet}: {ErrorMessage}",
                                       PalletSeleccionado.NumeroPallet, ex.Message, ex);
                        MessageBox.Show($"Error al eliminar pallet: {ex.Message}", "Error",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    _logger.LogInfo("Eliminación de pallet cancelada por el usuario: {NumeroPallet}", PalletSeleccionado.NumeroPallet);
                }
            }
        }

        public void ActualizarTotales()
        {
            _logger.LogDebug("Actualizando totales de cajas y peso");
            OnPropertyChanged(nameof(TotalCajas));
            OnPropertyChanged(nameof(PesoTotalViaje));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string nombrePropiedad)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}