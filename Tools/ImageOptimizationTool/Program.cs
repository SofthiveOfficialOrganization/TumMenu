using ImageMagick;
using Microsoft.Data.SqlClient;

const uint Quality = 75;

var options = Options.Parse(args);
var webRoot = ResolveWebRoot(options.WebRoot);
var execute = options.Execute;

Console.WriteLine(execute ? "EXECUTE mode" : "DRY-RUN mode");
Console.WriteLine($"WebRoot: {webRoot}");
Console.WriteLine();

var uploadFiles = EnumerateRasterFiles(Path.Combine(webRoot, "uploads")).ToList();
var staticFiles = EnumerateRasterFiles(Path.Combine(webRoot, "images"))
    .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
    .ToList();

Console.WriteLine($"Uploads found: {uploadFiles.Count}");
Console.WriteLine($"Static images found: {staticFiles.Count}");
Console.WriteLine();

SqlConnection? connection = null;
if (execute && !string.IsNullOrWhiteSpace(options.ConnectionString))
{
    connection = new SqlConnection(options.ConnectionString);
    await connection.OpenAsync();
}
else if (execute && uploadFiles.Count > 0)
{
    Console.WriteLine("WARNING: --connection-string verilmedi; uploads dosyaları dönüştürülecek ama Media tablosu güncellenmeyecek.");
}

try
{
    foreach (var sourcePath in uploadFiles)
    {
        await ProcessFileAsync(sourcePath, webRoot, updateDatabase: true, connection, execute, options.Cleanup);
    }

    foreach (var sourcePath in staticFiles)
    {
        await ProcessFileAsync(sourcePath, webRoot, updateDatabase: false, connection: null, execute, options.Cleanup);
    }
}
finally
{
    if (connection is not null)
        await connection.DisposeAsync();
}

Console.WriteLine();
Console.WriteLine("Done.");

static async Task ProcessFileAsync(
    string sourcePath,
    string webRoot,
    bool updateDatabase,
    SqlConnection? connection,
    bool execute,
    bool cleanup)
{
    var targetPath = GetTargetPath(sourcePath);
    var oldUrl = ToRelativeUrl(sourcePath, webRoot);
    var newUrl = ToRelativeUrl(targetPath, webRoot);
    var oldSize = new FileInfo(sourcePath).Length;

    Console.WriteLine($"{oldUrl} ({oldSize:N0} bytes) -> {newUrl}");

    if (!execute)
        return;

    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
    var info = OptimizeToWebp(sourcePath, targetPath);

    if (updateDatabase && connection is not null)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Medias
            SET MediaUrl = @NewUrl,
                Width = @Width,
                Height = @Height,
                FileSize = @FileSize,
                Extension = '.webp',
                MimeType = 'image/webp'
            WHERE MediaUrl = @OldUrl;
            """;
        command.Parameters.AddWithValue("@OldUrl", oldUrl);
        command.Parameters.AddWithValue("@NewUrl", newUrl);
        command.Parameters.AddWithValue("@Width", info.Width);
        command.Parameters.AddWithValue("@Height", info.Height);
        command.Parameters.AddWithValue("@FileSize", info.FileSize);
        var affected = await command.ExecuteNonQueryAsync();
        Console.WriteLine($"  DB rows updated: {affected}");
    }

    if (cleanup && !string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
    {
        File.Delete(sourcePath);
        Console.WriteLine("  Source deleted.");
    }

    Console.WriteLine($"  New size: {info.FileSize:N0} bytes");
}

static OptimizedImageInfo OptimizeToWebp(string sourcePath, string targetPath)
{
    var tempPath = $"{targetPath}.{Guid.NewGuid():N}.tmp";
    using (var image = new MagickImage(sourcePath))
    {
        image.AutoOrient();
        image.Strip();
        image.Format = MagickFormat.WebP;
        image.Quality = Quality;
        image.Write(tempPath);
        File.Move(tempPath, targetPath, overwrite: true);
        var fileSize = new FileInfo(targetPath).Length;
        return new OptimizedImageInfo((int)image.Width, (int)image.Height, fileSize);
    }
}

static IEnumerable<string> EnumerateRasterFiles(string root)
{
    if (!Directory.Exists(root))
        yield break;

    foreach (var path in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
    {
        var extension = Path.GetExtension(path);
        if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            yield return path;
        }
    }
}

static string GetTargetPath(string sourcePath)
{
    var extension = Path.GetExtension(sourcePath);
    if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        return Path.Combine(Path.GetDirectoryName(sourcePath)!, $"{Path.GetFileNameWithoutExtension(sourcePath)}.optimized.webp");

    return Path.ChangeExtension(sourcePath, ".webp");
}

static string ToRelativeUrl(string path, string webRoot)
{
    var relativePath = Path.GetRelativePath(webRoot, path);
    return "/" + relativePath.Replace(Path.DirectorySeparatorChar, '/');
}

static string ResolveWebRoot(string? configured)
{
    if (!string.IsNullOrWhiteSpace(configured))
        return Path.GetFullPath(configured);

    var candidate = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "WebUI", "wwwroot"));
    if (Directory.Exists(candidate))
        return candidate;

    candidate = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "WebUI", "wwwroot"));
    if (Directory.Exists(candidate))
        return candidate;

    throw new DirectoryNotFoundException("WebUI/wwwroot bulunamadı. --webroot ile belirtin.");
}

internal sealed record OptimizedImageInfo(int Width, int Height, long FileSize);

internal sealed record Options(string? WebRoot, string? ConnectionString, bool Execute, bool Cleanup)
{
    public static Options Parse(string[] args)
    {
        string? webRoot = null;
        string? connectionString = null;
        var execute = false;
        var cleanup = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--webroot":
                    webRoot = GetValue(args, ref i);
                    break;
                case "--connection-string":
                    connectionString = GetValue(args, ref i);
                    break;
                case "--execute":
                    execute = true;
                    break;
                case "--cleanup":
                    cleanup = true;
                    break;
            }
        }

        return new Options(webRoot, connectionString, execute, cleanup);
    }

    private static string GetValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException($"{args[index]} için değer bekleniyor.");

        index++;
        return args[index];
    }
}
