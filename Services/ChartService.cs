using CSharpFunctionalExtensions;
using Microsoft.JSInterop;
using NYC311Dashboard.Extensions;
using NYC311Dashboard.Services.Contracts;
using NYC311Dashboard.Services.Models;

namespace NYC311Dashboard.Services
{
    public class ChartService : IChartService
    {
        private readonly IJSRuntime _js;
        private readonly IRequestService _requestService;
        private readonly ILoadingService _loadingService;
        private readonly IMessagingService _messagingService;
        public ChartOptions? BarChartByBorough { get; private set; }
        public ChartOptions? LineChartByZipHour { get; private set; }

        public ChartService(IJSRuntime js, IRequestService requestService, ILoadingService loadingService, IMessagingService messagingService)
        {
            _js = js;
            _requestService = requestService;
            _loadingService = loadingService;
            _messagingService = messagingService;
        }

        public async Task RenderBarChart(string elementSelector, ChartOptions? options = null)
        {
            try
            {
                _loadingService.LoadingMessage = Resources.loading_service_loading_here;
                _loadingService.IsLoading = true;

                if (options == null)
                {
                    if (!(_requestService.SelectedBoroughs?.Count > 0))
                    {
                        _messagingService.ShowError(string.Format(Resources.empty_selction, Resources.groupy_category_boroughs));
                        return;
                    }

                    var categories = _requestService.SelectedBoroughs.ToList();

                    var totalDurations = _requestService.SelectedBoroughs
                        .Select(b => _requestService.RequestsByBoroughDate.Where(r => r.Borough.Equals(b, StringComparison.OrdinalIgnoreCase)).Sum(r => r.Duration))
                        .ToList();

                    var totalCounts = _requestService.SelectedBoroughs
                        .Select(b => (double)_requestService.Requests.Count(r => r.Borough.Equals(b, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    var series = new List<ApexSeries>
                    {
                        new ApexSeries { Name = Resources.chart_name_total_count, Data = totalCounts },
                        new ApexSeries { Name = Resources.chart_name_total_duration, Data = totalDurations }
                    };

                    var result = GetChartOptions(Resources.groupy_category_boroughs, categories, series, height: "380");

                    options = result.IsSuccess ? result.Value : BarChartByBorough;
                }

                BarChartByBorough = options;

                var error = await _js.InvokeAsync<string?>("renderApexBarChart", elementSelector, options);
                if (error != null)
                {
                    _messagingService.ShowError(error);
                }

            }
            catch
            {
                _messagingService.ShowError(Resources.failed_to_render_chart); //todo: dialog box with error message and retry button?
            }
            finally
            {
                _loadingService.IsLoading = false;
            }
        }

        public async Task RenderLineChart(string elementSelector, ChartOptions? options = null)
        {
            try
            {
                _loadingService.LoadingMessage = Resources.loading_service_loading_here;
                _loadingService.IsLoading = true;

                if (options == null)
                {
                    if (!(_requestService.SelectedBoroughs?.Count > 0))
                    {
                        _messagingService.ShowError(string.Format(Resources.empty_selction, Resources.groupy_category_boroughs));
                        return;
                    }

                    if (!(_requestService.SelectedZipCodes?.Count > 0))
                    {
                        _messagingService.ShowError(string.Format(Resources.empty_selction, Resources.groupy_category_zip_codes));
                        return;
                    }

                    var categories = _requestService.RequestsByZipHour
                    .Select(r => r.CreatedDate.ToDateTimeHour())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                    var series = _requestService.SelectedBoroughs
                        .Where(borough => _requestService.RequestsByZipHour.Any(r => r.Borough.Equals(borough, StringComparison.OrdinalIgnoreCase) && _requestService.SelectedZipCodes.Contains(r.Zip)))
                        .Select(borough => new ApexSeries
                        {
                            Name = borough,
                            Data = categories.Select(cat =>
                                _requestService.RequestsByZipHour
                                    .Where(r => r.Borough.Equals(borough, StringComparison.OrdinalIgnoreCase) && _requestService.SelectedZipCodes.Contains(r.Zip) && r.CreatedDate.ToDateTimeHour() == cat)
                                    .Sum(r => r.Duration)
                            ).ToList()
                        })
                        .ToList();

                    var result = GetChartOptions(Resources.groupy_category_zip_codes, categories, series, height: "380");

                    options = result.IsSuccess ? result.Value : LineChartByZipHour;
                }

                LineChartByZipHour = options;

                var error = await _js.InvokeAsync<string?>("renderApexChartMulti", elementSelector, options);
                if (error != null)
                {
                    _messagingService.ShowError(error);
                }
            }
            catch
            {
                _messagingService.ShowError(Resources.failed_to_render_chart);
            }
            finally
            {
                _loadingService.IsLoading = false;
            }
        }

        public async Task UpdateApexChart(ChartOptions options)
        {
            await _js.InvokeVoidAsync("updateApexChart", options);
        }

        public Result<ChartOptions> GetChartOptions(string selection, List<string> categories, List<ApexSeries> series, string? width = null, string? height = null)
        {
            try
            {
                _loadingService.LoadingMessage = Resources.loading_service_loading_here;
                _loadingService.IsLoading = true;
                string type;
                if (selection == Resources.groupy_category_boroughs)
                {
                    type = "bar";
                }
                else
                {
                    type = "line";
                }

                if (!categories.Any())
                {
                    _messagingService.ShowInfo(string.Format(Resources.empty_selction, selection));
                    _loadingService.IsLoading = false;
                    return Result.Failure<ChartOptions>(string.Format(Resources.empty_selction, Resources.groupy_category_boroughs));
                }

                categories.Sort();
                var options = new ChartOptions
                {
                    Chart = new Chart { Type = type },
                    XAxis = new XAxis { Categories = categories },
                    Series = series.OrderBy(r => r.Name).ToList(),
                    Width = width,
                    Height = height
                };

                if (type == "bar")
                {
                    BarChartByBorough = options;
                }
                if (type == "line")
                {
                    LineChartByZipHour = options;
                }

                //_messagingService.Clear();
                return Result.Success(options);
            }
            catch
            {
                _messagingService.ShowError(Resources.failed_chart_options);
                return Result.Failure<ChartOptions>(Resources.failed_chart_options);
            }
            finally
            {
                _loadingService.IsLoading = false;
            }
        }

        public async Task DisposeApexChart()
        {
            await _js.InvokeVoidAsync("disposeApexChart");
        }
    }
}
