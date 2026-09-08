using System.Collections.ObjectModel;

namespace ImperioChocoyo;

public partial class Dashboard : ContentPage
{
    public bool EsAdmin { get; set; } = false;
    public ObservableCollection<AutobusModel> ListaAutobuses { get; set; } = new();
    private AutobusModel? _autobusAEliminar = null;

    public Dashboard(bool esAdmin = false)
    {
        InitializeComponent();
        EsAdmin = esAdmin;
        CvAutobuses.ItemsSource = ListaAutobuses;
        UpdateThemeButtonText();
        AplicarPermisosPorRol();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DashContent.Opacity = 0;
        DashContent.TranslationY = 24;
        DashContent.FadeToAsync(1, 450, Easing.CubicOut);
        DashContent.TranslateToAsync(0, 0, 450, Easing.CubicOut);
        CargarDatosSupabase();
    }

    private void AplicarPermisosPorRol()
    {
        // Secretario puede capturar datos (crear), por eso el botón siempre visible.
        BtnRegistrarNuevo.IsVisible = true;
    }

    private async void CargarDatosSupabase()
    {
        try
        {
            await SupabaseService.InitializeAsync();
            var respuesta = await SupabaseService.Client.From<AutobusModel>().Get();

            ListaAutobuses.Clear();
            foreach (var autobus in respuesta.Models)
            {
                ListaAutobuses.Add(autobus);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los autobuses: {ex.Message}", "Aceptar");
        }

        CalcularKPIs();
    }

    private void CalcularKPIs()
    {
        LblTotal.Text = ListaAutobuses.Count.ToString();
        LblConteo.Text = ListaAutobuses.Count.ToString();
        LblActivos.Text = ListaAutobuses.Count(a => a.Estado == "Activo").ToString();
        LblMantenimiento.Text = ListaAutobuses.Count(a => a.Estado == "En Mantenimiento").ToString();
        LblInactivos.Text = ListaAutobuses.Count(a => a.Estado == "Inactivo").ToString();
    }

    private async void OnRegistrarNuevoClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new Registro());
    }

    private async void OnOperadoresClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new OperadorLista());
    }

    private async void OnRutasClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RutaLista());
    }

    private async void OnViajesClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ViajeLista());
    }

    private async void OnDocumentosClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new DocumentoLista());
    }

    private async void OnHistorialClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new HistorialLista());
    }

    private async void OnEditarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is AutobusModel autobus)
        {
            await Navigation.PushAsync(new Registro(autobus));
        }
    }

    private void OnEliminarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is AutobusModel autobus)
        {
            _autobusAEliminar = autobus;
            DeleteModal.IsVisible = true;
        }
    }

    private async void OnConfirmarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        if (_autobusAEliminar != null)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                await SupabaseService.Client
                    .From<AutobusModel>()
                    .Where(x => x.Id == _autobusAEliminar.Id)
                    .Delete();

                ListaAutobuses.Remove(_autobusAEliminar);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar el autobús: {ex.Message}", "Aceptar");
            }

            _autobusAEliminar = null;
            CalcularKPIs();
        }
    }

    private void OnCancelarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        _autobusAEliminar = null;
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
