// ViewModels/ViewModelPrincipal.cs - Parte 1: Propiedades básicas  
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AplicacionDespacho.Models;
using AplicacionDespacho.Services.DataAccess;

namespace AplicacionDespacho.ViewModels
{
    public class ViewModelPrincipal : INotifyPropertyChanged
    {
        private readonly IAccesoDatosPallet _accesoDatos;
        private readonly AccesoDatosViajes _accesoDatosViajes;

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

                // Solo cargar para edición si no hay un pallet siendo editado actualmente  
                // o si es una selección nueva diferente  
                if (value != null && (UltimoPalletEscaneado == null || UltimoPalletEscaneado.NumeroPallet != value.NumeroPallet))
                {
                    CargarPalletParaEdicion(value);
                }
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
        public ICommand ComandoEliminarPallet { get; }
        public ICommand ComandoAplicarCambios { get; }
        public ICommand ComandoRevertirCambios { get; }

        public ViewModelPrincipal(IAccesoDatosPallet accesoDatos)
        {
            _accesoDatos = accesoDatos;
            _accesoDatosViajes = new AccesoDatosViajes();

            ComandoEscanear = new ComandoRelevo(EscanearPallet, PuedeEscanearPallet);
            ComandoFinalizarDespacho = new ComandoRelevo(FinalizarDespacho, PuedeFinalizarDespacho);
            ComandoNuevoViaje = new ComandoRelevo(NuevoViaje, parameter => true);
            ComandoEditarViaje = new ComandoRelevo(EditarViaje, parameter => TieneViajeActivo);
            ComandoEliminarPallet = new ComandoRelevo(EliminarPallet, parameter => TienePalletSeleccionado);
            ComandoAplicarCambios = new ComandoRelevo(AplicarCambios, parameter => UltimoPalletEscaneado != null);
            ComandoRevertirCambios = new ComandoRelevo(RevertirCambios, parameter => UltimoPalletEscaneado != null);
        }
        private void EscanearPallet(object parameter)
        {
            if (!TieneViajeActivo)
            {
                MessageBox.Show("Debe crear un viaje antes de escanear pallets.", "Viaje Requerido",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // VALIDACIÓN: Verificar que el pallet no esté duplicado en el viaje actual  
            if (PalletsEscaneados.Any(p => p.NumeroPallet == EntradaNumeroPallet))
            {
                MessageBox.Show($"El pallet {EntradaNumeroPallet} ya fue escaneado en este viaje.",
                               "Pallet Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                EntradaNumeroPallet = string.Empty;
                return;
            }

            // VALIDACIÓN: Verificar que el pallet no haya sido enviado en otro viaje  
            if (_accesoDatosViajes.PalletYaFueEnviado(EntradaNumeroPallet))
            {
                MessageBox.Show($"El pallet {EntradaNumeroPallet} ya fue enviado en otro viaje y no puede ser reutilizado.",
                               "Pallet Ya Enviado", MessageBoxButton.OK, MessageBoxImage.Error);
                EntradaNumeroPallet = string.Empty;
                return;
            }

            InformacionPallet pallet = _accesoDatos.ObtenerDatosPallet(EntradaNumeroPallet);

            if (pallet != null)
            {
                // Guardar datos originales para tracking de modificaciones  
                pallet.VariedadOriginal = pallet.Variedad;
                pallet.CalibreOriginal = pallet.Calibre;
                pallet.EmbalajeOriginal = pallet.Embalaje;
                pallet.NumeroDeCajasOriginal = pallet.NumeroDeCajas;

                // Obtener peso del embalaje desde la base de datos  
                var pesoEmbalaje = _accesoDatosViajes.ObtenerPesoEmbalaje(pallet.Embalaje);
                if (pesoEmbalaje != null)
                {
                    pallet.PesoUnitario = pesoEmbalaje.PesoUnitario;
                    pallet.PesoTotal = pallet.NumeroDeCajas * pesoEmbalaje.PesoUnitario;
                }
                else
                {
                    pallet.PesoUnitario = 0;
                    pallet.PesoTotal = 0;
                    MessageBox.Show($"No se encontró peso configurado para el embalaje '{pallet.Embalaje}'.",
                                   "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                PalletsEscaneados.Add(pallet);
                UltimoPalletEscaneado = pallet;
                EntradaNumeroPallet = string.Empty;

                // Notificar cambios en propiedades calculadas  
                OnPropertyChanged(nameof(TotalCajas));
                OnPropertyChanged(nameof(PesoTotalViaje));
            }
            else
            {
                MessageBox.Show("El número de pallet no se encontró en la base de datos.", "Error de Búsqueda",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool PuedeEscanearPallet(object parameter)
        {
            return TieneViajeActivo && PalletsEscaneados.Count < 50;
        }
        private void FinalizarDespacho(object parameter)
        {
            if (!TieneViajeActivo)
            {
                MessageBox.Show("No hay viaje activo para finalizar.", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PalletsEscaneados.Count > 0)
            {
                // Guardar pallets en la base de datos  
                foreach (var pallet in PalletsEscaneados)
                {
                    // Asegurar que FechaModificacion tenga un valor  
                    if (pallet.FechaModificacion == null)
                    {
                        pallet.FechaModificacion = DateTime.Now;
                    }

                    _accesoDatosViajes.GuardarPalletViaje(pallet, ViajeActivo.ViajeId);
                }

                // Actualizar estado del viaje  
                ViajeActivo.Estado = "Finalizado";
                ViajeActivo.FechaModificacion = DateTime.Now;
                _accesoDatosViajes.ActualizarViaje(ViajeActivo);

                MessageBox.Show($"Despacho finalizado con éxito. Se registraron {PalletsEscaneados.Count} pallets.\n" +
                               $"Total de cajas: {TotalCajas}\n" +
                               $"Peso total: {PesoTotalViaje:F3} kg",
                               "Despacho Completo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar datos  
                PalletsEscaneados.Clear();
                UltimoPalletEscaneado = null;
                PalletSeleccionado = null;
                ViajeActivo = null;

                OnPropertyChanged(nameof(TotalCajas));
                OnPropertyChanged(nameof(PesoTotalViaje));
            }
            else
            {
                MessageBox.Show("No hay pallets registrados para finalizar el despacho.", "Advertencia",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool PuedeFinalizarDespacho(object parameter)
        {
            return TieneViajeActivo && PalletsEscaneados.Count > 0;
        }

        private void NuevoViaje(object parameter)
        {
            var ventanaRegistro = new RegistroViajeWindow();
            if (ventanaRegistro.ShowDialog() == true && ventanaRegistro.ViajeGuardado)
            {
                ViajeActivo = ventanaRegistro.ViajeCreado;
                PalletsEscaneados.Clear();
                UltimoPalletEscaneado = null;
                PalletSeleccionado = null;

                OnPropertyChanged(nameof(TotalCajas));
                OnPropertyChanged(nameof(PesoTotalViaje));
            }
        }

        private void EditarViaje(object parameter)
        {
            if (ViajeActivo != null)
            {
                var ventanaRegistro = new RegistroViajeWindow(ViajeActivo);
                if (ventanaRegistro.ShowDialog() == true && ventanaRegistro.ViajeGuardado)
                {
                    ViajeActivo = ventanaRegistro.ViajeCreado;
                }
            }
        }
        // MÉTODOS DE EDICIÓN DE PALLETS  
        private void CargarPalletParaEdicion(InformacionPallet pallet)
        {
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
        }

        private void AplicarCambios(object parameter)
        {
            if (PalletSeleccionado != null && UltimoPalletEscaneado != null)
            {
                // Verificar si hubo cambios comparando con valores originales  
                bool huboModificacion =
                    UltimoPalletEscaneado.Variedad != UltimoPalletEscaneado.VariedadOriginal ||
                    UltimoPalletEscaneado.Calibre != UltimoPalletEscaneado.CalibreOriginal ||
                    UltimoPalletEscaneado.Embalaje != UltimoPalletEscaneado.EmbalajeOriginal ||
                    UltimoPalletEscaneado.NumeroDeCajas != UltimoPalletEscaneado.NumeroDeCajasOriginal;

                // Recalcular peso si cambió el embalaje o número de cajas  
                if (UltimoPalletEscaneado.Embalaje != PalletSeleccionado.Embalaje ||
                    UltimoPalletEscaneado.NumeroDeCajas != PalletSeleccionado.NumeroDeCajas)
                {
                    var pesoEmbalaje = _accesoDatosViajes.ObtenerPesoEmbalaje(UltimoPalletEscaneado.Embalaje);
                    if (pesoEmbalaje != null)
                    {
                        UltimoPalletEscaneado.PesoUnitario = pesoEmbalaje.PesoUnitario;
                        UltimoPalletEscaneado.PesoTotal = UltimoPalletEscaneado.NumeroDeCajas * pesoEmbalaje.PesoUnitario;
                    }
                    else
                    {
                        MessageBox.Show($"No se encontró peso configurado para el embalaje '{UltimoPalletEscaneado.Embalaje}'.",
                                       "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                // Actualizar el pallet en la colección  
                var index = PalletsEscaneados.IndexOf(PalletSeleccionado);
                if (index >= 0)
                {
                    UltimoPalletEscaneado.Modificado = huboModificacion;
                    UltimoPalletEscaneado.FechaModificacion = huboModificacion ? DateTime.Now : PalletSeleccionado.FechaModificacion;

                    PalletsEscaneados[index] = UltimoPalletEscaneado;
                    PalletSeleccionado = UltimoPalletEscaneado;

                    // Actualizar totales  
                    OnPropertyChanged(nameof(TotalCajas));
                    OnPropertyChanged(nameof(PesoTotalViaje));

                    MessageBox.Show(huboModificacion ? "Cambios aplicados. Pallet marcado como modificado." : "Cambios aplicados.",
                                   "Edición", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void RevertirCambios(object parameter)
        {
            if (PalletSeleccionado != null)
            {
                // Crear una nueva instancia con los valores originales  
                UltimoPalletEscaneado = new InformacionPallet
                {
                    NumeroPallet = PalletSeleccionado.NumeroPallet,
                    // REVERTIR A VALORES ORIGINALES  
                    Variedad = PalletSeleccionado.VariedadOriginal ?? PalletSeleccionado.Variedad,
                    Calibre = PalletSeleccionado.CalibreOriginal ?? PalletSeleccionado.Calibre,
                    Embalaje = PalletSeleccionado.EmbalajeOriginal ?? PalletSeleccionado.Embalaje,
                    NumeroDeCajas = PalletSeleccionado.NumeroDeCajasOriginal != 0 ?
                                   PalletSeleccionado.NumeroDeCajasOriginal : PalletSeleccionado.NumeroDeCajas,
                    PesoUnitario = PalletSeleccionado.PesoUnitario,
                    PesoTotal = PalletSeleccionado.PesoTotal,
                    // Mantener referencias originales  
                    VariedadOriginal = PalletSeleccionado.VariedadOriginal,
                    CalibreOriginal = PalletSeleccionado.CalibreOriginal,
                    EmbalajeOriginal = PalletSeleccionado.EmbalajeOriginal,
                    NumeroDeCajasOriginal = PalletSeleccionado.NumeroDeCajasOriginal,
                    Modificado = false, // Resetear estado de modificación  
                    FechaModificacion = PalletSeleccionado.FechaModificacion
                };

                MessageBox.Show("Cambios revertidos a los valores originales.", "Edición",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            OnPropertyChanged(nameof(UltimoPalletEscaneado));
        }

        private void EliminarPallet(object parameter)
        {
            if (PalletSeleccionado != null)
            {
                var resultado = MessageBox.Show($"¿Está seguro de eliminar el pallet {PalletSeleccionado.NumeroPallet}?",
                                               "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    PalletsEscaneados.Remove(PalletSeleccionado);
                    PalletSeleccionado = null;
                    UltimoPalletEscaneado = null;

                    // Actualizar totales  
                    OnPropertyChanged(nameof(TotalCajas));
                    OnPropertyChanged(nameof(PesoTotalViaje));

                    MessageBox.Show("Pallet eliminado exitosamente.", "Eliminación",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        public void ActualizarTotales()
        {
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