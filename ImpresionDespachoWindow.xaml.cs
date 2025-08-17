// ImpresionDespachoWindow.xaml.cs  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using AplicacionDespacho.Models;

namespace AplicacionDespacho
{
    public partial class ImpresionDespachoWindow : Window
    {
        private Viaje _viaje;
        private List<InformacionPallet> _pallets;
        private List<ResumenVariedadEmbalaje> _resumen;

        public ImpresionDespachoWindow(Viaje viaje, List<InformacionPallet> pallets)
        {
            InitializeComponent();
            _viaje = viaje;
            _pallets = pallets;
            CargarDatos();
        }

        private void CargarDatos()
        {
            // Cargar información del viaje  
            txtFecha.Text = _viaje.Fecha.ToString("dd/MM/yyyy");
            txtNumeroViaje.Text = _viaje.NumeroViaje.ToString();
            txtNumeroGuia.Text = _viaje.NumeroGuia;
            txtResponsable.Text = _viaje.Responsable;
            txtEmpresa.Text = _viaje.NombreEmpresa;
            txtConductor.Text = _viaje.NombreConductor;
            txtPlaca.Text = _viaje.PlacaVehiculo;
            txtDestino.Text = _viaje.PuntoLlegada;

            // Cargar pallets  
            dgPallets.ItemsSource = _pallets;

            // Generar resumen por variedad y embalaje  
            GenerarResumen();

            // Calcular totales generales  
            var totalCajas = _pallets.Sum(p => p.NumeroDeCajas);
            var totalKilos = _pallets.Sum(p => p.PesoTotal);

            txtTotalCajasGeneral.Text = totalCajas.ToString();
            txtTotalKilosGeneral.Text = totalKilos.ToString("F3");
        }

        private void GenerarResumen()
        {
            _resumen = _pallets
                .GroupBy(p => new { p.Variedad, p.Embalaje })
                .Select(g => new ResumenVariedadEmbalaje
                {
                    VariedadEmbalaje = $"{g.Key.Variedad} - {g.Key.Embalaje}",
                    TotalCajas = g.Sum(p => p.NumeroDeCajas),
                    TotalKilos = g.Sum(p => p.PesoTotal)
                })
                .OrderBy(r => r.VariedadEmbalaje)
                .ToList();

            dgResumen.ItemsSource = _resumen;
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Crear documento para impresión  
                    FlowDocument documento = CrearDocumentoImpresion();

                    // Configurar documento para impresión  
                    documento.PageHeight = printDialog.PrintableAreaHeight;
                    documento.PageWidth = printDialog.PrintableAreaWidth;
                    documento.PagePadding = new Thickness(50);
                    documento.ColumnGap = 0;
                    documento.ColumnWidth = printDialog.PrintableAreaWidth;

                    // Imprimir  
                    IDocumentPaginatorSource idpSource = documento;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "Despacho - " + _viaje.NumeroGuia);

                    MessageBox.Show("Documento enviado a impresión correctamente.", "Impresión",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnVistaPrevia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Crear ventana de vista previa  
                var ventanaPrevia = new Window
                {
                    Title = "Vista Previa - Despacho " + _viaje.NumeroGuia,
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var documentViewer = new DocumentViewer();
                FlowDocument documento = CrearDocumentoImpresion();
                documentViewer.Document = documento;

                ventanaPrevia.Content = documentViewer;
                ventanaPrevia.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar vista previa: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument CrearDocumentoImpresion()
        {
            FlowDocument documento = new FlowDocument();

            // Título  
            Paragraph titulo = new Paragraph(new Run("DESPACHO DE PRODUCTOS"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            documento.Blocks.Add(titulo);

            // Información del viaje  
            Table tablaInfo = new Table();
            tablaInfo.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            tablaInfo.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup grupoInfo = new TableRowGroup();

            grupoInfo.Rows.Add(CrearFilaInfo("Fecha:", _viaje.Fecha.ToString("dd/MM/yyyy"), "N° Viaje:", _viaje.NumeroViaje.ToString()));
            grupoInfo.Rows.Add(CrearFilaInfo("N° Guía:", _viaje.NumeroGuia, "Responsable:", _viaje.Responsable));
            grupoInfo.Rows.Add(CrearFilaInfo("Empresa:", _viaje.NombreEmpresa, "Conductor:", _viaje.NombreConductor));
            grupoInfo.Rows.Add(CrearFilaInfo("Placa:", _viaje.PlacaVehiculo, "Destino:", _viaje.PuntoLlegada ?? ""));

            tablaInfo.RowGroups.Add(grupoInfo);
            documento.Blocks.Add(tablaInfo);

            // Espacio  
            documento.Blocks.Add(new Paragraph(new Run(" ")) { Margin = new Thickness(0, 10, 0, 10) });

            // Tabla de pallets  
            Paragraph tituloPallets = new Paragraph(new Run("DETALLE DE PALLETS"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            documento.Blocks.Add(tituloPallets);

            Table tablaPallets = CrearTablaPallets();
            documento.Blocks.Add(tablaPallets);

            // Espacio  
            documento.Blocks.Add(new Paragraph(new Run(" ")) { Margin = new Thickness(0, 10, 0, 10) });

            // Tabla resumen  
            Paragraph tituloResumen = new Paragraph(new Run("RESUMEN POR VARIEDAD Y EMBALAJE"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            documento.Blocks.Add(tituloResumen);

            Table tablaResumen = CrearTablaResumen();
            documento.Blocks.Add(tablaResumen);

            // Totales generales  
            var totalCajas = _pallets.Sum(p => p.NumeroDeCajas);
            var totalKilos = _pallets.Sum(p => p.PesoTotal);

            Paragraph totales = new Paragraph()
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            totales.Inlines.Add(new Run($"TOTAL GENERAL - Cajas: {totalCajas} - Kilos: {totalKilos:F3}"));
            documento.Blocks.Add(totales);

            return documento;
        }

        private TableRow CrearFilaInfo(string etiqueta1, string valor1, string etiqueta2, string valor2)
        {
            TableRow fila = new TableRow();

            TableCell celda1 = new TableCell();
            celda1.Blocks.Add(new Paragraph(new Run(etiqueta1) { FontWeight = FontWeights.Bold }));
            celda1.Blocks.Add(new Paragraph(new Run(valor1)));

            TableCell celda2 = new TableCell();
            celda2.Blocks.Add(new Paragraph(new Run(etiqueta2) { FontWeight = FontWeights.Bold }));
            celda2.Blocks.Add(new Paragraph(new Run(valor2)));

            fila.Cells.Add(celda1);
            fila.Cells.Add(celda2);

            return fila;
        }

        private Table CrearTablaPallets()
        {
            Table tabla = new Table();
            tabla.CellSpacing = 0;
            tabla.BorderBrush = Brushes.Black;
            tabla.BorderThickness = new Thickness(1);

            // Definir columnas  
            tabla.Columns.Add(new TableColumn { Width = new GridLength(80) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(100) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(80) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(100) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(70) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(90) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(100) });

            // Encabezados  
            TableRowGroup encabezados = new TableRowGroup();
            TableRow filaEncabezado = new TableRow();
            filaEncabezado.Background = Brushes.LightGray;

            string[] headers = { "Pallet", "Variedad", "Calibre", "Embalaje", "N° Cajas", "Peso Unit. (kg)", "Peso Total (kg)" };
            foreach (string header in headers)
            {
                TableCell celda = new TableCell(new Paragraph(new Run(header) { FontWeight = FontWeights.Bold }));
                celda.BorderBrush = Brushes.Black;
                celda.BorderThickness = new Thickness(1);
                celda.Padding = new Thickness(5);
                filaEncabezado.Cells.Add(celda);
            }
            encabezados.Rows.Add(filaEncabezado);
            tabla.RowGroups.Add(encabezados);

            // Datos  
            TableRowGroup datos = new TableRowGroup();
            foreach (var pallet in _pallets)
            {
                TableRow fila = new TableRow();

                string[] valores = {
                    pallet.NumeroPallet,
                    pallet.Variedad,
                    pallet.Calibre,
                    pallet.Embalaje,
                    pallet.NumeroDeCajas.ToString(),
                    pallet.PesoUnitario.ToString("F3"),
                    pallet.PesoTotal.ToString("F3")
                };

                foreach (string valor in valores)
                {
                    TableCell celda = new TableCell(new Paragraph(new Run(valor)));
                    celda.BorderBrush = Brushes.Black;
                    celda.BorderThickness = new Thickness(1);
                    celda.Padding = new Thickness(5);
                    fila.Cells.Add(celda);
                }
                datos.Rows.Add(fila);
            }
            tabla.RowGroups.Add(datos);

            return tabla;
        }

        private Table CrearTablaResumen()
        {
            Table tabla = new Table();
            tabla.CellSpacing = 0;
            tabla.BorderBrush = Brushes.Black;
            tabla.BorderThickness = new Thickness(1);

            // Definir columnas  
            tabla.Columns.Add(new TableColumn { Width = new GridLength(300) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(150) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(150) });

            // Encabezados  
            TableRowGroup encabezados = new TableRowGroup();
            TableRow filaEncabezado = new TableRow();
            filaEncabezado.Background = Brushes.LightGray;

            string[] headers = { "Variedad - Embalaje", "Total Cajas", "Total Kilos" };
            foreach (string header in headers)
            {
                TableCell celda = new TableCell(new Paragraph(new Run(header) { FontWeight = FontWeights.Bold }));
                celda.BorderBrush = Brushes.Black;
                celda.BorderThickness = new Thickness(1);
                celda.Padding = new Thickness(5);
                filaEncabezado.Cells.Add(celda);
            }
            encabezados.Rows.Add(filaEncabezado);
            tabla.RowGroups.Add(encabezados);

            // Datos del resumen  
            TableRowGroup datos = new TableRowGroup();
            foreach (var item in _resumen)
            {
                TableRow fila = new TableRow();

                string[] valores = {
                    item.VariedadEmbalaje,
                    item.TotalCajas.ToString(),
                    item.TotalKilos.ToString("F3")
                };

                foreach (string valor in valores)
                {
                    TableCell celda = new TableCell(new Paragraph(new Run(valor) { FontWeight = FontWeights.Bold }));
                    celda.BorderBrush = Brushes.Black;
                    celda.BorderThickness = new Thickness(1);
                    celda.Padding = new Thickness(5);
                    fila.Cells.Add(celda);
                }
                datos.Rows.Add(fila);
            }
            tabla.RowGroups.Add(datos);

            return tabla;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}