using RazorLight;

namespace CallAuditPortal1.Service.Helper
{
    public class RazorViewRenderer
    {

        private readonly RazorLightEngine _engine;


        public RazorViewRenderer()

        {
            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(
                    Path.Combine(Directory.GetCurrentDirectory(), "View"))
                .UseMemoryCachingProvider()
                .Build();
        }


        public async Task<string> RenderAsync<T>(string viewPath, T model)

        {

            return await _engine.CompileRenderAsync(viewPath, model);

        }

    }
}
