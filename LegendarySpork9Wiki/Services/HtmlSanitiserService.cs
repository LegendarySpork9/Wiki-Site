using Ganss.Xss;

namespace LegendarySpork9Wiki.Services
{
    public class HtmlSanitiserService
    {
        private readonly HtmlSanitizer _sanitizer;

        public HtmlSanitiserService()
        {
            _sanitizer = new HtmlSanitizer();
        }

        /// <summary>
        /// Sanitises the given HTML string, removing dangerous elements and attributes.
        /// </summary>
        public string Sanitise(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            return _sanitizer.Sanitize(html);
        }
    }
}
