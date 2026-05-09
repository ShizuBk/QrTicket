public class EventSearchFiltersDto
{
    public string? Term { get; set; } // Palabra clave (Nombre del Evento)
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? IsVisible { get; set; }
    public bool? IsSoldOut { get; set; }
}