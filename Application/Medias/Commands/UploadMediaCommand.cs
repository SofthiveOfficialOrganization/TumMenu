using Application.Abstractions;
using Application.Common.Interfaces;
using Application.Medias.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Application.Medias.Commands;

public class UploadMediaCommand : IRequest<MediaDTO>, ITransactionalRequest
{
    public IFormFile File { get; set; } = null!;
    public Guid ReferenceId { get; set; }
    public MediaRefType Type { get; set; }
    public string? AltText { get; set; } = null;
    public int SortOrder { get; set; } = 0;
    public string Slot { get; set; } = "default-gallery";
}

public class UploadMediaCommandHandler(
    IRepository<Media> mediaRepository,
    IRepository<Store> storeRepository,
    IRepository<Menu> menuRepository,
    IRepository<Category> categoryRepository,
    IRepository<Product> productRepository,
    IRepository<QRCode> qrRepository,
    IRepository<CategoryLibraryItem> categoryLibraryRepository,
    IRepository<Company> companyRepository,
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
                // Validate that Company exists
                var company = await companyRepository.GetByIdAsync(req.ReferenceId, ct);
                if (company == null)
                    throw new ArgumentException($"ID {req.ReferenceId} olan şirket bulunamadı");
                companyId = req.ReferenceId;
                break;
            case MediaRefType.Store:
                var store = await storeRepository.GetByIdAsync(req.ReferenceId, ct);
                if (store == null)
                    throw new ArgumentException($"ID {req.ReferenceId} olan mağaza bulunamadı");
                companyId = store?.CompanyId;
                break;
            case MediaRefType.Menu:
                var menu = await menuRepository.GetByIdAsync(req.ReferenceId, ct);
                if (menu == null)
                    throw new ArgumentException($"ID {req.ReferenceId} olan menü bulunamadı");
                companyId = menu?.CompanyId;
                break;
            case MediaRefType.Category:
                var category = await categoryRepository.Query()
                    .Include(c => c.Menu)
                    .FirstOrDefaultAsync(c => c.Id == req.ReferenceId, ct);
                if (category == null)
                    throw new ArgumentException($"ID {req.ReferenceId} olan kategori bulunamadı");
                companyId = category?.Menu?.CompanyId;
                break;
            case MediaRefType.Product:
                var product = await productRepository.Query()
                    .Include(p => p.Category)
                    .ThenInclude(c => c.Menu)
                    .FirstOrDefaultAsync(p => p.Id == req.ReferenceId, ct);
                if (product == null)
                    throw new ArgumentException($"ID {req.ReferenceId} olan ürün bulunamadı");
                companyId = product?.Category?.Menu?.CompanyId;
                break;
            case MediaRefType.QRCode:
                var qr = await qrRepository.Query()
                    .Include(q => q.Store)
                    .FirstOrDefaultAsync(q => q.Id == req.ReferenceId, ct);
                if (qr == null)
                    throw new ArgumentException($"ID {req.ReferenceId} olan QR kodu bulunamadı");
                companyId = qr?.Store?.CompanyId;
                break;
            case MediaRefType.CategoryLibraryItem:
                // Validate that CategoryLibraryItem exists
                Console.WriteLine($"DEBUG: Looking for CategoryLibraryItem with ID {req.ReferenceId}");
                var categoryLibraryItem = await categoryLibraryRepository.GetByIdAsync(req.ReferenceId, ct);
                if (categoryLibraryItem == null)
                {
                    Console.WriteLine($"HATA: ID {req.ReferenceId} olan KategoriKütüphaneÖğesi bulunamadı");
                    throw new ArgumentException($"ID {req.ReferenceId} olan KategoriKütüphaneÖğesi bulunamadı");
                }
                Console.WriteLine($"SUCCESS: CategoryLibraryItem found: {categoryLibraryItem.Title}");
                
                // CategoryLibraryItem is system-wide, no CompanyId needed
                companyId = null;
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
            ReferenceId = req.Type == MediaRefType.CategoryLibraryItem ? Guid.Empty : req.ReferenceId,
            Type = req.Type,
            CompanyId = companyId,
            Slot = req.Slot,
            Extension = Path.GetExtension(req.File.FileName),
            FileSize = req.File.Length,
            MimeType = req.File.ContentType,
            Kind = MediaKind.Image,
            CategoryLibraryItemId = req.Type == MediaRefType.CategoryLibraryItem ? req.ReferenceId : null
        };

        await mediaRepository.AddAsync(media, ct);

        return mapper.Map<MediaDTO>(media);
    }
}
