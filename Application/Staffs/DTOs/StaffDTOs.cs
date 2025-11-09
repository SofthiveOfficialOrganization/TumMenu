using Application.Auths.DTOs;
using Application.Common.Base.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Staffs.DTOs;
public sealed record StaffDTO(
	Guid CompanyId,
	string CompanyName,
	string Role,
	BasicApplicationUserDTO User
) : BaseDTO;

