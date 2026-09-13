using ImperioChocoyo;

namespace ImperioChocoyo;

public partial class Login : ContentPage
{
    public Login()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CardLogin.Opacity = 0;
        CardLogin.TranslationY = 28;
        CardLogin.FadeToAsync(1, 500, Easing.CubicOut);
        CardLogin.TranslateToAsync(0, 0, 500, Easing.CubicOut);
    }

    private async void OnIniciarSesionClicked(object? sender, EventArgs e)
    {
        string usuarioInput = TxtUsuario.Text?.Trim() ?? string.Empty;
        string passwordInput = TxtPassword.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(usuarioInput) || string.IsNullOrEmpty(passwordInput))
        {
            ErrorModalTitle.Text = "Campos vacíos";
            ErrorModalMessage.Text = "Debes ingresar tu correo electrónico y contraseña para iniciar sesión.";
            ErrorModal.IsVisible = true;
            return;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            // Obtener todos los usuarios activos y filtrar en memoria para evitar errores de sintaxis en PostgREST
            var respuesta = await SupabaseService.ReintentarAsync(() =>
                SupabaseService.Client
                    .From<UsuarioModel>()
                    .Where(x => x.Activo == true)
                    .Get());

            var usuarioEncontrado = respuesta.Models.FirstOrDefault(u =>
                (u.Nombre.Equals(usuarioInput, StringComparison.OrdinalIgnoreCase) ||
                 u.Email.Equals(usuarioInput, StringComparison.OrdinalIgnoreCase)) &&
                u.Password == passwordInput
            );

            if (usuarioEncontrado != null)
            {
                // Evaluar el rol exactamente como aparece en tu tabla ('administrador')
                bool esAdmin = string.Equals(usuarioEncontrado.Rol, "administrador", StringComparison.OrdinalIgnoreCase);
                Sesion.EsAdmin = esAdmin;

                this.Window.Page = new NavigationPage(new Dashboard(esAdmin));
            }
            else
            {
                ErrorModalTitle.Text = "Credenciales incorrectas";
                ErrorModalMessage.Text = "El correo o la contraseña que ingresaste no coinciden con ningún usuario activo en el sistema.";
                ErrorModal.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");
        }
    }

    private async void OnRegistrarClicked(object? sender, EventArgs e)
    {
        // Gate: solo administradores pueden registrar nuevos usuarios
        AdminErrorLabel.IsVisible = false;
        TxtAdminUsuario.Text = string.Empty;
        TxtAdminPassword.Text = string.Empty;
        AdminModal.IsVisible = true;
    }

    private void OnCancelarVerificacionClicked(object? sender, EventArgs e)
    {
        AdminModal.IsVisible = false;
    }

    private async void OnVerificarAdminClicked(object? sender, EventArgs e)
    {
        string adminUsuario = TxtAdminUsuario.Text?.Trim() ?? string.Empty;
        string adminPassword = TxtAdminPassword.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(adminUsuario) || string.IsNullOrEmpty(adminPassword))
        {
            AdminErrorLabel.Text = "Ingresa tu correo y contraseña de administrador.";
            AdminErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            var respuesta = await SupabaseService.ReintentarAsync(() =>
                SupabaseService.Client
                    .From<UsuarioModel>()
                    .Where(x => x.Activo == true)
                    .Get());

            var adminEncontrado = respuesta.Models.FirstOrDefault(u =>
                string.Equals(u.Rol, "administrador", StringComparison.OrdinalIgnoreCase) &&
                (u.Nombre.Equals(adminUsuario, StringComparison.OrdinalIgnoreCase) ||
                 u.Email.Equals(adminUsuario, StringComparison.OrdinalIgnoreCase)) &&
                u.Password == adminPassword
            );

            if (adminEncontrado != null)
            {
                AdminModal.IsVisible = false;
                await Navigation.PushAsync(new CrearUsuario());
            }
            else
            {
                AdminErrorLabel.Text = "No tienes permisos de administrador para registrar usuarios.";
                AdminErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            AdminErrorLabel.Text = $"Error de conexión: {ex.Message}";
            AdminErrorLabel.IsVisible = true;
        }
    }

    private void OnCerrarModalErrorClicked(object? sender, EventArgs e)
    {
        ErrorModal.IsVisible = false;
    }

    private void OnSalirClicked(object? sender, EventArgs e)
    {
        LogoutModal.IsVisible = true;
    }

    private void OnNoSalirClicked(object? sender, EventArgs e)
    {
        LogoutModal.IsVisible = false;
    }

    private void OnSiSalirClicked(object? sender, EventArgs e)
    {
        Application.Current?.CloseWindow(this.Window);
    }
}