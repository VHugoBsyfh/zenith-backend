using Backend.Data;
using Backend.Repositories.Interfaces;
using Backend.Repositories;
using Backend.Services.Interfaces;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
//using Backend.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddControllers(); // 👈 precisa disso antes do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Guilda Digital API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT (sem 'Bearer ' no início)",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var corsPolicyName = "AllowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy
            .WithOrigins("http://localhost:4200") 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configuração dinâmica do banco
var dbProvider = builder.Configuration["Database:Provider"];
var connectionString = builder.Configuration["Database:ConnectionString"];

builder.Services.AddDbContext<GuildaDigitalContext>(options =>
{
    if (dbProvider == "SqlServer")
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)
        );
    }
});
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// ✅ Registrar repositórios e serviços
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IMissaoRepository, MissaoRepository>();
builder.Services.AddScoped<IGrupoRepository, GrupoRepository>();
builder.Services.AddScoped<IMensagemRepository, MensagemRepository>();
builder.Services.AddScoped<IMissaoAceitaRepository, MissaoAceitaRepository>();
builder.Services.AddScoped<IConclusaoRepository, ConclusaoRepository>();
builder.Services.AddScoped<ICancelamentoRepository, CancelamentoRepository>();
builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
builder.Services.AddScoped<IReputacaoRepository, ReputacaoRepository>();
builder.Services.AddScoped<IHistoricoRepository, HistoricoRepository>();
builder.Services.AddScoped<IPenalidadeReadRepository, PenalidadeReadRepository>();
builder.Services.AddScoped<IContratoRepository, ContratoRepository>();
builder.Services.AddScoped<IRankingRepository, RankingRepository>();
builder.Services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
builder.Services.AddScoped<IMissaoQueryRepository, MissaoQueryRepository>();
builder.Services.AddScoped<IRecomendacaoRepository, RecomendacaoRepository>();
builder.Services.AddScoped<IUsuarioAdminRepository, UsuarioAdminRepository>();
builder.Services.AddScoped<IAuthGuardRepository, AuthGuardRepository>();
builder.Services.AddScoped<UsuarioAdminService>();
builder.Services.AddScoped<RecomendacaoService>();
builder.Services.AddScoped<MissaoQueryService>();
builder.Services.AddScoped<NotificacaoService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddScoped<ContratoService>();
builder.Services.AddScoped<PenalidadeQueryService>();
builder.Services.AddScoped<HistoricoService>();
builder.Services.AddScoped<ReputacaoService>();
builder.Services.AddScoped<AvaliacaoService>();
builder.Services.AddScoped<CancelamentoService>();
builder.Services.AddScoped<ConclusaoService>();
builder.Services.AddScoped<AceitacaoService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<GrupoService>();
builder.Services.AddScoped<MissaoService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Swagger e HTTPS
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
