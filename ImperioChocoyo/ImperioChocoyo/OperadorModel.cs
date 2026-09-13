using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace ImperioChocoyo;

[Table("operador")]
public class OperadorModel : BaseModel
{
    [PrimaryKey("id_operador", false)]
    public int Id { get; set; }

    [Column("dui")]
    public string Dui { get; set; } = string.Empty;

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Column("telefono")]
    public string? Telefono { get; set; }

    [Column("direccion")]
    public string? Direccion { get; set; }

    [Column("tipo_licencia")]
    public string? TipoLicencia { get; set; }

    [Column("licencia_numero")]
    public string? LicenciaNumero { get; set; }

    [Column("rol")]
    public string Rol { get; set; } = string.Empty;

    [JsonIgnore]
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
}