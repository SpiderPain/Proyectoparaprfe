using Postgrest.Attributes;
using Postgrest.Models;

namespace ImperioChocoyo;

[Table("documento")]
public class DocumentoModel : BaseModel
{
    [PrimaryKey("id_documento", false)]
    public int Id { get; set; }

    [Column("id_autobus")]
    public int IdAutobus { get; set; }

    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Column("numero")]
    public string Numero { get; set; } = string.Empty;

    [Column("fecha_emision")]
    public DateTime? FechaEmision { get; set; }

    [Column("fecha_vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    public string AutobusLabel { get; set; } = string.Empty;
}