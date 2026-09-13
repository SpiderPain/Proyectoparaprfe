using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace ImperioChocoyo;

[Table("ruta")]
public class RutaModel : BaseModel
{
    [PrimaryKey("id_ruta", false)]
    public int Id { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("origen")]
    public string Origen { get; set; } = string.Empty;

    [Column("destino")]
    public string Destino { get; set; } = string.Empty;

    [Column("distancia")]
    public decimal? Distancia { get; set; }

    [JsonIgnore]
    public string RutaLabel => $"{Nombre} ({Origen} → {Destino})";
}