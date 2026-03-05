using CSharpFunctionalExtensions;
using NYC311Dashboard.Constants;
using NYC311Dashboard.Extensions;
using NYC311Dashboard.Intrastructure.Contracts;
using NYC311Dashboard.Models;
using NYC311Dashboard.Services.Contracts;
using NYC311Dashboard.Services.Models;
using System.Linq.Expressions;


namespace NYC311Dashboard.Services
{
    public class RequestService : IRequestService
    {
        private readonly IHttpService _httpService;
        private readonly ILoadingService _loadingService;
        private readonly IMessagingService _messagingService;
        //private readonly string baseUrl = UrlConstants.BaseUrl;
        private readonly string sampleUrl = UrlConstants.SampleUrl;
        private readonly string sampleUrlAbbr = UrlConstants.SampleUrlAbbr;
        public List<RequestModel> Requests { get; private set; } = new();

        public List<string> Boroughs { get; private set; } = new();

        public HashSet<string>? SelectedBoroughs { get; private set; } = null;
        public HashSet<string>? SelectedZipCodes { get; private set; } = null;

        public List<BoroughDateTableRow> RequestsByBoroughDate { get; private set; } = new();
        public List<ZipHourTableRow> RequestsByZipHour { get; private set; } = new();

        public List<BoroughZipSelection> BoroughZipSelections { get; private set; } = new();
        public RequestService(IHttpService httpService, ILoadingService loadingService, IMessagingService messagingService)
        {
            _httpService = httpService;
            _loadingService = loadingService;
            _messagingService = messagingService;
        }

        public async Task GetNYC311RequestsDataAsync(string? url = null)
        {
            _loadingService.LoadingMessage = Resources.loading_service_loading_here;
            _loadingService.IsLoading = true;

            try
            {
                url ??= sampleUrl;
                var result = await LoadData<RequestModel>(url);
                if (result.IsFailure)
                {
                    _messagingService.ShowError(string.Join(" ", Resources.failed_to_get_data, result.Error));
                    _loadingService.IsLoading = false;
                    return;
                }

                Requests = result.Value.Where(r => r.Status.Equals(Resources.request_status_closed, StringComparison.OrdinalIgnoreCase)).ToList();

                Boroughs = Requests
                .Where(r => !string.IsNullOrWhiteSpace(r.Borough) && !r.Borough.Equals(Resources.borough_unspecified, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Borough.ToProperCase())
                .Distinct()
                .OrderBy(b => b)
                .ToList();

                SelectedBoroughs ??= new HashSet<string>(Boroughs, StringComparer.OrdinalIgnoreCase);
                _messagingService.Clear();
            }
            catch (Exception ex)
            {
                _messagingService.ShowError(Resources.messaging_service_error_occurred + ex.Message);
            }
            finally
            {
                _loadingService.IsLoading = false;
            }
        }

        public TableOptions<T> GetTableOptions<T>(Expression<Func<T, string>>? groupBy)
        {
            var options = new TableOptions<T>
            {
                GroupBy = groupBy,
                TableStyles = "table-flex-item"
            };

            return options;
        }

        public Result GenerateTableByBoroughDay()
        {
            _loadingService.LoadingMessage = Resources.loading_service_loading_here;
            _loadingService.IsLoading = true;
            try
            {
                if (SelectedBoroughs == null || SelectedBoroughs.Count == 0) // bring this error up to page level and zip too?
                {
                    _messagingService.ShowError(string.Join(" ", string.Format(Resources.empty_selction_table, Resources.groupy_category_boroughs)));
                    return Result.Failure(string.Join(" ", string.Format(Resources.empty_selction_table, Resources.groupy_category_boroughs)));
                }

                _messagingService.Clear(); // Clear old error if no boroughs selected

                var requestsTable = Requests
                    .Where(row => row.Status.Equals(Resources.request_status_closed, StringComparison.OrdinalIgnoreCase)
                                        && SelectedBoroughs.Contains(row.Borough)
                                        && row.CreatedDate.HasValue
                                        && row.ClosedDate.HasValue)
                    .GroupBy(row => new
                    {
                        Borough = row.Borough.ToProperCase(),
                        CreatedDate = DateOnly.FromDateTime(row.CreatedDate.Value)
                    })
                    .Select(g =>
                    {
                        var aggDictionary = new BoroughDateTableRow
                        {
                            Borough = g.Key.Borough,
                            CreatedDate = g.Key.CreatedDate,
                            Count = g.Count(),
                            Duration = g.Sum(row => (row.ClosedDate.Value - row.CreatedDate.Value).TotalMinutes)
                        };

                        return aggDictionary;
                    })
                    .ToList();

                RequestsByBoroughDate = requestsTable;

                if (!requestsTable.Any())
                {
                    _messagingService.ShowInfo();
                }

                return Result.Success();
            }
            catch
            {
                _messagingService.ShowError(Resources.messaging_service_error_occurred);
                return Result.Failure(Resources.messaging_service_error_occurred);
            }
            finally
            {
                _loadingService.IsLoading = false;
            }
        }

        public Result GenerateTableByZipHour()
        {
            _loadingService.LoadingMessage = Resources.loading_service_loading_here;
            _loadingService.IsLoading = true;

            try
            {
                if (!RequestsByZipHour.Any()) // Otherwise error will persist, but check if this clears borough errors (should return before it can?)
                {
                    _messagingService.Clear();
                }
                var result = PopulateZipCodes();
                if (result.IsFailure)
                {
                    _messagingService.ShowError(result.Error);
                    return Result.Failure(result.Error);
                }

                var requestsTable = Requests
                    .Where(row => row.Status.Equals(Resources.request_status_closed, StringComparison.OrdinalIgnoreCase)
                                        && SelectedBoroughs.Contains(row.Borough.ToProperCase())
                                        && SelectedZipCodes.Contains(row.IncidentZip)
                                        && row.CreatedDate.HasValue
                                        && row.ClosedDate.HasValue)
                    .GroupBy(row => new
                    {
                        Borough = row.Borough.ToProperCase(),
                        Zip = row.IncidentZip,
                        CreatedDate = row.CreatedDate.TruncateToHour()
                    })
                    .Select(g =>
                    {
                        var aggDictionary = new ZipHourTableRow
                        {
                            Borough = g.Key.Borough,
                            Zip = g.Key.Zip,
                            CreatedDate = g.Key.CreatedDate,
                            Count = g.Count(),
                            Duration = g.Sum(row => (row.ClosedDate.Value - row.CreatedDate.Value).TotalMinutes)
                        };

                        return aggDictionary;
                    })
                    .ToList();

                RequestsByZipHour = requestsTable;

                if (!requestsTable.Any())
                {
                    _messagingService.ShowInfo();
                }

                return Result.Success();
            }
            catch
            {
                _messagingService.ShowError(Resources.messaging_service_error_occurred);
                return Result.Failure(Resources.messaging_service_error_occurred);
            }
            finally
            {
                _loadingService.IsLoading = false;
            }
        }

        private Result PopulateZipCodes()
        {
            if (SelectedBoroughs == null || SelectedBoroughs.Count == 0)
            {
                return Result.Failure(string.Join(" ", string.Format(Resources.empty_selction_table, Resources.groupy_category_boroughs)));
            }

            foreach (var borough in SelectedBoroughs)
            {
                var availableZips = Requests
                    .Where(r => r.Borough?.ToProperCase() == borough && !string.IsNullOrEmpty(r.IncidentZip))
                    .Select(r => r.IncidentZip)
                    .Distinct()
                    .OrderBy(z => z)
                    .ToList();

                var existing = BoroughZipSelections.FirstOrDefault(b => b.Borough == borough);
                if (existing is null)
                {
                    BoroughZipSelections.Add(new BoroughZipSelection(borough, availableZips));  // all zips selected by default
                }
                else
                {
                    existing.AvailableZips = availableZips;  // preserve existing selections
                }
            }

            BoroughZipSelections.RemoveAll(b => !SelectedBoroughs.Contains(b.Borough));

            // single source of truth — model drives flat SelectedZipCodes
            SelectedZipCodes = BoroughZipSelections
                .SelectMany(b => b.SelectedZips)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (SelectedZipCodes == null || SelectedZipCodes.Count == 0)
            {
                return Result.Failure(string.Join(" ", string.Format(Resources.empty_selction_table, Resources.groupy_category_zip_codes)));
            }

            return Result.Success();
        }

        public async Task<Result<List<T>>> LoadData<T>(string url) where T : class
        {
            return await _httpService.GetAsync<List<T>>(url);
        }
    }
}
