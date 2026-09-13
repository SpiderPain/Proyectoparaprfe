namespace ImperioChocoyo;

public partial class Registro : ContentPage
{
    private AutobusModel? _autobusEdicion = null;

    // Constructor 1: Para Crear Nuevo Autobús
    public Registro()
    {
        InitializeComponent();
        UpdateThemeButtonText();
        CargarAnios();
    }

    private void CargarAnios()
    {
        var anios = new List<string>();
        int actual = DateTime.Now.Year;
        for (int a = actual; a >= 1990; a--)
        {
            anios.Add(a.ToString());
        }
        CmbAno.ItemsSource = anios;
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

    // Constructor 2: Para Editar Autobús Existente
    public Registro(AutobusModel autobus) : this()
    {
        _autobusEdicion = autobus;
        CargarDatosEnFormulario();
    }

    private void CargarDatosEnFormulario()
    {
        if (_autobusEdicion == null) return;

        TxtPlaca.Text = _autobusEdicion.Placa;
        TxtCapacidad.Text = _autobusEdicion.Capacidad.ToString();
        TxtVin.Text = _autobusEdicion.Vin;
        SeleccionarEnPicker(CmbAno, _autobusEdicion.Anio.ToString());
        SeleccionarEnPicker(CmbColor, _autobusEdicion.Color);
        SeleccionarEnPicker(CmbCategoria, _autobusEdicion.Categoria);
        SeleccionarEnPicker(CmbMarca, _autobusEdicion.Marca);
        SeleccionarEnPicker(CmbEstado, EstadoParaMostrar(_autobusEdicion.Estado));
        TxtModelo.Text = _autobusEdicion.Modelo;

        if (_autobusEdicion.FechaAdquisicion != default)
        {
            DpFecha.Date = _autobusEdicion.FechaAdquisicion.Date;
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

    private async void OnRegistrarAutobusClicked(object? sender, EventArgs e)
    {
        if (!ValidarCampos())
        {
            string camposFaltantes = "";
            if (string.IsNullOrWhiteSpace(TxtPlaca.Text)) camposFaltantes += "- Placa del vehículo\n";
            if (string.IsNullOrWhiteSpace(TxtCapacidad.Text)) camposFaltantes += "- Capacidad de pasajeros\n";
            if (string.IsNullOrWhiteSpace(TxtVin.Text)) camposFaltantes += "- Número VIN\n";
            if (string.IsNullOrWhiteSpace(TxtModelo.Text)) camposFaltantes += "- Modelo del vehículo\n";
            if (CmbAno.SelectedIndex == -1) camposFaltantes += "- Año de fabricación\n";
            if (CmbColor.SelectedIndex == -1) camposFaltantes += "- Color del vehículo\n";
            if (CmbCategoria.SelectedIndex == -1) camposFaltantes += "- Categoría del vehículo\n";
            if (CmbMarca.SelectedIndex == -1) camposFaltantes += "- Marca del vehículo\n";
            if (CmbEstado.SelectedIndex == -1) camposFaltantes += "- Estado del vehículo\n";

            await DisplayAlertAsync("Campos incompletos", 
                $"No se puede registrar el autobús porque faltan los siguientes campos:\n\n{camposFaltantes}\nPor favor completa todos los campos obligatorios marcados con (*).", 
                "Aceptar");
            return;
        }

        if (!int.TryParse(TxtCapacidad.Text, out int capacidad) ||
            !int.TryParse(CmbAno.SelectedItem?.ToString(), out int anio))
        {
            string errorDetalle = "";
            if (!int.TryParse(TxtCapacidad.Text, out _)) errorDetalle += "- La capacidad debe ser un número entero válido (ej: 45)\n";
            if (!int.TryParse(CmbAno.SelectedItem?.ToString(), out _)) errorDetalle += "- Debes seleccionar un año válido de la lista\n";

            await DisplayAlertAsync("Datos inválidos", 
                $"Los datos ingresados no son válidos:\n\n{errorDetalle}\nCorrige los campos marcados e intenta de nuevo.", 
                "Aceptar");
            return;
        }

        string placa = TxtPlaca.Text?.Trim() ?? string.Empty;
        string vin = TxtVin.Text?.Trim() ?? string.Empty;
        string modelo = TxtModelo.Text?.Trim() ?? string.Empty;

        const int MaxPlaca = 17;
        const int MaxVin = 17;
        const int MaxModelo = 100;
        string longError = "";
        if (placa.Length > MaxPlaca) longError += $"- Placa: máximo {MaxPlaca} caracteres (tienes {placa.Length})\n";
        if (vin.Length > MaxVin) longError += $"- VIN: máximo {MaxVin} caracteres (tienes {vin.Length})\n";
        if (modelo.Length > MaxModelo) longError += $"- Modelo: máximo {MaxModelo} caracteres (tienes {modelo.Length})\n";

        if (!string.IsNullOrEmpty(longError))
        {
            await DisplayAlertAsync("Texto demasiado largo",
                $"Los siguientes campos exceden el límite permitido en la base de datos:\n\n{longError}\nAcorta los valores e intenta de nuevo.",
                "Aceptar");
            return;
        }

try
        {
            await SupabaseService.InitializeAsync();

            var existentes = await SupabaseService.ReintentarAsync(() =>
                SupabaseService.Client
                    .From<AutobusModel>()
                    .Where(x => x.Placa == placa)
                    .Get());

            var duplicado = existentes.Models.FirstOrDefault(a =>
                string.Equals(a.Placa, placa, StringComparison.OrdinalIgnoreCase));

            bool esDuplicado = duplicado != null &&
                (_autobusEdicion == null || duplicado.Id != _autobusEdicion.Id);

            if (esDuplicado)
            {
                await DisplayAlertAsync("Placa ya registrada",
                    $"Ya existe un autobús con la placa \"{placa}\".\nUsa una placa diferente.",
                    "Aceptar");
                return;
            }

            if (_autobusEdicion == null)
            {
                var nuevoAutobus = new AutobusModel
                {
                    Placa = placa,
                    Capacidad = capacidad,
                    Vin = vin,
                    Modelo = modelo,
                    Anio = anio,
                    Color = CmbColor.SelectedItem?.ToString() ?? string.Empty,
                    Categoria = CmbCategoria.SelectedItem?.ToString() ?? string.Empty,
                    Marca = CmbMarca.SelectedItem?.ToString() ?? string.Empty,
                    Estado = EstadoParaGuardar(CmbEstado.SelectedItem?.ToString()),
FechaAdquisicion = DpFecha.Date ?? DateTime.Today
                };

                await SupabaseService.ReintentarAsync(() =>
                    SupabaseService.Client.From<AutobusModel>().Insert(nuevoAutobus));
            }
            else
            {
                _autobusEdicion.Placa = placa;
                _autobusEdicion.Capacidad = capacidad;
                _autobusEdicion.Vin = vin;
                _autobusEdicion.Modelo = modelo;
                _autobusEdicion.Anio = anio;
                _autobusEdicion.Color = CmbColor.SelectedItem?.ToString() ?? string.Empty;
                _autobusEdicion.Categoria = CmbCategoria.SelectedItem?.ToString() ?? string.Empty;
                _autobusEdicion.Marca = CmbMarca.SelectedItem?.ToString() ?? string.Empty;
                _autobusEdicion.Estado = EstadoParaGuardar(CmbEstado.SelectedItem?.ToString());
                _autobusEdicion.FechaAdquisicion = DpFecha.Date ?? DateTime.Today;

                await SupabaseService.ReintentarAsync(() =>
                    SupabaseService.Client
                        .From<AutobusModel>()
                        .Where(x => x.Id == _autobusEdicion.Id)
                        .Update(_autobusEdicion));
            }

            SuccessModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("No se pudo guardar", $"{ex.Message}", "Aceptar");
        }
    }

    private async void OnAceptarModalClicked(object? sender, EventArgs e)
    {
        SuccessModal.IsVisible = false;
        await Navigation.PopAsync(); // Regresa al Dashboard
    }

    private async void OnSiCancelarClicked(object? sender, EventArgs e)
    {
        CancelModal.IsVisible = false;
        await Navigation.PopAsync(); // Regresa al Dashboard
    }

    private bool ValidarCampos()
    {
        return !string.IsNullOrWhiteSpace(TxtPlaca.Text) &&
               !string.IsNullOrWhiteSpace(TxtCapacidad.Text) &&
               !string.IsNullOrWhiteSpace(TxtVin.Text) &&
               !string.IsNullOrWhiteSpace(TxtModelo.Text) &&
               CmbAno.SelectedIndex != -1 &&
               CmbColor.SelectedIndex != -1 &&
               CmbCategoria.SelectedIndex != -1 &&
               CmbMarca.SelectedIndex != -1 &&
               CmbEstado.SelectedIndex != -1;
    }

    private static string EstadoParaMostrar(string? estado)
    {
        return estado?.ToLowerInvariant() switch
        {
            "activo" => "Activo",
            "reparacion" => "En Reparación",
            _ => estado ?? string.Empty
        };
    }

    private static string EstadoParaGuardar(string? estado)
    {
        return estado?.ToLowerInvariant() switch
        {
            "activo" => "activo",
            "en reparación" => "reparacion",
            "en reparacion" => "reparacion",
            _ => (estado ?? string.Empty).ToLowerInvariant()
        };
    }

    private void OnCancelarClicked(object? sender, EventArgs e) => CancelModal.IsVisible = true;
    private void OnNoContinuarClicked(object? sender, EventArgs e) => CancelModal.IsVisible = false;

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
            ThemeToggleButton.Text = Application.Current.UserAppTheme == AppTheme.Dark ? "Modo Claro" : "Modo Oscuro";
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