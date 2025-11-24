using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Companies.DTOs;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Queries;

public record GetCompanyQuery() : IRequest<CompanyDTO>;

public class GetCompanyHandler(
    IRepository<Domain.Entities.Company> repoCompany,
    IMapper mapper
) : IRequestHandler<GetCompanyQuery, CompanyDTO>
{
    public async Task<CompanyDTO> Handle(GetCompanyQuery req, CancellationToken ct)
    {
        var company = await repoCompany.Query()
            .Include(c => c.BaseMenu)
            .Include(c => c.Subscription)
            .Include(c => c.PaymentMethods)
            .Include(c => c.Stores)
            .FirstOrDefaultAsync(ct);
        if(company == null)
            throw new NotFoundAppException("Şirket bulunamadı");
        var companyDTO = mapper.Map<CompanyDTO>(company);
        return companyDTO;
    }
}
