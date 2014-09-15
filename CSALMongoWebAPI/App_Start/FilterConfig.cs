using System.Web.Mvc;

using System.Diagnostics.CodeAnalysis;

namespace CSALMongoWebAPI {
    [ExcludeFromCodeCoverage]
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }
    }
}