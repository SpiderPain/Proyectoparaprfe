using Postgrest.Attributes;
using Postgrest.Models;

namespace ImperioChocoyo;

[Table("usuario")]
public class UsuarioModel : BaseModel
{
    [PrimaryKey("id_usuario", false)]
    public int IdUsuario { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("rol")]
    public string Rol { get; set; } = string.Empty; // 'secretario' o 'administrador'

    [Column("telefono")]
    public string? Telefono { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;
}