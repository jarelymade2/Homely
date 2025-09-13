namespace StayGo.Models.ValueObjects;
public class Direccion
{
    public string Pais { get; set; } = "";
    public string Ciudad { get; set; } = "";
    public string Linea1 { get; set; } = "";
    public string? Linea2 { get; set; }
    public string? CodigoPostal { get; set; }
}
