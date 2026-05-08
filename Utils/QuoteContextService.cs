using MOZ_UPGRADE.Interfaces;

namespace MOZ_UPGRADE.Utils
{
    public class QuoteContextService : IQuoteContextService
    {
        private QuoteContext? _context;

        public void SetQuote(QuoteContext context)
        {
            _context = context;
        }

        public QuoteContext? GetQuote()
        {
            return _context;
        }

        public void Clear()
        {
            _context = null;
        }
    }
}
