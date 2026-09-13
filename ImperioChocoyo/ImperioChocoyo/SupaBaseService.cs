using Supabase;

namespace ImperioChocoyo;

public static class SupabaseService
{
    private const string SupabaseUrl = "https://iwxdidcfnadmjxzsjopf.supabase.co";
    private const string SupabaseKey = "sb_publishable_ExzZrqOxj7TuKg-7C4_aZQ_xUtqN4Gs";

    private static Client? _client;

    public static Client Client
    {
        get
        {
            if (_client == null)
            {
                var options = new SupabaseOptions
                {
                    AutoRefreshToken = false,
                    AutoConnectRealtime = false
                };
                _client = new Client(SupabaseUrl, SupabaseKey, options);
            }
            return _client;
        }
    }

    public static async Task InitializeAsync()
    {
        await ReintentarAsync(() => Client.InitializeAsync());
    }

    public static async Task ReintentarAsync(Func<Task> accion, int intentos = 3)
    {
        for (int intento = 1; intento <= intentos; intento++)
        {
            try
            {
                await accion();
                return;
            }
            catch (Exception ex) when (EsErrorTransitorio(ex) && intento < intentos)
            {
                await Task.Delay(500 * intento);
            }
        }
    }

    public static async Task<T> ReintentarAsync<T>(Func<Task<T>> accion, int intentos = 3)
    {
        for (int intento = 1; intento <= intentos; intento++)
        {
            try
            {
                return await accion();
            }
            catch (Exception ex) when (EsErrorTransitorio(ex) && intento < intentos)
            {
                await Task.Delay(500 * intento);
            }
        }

        throw new InvalidOperationException("No se pudo completar la operación.");
    }

    private static bool EsErrorTransitorio(Exception ex)
    {
        var mensaje = ex.Message ?? string.Empty;
        if (ex.InnerException != null) mensaje += " " + ex.InnerException.Message;

        return mensaje.Contains("Gateway Timeout", StringComparison.OrdinalIgnoreCase) ||
               mensaje.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
               mensaje.Contains("502", StringComparison.OrdinalIgnoreCase) ||
               mensaje.Contains("503", StringComparison.OrdinalIgnoreCase) ||
               mensaje.Contains("504", StringComparison.OrdinalIgnoreCase) ||
               mensaje.Contains("temporarily", StringComparison.OrdinalIgnoreCase) ||
               mensaje.Contains("too many requests", StringComparison.OrdinalIgnoreCase) ||
               mensaje.Contains("SocketException", StringComparison.OrdinalIgnoreCase);
    }
}