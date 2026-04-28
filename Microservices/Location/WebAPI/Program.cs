using Microsoft.OpenApi.Models;
using WebAPI;
using WebAPI.Authorization;
using WebAPI.Exceptions;
using WebAPI.Extensions;

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

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.EnableAnnotations();
    opt.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    opt.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Basic",
        In = ParameterLocation.Header,
        Description = "Enter 'Basic' followed by a space and then your Base64-encoded credentials."
    });
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.MigrateDatabaseAsync();
await app.SeedDataAsync();
app.ConfigureCustomExceptionMiddleware();
app.UseHttpsRedirection();
app.UseCors("HappencodePolicy");
app.UseMiddleware<BasicAuthMiddleware>();
app.MapControllers();
app.Run();
