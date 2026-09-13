using System.Collections.ObjectModel;

namespace ImperioChocoyo;

public partial class DocumentoLista : ContentPage
{
    public ObservableCollection<DocumentoModel> ListaDocumentos { get; set; } = new();
    private DocumentoModel? _documentoAEliminar = null;

    public DocumentoLista()
    {
        InitializeComponent();
        CvDocumentos.ItemsSource = ListaDocumentos;
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

            var respuesta = await SupabaseService.Client.From<DocumentoModel>().Get();
            var autobuses = await SupabaseService.Client.From<AutobusModel>().Get();

            var dictAutobuses = autobuses.Models.ToDictionary(a => a.Id, a => a.Placa);

            ListaDocumentos.Clear();
            foreach (var documento in respuesta.Models)
            {
                documento.AutobusLabel = dictAutobuses.TryGetValue(documento.IdAutobus, out var placa) ? placa : $"#{documento.IdAutobus}";
                ListaDocumentos.Add(documento);
            }
        }
        catch (Exception ex)
        {
            LblError.Text = $"No se pudieron cargar los documentos: {ex.Message}";
            LblError.IsVisible = true;
            await DisplayAlertAsync("Error", LblError.Text, "Aceptar");
        }

        CalcularConteo();
    }

    private void CalcularConteo()
    {
        LblConteo.Text = ListaDocumentos.Count.ToString();
    }

    private async void OnNuevoClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new DocumentoRegistro());
    }

    private async void OnEditarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is DocumentoModel documento)
        {
            await Navigation.PushAsync(new DocumentoRegistro(documento));
        }
    }

    private void OnEliminarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is DocumentoModel documento)
        {
            _documentoAEliminar = documento;
            DeleteModal.IsVisible = true;
        }
    }

    private async void OnConfirmarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        if (_documentoAEliminar != null)
        {
            try
            {
                await SupabaseService.InitializeAsync();
                await SupabaseService.Client
                    .From<DocumentoModel>()
                    .Where(x => x.Id == _documentoAEliminar.Id)
                    .Delete();

                ListaDocumentos.Remove(_documentoAEliminar);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar el documento: {ex.Message}", "Aceptar");
            }

            _documentoAEliminar = null;
            CalcularConteo();
        }
    }

    private void OnCancelarEliminarClicked(object? sender, EventArgs e)
    {
        DeleteModal.IsVisible = false;
        _documentoAEliminar = null;
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