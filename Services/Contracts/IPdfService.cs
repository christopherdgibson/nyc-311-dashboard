using NYC311Dashboard.Services.Models;

namespace NYC311Dashboard.Services.Contracts
{
    public interface IPdfService
    {
        Task RenderPdfDownload(string elementId, PdfOptions options);

        PdfOptions GetPdfOptions();
    }
}
