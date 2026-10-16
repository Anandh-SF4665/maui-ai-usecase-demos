namespace NutriLens.Services;

/// <summary>
/// Captures or picks an image of a food label and copies it into the app cache
/// so the file outlives MAUI's transient picker temp directory.
/// </summary>
public interface IImagePickerService
{
    Task<FileResult?> CapturePhotoAsync(CancellationToken cancellationToken = default);
    Task<FileResult?> PickPhotoAsync(CancellationToken cancellationToken = default);
}

public sealed class ImagePickerService : IImagePickerService
{
    public async Task<FileResult?> CapturePhotoAsync(
        CancellationToken cancellationToken = default)
    {
        // Unpackaged Windows apps (WindowsPackageType=None) cannot launch the
        // WinRT CameraCaptureUI because it requires package identity. Fall back
        // to the gallery picker on WinUI.
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            return await PickPhotoAsync(cancellationToken);

        if (!MediaPicker.Default.IsCaptureSupported)
            return await PickPhotoAsync(cancellationToken);

        var photo = await MediaPicker.Default.CapturePhotoAsync(
            new MediaPickerOptions { Title = "Capture food label" });

        return await CopyToAppCacheAsync(photo);
    }

    public async Task<FileResult?> PickPhotoAsync(
        CancellationToken cancellationToken = default)
    {
        var photo = await MediaPicker.Default.PickPhotoAsync(
            new MediaPickerOptions { Title = "Select food label" });

        return await CopyToAppCacheAsync(photo);
    }

    // MAUI deletes the temp capture files once the picker closes. Copy to a
    // path we own so downstream pages can still open the FileResult.
    private static async Task<FileResult?> CopyToAppCacheAsync(FileResult? file)
    {
        if (file is null)
            return null;

        var targetDir = Path.Combine(
            FileSystem.CacheDirectory, "captures");
        Directory.CreateDirectory(targetDir);

        var target = Path.Combine(
            targetDir,
            $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}");

        await using var source = await file.OpenReadAsync();
        await using var destination = File.Create(target);
        await source.CopyToAsync(destination);

        return new FileResult(target);
    }
}