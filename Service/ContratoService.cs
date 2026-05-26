using System.Text;
using Backend.DTOs.Contratos;
using Backend.Models;
using Backend.Repositories.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace Backend.Services
{
    public class ContratoService
    {
        private readonly IContratoRepository _repo;
        private readonly IGrupoRepository _grupos;

        public ContratoService(IContratoRepository repo, IGrupoRepository grupos)
        {
            _repo = repo;
            _grupos = grupos;
        }

        public async Task<ContratoResponse> GerarAsync(int idMissaoAceita, int solicitanteId, ContratoGenerateRequest req)
        {
            // 1) Conferir aceitação existe
            var ace = await _repo.GetAceitacaoAsync(idMissaoAceita)
                      ?? throw new KeyNotFoundException("Aceitação não encontrada.");

            // 2) Conferir autorização: participante (solo/grupo) pode gerar
            var autorizado = false;
            if (ace.IdUsuario.HasValue) autorizado = ace.IdUsuario.Value == solicitanteId;
            else if (ace.IdGrupo.HasValue) autorizado = await _grupos.IsMembroAsync(ace.IdGrupo.Value, solicitanteId);

            if (!autorizado) throw new UnauthorizedAccessException("Você não participou desta missão.");

            // 3) Buscar missão e criador
            var missao = await _repo.GetMissaoAsync(ace.IdMissao)
                         ?? throw new KeyNotFoundException("Missão não encontrada.");
            var criador = await _repo.GetUsuarioAsync(missao.IdCriador)
                         ?? throw new KeyNotFoundException("Criador não encontrado.");

            // 4) Montar lista de participantes
            var participantes = new List<Usuario>();
            if (ace.IdUsuario.HasValue)
            {
                var u = await _repo.GetUsuarioAsync(ace.IdUsuario.Value);
                if (u != null) participantes.Add(u);
            }
            else if (ace.IdGrupo.HasValue)
            {
                var ids = await _repo.ListarMembrosDoGrupoAsync(ace.IdGrupo.Value);
                foreach (var id in ids)
                {
                    var u = await _repo.GetUsuarioAsync(id);
                    if (u != null) participantes.Add(u);
                }
            }

            // 5) Montar termos (HTML simples)
            var termos = BuildHtmlContrato(missao, criador, participantes, req);
            var pdfBytes = GerarPdfContrato(missao, criador, participantes, req.Penalidades, req.Recompensas, req.Observacoes);
            var contratoExistente = await _repo.GetByMissaoAceitaAsync(idMissaoAceita);
            if (contratoExistente != null)
            {
                contratoExistente.Termos = termos;
                contratoExistente.Penalidades = req.Penalidades;
                contratoExistente.Recompensas = req.Recompensas;
                contratoExistente.VersaoPDF = pdfBytes;
                contratoExistente.DataGeracao = DateTime.Now;
                await _repo.SaveAsync();
                return ToResponse(contratoExistente);
            }

            var contrato = new Contrato
            {
                IdMissaoAceita = idMissaoAceita,
                Termos = termos,
                Penalidades = req.Penalidades,
                Recompensas = req.Recompensas,
                VersaoPDF = pdfBytes
            };

            var saved = await _repo.CreateAsync(contrato);
            return ToResponse(saved);
        }

        public async Task<ContratoResponse?> ObterAsync(int idContrato, int solicitanteId)
        {
            var c = await _repo.GetByIdAsync(idContrato) ?? null;
            if (c == null) return null;

            // autorização: qualquer participante OU criador da missão
            var ace = await _repo.GetAceitacaoAsync(c.IdMissaoAceita) ?? throw new KeyNotFoundException();
            var missao = await _repo.GetMissaoAsync(ace.IdMissao) ?? throw new KeyNotFoundException();

            var autorizado = missao.IdCriador == solicitanteId;
            if (!autorizado)
            {
                if (ace.IdUsuario.HasValue) autorizado = ace.IdUsuario.Value == solicitanteId;
                else if (ace.IdGrupo.HasValue) autorizado = await _grupos.IsMembroAsync(ace.IdGrupo.Value, solicitanteId);
            }
            if (!autorizado) throw new UnauthorizedAccessException("Sem permissão.");

            return ToResponse(c);
        }

        public async Task<(byte[] bytes, string fileName)?> BaixarAsync(int idContrato, int solicitanteId)
        {
            var c = await _repo.GetByIdAsync(idContrato) ?? null;
            if (c == null || c.VersaoPDF == null) return null;

            // mesma checagem de permissão do ObterAsync
            var ace = await _repo.GetAceitacaoAsync(c.IdMissaoAceita) ?? throw new KeyNotFoundException();
            var missao = await _repo.GetMissaoAsync(ace.IdMissao) ?? throw new KeyNotFoundException();

            var autorizado = missao.IdCriador == solicitanteId;
            if (!autorizado)
            {
                if (ace.IdUsuario.HasValue) autorizado = ace.IdUsuario.Value == solicitanteId;
                else if (ace.IdGrupo.HasValue) autorizado = await _grupos.IsMembroAsync(ace.IdGrupo.Value, solicitanteId);
            }
            if (!autorizado) throw new UnauthorizedAccessException("Sem permissão.");

            var name = $"Contrato_MissaoAceita_{c.IdMissaoAceita}_{c.Id}.pdf";
            return (c.VersaoPDF, name);
        }

        // ---- helpers ----
        private static string BuildHtmlContrato(Missao m, Usuario criador, List<Usuario> participantes, ContratoGenerateRequest req)
        {
            var participantesStr = string.Join(", ", participantes.Select(p => $"{p.Nome} (Nível {p.Nivel}, {p.Classe})"));
            var penal = string.IsNullOrWhiteSpace(req.Penalidades) ? "Penalidades padrão por descumprimento e abandono." : req.Penalidades!;
            var recomp = string.IsNullOrWhiteSpace(req.Recompensas) ? $"Recompensa base: {m.Recompensa:c}" : req.Recompensas!;

            var obs = string.IsNullOrWhiteSpace(req.Observacoes) ? "" : $"<p><b>Observações:</b> {req.Observacoes}</p>";

            return $@"
<html>
  <head><meta charset='utf-8'><title>Contrato da Missão</title></head>
  <body>
    <h1>Contrato da Missão: {m.Titulo}</h1>
    <p><b>Criador:</b> {criador.Nome} (Id {criador.Id})</p>
    <p><b>Localização:</b> {m.Localizacao} | <b>Tipo:</b> {m.TipoMissao} | <b>Nível mínimo:</b> {m.NivelMinimo}</p>
    <p><b>Participantes:</b> {participantesStr}</p>
    <h3>Termos</h3>
    <p>{m.Descricao}</p>
    <h3>Penalidades</h3>
    <p>{penal}</p>
    <h3>Recompensas</h3>
    <p>{recomp}</p>
    {obs}
    <p><i>Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}</i></p>
  </body>
</html>";
        }

        // Placeholder de “PDF”: guardamos os bytes do HTML.
        private static byte[] GerarPdfContrato(
    Missao m, Usuario criador, List<Usuario> participantes, string? penalidades, string? recompensas, string? observacoes)
        {
            var participantesStr = string.Join(", ", participantes.Select(p => $"{p.Nome} (Nível {p.Nivel}, {p.Classe})"));
            penalidades ??= "Penalidades padrão por descumprimento e abandono.";
            recompensas ??= $"Recompensa base: {m.Recompensa:c}";

            var agora = DateTime.Now;

            // Documento QuestPDF
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);
                    page.Size(PageSizes.A4);

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Guilda Digital").SemiBold().FontSize(16);
                            col.Item().Text($"Contrato da Missão").FontSize(12);
                        });
                        row.ConstantItem(120).AlignRight().Text($"{agora:dd/MM/yyyy HH:mm}");
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(6);

                        col.Item().Text(m.Titulo).Bold().FontSize(18);

                        col.Item().Text(txt =>
                        {
                            txt.Span("Criador: ").SemiBold();
                            txt.Span($"{criador.Nome} (Id {criador.Id})");
                        });

                        col.Item().Text($"Localização: {m.Localizacao}").FontSize(10);
                        col.Item().Text($"Tipo: {m.TipoMissao} | Nível mínimo: {m.NivelMinimo}").FontSize(10);

                        col.Item().LineHorizontal(0.5f);

                        col.Item().Text("Participantes").Bold().FontSize(12);
                        col.Item().Text(participantesStr).FontSize(10);

                        col.Item().Text("Termos").Bold().FontSize(12);
                        col.Item().Text(m.Descricao).FontSize(10);

                        col.Item().Text("Penalidades").Bold().FontSize(12);
                        col.Item().Text(penalidades).FontSize(10);

                        col.Item().Text("Recompensas").Bold().FontSize(12);
                        col.Item().Text(recompensas).FontSize(10);

                        if (!string.IsNullOrWhiteSpace(observacoes))
                        {
                            col.Item().Text("Observações").Bold().FontSize(12);
                            col.Item().Text(observacoes).FontSize(10);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Gerado por ").FontSize(9).Light();
                        x.Span("Guilda Digital").SemiBold().FontSize(9).Light();
                        x.Span(" • ").FontSize(9).Light();
                        x.Span($"{agora:dd/MM/yyyy HH:mm}").FontSize(9).Light();
                    });
                });
            }).GeneratePdf();
        }


        private static ContratoResponse ToResponse(Contrato c) => new()
        {
            Id = c.Id,
            IdMissaoAceita = c.IdMissaoAceita,
            Termos = c.Termos,
            Penalidades = c.Penalidades,
            Recompensas = c.Recompensas,
            DataGeracao = c.DataGeracao,
            TemArquivo = c.VersaoPDF != null
        };
    }
}
