using ImperioChocoyo;

namespace ImperioChocoyo;

public partial class CrearUsuario : ContentPage
{
    public CrearUsuario()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CardRegistro.Opacity = 0;
        CardRegistro.TranslationY = 28;
        CardRegistro.FadeToAsync(1, 500, Easing.CubicOut);
        CardRegistro.TranslateToAsync(0, 0, 500, Easing.CubicOut);
    }

    private async void OnRegistrarUsuarioClicked(object? sender, EventArgs e)
    {
        string nombreInput = TxtNuevoUsuario.Text?.Trim() ?? string.Empty;
        string passwordInput = TxtNuevaPassword.Text?.Trim() ?? string.Empty;
        string rolSeleccionado = CmbRol.SelectedItem?.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(nombreInput) || string.IsNullOrEmpty(passwordInput) || string.IsNullOrEmpty(rolSeleccionado))
        {
            await DisplayAlertAsync("Atención", "Por favor completa todos los campos.", "Aceptar");
            return;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            string rolGuardar = rolSeleccionado.Contains("Administrador") ? "administrador" : "secretario";

            var nuevoUsuario = new UsuarioModel
            {
                Nombre = nombreInput,
                Apellido = "Registrado",
                Email = $"{nombreInput.ToLower().Replace(" ", "")}@imperiochocoyo.com",
                Password = passwordInput,
                Rol = rolGuardar,
                Telefono = "7000-0000",
                Activo = true
            };

            await SupabaseService.Client.From<UsuarioModel>().Insert(nuevoUsuario);

            SuccessModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo crear: {ex.Message}", "Aceptar");
        }
    }

    private async void OnCerrarSuccessModalClicked(object? sender, EventArgs e)
    {
        SuccessModal.IsVisible = false;
        await Navigation.PopAsync();
    }

    private async void OnSalirClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}