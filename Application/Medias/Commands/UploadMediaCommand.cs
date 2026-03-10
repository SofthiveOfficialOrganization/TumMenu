using Application.Abstractions;
using Application.Medias.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Application.Medias.Commands;

public sealed record UploadMediaCommand(
    IFormFile File,
    Guid ReferenceId,
    MediaRefType Type,
    string? AltText = null,
    int SortOrder = 0,
    string Slot = "default-gallery"
) : IRequest<MediaDTO>, ITransactionalRequest;

public class UploadMediaCommandHandler(
    IRepository<Media> mediaRepository,
    IRepository<Store> storeRepository,
    IRepository<Menu> menuRepository,
    IRepository<Category> categoryRepository,
    IRepository<Product> productRepository,
    IRepository<QRCode> qrRepository,
    IStorageService storageService,
    IMapper mapper
    ) : IRequestHandler<UploadMediaCommand, MediaDTO>
{
    public async Task<MediaDTO> Handle(UploadMediaCommand req, CancellationToken ct)
    {
        // 1. Resolve CompanyId
        Guid? companyId = null;
        switch (req.Type)
        {
            case MediaRefType.Company:
                companyId = req.ReferenceId;
                break;
            case MediaRefType.Store:
                var store = await storeRepository.GetByIdAsync(req.ReferenceId, ct);
                companyId = store?.CompanyId;
                break;
            case MediaRefType.Menu:
                var menu = await menuRepository.GetByIdAsync(req.ReferenceId, ct);
                companyId = menu?.CompanyId;
                break;
            case MediaRefType.Category:
                var category = await categoryRepository.Query()
                    .Include(c => c.Menu)
                    .FirstOrDefaultAsync(c => c.Id == req.ReferenceId, ct);
                companyId = category?.Menu?.CompanyId;
                break;
            case MediaRefType.Product:
                var product = await productRepository.Query()
                    .Include(p => p.Category)
                    .ThenInclude(c => c.Menu)
                    .FirstOrDefaultAsync(p => p.Id == req.ReferenceId, ct);
                companyId = product?.Category?.Menu?.CompanyId;
                break;
            case MediaRefType.QRCode:
                var qr = await qrRepository.Query()
                    .Include(q => q.Store)
                    .FirstOrDefaultAsync(q => q.Id == req.ReferenceId, ct);
                companyId = qr?.Store?.CompanyId;
                break;
            case MediaRefType.CategoryLibraryItem:
                companyId = null; // System-wide library items
                break;
        }

        // 2. Upload file
        var folder = req.Type.ToString().ToLower();
        var url = await storageService.UploadAsync(req.File, folder, ct);

        // 3. Create Media entity
        var media = new Media
        {
            MediaUrl = url,
            AltText = req.AltText ?? req.File.FileName,
            SortOrder = req.SortOrder,
            ReferenceId = req.ReferenceId,
            Type = req.Type,
            CompanyId = companyId,
            Slot = req.Slot,
            Extension = Path.GetExtension(req.File.FileName),
            FileSize = req.File.Length,
            MimeType = req.File.ContentType,
            Kind = MediaKind.Image
        };

        await mediaRepository.AddAsync(media, ct);

        return mapper.Map<MediaDTO>(media);
    }
}
