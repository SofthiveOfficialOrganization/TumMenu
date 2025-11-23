using Application.Common.Errors;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class DbErrorTranslator
{
    public static (string code, string message) Translate(DbUpdateException ex)
    {
        if(ex.InnerException is SqlException sql)
        {
            return sql.Number switch
            {
                2627 or 2601 => (ErrorCodes.DbDuplicate, "Bu kayıt zaten mevcut (unique constraint)."),
                547 => (ErrorCodes.DbForeignKey, "Bağımlı kayıt bulundu (foreign key)."),
                _ => (ErrorCodes.DbGeneric, "Veritabanı hatası.")
            };
        }
        return (ErrorCodes.DbGeneric, "Veritabanı hatası.");
    }
}
