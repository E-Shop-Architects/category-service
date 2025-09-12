// ✅ İkinci Program.cs'den eklenen using'ler
using Application; // Eğer CategoryService.Services içinde Application katmanınız varsa
using CategoryService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Core.Security.Encryption;
using Core.Security.JWT;
using Persistence; // Eğer CategoryService.Services içinde Persistence katmanınız varsa ve BaseDbContext'iniz farklı bir Persistence'tan geliyorsa
using Persistence.Context;
using Serilog;
using Serilog.AspNetCore;
using System.Net; // Dns sınıfı için ekleyin

using Serilog.Formatting.Json;
using Serilog.Settings.Configuration;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebAPI; // Eğer WebAPI katmanınız varsa (genellikle Program.cs'nin bulunduğu katman)

var builder = WebApplication.CreateBuilder(args);

// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();



builder.Host.UseSerilog();


builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// DbContext ve servisler (mevcut hali korunuyor, ancak Persistence.Context yerine NArchitecture.Persistence kullanılabilir)
builder.Services.AddDbContext<BaseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseContext")));

// Add services to the container.
builder.Services.AddSingleton<RabbitMQPublisher>(); // Mevcut

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Mevcut

// ✅ Distributed Caching eklenmesi
builder.Services.AddDistributedMemoryCache();

// ✅ CORS yapılandırması (ikinci program.cs'deki gibi genel bir politika ekliyoruz)
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
    {
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    })
);

// Ancak özel "AllowReactApp" politikanızı da koruyalım, çünkü React uygulamanız için spesifik olabilir.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// JWT Doğrulama (ikinci Program.cs'deki yapıya uygun hale getiriliyor)
//const string tokenOptionsConfigurationSection = "TokenOptions";
//TokenOptions tokenOptions =
//    builder.Configuration.GetSection(tokenOptionsConfigurationSection).Get<TokenOptions>()
//    ?? throw new InvalidOperationException($"\"{tokenOptionsConfigurationSection}\" section cannot found in configuration.");
var authority = Environment.GetEnvironmentVariable("AUTH__AUTHORITY")
                ?? builder.Configuration["Auth:Authority"]   // appsettings.json
                ?? "http://keycloak:8080/realms/main";       // docker içi varsayılan
var audience = Environment.GetEnvironmentVariable("AUTH__AUDIENCE")
                ?? builder.Configuration["Auth:Audience"]
                ?? "category-api";                            // Keycloak clientId

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", o =>
    {
        o.Authority = authority;                 // Keycloak issuer
        o.Audience = audience;                  // Bu servisin clientId'si
        o.RequireHttpsMetadata = false;          // dev/docker
        o.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = "role",              // Keycloak mapper ile "role" dizisi
            NameClaimType = "email",
            ValidateAudience = true              // Gateway'de kapatıyorsan serviste açık tut
        };
    });

// Authorization politikaları (mevcut hali korunuyor)
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("CategoryRead", p => p.RequireRole("Admin", "Customer"));
    o.AddPolicy("CategoryWrite", p => p.RequireRole("Admin"));
});

// Swagger Gen (ikinci Program.cs'deki daha detaylı güvenlik tanımı ile güncelleniyor)
builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition(
        name: "Bearer",
        securityScheme: new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer YOUR_TOKEN\". \r\n\r\n"
                + "`Enter your token in the text input below.`"
        }
    );
    //opt.OperationFilter<BearerSecurityRequirementOperationFilter>(); // NArchitecture'dan gelen filtre
});


var app = builder.Build();

app.UseRouting(); // Mevcut

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt => // İkinci Program.cs'deki DocExpansion ayarı ekleniyor
    {
        opt.DocExpansion(DocExpansion.None);
    });
}

// ✅ Üretim ortamı için özel Exception Middleware
if (app.Environment.IsProduction())
    //app.ConfigureCustomExceptionMiddleware();

// ✅ Veritabanı Migrasyon Uygulayıcı (NArchitecture'dan)
//app.UseDbMigrationApplier();


app.UseHttpsRedirection(); // Mevcut
app.UseAuthentication(); // Mevcut
app.UseAuthorization(); // Mevcut

app.MapControllers(); // Mevcut

// ✅ Dinamik CORS politikası (WebAPIConfiguration üzerinden)
const string webApiConfigurationSection = "WebAPIConfiguration";
//WebApiConfiguration webApiConfiguration =
//    app.Configuration.GetSection(webApiConfigurationSection).Get<WebApiConfiguration>()
//    ?? throw new InvalidOperationException($"\"{webApiConfigurationSection}\" section cannot found in configuration.");
//app.UseCors(opt => opt.WithOrigins(webApiConfiguration.AllowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());

// Ancak mevcut "AllowReactApp" politikanızı da kullanmaya devam etmek için
// app.UseCors("AllowReactApp"); // Bu satır, üstteki dinamik CORS ile çakışabilir. Hangisini kullanacağınıza karar vermelisiniz.

// ✅ Yanıt Lokalizasyonu
//app.UseResponseLocalization();


app.Run();