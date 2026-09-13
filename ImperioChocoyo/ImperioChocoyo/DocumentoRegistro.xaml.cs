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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        CardRegistro.Opacity = 0;
        CardRegistro.TranslationY = 28;
        await CardRegistro.FadeToAsync(1, 500, Easing.CubicOut);
        await CardRegistro.TranslateToAsync(0, 0, 500, Easing.CubicOut);

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
        SeleccionarEnPicker(CmbTipo, _documentoEdicion.Tipo);
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

    private static void SeleccionarEnPicker(Picker picker, string? valor)
    {
        if (picker == null || string.IsNullOrWhiteSpace(valor)) return;

        var lista = new List<string>();
        if (picker.ItemsSource is System.Collections.IList items)
        {
            foreach (var it in items)
            {
                var txt = it?.ToString() ?? string.Empty;
                if (!lista.Contains(txt)) lista.Add(txt);
            }
        }
        if (!lista.Contains(valor)) lista.Add(valor);
        picker.ItemsSource = lista;
        picker.SelectedItem = valor;
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

            var datos = new DocumentoModel
            {
                IdAutobus = ((AutobusModel)CmbAutobus.SelectedItem).Id,
                Tipo = CmbTipo.SelectedItem?.ToString() ?? string.Empty,
                Numero = TxtNumero.Text?.Trim() ?? string.Empty,
                FechaEmision = DpEmision.Date,
                FechaVencimiento = DpVencimiento.Date
            };

            if (_documentoEdicion == null)
            {
                await SupabaseService.ReintentarAsync(() =>
                    SupabaseService.Client.From<DocumentoModel>().Insert(datos));
            }
            else
            {
                await SupabaseService.ReintentarAsync(() =>
                    SupabaseService.Client
                        .From<DocumentoModel>()
                        .Where(x => x.Id == _documentoEdicion.Id)
                        .Update(datos));
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