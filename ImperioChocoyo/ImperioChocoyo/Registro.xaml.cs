namespace ImperioChocoyo;

public partial class Registro : ContentPage
{
    private AutobusModel? _autobusEdicion = null;

    // Constructor 1: Para Crear Nuevo Autobús
    public Registro()
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

    // Constructor 2: Para Editar Autobús Existente
    public Registro(AutobusModel autobus) : this()
    {
        _autobusEdicion = autobus;
        CargarDatosEnFormulario();
    }

    private void CargarDatosEnFormulario()
    {
        if (_autobusEdicion == null) return;

        TxtPlaca.Text = _autobusEdicion.Placa;
        TxtPasajeros.Text = _autobusEdicion.Pasajeros.ToString();
        TxtVin.Text = _autobusEdicion.Vin;
        TxtDui.Text = _autobusEdicion.DuiPropietario;
        TxtKilometraje.Text = _autobusEdicion.Kilometraje.ToString();
        TxtMotor.Text = _autobusEdicion.Motor;
        TxtModelo.Text = _autobusEdicion.Modelo;
        TxtRuta.Text = _autobusEdicion.Ruta;

        CmbCombustible.SelectedItem = _autobusEdicion.Combustible;
        CmbAno.SelectedItem = _autobusEdicion.Ano;
        CmbColor.SelectedItem = _autobusEdicion.Color;
        CmbCategoria.SelectedItem = _autobusEdicion.Categoria;
        CmbMarca.SelectedItem = _autobusEdicion.Marca;
        CmbEstado.SelectedItem = _autobusEdicion.Estado;
    }

    private async void OnRegistrarAutobusClicked(object? sender, EventArgs e)
    {
        if (!ValidarCampos())
        {
            await DisplayAlertAsync("Campos incompletos", "Por favor completa todos los campos requeridos", "Aceptar");
            return;
        }

        if (!int.TryParse(TxtPasajeros.Text, out int pasajeros) ||
            !int.TryParse(TxtKilometraje.Text, out int kilometraje))
        {
            await DisplayAlertAsync("Datos inválidos", "La capacidad de pasajeros y el kilometraje deben ser números.", "Aceptar");
            return;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            if (_autobusEdicion == null)
            {
                var nuevoAutobus = new AutobusModel
                {
                    Placa = TxtPlaca.Text?.Trim() ?? string.Empty,
                    Pasajeros = pasajeros,
                    Vin = TxtVin.Text?.Trim() ?? string.Empty,
                    DuiPropietario = TxtDui.Text?.Trim() ?? string.Empty,
                    Kilometraje = kilometraje,
                    Motor = TxtMotor.Text?.Trim() ?? string.Empty,
                    Modelo = TxtModelo.Text?.Trim() ?? string.Empty,
                    Ruta = TxtRuta.Text?.Trim() ?? string.Empty,
                    Combustible = CmbCombustible.SelectedItem?.ToString() ?? string.Empty,
                    Ano = CmbAno.SelectedItem?.ToString() ?? string.Empty,
                    Color = CmbColor.SelectedItem?.ToString() ?? string.Empty,
                    Categoria = CmbCategoria.SelectedItem?.ToString() ?? string.Empty,
                    Marca = CmbMarca.SelectedItem?.ToString() ?? string.Empty,
                    Estado = CmbEstado.SelectedItem?.ToString() ?? string.Empty
                };

                await SupabaseService.Client.From<AutobusModel>().Insert(nuevoAutobus);
            }
            else
            {
                _autobusEdicion.Placa = TxtPlaca.Text?.Trim() ?? string.Empty;
                _autobusEdicion.Pasajeros = pasajeros;
                _autobusEdicion.Vin = TxtVin.Text?.Trim() ?? string.Empty;
                _autobusEdicion.DuiPropietario = TxtDui.Text?.Trim() ?? string.Empty;
                _autobusEdicion.Kilometraje = kilometraje;
                _autobusEdicion.Motor = TxtMotor.Text?.Trim() ?? string.Empty;
                _autobusEdicion.Modelo = TxtModelo.Text?.Trim() ?? string.Empty;
                _autobusEdicion.Ruta = TxtRuta.Text?.Trim() ?? string.Empty;
                _autobusEdicion.Combustible = CmbCombustible.SelectedItem?.ToString() ?? string.Empty;
                _autobusEdicion.Ano = CmbAno.SelectedItem?.ToString() ?? string.Empty;
                _autobusEdicion.Color = CmbColor.SelectedItem?.ToString() ?? string.Empty;
                _autobusEdicion.Categoria = CmbCategoria.SelectedItem?.ToString() ?? string.Empty;
                _autobusEdicion.Marca = CmbMarca.SelectedItem?.ToString() ?? string.Empty;
                _autobusEdicion.Estado = CmbEstado.SelectedItem?.ToString() ?? string.Empty;

                await SupabaseService.Client.From<AutobusModel>().Update(_autobusEdicion);
            }

            SuccessModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar el autobús: {ex.Message}", "Aceptar");
        }
    }

    private async void OnAceptarModalClicked(object? sender, EventArgs e)
    {
        SuccessModal.IsVisible = false;
        await Navigation.PopAsync(); // Regresa al Dashboard
    }

    private async void OnSiCancelarClicked(object? sender, EventArgs e)
    {
        CancelModal.IsVisible = false;
        await Navigation.PopAsync(); // Regresa al Dashboard
    }

    private bool ValidarCampos()
    {
        return !string.IsNullOrWhiteSpace(TxtPlaca.Text) &&
               !string.IsNullOrWhiteSpace(TxtPasajeros.Text) &&
               !string.IsNullOrWhiteSpace(TxtVin.Text) &&
               !string.IsNullOrWhiteSpace(TxtDui.Text) &&
               !string.IsNullOrWhiteSpace(TxtKilometraje.Text) &&
               !string.IsNullOrWhiteSpace(TxtMotor.Text) &&
               !string.IsNullOrWhiteSpace(TxtModelo.Text) &&
               !string.IsNullOrWhiteSpace(TxtRuta.Text) &&
               CmbCombustible.SelectedIndex != -1 &&
               CmbAno.SelectedIndex != -1 &&
               CmbColor.SelectedIndex != -1 &&
               CmbCategoria.SelectedIndex != -1 &&
               CmbMarca.SelectedIndex != -1 &&
               CmbEstado.SelectedIndex != -1;
    }

    private void OnCancelarClicked(object? sender, EventArgs e) => CancelModal.IsVisible = true;
    private void OnNoContinuarClicked(object? sender, EventArgs e) => CancelModal.IsVisible = false;

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
            ThemeToggleButton.Text = Application.Current.UserAppTheme == AppTheme.Dark ? "☀️ Modo Claro" : "🌙 Modo Oscuro";
        }
    }

    private void OnSalirClicked(object? sender, EventArgs e) => LogoutModal.IsVisible = true;
    private void OnNoSalirClicked(object? sender, EventArgs e) => LogoutModal.IsVisible = false;
    private void OnSiSalirClicked(object? sender, EventArgs e)
    {
        Application.Current?.CloseWindow(this.Window);
    }
}