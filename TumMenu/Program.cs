using Microsoft.AspNetCore.HttpOverrides;
using TumMenu.Extensions;
using TumMenu.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServices();

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
	ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
