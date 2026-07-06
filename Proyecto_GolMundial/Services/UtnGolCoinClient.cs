namespace Proyecto_GolMundial.Services
{
    public class UtnGolCoinClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UtnGolCoinClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task NotificarResultadoLiquidacion(int partidoId, string resultado, int cuotaAplicada)
        {
            // URL base del backend de UTNGolCoin (Jakarta EE - Java)
            // Se asume que este backend estará corriendo en algún puerto, e.g. 8080
            var golCoinApiUrl = _configuration["UtnGolCoinApiUrl"] ?? "http://localhost:8080/api";
            
            var payload = new
            {
                PartidoId = partidoId,
                ResultadoOficial = resultado,
                Cuota = cuotaAplicada
            };

            try
            {
                // Realiza la petición POST a la API de GolCoin (RF12)
                var response = await _httpClient.PostAsJsonAsync($"{golCoinApiUrl}/liquidaciones", payload);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // En un escenario real aquí habría un logger, manejo de reintentos o cola de mensajes
                Console.WriteLine($"Error al notificar a UTNGolCoin: {ex.Message}");
            }
        }
    }
}
