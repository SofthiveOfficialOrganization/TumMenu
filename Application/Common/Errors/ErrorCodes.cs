using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Errors;

public static class ErrorCodes
{
	public const string Validation = "VALIDATION_ERROR";
	public const string NotFound = "NOT_FOUND";
	public const string Conflict = "CONFLICT";
	public const string Forbidden = "FORBIDDEN";
	public const string Unauthorized = "UNAUTHORIZED";
	public const string DbDuplicate = "DB_DUPLICATE";
	public const string DbForeignKey = "DB_FOREIGN_KEY";
	public const string DbGeneric = "DB_ERROR";
	public const string Unknown = "UNKNOWN_ERROR";
}
