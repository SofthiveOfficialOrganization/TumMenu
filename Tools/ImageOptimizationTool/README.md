# ImageOptimizationTool

Dry-run varsayılandır; dosya ya da DB değiştirmez.

```powershell
dotnet run --project Tools\ImageOptimizationTool\ImageOptimizationTool.csproj -- --webroot WebUI\wwwroot
```

Execute modunda uploads altındaki JPG/PNG/WEBP dosyaları WebP kopyasına dönüştürür ve connection string verilirse `Medias` tablosundaki URL/metadata alanlarını günceller. Eski dosyaları silmez.

```powershell
dotnet run --project Tools\ImageOptimizationTool\ImageOptimizationTool.csproj -- --webroot WebUI\wwwroot --connection-string "<connection-string>" --execute
```

Eski dosyaların silinmesi ayrı ve bilinçli bir adımdır:

```powershell
dotnet run --project Tools\ImageOptimizationTool\ImageOptimizationTool.csproj -- --webroot WebUI\wwwroot --connection-string "<connection-string>" --execute --cleanup
```
