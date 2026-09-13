namespace ImperioChocoyo;

public partial class HistorialRegistro : ContentPage
{
    private HistorialModel? _historialEdicion = null;
    private bool _datosCargados = false;

    private List<AutobusModel> _autobuses = new();

    public HistorialRegistro()
    {
        InitializeComponent();
        UpdateThemeButtonText();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        CardRegistro.Opacity = 0;
        CardRegistro.TranslationY = 28;
        CardRegistro.FadeToAsync(1, 500, Easing.CubicOut);
        CardRegistro.TranslateToAsync(0, 0, 500, Easing.CubicOut);

        if (!_datosCargados)
        {
            await CargarAutobuses();
            _datosCargados = true;
            if (_historialEdicion != null)
            {
                CargarDatosEnFormulario();
            }
        }
    }

    public HistorialRegistro(HistorialModel registro) : this()
    {
        _historialEdicion = registro;
    }

    private async Task CargarAutobuses()
    {
        try
        {
            await SupabaseService.InitializeAsync();
            var autobuses = await SupabaseService.Client.From<AutobusModel>().Get();
            _autobuses = autobuses.Models;
            CmbAutobus.ItemsSource = _autobuses;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los autobuses: {ex.Message}", "Aceptar");
        }
    }

    private void CargarDatosEnFormulario()
    {
        if (_historialEdicion == null) return;

        CmbAutobus.SelectedItem = _autobuses.FirstOrDefault(a => a.Id == _historialEdicion.IdAutobus);
        CmbTipo.SelectedItem = _historialEdicion.Tipo;
        TxtDescripcion.Text = _historialEdicion.Descripcion;
        TxtCosto.Text = _historialEdicion.Costo?.ToString() ?? string.Empty;
        TxtKilometraje.Text = _historialEdicion.Kilometraje?.ToString() ?? string.Empty;

        if (_historialEdicion.FechaRegistro.HasValue)
        {
            DpFecha.Date = _historialEdicion.FechaRegistro.Value.Date;
        }
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        if (!ValidarCampos())
        {
            await DisplayAlertAsync("Campos incompletos", "Selecciona autobús y tipo de evento", "Aceptar");
            return;
        }

        decimal? costo = null;
        if (!string.IsNullOrWhiteSpace(TxtCosto.Text))
        {
            if (!decimal.TryParse(TxtCosto.Text, out var c))
            {
                await DisplayAlertAsync("Dato inválido", "El costo debe ser un número (ej. 150.00)", "Aceptar");
                return;
            }
            costo = c;
        }

        int? kilometraje = null;
        if (!string.IsNullOrWhiteSpace(TxtKilometraje.Text))
        {
            if (!int.TryParse(TxtKilometraje.Text, out var km))
            {
                await DisplayAlertAsync("Dato inválido", "El kilometraje debe ser un número entero", "Aceptar");
                return;
            }
            kilometraje = km;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            if (_historialEdicion == null)
            {
                var nuevo = new HistorialModel
                {
                    IdAutobus = ((AutobusModel)CmbAutobus.SelectedItem).Id,
                    Tipo = CmbTipo.SelectedItem?.ToString() ?? string.Empty,
                    Descripcion = TxtDescripcion.Text?.Trim(),
                    Costo = costo,
                    Kilometraje = kilometraje,
                    FechaRegistro = DpFecha.Date
                };

                await SupabaseService.Client.From<HistorialModel>().Insert(nuevo);
            }
            else
            {
                _historialEdicion.IdAutobus = ((AutobusModel)CmbAutobus.SelectedItem).Id;
                _historialEdicion.Tipo = CmbTipo.SelectedItem?.ToString() ?? string.Empty;
                _historialEdicion.Descripcion = TxtDescripcion.Text?.Trim();
                _historialEdicion.Costo = costo;
                _historialEdicion.Kilometraje = kilometraje;
                _historialEdicion.FechaRegistro = DpFecha.Date;

                await SupabaseService.Client.From<HistorialModel>().Update(_historialEdicion);
            }

            SuccessModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar el registro: {ex.Message}", "Aceptar");
        }
    }

    private bool ValidarCampos()
    {
        return CmbAutobus.SelectedItem != null && CmbTipo.SelectedIndex != -1;
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