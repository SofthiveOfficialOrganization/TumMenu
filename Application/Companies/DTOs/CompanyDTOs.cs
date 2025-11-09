using Application.Common.Base.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Application.Companies.DTOs;

public sealed record CompanyLiteDTO(
	string Name,
	string Slug
) : BaseDTO, ISluggableDTO;
