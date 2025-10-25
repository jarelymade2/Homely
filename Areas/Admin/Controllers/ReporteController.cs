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
        // 1. Obtenemos los datos (¡reutilizando los métodos!)
        var topPropiedades = await GetTopPropiedadesAsync(take: 50); // Podemos incluso tomar más para Excel
        var topUsuarios = await GetTopUsuariosAsync(take: 50);
        
        ExcelPackage.License.SetNonCommercialPersonal("YourName");
        using (var package = new ExcelPackage())
        {
            // Hoja 1: Top Propiedades
            var worksheet1 = package.Workbook.Worksheets.Add("Top Propiedades");
            // Usamos los DTOs, que funciona perfecto con LoadFromCollection
            worksheet1.Cells["A1"].LoadFromCollection(topPropiedades, true); 
            worksheet1.Cells.AutoFitColumns();

            // Hoja 2: Top Usuarios
            var worksheet2 = package.Workbook.Worksheets.Add("Top Usuarios");
            worksheet2.Cells["A1"].LoadFromCollection(topUsuarios, true);
            worksheet2.Cells.AutoFitColumns();

            // 3. Devolvemos el archivo
            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;
            
            string excelName = $"ReporteStayGo-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
    }

    public async Task<IActionResult> ExportarPdf()
    {
        // 1. Obtenemos los datos (¡reutilizando de nuevo!)
        var topPropiedades = await GetTopPropiedadesAsync();
        var topUsuarios = await GetTopUsuariosAsync();

        // 2. Configura QuestPDF
        // 2. Configura QuestPDF
        QuestPDF.Settings.License = LicenseType.Community; // This is the correct setting

        // 3. Genera el documento PDF (tu código de PDF es perfecto, no cambia)
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text($"Reporte StayGo - {DateTime.Now:g}")
                    .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                page.Content()
                    .Column(col =>
                    {
                        col.Spacing(20);
                        col.Item().Text("Propiedades Más Reservadas").Bold().FontSize(14);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Propiedad");
                                header.Cell().AlignRight().Text("Reservas");
                            });
                            foreach (var item in topPropiedades)
                            {
                                table.Cell().Text(item.NombrePropiedad);
                                table.Cell().AlignRight().Text(item.CantidadReservas.ToString());
                            }
                        });
                        
                        col.Item().Text("Usuarios Más Activos").Bold().FontSize(14);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Email Usuario");
                                header.Cell().AlignRight().Text("Reservas");
                            });
                            foreach (var item in topUsuarios)
                            {
                                table.Cell().Text(item.EmailUsuario);
                                table.Cell().AlignRight().Text(item.CantidadReservas.ToString());
                            }
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
            });
        });

        // 4. Devuelve el archivo PDF
        byte[] pdfBytes = document.GeneratePdf();
        string pdfName = $"ReporteStayGo-{DateTime.Now:yyyyMMddHHmmss}.pdf";
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