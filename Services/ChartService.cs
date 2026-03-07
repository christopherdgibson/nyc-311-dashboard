using CSharpFunctionalExtensions;
using Microsoft.JSInterop;
using NYC311Dashboard.Extensions;
using NYC311Dashboard.Services.Contracts;
using NYC311Dashboard.Services.Models;
using System.Data;

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
        public ChartOptions? ChartByPrecinct { get; private set; }

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

                var dataset = new ApexDataSet();
                if (options == null)
                {
                    if (!(_requestService.SelectedBoroughs?.Count > 0))
                    {
                        _messagingService.ShowError(string.Format(Resources.empty_selction, Resources.groupby_category_boroughs));
                        return;
                    }

                    var boroughs = _requestService.SelectedBoroughs
                        .Where(b => _requestService.RequestsByBoroughDate
                            .Any(r => r.Borough.Equals(b, StringComparison.OrdinalIgnoreCase)))
                        .OrderBy(p => p)
                        .ToList();

                    dataset = new ApexDataSet
                    {
                        Categories = boroughs,
                        Series = new List<ApexSeries>
                        {
                            new ApexSeries
                            {
                                Name = Resources.chart_name_total_count,
                                Data = boroughs
                                    .Select(b => (double)_requestService.Requests
                                        .Count(r => r.Borough.Equals(b, StringComparison.OrdinalIgnoreCase)))
                                    .ToList()
                            },
                            new ApexSeries
                            {
                                Name = Resources.chart_name_total_duration,
                                Data = boroughs
                                    .Select(b => _requestService.RequestsByBoroughDate
                                        .Where(r => r.Borough.Equals(b, StringComparison.OrdinalIgnoreCase))
                                        .Sum(r => r.Duration))
                                    .ToList()
                            }
                        }

                    };

                    var result = GetChartOptions(Resources.groupby_category_boroughs, height: "380");

                    options = result.IsSuccess ? result.Value : BarChartByBorough;

                    if (options == null)
                    {
                        _messagingService.ShowError(Resources.failed_chart_options);
                        return;
                    }
                    else if (options.DataLabels == null)
                    {
                        options.DataLabels = new DataLabels
                        {
                            Enabled = true
                        };
                    }
                    else
                    {
                        options.DataLabels.Enabled = true;
                    }
                }

                BarChartByBorough = options;

                var error = await _js.InvokeAsync<string?>("renderApexChart", elementSelector, dataset, options);
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
            _loadingService.LoadingMessage = Resources.loading_service_loading_here;
            _loadingService.IsLoading = true;

            try
            {
                var dataset = new ApexDataSet();
                if (options == null)
                {
                    if (!(_requestService.SelectedBoroughs?.Count > 0))
                    {
                        _messagingService.ShowError(string.Format(Resources.empty_selction, Resources.groupby_category_boroughs));
                        return;
                    }

                    if (!(_requestService.SelectedZipCodes?.Count > 0))
                    {
                        _messagingService.ShowError(string.Format(Resources.empty_selction, Resources.groupby_category_zip_codes));
                        return;
                    }

                    var categories = _requestService.RequestsByZipHour
                    .Select(r => r.CreatedDate.ToDateTimeHour())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                    dataset = new ApexDataSet
                    {
                        Categories = categories,

                        Series = _requestService.SelectedBoroughs
                        .Where(borough => _requestService.RequestsByZipHour.Any(r => r.Borough.Equals(borough, StringComparison.OrdinalIgnoreCase) && _requestService.SelectedZipCodes.Contains(r.Zip)))
                        .Select(borough => new ApexSeries
                        {
                            Name = borough,
                            Data = categories.Select(cat =>
                                _requestService.RequestsByZipHour
                                    .Where(r => r.Borough.Equals(borough, StringComparison.OrdinalIgnoreCase) && _requestService.SelectedZipCodes.Contains(r.Zip) && r.CreatedDate.ToDateTimeHour() == cat)
                                    .Sum(r => r.Duration)).
                            ToList()
                        })
                        .OrderBy(s => s.Name)
                        .ToList()
                    };

                    var result = GetChartOptions(Resources.groupby_category_zip_codes, height: "380");

                    options = result.IsSuccess ? result.Value : LineChartByZipHour;

                    if (options == null)
                    {
                        _messagingService.ShowError(Resources.failed_chart_options);
                        return;
                    }
                }

                LineChartByZipHour = options;

                var error = await _js.InvokeAsync<string?>("renderApexChartMulti", elementSelector, dataset, options);
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

        public async Task RenderPrecinctChart(string elementSelector, ChartOptions? options = null)
        {
            _loadingService.LoadingMessage = Resources.loading_service_loading_here;
            _loadingService.IsLoading = true;

            try
            {
                var dataGroups = new List<ApexDataGroup>();
                if (options == null)
                {
                    if (!(_requestService.SelectedBoroughs?.Count > 0))
                    {
                        _messagingService.ShowError(string.Format(Resources.empty_selction, Resources.groupby_category_boroughs));
                        return;
                    }

                    if (!(_requestService.SelectedPrecincts?.Count > 0))
                    {
                        _messagingService.ShowError(string.Format(Resources.empty_selction, Resources.groupby_category_precinct));
                        return;
                    }

                    dataGroups = _requestService.SelectedBoroughs
                        .OrderBy(b => b)
                        .Select(borough =>
                        {
                            var boroughPrecincts = _requestService.SelectedPrecincts
                                .Where(p => _requestService.RequestsByPrecinct
                                    .Any(r => r.Precinct.Equals(p, StringComparison.OrdinalIgnoreCase)
                                           && r.Borough.Equals(borough, StringComparison.OrdinalIgnoreCase)))
                                .OrderBy(p => p)
                                .ToList();

                            return new ApexDataGroup
                            {
                                GroupKey = borough,
                                Dataset = new ApexDataSet
                                {
                                    Categories = boroughPrecincts,
                                    Series = new List<ApexSeries>
                                    {
                                    new ApexSeries
                                    {
                                        Name = Resources.chart_name_total_count,
                                        Data = boroughPrecincts
                                            .Select(p => (double)_requestService.Requests
                                                .Count(r => r.PolicePrecinct.Equals(p, StringComparison.OrdinalIgnoreCase)))
                                            .ToList()
                                    },
                                    new ApexSeries
                                    {
                                        Name = Resources.chart_name_total_duration,
                                        Data = boroughPrecincts
                                            .Select(p => _requestService.RequestsByPrecinct
                                                .Where(r => r.Precinct.Equals(p, StringComparison.OrdinalIgnoreCase))
                                                .Sum(r => r.Duration))
                                            .ToList()
                                    }
                                    }
                                }
                            };
                        }).ToList();

                    var result = GetChartOptions(Resources.groupby_category_precinct, height: "380");

                    options = result.IsSuccess ? result.Value : ChartByPrecinct;

                    if (options == null)
                    {
                        _messagingService.ShowError(Resources.failed_chart_options);
                        return;
                    }
                }

                ChartByPrecinct = options;

                // Check here for empty dataGroups?

                foreach (var group in dataGroups)
                {
                    var divId = group.GroupKey == null ? elementSelector : string.Join("-", elementSelector, group.GroupKey.ToKebabCase());
                    var dataset = group.Dataset;
                    var error = await _js.InvokeAsync<string?>("renderApexChart", divId, dataset, options);
                    if (error != null)
                    {
                        _messagingService.ShowError(error);
                    }
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

        public Result<ChartOptions> GetChartOptions(string selection, string? width = null, string? height = null)
        {
            _loadingService.LoadingMessage = Resources.loading_service_loading_here;
            _loadingService.IsLoading = true;
            try
            {
                string type = "bar";
                if (selection == Resources.groupby_category_zip_codes)
                {
                    type = "line";
                }

                //if (!(categories?.Count > 0))
                //{
                //    _messagingService.ShowInfo(string.Format(Resources.empty_selction, selection));
                //    _loadingService.IsLoading = false;
                //    return Result.Failure<ChartOptions>(string.Format(Resources.empty_selction, Resources.groupby_category_boroughs));
                //}

                var options = new ChartOptions
                {
                    Chart = new Chart
                    {
                        Type = type,
                        Width = width,
                        Height = height
                    }
                };

                if (selection == Resources.groupby_category_boroughs)
                {
                    BarChartByBorough = options;
                }
                if (selection == Resources.groupby_category_precinct)
                {
                    ChartByPrecinct = options;
                }
                if (selection == Resources.groupby_category_zip_codes)
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
