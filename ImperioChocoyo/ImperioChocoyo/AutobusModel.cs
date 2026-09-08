using Postgrest.Attributes;
using Postgrest.Models;

namespace ImperioChocoyo;

[Table("autobus")]
public class AutobusModel : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("placa")]
    public string Placa { get; set; } = string.Empty;

    [Column("pasajeros")]
    public int Pasajeros { get; set; }

    [Column("vin")]
    public string Vin { get; set; } = string.Empty;

    [Column("dui_propietario")]
    public string DuiPropietario { get; set; } = string.Empty;

    [Column("kilometraje")]
    public int Kilometraje { get; set; }

    [Column("motor")]
    public string Motor { get; set; } = string.Empty;

    [Column("modelo")]
    public string Modelo { get; set; } = string.Empty;

    [Column("ruta")]
    public string Ruta { get; set; } = string.Empty;

    [Column("combustible")]
    public string Combustible { get; set; } = string.Empty;

    [Column("ano")]
    public string Ano { get; set; } = string.Empty;

    [Column("color")]
    public string Color { get; set; } = string.Empty;

    [Column("categoria")]
    public string Categoria { get; set; } = string.Empty;

    [Column("marca")]
    public string Marca { get; set; } = string.Empty;

    [Column("estado")]
    public string Estado { get; set; } = string.Empty;
}