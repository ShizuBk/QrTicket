using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Layout.Properties;
using iText.Kernel.Geom;
using iText.Layout.Borders;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf.Canvas;
using TicketAPI.Dto;
using System.Globalization;
using System;
using System.IO;
using System.Linq;

namespace TicketAPI.Services.Processes
{
    public class TicketManager
    {
        public byte[] CreatePDFTicket(byte[] qr, TicketDetailsDto ticketDetails)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                
                    PdfWriter writer = new PdfWriter(ms);
                    PdfDocument pdfDocument = new PdfDocument(writer);
                    Document document = new Document(pdfDocument, PageSize.A4);
                    document.SetMargins(40, 40, 40, 40);

                    Color azulPartenon = new DeviceRgb(0, 120, 212);
                    PdfFont fontCursiva = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

                    // MARCA DE AGUA ---
                    string rootPath = Directory.GetCurrentDirectory();
                    string rutaIcono = System.IO.Path.Combine(rootPath, "wwwroot", "img", "partenon-qr.png");
                    
                    if (!File.Exists(rutaIcono))
                    {
                        rutaIcono = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "img", "partenon-qr.png");
                    }

                    pdfDocument.AddNewPage();
                    
                    if (File.Exists(rutaIcono))
                    {
                        ImageData imageData = ImageDataFactory.Create(rutaIcono);
                        Image watermark = new Image(imageData).SetWidth(300).SetOpacity(0.25f); // Reducido un poco para no estorbar lectura
                        
                        PdfCanvas canvas = new PdfCanvas(pdfDocument.GetFirstPage());
                        new Canvas(canvas, pdfDocument.GetDefaultPageSize())
                                .ShowTextAligned(new Paragraph().Add(watermark), 297, 580, 1, TextAlignment.CENTER, VerticalAlignment.MIDDLE, 0);
                    }

                    // ---  DISEÑO DEL TICKET-PDF ---
                    Table ticketWrapper = new Table(1).SetWidth(UnitValue.CreatePercentValue(100));
                    ticketWrapper.SetBorder(new SolidBorder(azulPartenon, 2f));
                    ticketWrapper.SetBackgroundColor(ColorConstants.WHITE, 0.6f); 

                    // ENCABEZADO
                    Cell headerCell = new Cell().SetBorder(Border.NO_BORDER).SetPadding(20).SetTextAlignment(TextAlignment.CENTER);
                    headerCell.Add(new Paragraph("PARTENÓN ZIHUATANEJO")
                        .SetFontColor(azulPartenon).SetFontSize(26).SimulateBold());
                    headerCell.Add(new Paragraph("BOLETO DIGITAL DE ACCESO")
                        .SetFont(fontCursiva).SetFontSize(10).SetFontColor(ColorConstants.GRAY));
                    ticketWrapper.AddCell(headerCell);

                    // LÍNEA DIVISORIA
                    ticketWrapper.AddCell(new Cell()
                        .SetBorderTop(new DashedBorder(azulPartenon, 1f))
                        .SetBorderBottom(Border.NO_BORDER).SetBorderLeft(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER));

                    // CUERPO (DATOS + QR)
                    Table contentTable = new Table(UnitValue.CreatePercentArray(new float[] { 60, 40 })).SetWidth(UnitValue.CreatePercentValue(100));
                    
                    Cell infoCell = new Cell().SetBorder(Border.NO_BORDER).SetPadding(25);
                    infoCell.Add(new Paragraph("TITULAR").SetFontSize(8).SetFontColor(ColorConstants.GRAY));
                    infoCell.Add(new Paragraph($"{ticketDetails.TitularName} {ticketDetails.TitularLastName}").SetFontSize(14).SimulateBold());
                    
                    infoCell.Add(new Paragraph("\nFECHA DE COMPRA").SetFontSize(8).SetFontColor(ColorConstants.GRAY));
                    infoCell.Add(new Paragraph(ticketDetails.PurchaseDate.ToString("dd/MM/yyyy HH:mm")).SetFontSize(12));
                    
                    infoCell.Add(new Paragraph("\nASISTENTES").SetFontSize(8).SetFontColor(ColorConstants.GRAY));
                    infoCell.Add(new Paragraph($"{ticketDetails.AssistantNumber} Persona(s)").SetFontSize(12));
                    contentTable.AddCell(infoCell);

                    Cell qrCell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE).SetTextAlignment(TextAlignment.CENTER).SetPaddingRight(20);
                    if (qr != null && qr.Length > 0)
                    {
                        Image img = new Image(ImageDataFactory.Create(qr)).SetWidth(130);
                        qrCell.Add(img);
                        qrCell.Add(new Paragraph("Pase de Seguridad").SetFontSize(8).SetFontColor(azulPartenon).SetMarginTop(5));
                    }
                    contentTable.AddCell(qrCell);
                    ticketWrapper.AddCell(new Cell().Add(contentTable).SetBorder(Border.NO_BORDER));

                    // --- DETALLE DE COMPRA Y TOTAL PAGADO ---
                    Table footerInfoTable = new Table(UnitValue.CreatePercentArray(new float[] { 55, 45 })).SetWidth(UnitValue.CreatePercentValue(100));

                    Cell detailsCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingLeft(25).SetPaddingBottom(15);
                    if (ticketDetails.AssistantDetails != null && ticketDetails.AssistantDetails.Any())
                    {
                        detailsCell.Add(new Paragraph("RESUMEN DE ENTRADAS:").SetFontSize(9).SimulateBold().SetFontColor(azulPartenon));
                        
                        // Agrupamiento por categoría
                        var resumenCategorias = ticketDetails.AssistantDetails
                            .Select(d => d.Contains("-") ? d.Split('-')[1].Trim() : d.Trim())
                            .GroupBy(tipo => tipo)
                            .Select(grupo => new { 
                                Categoria = grupo.Key, 
                                Cantidad = grupo.Count() 
                            });

                        foreach (var item in resumenCategorias)
                        {
                            detailsCell.Add(new Paragraph($"{item.Categoria}: {item.Cantidad}")
                                .SetFontSize(8)
                                .SetMarginLeft(5));
                        }

                        detailsCell.Add(new Paragraph("__________________________")
                            .SetFontSize(8).SetFontColor(ColorConstants.LIGHT_GRAY));
                        
                        detailsCell.Add(new Paragraph($"TOTAL PAGADO: {ticketDetails.TotalAmount.ToString("C2", CultureInfo.GetCultureInfo("es-MX"))} MXN")
                            .SetFontSize(11)
                            .SimulateBold()
                            .SetFontColor(ColorConstants.BLACK)
                            .SetMarginTop(5));
                    }
                    footerInfoTable.AddCell(detailsCell);

                    Cell rightColumnCell = new Cell().SetBorder(Border.NO_BORDER).SetPadding(0);

                    Table innerNoteTable = new Table(1).SetWidth(UnitValue.CreatePercentValue(90)); 
                    innerNoteTable.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                    innerNoteTable.SetMarginRight(20);

                    Cell noteBox = new Cell().SetPadding(8);
                    noteBox.SetBorder(new SolidBorder(azulPartenon, 0.8f));
                    noteBox.SetVerticalAlignment(VerticalAlignment.TOP); 

                    noteBox.Add(new Paragraph("NOTA IMPORTANTE:")
                        .SetFontSize(7)
                        .SimulateBold()
                        .SetFontColor(ColorConstants.RED)
                        .SetMarginBottom(2));

                    Paragraph pNota = new Paragraph()
                        .SetFontSize(6.5f)
                        .SetMultipliedLeading(1.0f)
                        .Add("Es indispensable presentar documentación vigente:\n")
                        .Add("• ").Add(new Text("Estudiantes:").SimulateBold()).Add(" Credencial escolar.\n")
                        .Add("• ").Add(new Text("Locales:").SimulateBold()).Add(" INE con domicilio local.\n")
                        .Add("• ").Add(new Text("Adultos Mayores:").SimulateBold()).Add(" Credencial INAPAM.\n")
                        .Add("• ").Add(new Text("Niño menor a 5 años:").SimulateBold()).Add(" No paga.\n");    

                    noteBox.Add(pNota);

                    innerNoteTable.AddCell(noteBox);
                    rightColumnCell.Add(innerNoteTable);

                    footerInfoTable.AddCell(rightColumnCell);

                    ticketWrapper.AddCell(new Cell().Add(footerInfoTable).SetBorder(Border.NO_BORDER));

                    // PIE DE PÁGINA
                    Cell footerCell = new Cell().SetBorder(Border.NO_BORDER).SetPadding(15).SetBackgroundColor(new DeviceRgb(245, 245, 245)).SetTextAlignment(TextAlignment.CENTER);
                    footerCell.Add(new Paragraph("Presente este código QR en la entrada principal del recinto.")
                        .SetFontSize(8).SetFontColor(ColorConstants.DARK_GRAY));
                    footerCell.Add(new Paragraph("Carretera Escénica a Playa La Ropa S/N, Zihuatanejo, Guerrero.")
                        .SetFontSize(7).SetFontColor(ColorConstants.GRAY));
                    ticketWrapper.AddCell(footerCell);

                    // FIN DEL CUERPO DEL TICKET-PDF
                    document.Add(ticketWrapper);
                    document.Close();
                    
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en iText: {ex.Message}");
                return Array.Empty<byte>();
            }
        }
    }
}