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
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                };
                _client = new Client(SupabaseUrl, SupabaseKey, options);
            }
            return _client;
        }
    }

    public static async Task InitializeAsync()
    {
        await Client.InitializeAsync();
    }
}