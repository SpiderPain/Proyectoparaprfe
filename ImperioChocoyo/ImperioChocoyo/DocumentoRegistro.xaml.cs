namespace ImperioChocoyo;

public partial class DocumentoRegistro : ContentPage
{
    private DocumentoModel? _documentoEdicion = null;
    private bool _datosCargados = false;

    private List<AutobusModel> _autobuses = new();

    public DocumentoRegistro()
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
            if (_documentoEdicion != null)
            {
                CargarDatosEnFormulario();
            }
        }
    }

    public DocumentoRegistro(DocumentoModel documento) : this()
    {
        _documentoEdicion = documento;
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
        if (_documentoEdicion == null) return;

        CmbAutobus.SelectedItem = _autobuses.FirstOrDefault(a => a.Id == _documentoEdicion.IdAutobus);
        CmbTipo.SelectedItem = _documentoEdicion.Tipo;
        TxtNumero.Text = _documentoEdicion.Numero;

        if (_documentoEdicion.FechaEmision.HasValue)
        {
            DpEmision.Date = _documentoEdicion.FechaEmision.Value.Date;
        }
        if (_documentoEdicion.FechaVencimiento.HasValue)
        {
            DpVencimiento.Date = _documentoEdicion.FechaVencimiento.Value.Date;
        }
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        if (!ValidarCampos())
        {
            await DisplayAlertAsync("Campos incompletos", "Selecciona autobús y tipo de documento, y escribe el número", "Aceptar");
            return;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            if (_documentoEdicion == null)
            {
                var nuevo = new DocumentoModel
                {
                    IdAutobus = ((AutobusModel)CmbAutobus.SelectedItem).Id,
                    Tipo = CmbTipo.SelectedItem?.ToString() ?? string.Empty,
                    Numero = TxtNumero.Text?.Trim() ?? string.Empty,
                    FechaEmision = DpEmision.Date,
                    FechaVencimiento = DpVencimiento.Date
                };

                await SupabaseService.Client.From<DocumentoModel>().Insert(nuevo);
            }
            else
            {
                _documentoEdicion.IdAutobus = ((AutobusModel)CmbAutobus.SelectedItem).Id;
                _documentoEdicion.Tipo = CmbTipo.SelectedItem?.ToString() ?? string.Empty;
                _documentoEdicion.Numero = TxtNumero.Text?.Trim() ?? string.Empty;
                _documentoEdicion.FechaEmision = DpEmision.Date;
                _documentoEdicion.FechaVencimiento = DpVencimiento.Date;

                await SupabaseService.Client.From<DocumentoModel>().Update(_documentoEdicion);
            }

            SuccessModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar el documento: {ex.Message}", "Aceptar");
        }
    }

    private bool ValidarCampos()
    {
        return CmbAutobus.SelectedItem != null &&
               CmbTipo.SelectedIndex != -1 &&
               !string.IsNullOrWhiteSpace(TxtNumero.Text);
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