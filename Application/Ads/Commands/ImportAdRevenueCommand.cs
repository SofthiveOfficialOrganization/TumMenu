using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

public class ImportAdRevenueCommand : IRequest<ImportAdRevenueResult>
{
    public IFormFile File { get; set; } = null!;
}

public record ImportAdRevenueResult(int ImportedCount, int SkippedCount);

public class ImportAdRevenueCommandHandler(
    IRepository<AdRevenueImport> repo)
    : IRequestHandler<ImportAdRevenueCommand, ImportAdRevenueResult>
{
    public async Task<ImportAdRevenueResult> Handle(ImportAdRevenueCommand request, CancellationToken ct)
    {
        using var reader = new StreamReader(request.File.OpenReadStream());
        var existingDays = await System.Threading.Tasks.Task.Run(
            () => repo.Query().Select(r => r.Day).ToHashSet(), ct);

        var toInsert = new List<AdRevenueImport>();
        int skipped = 0;

        string? line;
        bool firstLine = true;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (firstLine) { firstLine = false; continue; }
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 5) { skipped++; continue; }

            if (!DateOnly.TryParse(parts[0].Trim(), out var day)) { skipped++; continue; }
            if (!decimal.TryParse(parts[3].Trim(), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var revenue))
            { skipped++; continue; }

            if (existingDays.Contains(day)) { skipped++; continue; }

            var currency = parts[4].Trim();
            toInsert.Add(new AdRevenueImport
            {
                Day = day,
                Revenue = revenue,
                Currency = string.IsNullOrEmpty(currency) ? "TRY" : currency,
                Network = "AdSense",
                SourceFile = request.File.FileName
            });
            existingDays.Add(day);
        }

        foreach (var item in toInsert)
            await repo.AddAsync(item, ct);

        await repo.SaveChangesAsync(ct);

        return new ImportAdRevenueResult(toInsert.Count, skipped);
    }
}
