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
            ErrorModal.IsVisible = true;
            return;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            // Obtener todos los usuarios activos y filtrar en memoria para evitar errores de sintaxis en PostgREST
            var respuesta = await SupabaseService.Client
                .From<UsuarioModel>()
                .Where(x => x.Activo == true)
                .Get();

            var usuarioEncontrado = respuesta.Models.FirstOrDefault(u =>
                (u.Nombre.Equals(usuarioInput, StringComparison.OrdinalIgnoreCase) ||
                 u.Email.Equals(usuarioInput, StringComparison.OrdinalIgnoreCase)) &&
                u.Password == passwordInput
            );

            if (usuarioEncontrado != null)
            {
                // Evaluar el rol exactamente como aparece en tu tabla ('administrador')
                bool esAdmin = string.Equals(usuarioEncontrado.Rol, "administrador", StringComparison.OrdinalIgnoreCase);

                this.Window.Page = new NavigationPage(new Dashboard(esAdmin));
            }
            else
            {
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
        // Navega a la ventana de registro de nuevo usuario
        await Navigation.PushAsync(new CrearUsuario());
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