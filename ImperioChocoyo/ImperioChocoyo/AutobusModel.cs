using Postgrest.Attributes;
using Postgrest.Models;

namespace ImperioChocoyo;

[Table("autobus")]
public class AutobusModel : BaseModel
{
    [PrimaryKey("id_autobus", false)]
    public int Id { get; set; }

    [Column("placa")]
    public string Placa { get; set; } = string.Empty;

    [Column("capacidad")]
    public int Capacidad { get; set; }

    [Column("vin")]
    public string Vin { get; set; } = string.Empty;

    [Column("marca")]
    public string Marca { get; set; } = string.Empty;

    [Column("modelo")]
    public string Modelo { get; set; } = string.Empty;

    [Column("anio")]
    public int Anio { get; set; }

    [Column("color")]
    public string Color { get; set; } = string.Empty;

    [Column("categoria")]
    public string Categoria { get; set; } = string.Empty;

    [Column("fecha_adquisicion")]
    public DateTime FechaAdquisicion { get; set; }

    private string _estado = string.Empty;

    [Column("estado")]
    public string Estado
    {
        get => _estado;
        set => _estado = NormalizarEstado(value);
    }

    private static string NormalizarEstado(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado)) return string.Empty;

        var normalizado = estado.Trim();
        if (string.Equals(normalizado, "activo", StringComparison.OrdinalIgnoreCase)) return "Activo";
        if (string.Equals(normalizado, "en mantenimiento", StringComparison.OrdinalIgnoreCase)) return "En Mantenimiento";
        if (string.Equals(normalizado, "inactivo", StringComparison.OrdinalIgnoreCase)) return "Inactivo";

        return normalizado;
    }
}