using System.Net.Http; // <-- ADICIONE ESTA LINHA AQUI!
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Backend.Services
{
    public class PagamentoService
    {
        private readonly HttpClient _http;
        private readonly string _accessToken = "SEU_ACCESS_TOKEN_DE_TESTE_AQUI";

        public PagamentoService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GerarPixMercadoPagoAsync(decimal valor, string emailPagador, string descricao)
        {
            // 1. Monta o payload (o que o Mercado Pago exige)
            var payload = new
            {
                transaction_amount = valor,
                description = descricao,
                payment_method_id = "pix",
                payer = new { email = emailPagador }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // 2. Chama a API do Mercado Pago
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var response = await _http.PostAsync("https://api.mercadopago.com/v1/payments", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Erro ao gerar PIX na API externa.");

            // 3. Lê o retorno (que contém o QR Code e o Copia e Cola)
            var responseBody = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseBody);

            // Pega exatamente a string "Pix Copia e Cola" gerada pelo banco deles!
            var pixCopiaECola = jsonDoc.RootElement
                .GetProperty("point_of_interaction")
                .GetProperty("transaction_data")
                .GetProperty("qr_code").GetString();

            return pixCopiaECola;
        }
    }
}
