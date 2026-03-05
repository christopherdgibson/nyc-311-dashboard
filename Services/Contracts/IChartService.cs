using CSharpFunctionalExtensions;
using NYC311Dashboard.Services.Models;

namespace NYC311Dashboard.Services.Contracts
{
    public interface IChartService
    {
        ChartOptions? BarChartByBorough { get; }
        ChartOptions? LineChartByZipHour { get; }

        Result<ChartOptions> GetChartOptions(string selection, List<string> categories, List<ApexSeries> series, string? width = null, string? height = null);

        Task RenderBarChart(string elementSelector, ChartOptions? options = null);

        Task RenderLineChart(string elementSelector, ChartOptions? options = null);

        Task RenderPrecinctChart(string elementSelector, ChartOptions? options = null);

        Task UpdateApexChart(ChartOptions options);

        Task DisposeApexChart();
    }
}
