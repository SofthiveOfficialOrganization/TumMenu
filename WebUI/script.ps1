
$controllersDir = "d:\SoftHive\Projects\2025\TumMenu\WebUI\Areas\Admin\Controllers"

$map = @{
    "StoreController.cs" = "[Route(`"{role:regex(admin)}/all-stores`")]`r`n[Route(`"{role:regex(owner)}/mystores`")]"
    "CompanyController.cs" = "[Route(`"{role:regex(admin)}/all-companies`")]`r`n[Route(`"{role:regex(owner)}/mycompany`")]"
    "ProductController.cs" = "[Route(`"{role:regex(admin)}/all-products`")]`r`n[Route(`"{role:regex(owner)}/myproducts`")]"
    "CategoryController.cs" = "[Route(`"{role:regex(admin)}/all-categories`")]`r`n[Route(`"{role:regex(owner)}/mycategories`")]"
    "MenuController.cs" = "[Route(`"{role:regex(admin)}/all-menus`")]`r`n[Route(`"{role:regex(owner)}/mymenu`")]"
    "MediaController.cs" = "[Route(`"{role:regex(admin)}/all-media`")]`r`n[Route(`"{role:regex(owner)}/mymedia`")]"
    "QRManagementController.cs" = "[Route(`"{role:regex(admin)}/all-qr`")]`r`n[Route(`"{role:regex(owner)}/myqr`")]"
    "AnalysisController.cs" = "[Route(`"{role:regex(admin)}/all-analysis`")]`r`n[Route(`"{role:regex(owner)}/myanalysis`")]"
    "UserController.cs" = "[Route(`"{role:regex(admin)}/all-users`")]"
}

foreach ($key in $map.Keys) {
    $path = Join-Path $controllersDir $key
    if (Test-Path $path) {
        $text = [System.IO.File]::ReadAllText($path)
        $text = $text.Replace("[Route(`"{role:regex(admin|owner)}/[controller]`")]", $map[$key])
        [System.IO.File]::WriteAllText($path, $text)
    }
}
