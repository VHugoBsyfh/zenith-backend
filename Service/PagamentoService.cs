using System.Net.Http; // <-- ADICIONE ESTA LINHA AQUI!
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Backend.Services
{
    public class PagamentoService
    {
        private readonly HttpClient _http;
        private readonly string _accessToken = "TEST-5831263542615793-061200-37f9e564fa2f4d68db8c4ff2728e8096-3469115004";

        public PagamentoService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GerarPixMercadoPagoAsync(decimal valor, string emailPagador, string descricao)
        {
            var payload = new
            {
                transaction_amount = valor,
                description = descricao,
                payment_method_id = "pix",
                payer = new { email = emailPagador }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // 1. Montamos a requisição de forma mais segura para o C#
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadopago.com/v1/payments");
            request.Content = content;

            // 2. Passamos o Token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            // 3. NOVO: O Mercado Pago exige essa chave única para evitar pagamentos duplicados
            request.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());

            // 4. Envia para o Mercado Pago
            var response = await _http.SendAsync(request);

            // 5. NOVO: Se o Mercado Pago der erro, vamos ler EXATAMENTE o que ele está reclamando!
            if (!response.IsSuccessStatusCode)
            {
                var erroDetalhado = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro do Mercado Pago: {erroDetalhado}");
            }

            // 6. Lê o retorno de sucesso
            var responseBody = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseBody);

            // 7. Pega a string do Pix
            var pixCopiaECola = jsonDoc.RootElement
                .GetProperty("point_of_interaction")
                .GetProperty("transaction_data")
                .GetProperty("qr_code").GetString();

            return pixCopiaECola;
        }
    }
}
