using System.Collections.ObjectModel;

namespace ImperioChocoyo;

public partial class HistorialLista : ContentPage
{
    public ObservableCollection<HistorialModel> ListaHistorial { get; set; } = new();
    private HistorialModel? _historialAEliminar = null;

    public HistorialLista()
    {
        InitializeComponent();
        CvHistorial.ItemsSource = ListaHistorial;
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

            var respuesta = await SupabaseService.Client.From<HistorialModel>().Get();
            var autobuses = await SupabaseService.Client.From<AutobusModel>().Get();

            var dictAutobuses = autobuses.Models.ToDictionary(a => a.Id, a => a.Placa);

            ListaHistorial.Clear();
            foreach (var registro in respuesta.Models)
            {
                registro.AutobusLabel = dictAutobuses.TryGetValue(registro.IdAutobus, out var placa) ? placa : $"#{registro.IdAutobus}";
                ListaHistorial.Add(registro);
            }
        }
        catch (Exception ex)
        {
            LblError.Text = $"No se pudo cargar el historial: {ex.Message}";
            LblError.IsVisible = true;
            await DisplayAlertAsync("Error", LblError.Text, "Aceptar");
        }

        CalcularConteo();
    }

    private void CalcularConteo()
    {
        LblConteo.Text = ListaHistorial.Count.ToString();
    }

    private async void OnNuevoClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new HistorialRegistro());
    }

    private async void OnEditarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is HistorialModel registro)
        {
            await Navigation.PushAsync(new HistorialRegistro(registro));
        }
    }

    private void OnEliminarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is HistorialModel registro)
        {
            _historialAEliminar = registro;
            DeleteModal.IsVisible = true;
        }
    }

    private async void OnConfirmarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        if (_historialAEliminar != null)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                await SupabaseService.Client
                    .From<HistorialModel>()
                    .Where(x => x.Id == _historialAEliminar.Id)
                    .Delete();

                ListaHistorial.Remove(_historialAEliminar);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar el registro: {ex.Message}", "Aceptar");
            }

            _historialAEliminar = null;
            CalcularConteo();
        }
    }

    private void OnCancelarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        _historialAEliminar = null;
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