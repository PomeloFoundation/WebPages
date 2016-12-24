namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseWebPages(this IApplicationBuilder self)
        {
            return self.UseMvc();
        }
    }
}
