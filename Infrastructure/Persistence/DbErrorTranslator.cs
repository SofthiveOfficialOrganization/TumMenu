using Application.Common.Errors;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Infrastructure.Persistence;

public static partial class DbErrorTranslator
{
    public static (string code, string message) Translate(DbUpdateException ex)
    {
        if(ex.InnerException is SqlException sql)
        {
            return sql.Number switch
            {
                2627 or 2601 => (ErrorCodes.DbDuplicate, BuildDuplicateMessage(sql)),
                547 => (ErrorCodes.DbForeignKey, "Bağımlı kayıt bulundu (foreign key)."),
                8152 => (ErrorCodes.DbGeneric, "Girilen değer izin verilen maksimum uzunluğu aşıyor."),
                _ => (ErrorCodes.DbGeneric, "Veritabanı hatası.")
            };
        }
        return (ErrorCodes.DbGeneric, "Veritabanı hatası.");
    }

    private static string BuildDuplicateMessage(SqlException sql)
    {
        // Try to extract constraint name like IX_Companies_OwnerId
        var match = ConstraintNameRegex().Match(sql.Message);
        if(match.Success)
        {
            var constraint = match.Groups[1].Value;
            if(string.Equals(constraint, "IX_Products_CategoryId_Slug", StringComparison.OrdinalIgnoreCase))
                return "Bu kategoride aynı URL yoluna sahip bir ürün zaten mevcut.";

            // Extract field hints: IX_Companies_OwnerId → OwnerId
            var parts = constraint.Split('_');
            if(parts.Length >= 3)
            {
                var field = string.Join(", ", parts.Skip(2));
                return $"Bu kayıt zaten mevcut. Çakışan alan: {field}";
            }
            return $"Bu kayıt zaten mevcut (constraint: {constraint}).";
        }
        return "Bu kayıt zaten mevcut (unique constraint).";
    }

    [GeneratedRegex(@"'([^']+)'")]
    private static partial Regex ConstraintNameRegex();
}
