using Application.Auths.DTOs;
using Application.Common.Base.DTOs;

namespace Application.Staffs.DTOs;

public sealed record StaffDTO(
    string Role,
    Guid StoreId,
    string StoreName,
    ApplicationUserLiteDTO User
) : BaseDTO;

