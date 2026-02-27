using LegendarySpork9Wiki.Models;

namespace LegendarySpork9Wiki.Services
{
    public class TemplateService
    {
        private readonly string _templatesPath;

        public TemplateService(SharedSettingsModel settings, IWebHostEnvironment env)
        {
            _templatesPath = Path.IsPathRooted(settings.TemplatesLocation)
                ? settings.TemplatesLocation
                : Path.Combine(env.ContentRootPath, settings.TemplatesLocation);
        }

        public List<string> GetTemplateNames()
        {
            if (!Directory.Exists(_templatesPath))
            {
                return new List<string>();
            }

            return Directory.GetFiles(_templatesPath, "*.html")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .OrderBy(n => n)
                .ToList();
        }

        public async Task<string> GetTemplateContent(string name)
        {
            string filePath = Path.Combine(_templatesPath, $"{name}.html");

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            return await File.ReadAllTextAsync(filePath);
        }
    }
}
