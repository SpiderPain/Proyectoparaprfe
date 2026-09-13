namespace ImperioChocoyo;

public partial class OperadorRegistro : ContentPage
{
    private OperadorModel? _operadorEdicion = null;

    public OperadorRegistro()
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

    public OperadorRegistro(OperadorModel operador) : this()
    {
        _operadorEdicion = operador;
        CargarDatosEnFormulario();
    }

    private void CargarDatosEnFormulario()
    {
        if (_operadorEdicion == null) return;

        TxtDui.Text = _operadorEdicion.Dui;
        TxtNombre.Text = _operadorEdicion.Nombre;
        TxtApellido.Text = _operadorEdicion.Apellido;
        TxtTelefono.Text = _operadorEdicion.Telefono;
        TxtTipoLicencia.Text = _operadorEdicion.TipoLicencia;
        TxtLicenciaNumero.Text = _operadorEdicion.LicenciaNumero;
        TxtDireccion.Text = _operadorEdicion.Direccion;
        TxtRol.Text = _operadorEdicion.Rol;
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        if (!ValidarCampos())
        {
            await DisplayAlertAsync("Campos incompletos", "Por favor completa los campos obligatorios (DUI, Nombre, Apellido y Rol)", "Aceptar");
            return;
        }

        try
        {
            await SupabaseService.InitializeAsync();

            string dui = TxtDui.Text?.Trim() ?? string.Empty;

            var existentes = await SupabaseService.ReintentarAsync(() =>
                SupabaseService.Client
                    .From<OperadorModel>()
                    .Where(x => x.Dui == dui)
                    .Get());

            var duplicado = existentes.Models.FirstOrDefault(o =>
                string.Equals(o.Dui, dui, StringComparison.OrdinalIgnoreCase));

            bool esDuplicado = duplicado != null &&
                (_operadorEdicion == null || duplicado.Id != _operadorEdicion.Id);

            if (esDuplicado)
            {
                await DisplayAlertAsync("DUI ya registrado",
                    $"Ya existe un operador con el DUI \"{dui}\".\nUsa un DUI diferente.",
                    "Aceptar");
                return;
            }

            var datos = new OperadorModel
            {
                Dui = dui,
                Nombre = TxtNombre.Text?.Trim() ?? string.Empty,
                Apellido = TxtApellido.Text?.Trim() ?? string.Empty,
                Telefono = TxtTelefono.Text?.Trim(),
                TipoLicencia = TxtTipoLicencia.Text?.Trim(),
                LicenciaNumero = TxtLicenciaNumero.Text?.Trim(),
                Direccion = TxtDireccion.Text?.Trim(),
                Rol = TxtRol.Text?.Trim() ?? string.Empty
            };

            if (_operadorEdicion == null)
            {
                await SupabaseService.ReintentarAsync(() =>
                    SupabaseService.Client.From<OperadorModel>().Insert(datos));
            }
            else
            {
                await SupabaseService.ReintentarAsync(() =>
                    SupabaseService.Client
                        .From<OperadorModel>()
                        .Where(x => x.Id == _operadorEdicion.Id)
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
        return !string.IsNullOrWhiteSpace(TxtDui.Text) &&
               !string.IsNullOrWhiteSpace(TxtNombre.Text) &&
               !string.IsNullOrWhiteSpace(TxtApellido.Text) &&
               !string.IsNullOrWhiteSpace(TxtRol.Text);
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