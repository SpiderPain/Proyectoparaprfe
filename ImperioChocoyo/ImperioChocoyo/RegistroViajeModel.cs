using Postgrest.Attributes;
using Postgrest.Models;

namespace ImperioChocoyo;

[Table("registro_viajes")]
public class RegistroViajeModel : BaseModel
{
    [PrimaryKey("id_viaje", false)]
    public int Id { get; set; }

    [Column("id_autobus")]
    public int IdAutobus { get; set; }

    [Column("id_operador")]
    public int IdOperador { get; set; }

    [Column("id_ruta")]
    public int IdRuta { get; set; }

    [Column("fecha_hora_salida")]
    public DateTime FechaHoraSalida { get; set; }

    [Column("fecha_hora_llegada")]
    public DateTime? FechaHoraLlegada { get; set; }

    [Column("observaciones")]
    public string? Observaciones { get; set; }

    public string AutobusLabel { get; set; } = string.Empty;
    public string OperadorLabel { get; set; } = string.Empty;
    public string RutaLabel { get; set; } = string.Empty;
}