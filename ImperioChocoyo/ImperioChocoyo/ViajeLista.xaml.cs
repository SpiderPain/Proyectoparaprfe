using System.Collections.ObjectModel;

namespace ImperioChocoyo;

public partial class ViajeLista : ContentPage
{
    public ObservableCollection<RegistroViajeModel> ListaViajes { get; set; } = new();
    private RegistroViajeModel? _viajeAEliminar = null;

    private Dictionary<int, string> _autobuses = new();
    private Dictionary<int, string> _operadores = new();
    private Dictionary<int, string> _rutas = new();

    public ViajeLista()
    {
        InitializeComponent();
        CvViajes.ItemsSource = ListaViajes;
        UpdateThemeButtonText();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Contenido.Opacity = 0;
        Contenido.TranslationY = 24;
        Contenido.FadeToAsync(1, 450, Easing.CubicOut);
        Contenido.TranslateToAsync(0, 0, 450, Easing.CubicOut);
        CargarDatosSupabase();
    }

    private async void CargarDatosSupabase()
    {
        try
        {
            await SupabaseService.InitializeAsync();

            var respuesta = await SupabaseService.Client.From<RegistroViajeModel>().Get();
            var autobuses = await SupabaseService.Client.From<AutobusModel>().Get();
            var operadores = await SupabaseService.Client.From<OperadorModel>().Get();
            var rutas = await SupabaseService.Client.From<RutaModel>().Get();

            _autobuses = autobuses.Models.ToDictionary(a => a.Id, a => a.Placa);
            _operadores = operadores.Models.ToDictionary(o => o.Id, o => o.NombreCompleto);
            _rutas = rutas.Models.ToDictionary(r => r.Id, r => r.Nombre);

            ListaViajes.Clear();
            foreach (var viaje in respuesta.Models)
            {
                viaje.AutobusLabel = _autobuses.TryGetValue(viaje.IdAutobus, out var placa) ? placa : $"#{viaje.IdAutobus}";
                viaje.OperadorLabel = _operadores.TryGetValue(viaje.IdOperador, out var op) ? op : $"#{viaje.IdOperador}";
                viaje.RutaLabel = _rutas.TryGetValue(viaje.IdRuta, out var ruta) ? ruta : $"#{viaje.IdRuta}";
                ListaViajes.Add(viaje);
            }
        }
        catch (Exception ex)
        {
            LblError.Text = $"No se pudieron cargar los viajes: {ex.Message}";
            LblError.IsVisible = true;
            await DisplayAlertAsync("Error", LblError.Text, "Aceptar");
        }

        CalcularConteo();
    }

    private void CalcularConteo()
    {
        LblConteo.Text = ListaViajes.Count.ToString();
    }

    private async void OnNuevoClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ViajeRegistro());
    }

    private async void OnEditarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is RegistroViajeModel viaje)
        {
            await Navigation.PushAsync(new ViajeRegistro(viaje));
        }
    }

    private void OnEliminarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is RegistroViajeModel viaje)
        {
            _viajeAEliminar = viaje;
            DeleteModal.IsVisible = true;
        }
    }

    private async void OnConfirmarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        if (_viajeAEliminar != null)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                await SupabaseService.Client
                    .From<RegistroViajeModel>()
                    .Where(x => x.Id == _viajeAEliminar.Id)
                    .Delete();

                ListaViajes.Remove(_viajeAEliminar);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar el viaje: {ex.Message}", "Aceptar");
            }

            _viajeAEliminar = null;
            CalcularConteo();
        }
    }

    private void OnCancelarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        _viajeAEliminar = null;
    }

    private async void OnVolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

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
            ThemeToggleButton.Text = Application.Current.UserAppTheme == AppTheme.Dark ? "Claro" : "Oscuro";
        }
    }

    private void OnSalirClicked(object? sender, EventArgs e)
    {
        Application.Current?.CloseWindow(this.Window);
    }

    private void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        this.Window.Page = new NavigationPage(new Login());
    }
}