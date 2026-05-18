using System.ComponentModel.DataAnnotations;

namespace WebUI.Areas.Admin.Models.DataMigration;

public sealed class TestConnectionsDto
{
    [Required] public string Source { get; set; } = string.Empty;
    [Required] public string Target { get; set; } = string.Empty;
}
