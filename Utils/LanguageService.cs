using System;
using System.Globalization;
using System.Threading.Tasks;

namespace MOZ_UPGRADE.Utils
{
    public class LanguageService
    {
        private string _current = "pt"; // default Portuguese
        public string CurrentLang
        {
            get => _current;
            private set
            {
                if (_current == value) return;
                _current = value;
                try
                {
                    var culture = _current == "pt" ? new CultureInfo("pt-PT") : new CultureInfo("en-US");
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;
                }
                catch { }
                OnChange?.Invoke();
            }
        }

        public event Action OnChange;

        public void SetLang(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang)) lang = "pt";
            lang = lang.ToLowerInvariant();
            if (lang != "en" && lang != "pt") lang = "pt";
            CurrentLang = lang;
        }

        public string T(string en, string pt) => CurrentLang == "pt" ? pt : en;
    }
}
