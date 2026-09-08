namespace ImperioChocoyo;

public partial class ViajeRegistro : ContentPage
{
    private RegistroViajeModel? _viajeEdicion = null;
    private bool _datosCargados = false;

    private List<AutobusModel> _autobuses = new();
    private List<OperadorModel> _operadores = new();
    private List<RutaModel> _rutas = new();

    public ViajeRegistro()
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
            await CargarReferencias();
            _datosCargados = true;
            if (_viajeEdicion != null)
            {
                CargarDatosEnFormulario();
            }
        }
    }

    public ViajeRegistro(RegistroViajeModel viaje) : this()
    {
        _viajeEdicion = viaje;
    }

    private async Task CargarReferencias()
    {
        try
        {
            await SupabaseService.InitializeAsync();

            var autobuses = await SupabaseService.Client.From<AutobusModel>().Get();
            var operadores = await SupabaseService.Client.From<OperadorModel>().Get();
            var rutas = await SupabaseService.Client.From<RutaModel>().Get();

            _autobuses = autobuses.Models;
            _operadores = operadores.Models;
            _rutas = rutas.Models;

            CmbAutobus.ItemsSource = _autobuses;
            CmbOperador.ItemsSource = _operadores;
            CmbRuta.ItemsSource = _rutas;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los datos: {ex.Message}", "Aceptar");
        }
    }

    private void CargarDatosEnFormulario()
    {
        if (_viajeEdicion == null) return;

        CmbAutobus.SelectedItem = _autobuses.FirstOrDefault(a => a.Id == _viajeEdicion.IdAutobus);
        CmbOperador.SelectedItem = _operadores.FirstOrDefault(o => o.Id == _viajeEdicion.IdOperador);
        CmbRuta.SelectedItem = _rutas.FirstOrDefault(r => r.Id == _viajeEdicion.IdRuta);

        DpSalida.Date = _viajeEdicion.FechaHoraSalida.Date;
        TpSalida.Time = _viajeEdicion.FechaHoraSalida.TimeOfDay;

        if (_viajeEdicion.FechaHoraLlegada.HasValue)
        {
            DpLlegada.Date = _viajeEdicion.FechaHoraLlegada.Value.Date;
            TpLlegada.Time = _viajeEdicion.FechaHoraLlegada.Value.TimeOfDay;
        }

        TxtObservaciones.Text = _viajeEdicion.Observaciones;
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        if (!ValidarCampos())
        {
            await DisplayAlertAsync("Campos incompletos", "Selecciona autobús, operador, ruta y la fecha/hora de salida", "Aceptar");
            return;
        }

        var salida = (DpSalida.Date ?? DateTime.Today).Date + (TpSalida.Time ?? TimeSpan.Zero);
        var llegada = (DpLlegada.Date ?? DateTime.Today).Date + (TpLlegada.Time ?? TimeSpan.Zero);

        try
        {
            await SupabaseService.InitializeAsync();

            if (_viajeEdicion == null)
            {
                var nuevo = new RegistroViajeModel
                {
                    IdAutobus = ((AutobusModel)CmbAutobus.SelectedItem).Id,
                    IdOperador = ((OperadorModel)CmbOperador.SelectedItem).Id,
                    IdRuta = ((RutaModel)CmbRuta.SelectedItem).Id,
                    FechaHoraSalida = salida,
                    FechaHoraLlegada = llegada,
                    Observaciones = TxtObservaciones.Text?.Trim()
                };

                await SupabaseService.Client.From<RegistroViajeModel>().Insert(nuevo);
            }
            else
            {
                _viajeEdicion.IdAutobus = ((AutobusModel)CmbAutobus.SelectedItem).Id;
                _viajeEdicion.IdOperador = ((OperadorModel)CmbOperador.SelectedItem).Id;
                _viajeEdicion.IdRuta = ((RutaModel)CmbRuta.SelectedItem).Id;
                _viajeEdicion.FechaHoraSalida = salida;
                _viajeEdicion.FechaHoraLlegada = llegada;
                _viajeEdicion.Observaciones = TxtObservaciones.Text?.Trim();

                await SupabaseService.Client.From<RegistroViajeModel>().Update(_viajeEdicion);
            }

            SuccessModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar el viaje: {ex.Message}", "Aceptar");
        }
    }

    private bool ValidarCampos()
    {
        return CmbAutobus.SelectedItem != null &&
               CmbOperador.SelectedItem != null &&
               CmbRuta.SelectedItem != null &&
               DpSalida.Date.HasValue;
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