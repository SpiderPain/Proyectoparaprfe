namespace ImperioChocoyo;

public partial class RutaRegistro : ContentPage
{
    private RutaModel? _rutaEdicion = null;

    public RutaRegistro()
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

    public RutaRegistro(RutaModel ruta) : this()
    {
        _rutaEdicion = ruta;
        CargarDatosEnFormulario();
    }

    private void CargarDatosEnFormulario()
    {
        if (_rutaEdicion == null) return;

        TxtNombre.Text = _rutaEdicion.Nombre;
        TxtOrigen.Text = _rutaEdicion.Origen;
        TxtDestino.Text = _rutaEdicion.Destino;
        TxtDistancia.Text = _rutaEdicion.Distancia?.ToString() ?? string.Empty;
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        if (!ValidarCampos())
        {
            await DisplayAlertAsync("Campos incompletos", "Por favor completa Nombre, Origen y Destino", "Aceptar");
            return;
        }

        decimal? distancia = null;
        if (!string.IsNullOrWhiteSpace(TxtDistancia.Text))
        {
            if (!decimal.TryParse(TxtDistancia.Text, out var d))
            {
                await DisplayAlertAsync("Dato inválido", "La distancia debe ser un número (ej. 75.5)", "Aceptar");
                return;
            }
            distancia = d;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            if (_rutaEdicion == null)
            {
                var nuevo = new RutaModel
                {
                    Nombre = TxtNombre.Text?.Trim() ?? string.Empty,
                    Origen = TxtOrigen.Text?.Trim() ?? string.Empty,
                    Destino = TxtDestino.Text?.Trim() ?? string.Empty,
                    Distancia = distancia
                };

                await SupabaseService.Client.From<RutaModel>().Insert(nuevo);
            }
            else
            {
                _rutaEdicion.Nombre = TxtNombre.Text?.Trim() ?? string.Empty;
                _rutaEdicion.Origen = TxtOrigen.Text?.Trim() ?? string.Empty;
                _rutaEdicion.Destino = TxtDestino.Text?.Trim() ?? string.Empty;
                _rutaEdicion.Distancia = distancia;

                await SupabaseService.Client.From<RutaModel>().Update(_rutaEdicion);
            }

            SuccessModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar la ruta: {ex.Message}", "Aceptar");
        }
    }

    private bool ValidarCampos()
    {
        return !string.IsNullOrWhiteSpace(TxtNombre.Text) &&
               !string.IsNullOrWhiteSpace(TxtOrigen.Text) &&
               !string.IsNullOrWhiteSpace(TxtDestino.Text);
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