namespace ImperioChocoyo;

public partial class RutaRegistro : ContentPage
{
    private RutaModel? _rutaEdicion = null;

    public RutaRegistro()
    {
        InitializeComponent();
        UpdateThemeButtonText();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        AplicarDiseñoResponsivo(width);
    }

    private void AplicarDiseñoResponsivo(double width)
    {
        if (FieldGrid == null || FieldGrid.Children.Count == 0) return;

        bool compacto = width < 700;
        var campos = FieldGrid.Children.Cast<View>().ToList();

        int totalFilas = RenderingFilas(campos, compacto);
        FieldGrid.ColumnDefinitions.Clear();
        for (int c = 0; c < (compacto ? 1 : 2); c++)
        {
            FieldGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }
        FieldGrid.RowDefinitions.Clear();
        for (int r = 0; r < totalFilas; r++)
        {
            FieldGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        int fila = 0;
        int col = 0;
        for (int i = 0; i < campos.Count; i++)
        {
            bool esNota = EsNota(campos[i]);
            if (compacto)
            {
                Grid.SetRow(campos[i], i);
                Grid.SetColumn(campos[i], 0);
                Grid.SetColumnSpan(campos[i], 1);
                continue;
            }

            if (esNota)
            {
                Grid.SetRow(campos[i], fila);
                Grid.SetColumn(campos[i], 0);
                Grid.SetColumnSpan(campos[i], 2);
                fila++;
                col = 0;
                continue;
            }

            Grid.SetRow(campos[i], fila);
            Grid.SetColumn(campos[i], col);
            Grid.SetColumnSpan(campos[i], 1);
            col++;
            if (col == 2)
            {
                col = 0;
                fila++;
            }
        }

        FieldGrid.RowSpacing = compacto ? 14 : 18;

        if (CardRegistro != null)
        {
            CardRegistro.Padding = compacto ? new Thickness(18, 24) : new Thickness(30, 28);
        }
    }

    private static int RenderingFilas(List<View> campos, bool compacto)
    {
        if (compacto) return campos.Count;
        int filas = 0;
        int col = 0;
        foreach (var campo in campos)
        {
            if (EsNota(campo)) { filas++; col = 0; continue; }
            col++;
            if (col == 2) { col = 0; filas++; }
        }
        if (col == 1) filas++;
        return filas;
    }

    private static bool EsNota(View vista) =>
        vista is VerticalStackLayout vs && vs.Children.Count == 1 && vs.Children[0] is Label;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CardRegistro.Opacity = 0;
        CardRegistro.TranslationY = 28;
        CardRegistro.FadeToAsync(1, 500, Easing.CubicOut);
        CardRegistro.TranslateToAsync(0, 0, 500, Easing.CubicOut);
    }

    public RutaRegistro(RutaModel ruta) : this()
    {
        _rutaEdicion = ruta;
        CargarDatosEnFormulario();
    }

    private void CargarDatosEnFormulario()
    {
        if (_rutaEdicion == null) return;

        TxtNombre.Text = _rutaEdicion.Nombre;
        TxtOrigen.Text = _rutaEdicion.Origen;
        TxtDestino.Text = _rutaEdicion.Destino;
        TxtDistancia.Text = _rutaEdicion.Distancia?.ToString() ?? string.Empty;
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        if (!ValidarCampos())
        {
            await DisplayAlertAsync("Campos incompletos", "Por favor completa Nombre, Origen y Destino", "Aceptar");
            return;
        }

        decimal? distancia = null;
        if (!string.IsNullOrWhiteSpace(TxtDistancia.Text))
        {
            if (!decimal.TryParse(TxtDistancia.Text, out var d))
            {
                await DisplayAlertAsync("Dato inválido", "La distancia debe ser un número (ej. 75.5)", "Aceptar");
                return;
            }
            distancia = d;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            string nombreRuta = TxtNombre.Text?.Trim() ?? string.Empty;

            var existentes = await SupabaseService.ReintentarAsync(() =>
                SupabaseService.Client
                    .From<RutaModel>()
                    .Where(x => x.Nombre == nombreRuta)
                    .Get());

            var duplicado = existentes.Models.FirstOrDefault(r =>
                string.Equals(r.Nombre, nombreRuta, StringComparison.OrdinalIgnoreCase));

            bool esDuplicado = duplicado != null &&
                (_rutaEdicion == null || duplicado.Id != _rutaEdicion.Id);

            if (esDuplicado)
            {
                await DisplayAlertAsync("Ruta ya registrada",
                    $"Ya existe una ruta con el nombre \"{nombreRuta}\".\nUsa un nombre diferente.",
                    "Aceptar");
                return;
            }

            var datos = new RutaModel
            {
                Nombre = nombreRuta,
                Origen = TxtOrigen.Text?.Trim() ?? string.Empty,
                Destino = TxtDestino.Text?.Trim() ?? string.Empty,
                Distancia = distancia
            };

            if (_rutaEdicion == null)
            {
                await SupabaseService.ReintentarAsync(() =>
                    SupabaseService.Client.From<RutaModel>().Insert(datos));
            }
            else
            {
                await SupabaseService.ReintentarAsync(() =>
                    SupabaseService.Client
                        .From<RutaModel>()
                        .Where(x => x.Id == _rutaEdicion.Id)
                        .Update(datos));
            }

            SuccessModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("No se pudo guardar", $"{ex.Message}", "Aceptar");
        }
    }

    private bool ValidarCampos()
    {
        return !string.IsNullOrWhiteSpace(TxtNombre.Text) &&
               !string.IsNullOrWhiteSpace(TxtOrigen.Text) &&
               !string.IsNullOrWhiteSpace(TxtDestino.Text);
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
            ThemeToggleButton.Text = Application.Current.UserAppTheme == AppTheme.Dark ? "Claro" : "Oscuro";
        }
    }

    private void OnSalirClicked(object? sender, EventArgs e) => LogoutModal.IsVisible = true;
    private void OnNoSalirClicked(object? sender, EventArgs e) => LogoutModal.IsVisible = false;
    private void OnSiSalirClicked(object? sender, EventArgs e)
    {
        Application.Current?.CloseWindow(this.Window);
    }

    private void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        this.Window.Page = new NavigationPage(new Login());
    }
}