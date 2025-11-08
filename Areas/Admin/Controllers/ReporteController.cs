using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data; 
using StayGo.Models.Enums;

using System.Globalization;
using StayGo.ViewModels.Admin;

// LIBRERÍAS DE EXPORTACIÓN
using OfficeOpenXml; 
using QuestPDF.Fluent; 
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace StayGo.Areas.Admin.Controllers;

[Area("Admin")] // ← AGREGAR ESTE ATTRIBUTE
[Authorize(Roles = "Admin")]
public class ReporteController : Controller
{
    private readonly StayGoContext _context;

    public ReporteController(StayGoContext context)
    {
        _context = context;
    }

    // --- ACCIÓN PRINCIPAL ---

    public async Task<IActionResult> Index()
    {
        // 1. OBTENER DATOS (Ahora llamando a los métodos privados)
        var reservasPorMes = await GetReservasPorMesAsync();
        var topPropiedades = await GetTopPropiedadesAsync();
        var topUsuarios = await GetTopUsuariosAsync();

        // 2. ARMAR VIEWMODEL Y ENVIAR A LA VISTA
        var viewModel = new ReportesViewModel
        {
            ReservasPorMes = reservasPorMes,
            TopPropiedades = topPropiedades,
            TopUsuarios = topUsuarios
        };

        return View(viewModel);
    }

    // --- ACCIONES DE EXPORTACIÓN ---

 
public async Task<IActionResult> ExportarExcel()
{
    var topPropiedades = await GetTopPropiedadesAsync(take: 50);
    var topUsuarios = await GetTopUsuariosAsync(take: 50);

    using (var package = new ExcelPackage())
    {

        var brandHeader = System.Drawing.Color.FromArgb(28, 64, 52);    // verde Homely oscuro
        var accentColor = System.Drawing.Color.FromArgb(212, 175, 55);  // dorado elegante
        var headerBg    = System.Drawing.Color.FromArgb(245, 245, 245); // gris claro

        // ===================== HOJA 1: PROPIEDADES =====================
        var ws1 = package.Workbook.Worksheets.Add("Top Propiedades");

        // ENCABEZADO PRINCIPAL
        ws1.Cells["A1:C1"].Merge = true;
        ws1.Cells["A1"].Value = "HOMELY - Reporte de Propiedades";
        ws1.Cells["A1"].Style.Font.Bold = true;
        ws1.Cells["A1"].Style.Font.Size = 18;
        ws1.Cells["A1"].Style.Font.Color.SetColor(accentColor);
        ws1.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
        ws1.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        ws1.Cells["A1"].Style.Fill.BackgroundColor.SetColor(brandHeader);
        ws1.Row(1).Height = 28;

        // FECHA DE GENERACIÓN
        ws1.Cells["A2"].Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws1.Cells["A2"].Style.Font.Italic = true;
        ws1.Cells["A2"].Style.Font.Size = 10;
        ws1.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

        // SUBTÍTULO
        ws1.Cells["A3"].Value = "Propiedades más reservadas";
        ws1.Cells["A3"].Style.Font.Bold = true;
        ws1.Cells["A3"].Style.Font.Size = 13;

        // DATOS
        ws1.Cells["A5"].LoadFromCollection(topPropiedades, true);
        ws1.Cells["A5"].Value = "ID Propiedad";
        ws1.Cells["B5"].Value = "Nombre de la propiedad";
        ws1.Cells["C5"].Value = "Cantidad de reservas";

        // CABECERAS
        using (var range = ws1.Cells["A5:C5"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(headerBg);
            range.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(64, 64, 64));
            range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.LightGray);
        }

        // TABLA
        var lastRow1 = 5 + topPropiedades.Count;
        var tableRange1 = ws1.Cells[$"A5:C{lastRow1}"];
        var table1 = ws1.Tables.Add(tableRange1, "TablaPropiedades");
        table1.TableStyle = OfficeOpenXml.Table.TableStyles.Medium15; // gris moderno

        ws1.Cells[ws1.Dimension.Address].AutoFitColumns();
        ws1.View.FreezePanes(6, 1);

        // ===================== HOJA 2: USUARIOS =====================
        var ws2 = package.Workbook.Worksheets.Add("Top Usuarios");

        ws2.Cells["A1:C1"].Merge = true;
        ws2.Cells["A1"].Value = "HOMELY - Reporte de Usuarios";
        ws2.Cells["A1"].Style.Font.Bold = true;
        ws2.Cells["A1"].Style.Font.Size = 18;
        ws2.Cells["A1"].Style.Font.Color.SetColor(accentColor);
        ws2.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
        ws2.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        ws2.Cells["A1"].Style.Fill.BackgroundColor.SetColor(brandHeader);
        ws2.Row(1).Height = 28;

        ws2.Cells["A2"].Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws2.Cells["A2"].Style.Font.Italic = true;
        ws2.Cells["A2"].Style.Font.Size = 10;
        ws2.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

        ws2.Cells["A3"].Value = "Usuarios con más reservas";
        ws2.Cells["A3"].Style.Font.Bold = true;
        ws2.Cells["A3"].Style.Font.Size = 13;

        ws2.Cells["A5"].LoadFromCollection(topUsuarios, true);
        ws2.Cells["A5"].Value = "ID Usuario";
        ws2.Cells["B5"].Value = "Email";
        ws2.Cells["C5"].Value = "Cantidad de reservas";

        using (var range = ws2.Cells["A5:C5"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(headerBg);
            range.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(64, 64, 64));
            range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.LightGray);
        }

        var lastRow2 = 5 + topUsuarios.Count;
        var tableRange2 = ws2.Cells[$"A5:C{lastRow2}"];
        var table2 = ws2.Tables.Add(tableRange2, "TablaUsuarios");
        table2.TableStyle = OfficeOpenXml.Table.TableStyles.Medium15;

        ws2.Cells[ws2.Dimension.Address].AutoFitColumns();
        ws2.View.FreezePanes(6, 1);

        // ===================== DEVOLVER =====================
        var stream = new MemoryStream();
        await package.SaveAsAsync(stream);
        stream.Position = 0;

        string excelName = $"Homely-Reporte-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            excelName);
    }
}



public async Task<IActionResult> ExportarPdf()
{
    var topPropiedades = await GetTopPropiedadesAsync();
    var topUsuarios = await GetTopUsuariosAsync();

    // totales para poner en el resumen
    var totalReservasProp = topPropiedades.Sum(x => x.CantidadReservas);
    var totalReservasUsuarios = topUsuarios.Sum(x => x.CantidadReservas);

    QuestPDF.Settings.License = LicenseType.Community;

    var document = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(11));

            // ===== HEADER =====
            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("HOMELY - Reporte de Actividad")
                        .SemiBold().FontSize(18).FontColor(Colors.Green.Darken2);
                    col.Item().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(80).AlignRight().Text(text =>
                {
                    text.Span("Estado").SemiBold();
                    text.Line("Administración").FontSize(10);
                });
            });

            // ===== CONTENIDO =====
            page.Content().Column(col =>
            {
                col.Spacing(20);

                // ---------- bloque de resumen ----------
                col.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(10).Background(Colors.Grey.Lighten5).Column(box =>
                {
                    box.Spacing(5);
                    box.Item().Text("Resumen").SemiBold();
                    box.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Propiedades listadas en el top: {topPropiedades.Count}");
                        r.RelativeItem().Text($"Reservas totales (propiedades): {totalReservasProp}");
                    });
                    box.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Usuarios en el top: {topUsuarios.Count}");
                        r.RelativeItem().Text($"Reservas totales (usuarios): {totalReservasUsuarios}");
                    });
                });

                // ---------- tabla de propiedades ----------
                col.Item().Text("Propiedades Más Reservadas")
                    .SemiBold().FontSize(13);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);   // nombre propiedad
                        columns.RelativeColumn(1);   // reservas
                    });

                    // cabecera
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Green.Lighten4).Padding(5)
                            .Text("Propiedad").SemiBold();
                        header.Cell().Background(Colors.Green.Lighten4).Padding(5)
                            .AlignRight().Text("Reservas").SemiBold();
                    });

                    foreach (var item in topPropiedades)
                    {
                        table.Cell().Padding(5).Text(item.NombrePropiedad);
                        table.Cell().Padding(5).AlignRight().Text(item.CantidadReservas.ToString());
                    }
                });

                // ---------- tabla de usuarios ----------
                col.Item().PaddingTop(10).Text("Usuarios Más Activos")
                    .SemiBold().FontSize(13);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);  // email
                        columns.RelativeColumn(1);  // reservas
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Green.Lighten4).Padding(5)
                            .Text("Email Usuario").SemiBold();
                        header.Cell().Background(Colors.Green.Lighten4).Padding(5)
                            .AlignRight().Text("Reservas").SemiBold();
                    });

                    foreach (var item in topUsuarios)
                    {
                        table.Cell().Padding(5).Text(item.EmailUsuario);
                        table.Cell().Padding(5).AlignRight().Text(item.CantidadReservas.ToString());
                    }
                });
            });

            // ===== FOOTER =====
        page.Footer().AlignCenter().Text(x =>
        {
            x.DefaultTextStyle(s => s.FontSize(10).FontColor(Colors.Grey.Darken1));
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
        });

        });
    });

    var pdfBytes = document.GeneratePdf();
    var pdfName = $"Homely-Reporte-{DateTime.Now:yyyyMMddHHmmss}.pdf";
    return File(pdfBytes, "application/pdf", pdfName);
}


    // --- MÉTODOS PRIVADOS DE DATOS (REFACTORIZACIÓN) ---

private async Task<List<ReservasPorMesDto>> GetReservasPorMesAsync()
{
    var culture = new CultureInfo("es-ES");
    
    // Obtener todos los meses del último año para asegurar que tengamos todos los meses
    var fechaActual = DateTime.Now;
    var ultimoAno = fechaActual.Year;
    var mesesDelAno = Enumerable.Range(1, 12)
        .Select(mes => new DateTime(ultimoAno, mes, 1).ToString("MMM yyyy", culture))
        .ToList();

    // Consulta a la base de datos
    var resultadosDb = await _context.Reservas
        .Where(r => (r.Estado == EstadoReserva.Confirmada || r.Estado == EstadoReserva.Finalizada) 
                 && r.CheckIn.Year >= ultimoAno - 1) // Último año + año actual
        .GroupBy(r => new { r.CheckIn.Year, r.CheckIn.Month })
        .Select(g => new
        {
            Year = g.Key.Year,
            Month = g.Key.Month,
            Cantidad = g.Count()
        })
        .ToListAsync();

    // Combinar resultados
    var resultados = mesesDelAno.Select(mesAno => 
    {
        var fecha = DateTime.ParseExact(mesAno, "MMM yyyy", culture);
        var dato = resultadosDb.FirstOrDefault(r => r.Year == fecha.Year && r.Month == fecha.Month);
        
        return new ReservasPorMesDto
        {
            MesAno = mesAno,
            Cantidad = dato?.Cantidad ?? 0
        };
    }).ToList();

    return resultados;
}

    private async Task<List<PropiedadMasReservadaDto>> GetTopPropiedadesAsync(int take = 10)
    {
        return await _context.Reservas
            .Where(r => r.Estado == EstadoReserva.Confirmada || r.Estado == EstadoReserva.Finalizada)
            .Include(r => r.Propiedad)
            .GroupBy(r => new { r.PropiedadId, r.Propiedad.Titulo })
            .Select(g => new PropiedadMasReservadaDto
            {
                PropiedadId = g.Key.PropiedadId,
                NombrePropiedad = g.Key.Titulo,
                CantidadReservas = g.Count()
            })
            .OrderByDescending(x => x.CantidadReservas)
            .Take(take) // Usamos el parámetro
            .ToListAsync();
    }

    private async Task<List<UsuarioMasActivoDto>> GetTopUsuariosAsync(int take = 10)
    {
        return await _context.Reservas
            .Where(r => r.Estado == EstadoReserva.Confirmada || r.Estado == EstadoReserva.Finalizada)
            .Include(r => r.Usuario)
            .GroupBy(r => new { r.UsuarioId, r.Usuario.Email })
            .Select(g => new UsuarioMasActivoDto
            {
                UsuarioId = g.Key.UsuarioId,
                EmailUsuario = g.Key.Email ?? "Usuario no disponible",
                CantidadReservas = g.Count()
            })
            .OrderByDescending(x => x.CantidadReservas)
            .Take(take) // Usamos el parámetro
            .ToListAsync();
    }
}