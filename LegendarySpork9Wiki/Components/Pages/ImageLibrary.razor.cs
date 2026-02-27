using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Pages
{
    public partial class ImageLibrary : IDisposable
    {
        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        [Inject]
        private SharedSettingsModel Settings { get; set; } = default!;

        [Inject]
        private LoggerService Logger { get; set; } = default!;

        private APIImagesModel? _images;
        private int _currentPage = 1;
        private bool _loading = true;

        private IBrowserFile? _selectedFile;
        private bool _uploading;

        private string? _confirmDeleteId;
        private bool _deleting;

        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private string _acceptFormats = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            User.OnDarkModeChanged += StateHasChanged;

            _acceptFormats = string.Join(",", Settings.AllowedImageFormats
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

            if (User.IsLoggedIn && User.Admin)
            {
                await LoadImages();
            }
        }

        private async Task LoadImages()
        {
            _loading = true;
            StateHasChanged();

            try
            {
                _images = await APIService.GetImages(_currentPage);
            }
            catch
            {
                _errorMessage = "Failed to load images.";
            }
            finally
            {
                _loading = false;
                StateHasChanged();
            }
        }

        private async Task LoadPage(int pageNumber)
        {
            _currentPage = pageNumber;
            await LoadImages();
        }

        private void OnFileSelected(InputFileChangeEventArgs e)
        {
            _errorMessage = string.Empty;
            _successMessage = string.Empty;

            var file = e.File;
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            var allowedFormats = Settings.AllowedImageFormats
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(f => f.ToLowerInvariant())
                .ToList();

            if (!allowedFormats.Contains(extension))
            {
                _errorMessage = $"File type '{extension}' is not allowed. Allowed formats: {Settings.AllowedImageFormats}";
                _selectedFile = null;
                return;
            }

            _selectedFile = file;
        }

        private async Task HandleUpload()
        {
            if (_selectedFile == null) return;

            _errorMessage = string.Empty;
            _successMessage = string.Empty;
            _uploading = true;
            StateHasChanged();

            try
            {
                var fileName = Path.GetFileNameWithoutExtension(_selectedFile.Name);
                var extension = Path.GetExtension(_selectedFile.Name).ToLowerInvariant();
                var fileSize = _selectedFile.Size;

                string savedFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                string url = savedFileName;

                if (!string.IsNullOrEmpty(Settings.ImageStoragePath))
                {
                    var storagePath = Settings.ImageStoragePath;

                    if (!Directory.Exists(storagePath))
                    {
                        Directory.CreateDirectory(storagePath);
                    }

                    var filePath = Path.Combine(storagePath, savedFileName);

                    await using var stream = _selectedFile.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
                    await using var fileStream = new FileStream(filePath, FileMode.Create);
                    await stream.CopyToAsync(fileStream);
                }

                var result = await APIService.RegisterImage(fileName, extension, fileSize, url);

                if (result != null)
                {
                    _successMessage = $"Image '{_selectedFile.Name}' uploaded successfully.";
                    _selectedFile = null;
                    _currentPage = 1;
                    await LoadImages();
                }
                else
                {
                    _errorMessage = "Failed to register the image. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Image upload failed", ex);
                _errorMessage = "An error occurred while uploading the image.";
            }
            finally
            {
                _uploading = false;
                StateHasChanged();
            }
        }

        private void RequestDelete(string imageId)
        {
            _confirmDeleteId = imageId;
        }

        private void CancelDelete()
        {
            _confirmDeleteId = null;
        }

        private async Task ConfirmDelete(string imageId)
        {
            _errorMessage = string.Empty;
            _successMessage = string.Empty;
            _deleting = true;
            StateHasChanged();

            try
            {
                var image = _images?.Images.FirstOrDefault(i => i.Id == imageId);

                var apiResult = await APIService.DeleteImage(imageId);

                if (apiResult)
                {
                    if (!string.IsNullOrEmpty(Settings.ImageStoragePath) && image != null)
                    {
                        var filePath = Path.Combine(Settings.ImageStoragePath, $"{image.FileName}{image.Extension}");

                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }

                    _successMessage = "Image deleted successfully.";
                    _confirmDeleteId = null;
                    await LoadImages();
                }
                else
                {
                    _errorMessage = "Failed to delete the image. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Image delete failed", ex);
                _errorMessage = "An error occurred while deleting the image.";
            }
            finally
            {
                _deleting = false;
                StateHasChanged();
            }
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1048576)
            {
                return $"{bytes / 1048576.0:F1} MB";
            }

            if (bytes >= 1024)
            {
                return $"{bytes / 1024.0:F1} KB";
            }

            return $"{bytes} B";
        }

        public void Dispose()
        {
            User.OnDarkModeChanged -= StateHasChanged;
        }
    }
}
