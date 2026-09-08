using System.Collections.ObjectModel;

namespace ImperioChocoyo;

public partial class RutaLista : ContentPage
{
    public ObservableCollection<RutaModel> ListaRutas { get; set; } = new();
    private RutaModel? _rutaAEliminar = null;

    public RutaLista()
    {
        InitializeComponent();
        CvRutas.ItemsSource = ListaRutas;
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
            var respuesta = await SupabaseService.Client.From<RutaModel>().Get();

            ListaRutas.Clear();
            foreach (var ruta in respuesta.Models)
            {
                ListaRutas.Add(ruta);
            }
        }
        catch (Exception ex)
        {
            LblError.Text = $"No se pudieron cargar las rutas: {ex.Message}";
            LblError.IsVisible = true;
            await DisplayAlertAsync("Error", LblError.Text, "Aceptar");
        }

        CalcularConteo();
    }

    private void CalcularConteo()
    {
        LblConteo.Text = ListaRutas.Count.ToString();
    }

    private async void OnNuevoClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RutaRegistro());
    }

    private async void OnEditarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is RutaModel ruta)
        {
            await Navigation.PushAsync(new RutaRegistro(ruta));
        }
    }

    private void OnEliminarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is RutaModel ruta)
        {
            _rutaAEliminar = ruta;
            DeleteModal.IsVisible = true;
        }
    }

    private async void OnConfirmarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        if (_rutaAEliminar != null)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                await SupabaseService.Client
                    .From<RutaModel>()
                    .Where(x => x.Id == _rutaAEliminar.Id)
                    .Delete();

                ListaRutas.Remove(_rutaAEliminar);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar la ruta: {ex.Message}", "Aceptar");
            }

            _rutaAEliminar = null;
            CalcularConteo();
        }
    }

    private void OnCancelarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        _rutaAEliminar = null;
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
            ThemeToggleButton.Text = Application.Current.UserAppTheme == AppTheme.Dark ? "☀️" : "🌙";
        }
    }

    private void OnSalirClicked(object? sender, EventArgs e)
    {
        Application.Current?.CloseWindow(this.Window);
    }
}