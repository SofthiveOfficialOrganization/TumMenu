using Microsoft.OpenApi.Models;
using WebAPI;
using WebAPI.Authorization;
using WebAPI.Exceptions;
using WebAPI.Extensions;
using WebAPI.Models.Concrete;
using AwsOptions = WebAPI.Models.Concrete.AwsOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCustomServices(builder.Configuration);

var webApiConfiguration = builder.Configuration.GetSection("WebAPIConfiguration").Get<WebAPIConfiguration>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("HappencodePolicy", policy =>
    {
        policy
            .WithOrigins(webApiConfiguration?.AllowedOrigins ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var awsConfig = builder.Configuration.GetSection("AwsSettings");
builder.Services.Configure<AwsOptions>(awsConfig);

var googleCloudConfig = builder.Configuration.GetSection("GoogleCloudSettings");
builder.Services.Configure<GoogleCloudOptions>(googleCloudConfig);

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.EnableAnnotations();

    // 🔹 Basic Authentication İçin Swagger'a Destek Ekliyoruz
    opt.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Basic",
        In = ParameterLocation.Header,
        Description = "Enter 'Basic' followed by a space and then your Base64-encoded credentials (e.g., Basic dXNlcm5hbWU6cGFzc3dvcmQ=)."
    });

    // 🔹 Swagger'a Security Requirement (Yetkilendirme Zorunluluğu) Ekliyoruz
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Basic" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Hata Yönetimi Middleware (Önce hataları yakala)
app.ConfigureCustomExceptionMiddleware();

app.UseHttpsRedirection();

app.UseCors("HappencodePolicy");

// Kimlik Doğrulama Middleware (Basic Auth)
app.UseMiddleware<BasicAuthMiddleware>();

app.MapControllers();
app.Run();

