using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace ImperioChocoyo;

[Table("historial")]
public class HistorialModel : BaseModel
{
    [PrimaryKey("id_historial", false)]
    public int Id { get; set; }

    [Column("id_autobus")]
    public int IdAutobus { get; set; }

    [Column("fecha_registro")]
    public DateTime? FechaRegistro { get; set; }

    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("costo")]
    public decimal? Costo { get; set; }

    [Column("kilometraje")]
    public int? Kilometraje { get; set; }

    [JsonIgnore]
    public string AutobusLabel { get; set; } = string.Empty;
}