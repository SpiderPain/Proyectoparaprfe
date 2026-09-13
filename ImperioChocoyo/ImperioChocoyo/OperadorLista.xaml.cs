using System.Collections.ObjectModel;

namespace ImperioChocoyo;

public partial class OperadorLista : ContentPage
{
    public ObservableCollection<OperadorModel> ListaOperadores { get; set; } = new();
    private OperadorModel? _operadorAEliminar = null;

    public OperadorLista()
    {
        InitializeComponent();
        CvOperadores.ItemsSource = ListaOperadores;
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
            var respuesta = await SupabaseService.Client.From<OperadorModel>().Get();

            ListaOperadores.Clear();
            foreach (var operador in respuesta.Models)
            {
                ListaOperadores.Add(operador);
            }
        }
        catch (Exception ex)
        {
            LblError.Text = $"No se pudo cargar los operadores: {ex.Message}";
            LblError.IsVisible = true;
            await DisplayAlertAsync("Error", LblError.Text, "Aceptar");
        }

        CalcularConteo();
    }

    private void CalcularConteo()
    {
        LblConteo.Text = ListaOperadores.Count.ToString();
    }

    private async void OnNuevoClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new OperadorRegistro());
    }

    private async void OnEditarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is OperadorModel operador)
        {
            await Navigation.PushAsync(new OperadorRegistro(operador));
        }
    }

    private void OnEliminarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is OperadorModel operador)
        {
            _operadorAEliminar = operador;
            DeleteModal.IsVisible = true;
        }
    }

    private async void OnConfirmarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        if (_operadorAEliminar != null)
        {
            try
            {
                await SupabaseService.InitializeAsync();

                var viajes = await SupabaseService.ReintentarAsync(() =>
                    SupabaseService.Client
                        .From<RegistroViajeModel>()
                        .Where(x => x.IdOperador == _operadorAEliminar.Id)
                        .Get());

                if (viajes.Models.Count > 0)
                {
                    await DisplayAlertAsync("No se puede eliminar",
                        $"El operador {_operadorAEliminar.Nombre} {_operadorAEliminar.Apellido} tiene {viajes.Models.Count} viaje(s) asociado(s).\n\nElimina primero esos viajes para poder borrar al operador.",
                        "Aceptar");
                    return;
                }

                await SupabaseService.ReintentarAsync(() =>
                    SupabaseService.Client
                        .From<OperadorModel>()
                        .Where(x => x.Id == _operadorAEliminar.Id)
                        .Delete());

                ListaOperadores.Remove(_operadorAEliminar);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar el operador: {ex.Message}", "Aceptar");
            }

            _operadorAEliminar = null;
            CalcularConteo();
        }
    }

    private void OnCancelarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        _operadorAEliminar = null;
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