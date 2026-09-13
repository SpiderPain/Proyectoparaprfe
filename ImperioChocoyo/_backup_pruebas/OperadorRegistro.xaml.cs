namespace ImperioChocoyo;

public partial class OperadorRegistro : ContentPage
{
    private OperadorModel? _operadorEdicion = null;

    public OperadorRegistro()
    {
        InitializeComponent();
        UpdateThemeButtonText();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CardRegistro.Opacity = 0;
        CardRegistro.TranslationY = 28;
        CardRegistro.FadeToAsync(1, 500, Easing.CubicOut);
        CardRegistro.TranslateToAsync(0, 0, 500, Easing.CubicOut);
    }

    public OperadorRegistro(OperadorModel operador) : this()
    {
        _operadorEdicion = operador;
        CargarDatosEnFormulario();
    }

    private void CargarDatosEnFormulario()
    {
        if (_operadorEdicion == null) return;

        TxtDui.Text = _operadorEdicion.Dui;
        TxtNombre.Text = _operadorEdicion.Nombre;
        TxtApellido.Text = _operadorEdicion.Apellido;
        TxtTelefono.Text = _operadorEdicion.Telefono;
        TxtTipoLicencia.Text = _operadorEdicion.TipoLicencia;
        TxtLicenciaNumero.Text = _operadorEdicion.LicenciaNumero;
        TxtDireccion.Text = _operadorEdicion.Direccion;
        TxtRol.Text = _operadorEdicion.Rol;
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        if (!ValidarCampos())
        {
            await DisplayAlertAsync("Campos incompletos", "Por favor completa los campos obligatorios (DUI, Nombre, Apellido y Rol)", "Aceptar");
            return;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            if (_operadorEdicion == null)
            {
                var nuevo = new OperadorModel
                {
                    Dui = TxtDui.Text?.Trim() ?? string.Empty,
                    Nombre = TxtNombre.Text?.Trim() ?? string.Empty,
                    Apellido = TxtApellido.Text?.Trim() ?? string.Empty,
                    Telefono = TxtTelefono.Text?.Trim(),
                    TipoLicencia = TxtTipoLicencia.Text?.Trim(),
                    LicenciaNumero = TxtLicenciaNumero.Text?.Trim(),
                    Direccion = TxtDireccion.Text?.Trim(),
                    Rol = TxtRol.Text?.Trim() ?? string.Empty
                };

                await SupabaseService.Client.From<OperadorModel>().Insert(nuevo);
            }
            else
            {
                _operadorEdicion.Dui = TxtDui.Text?.Trim() ?? string.Empty;
                _operadorEdicion.Nombre = TxtNombre.Text?.Trim() ?? string.Empty;
                _operadorEdicion.Apellido = TxtApellido.Text?.Trim() ?? string.Empty;
                _operadorEdicion.Telefono = TxtTelefono.Text?.Trim();
                _operadorEdicion.TipoLicencia = TxtTipoLicencia.Text?.Trim();
                _operadorEdicion.LicenciaNumero = TxtLicenciaNumero.Text?.Trim();
                _operadorEdicion.Direccion = TxtDireccion.Text?.Trim();
                _operadorEdicion.Rol = TxtRol.Text?.Trim() ?? string.Empty;

                await SupabaseService.Client.From<OperadorModel>().Update(_operadorEdicion);
            }

            SuccessModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar el operador: {ex.Message}", "Aceptar");
        }
    }

    private bool ValidarCampos()
    {
        return !string.IsNullOrWhiteSpace(TxtDui.Text) &&
               !string.IsNullOrWhiteSpace(TxtNombre.Text) &&
               !string.IsNullOrWhiteSpace(TxtApellido.Text) &&
               !string.IsNullOrWhiteSpace(TxtRol.Text);
    }

    private async void OnAceptarModalClicked(object? sender, EventArgs e)
    {
        SuccessModal.IsVisible = false;
        await Navigation.PopAsync();
    }

    private async void OnSiCancelarClicked(object? sender, EventArgs e)
    {
        CancelModal.IsVisible = false;
        await Navigation.PopAsync();
    }

    private void OnNoContinuarClicked(object? sender, EventArgs e) => CancelModal.IsVisible = false;
    private void OnCancelarClicked(object? sender, EventArgs e) => CancelModal.IsVisible = true;

    private void OnThemeToggleClicked(object? sender, EventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = Application.Current.UserAppTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
            UpdateThemeButtonText();
        }
    }

    private void UpdateThemeButtonText()
    {
        if (Application.Current != null)
        {
            ThemeToggleButton.Text = Application.Current.UserAppTheme == AppTheme.Dark ? "☀️" : "🌙";
        }
    }

    private void OnSalirClicked(object? sender, EventArgs e) => LogoutModal.IsVisible = true;
    private void OnNoSalirClicked(object? sender, EventArgs e) => LogoutModal.IsVisible = false;
    private void OnSiSalirClicked(object? sender, EventArgs e)
    {
        Application.Current?.CloseWindow(this.Window);
    }
}