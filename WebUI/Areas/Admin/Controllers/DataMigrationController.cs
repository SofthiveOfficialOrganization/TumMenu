using Infrastructure.DataMigration;
using Infrastructure.DataMigration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Areas.Admin.Models.DataMigration;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class DataMigrationController(IDataMigrationService migrationService) : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Tables()
    {
        var tables = migrationService.ListTables();
        return Json(tables.Select(t => new { name = t.Name }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestConnections(
        [FromBody] TestConnectionsDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await migrationService.TestConnectionsAsync(dto.Source, dto.Target, ct);
        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Migrate(
        [FromBody] MigrateRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await migrationService.MigrateTableAsync(new MigrationRequest
            {
                Source = dto.Source,
                Target = dto.Target,
                TableName = dto.TableName
            }, ct);
            return Json(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
