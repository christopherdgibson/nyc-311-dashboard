using Microsoft.JSInterop;
using NYC311Dashboard.Intrastructure.Contracts;
using NYC311Dashboard.Services.Contracts;
using NYC311Dashboard.Services.Models;

namespace NYC311Dashboard.Services
{
    public class PdfService : IPdfService
    {
        private readonly IJSRuntime _js;
        private readonly ILoadingService _loadingService;
        private readonly IMessagingService _messagingService;

        public PdfService(IJSRuntime js, ILoadingService loadingService, IMessagingService messagingService)
        {
            _js = js;
            _loadingService = loadingService;
            _messagingService = messagingService;
        }

        public async Task RenderPdfDownload(string elementId, PdfOptions options)
        {
            _loadingService.LoadingMessage = Resources.loading_service_loading_here;
            _loadingService.IsLoading = true;
            await Task.Yield();
            try
            {
                await _js.InvokeVoidAsync("scrollToTop", "instant");
                await _js.InvokeVoidAsync("saveElementAsPdf", "pdf-content", options);
            }
            catch
            {
                _messagingService.ShowError(Resources.messaging_service_error_occurred);
                //return Result.Failure(Resources.messaging_service_error_occurred);
            }
            finally
            {
                _loadingService.IsLoading = false;
            }
        }

        public PdfOptions GetPdfOptions() => new PdfOptions
        {
            Margin = [10, 10, 10, 10],
            FileName = "custom.pdf",
            Image = new ImageOptions
            {
                Type = "jpeg",
                Quality = 0.98
            },
            Html2Canvas = new Html2CanvasOptions
            {
                Scale = 2
            },
            JsPDF = new JsPdfOptions
            {
                Unit = "mm",
                Format = "a4",
                Orientation = "portrait"
            },
            PageBreak = new PageBreak
            {
                Mode = ["css"],
                Avoid = ["tr", "td", "h2", "h3", "h4"]
            }
        };
    }
}
