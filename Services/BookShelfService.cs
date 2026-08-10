using Projects.Config.Db;
using Projects.Models;
using Projects.Services.Base;
using Microsoft.Extensions.Options;

namespace Projects.Services
{
    public class BookShelfService : BaseService<BookShelf>
    {
        public BookShelfService(
            IOptions<DbSettings> dbSettings,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor
        )
            : base(DbCollections.BookShelfs, dbSettings, serviceProvider, httpContextAccessor) { }
    }
}
