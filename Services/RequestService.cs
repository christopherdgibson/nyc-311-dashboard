using CSharpFunctionalExtensions;
using NYC311Dashboard.Extensions;
using NYC311Dashboard.Intrastructure.Contracts;
using NYC311Dashboard.Models;
using NYC311Dashboard.Services.Contracts;

namespace NYC311Dashboard.Services
{
    public class RequestService : IRequestService
    {
        private readonly IHttpService _httpService;
        private readonly ILoadingService _loadingService;
        private readonly IMessagingService _messagingService;
        //private readonly string baseUrl = "https://data.cityofnewyork.us/resource/erm2-nwe9.json";
        private readonly string sampleUrl = "sample-data/NYC311Requests.json";
        private readonly string sampleUrlAbbr = "sample-data/NYC311RequestsAbr.json";
        public List<RequestModel> Requests { get; private set; } = new();

        public List<string> Boroughs { get; private set; } = new();
        public List<string> ZipCodes { get; private set; } = new();

        public HashSet<string>? SelectedBoroughs { get; private set; } = null;
        public HashSet<string>? SelectedZipCodes { get; private set; } = null;
        private HashSet<string> SelectedZipBoroughs = new();

        public List<BoroughDateTableRow> RequestsByBoroughDate { get; private set; } = new();
        public List<ZipHourTableRow> RequestsByZipHour { get; private set; } = new();

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

                //     requests.Clear();
                //     sortOrder = 0;

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
                Console.WriteLine("ERROR: " + ex.Message);
                Console.WriteLine(ex.ToString());
                _messagingService.ShowError(Resources.messaging_service_error_occurred);
            }
            finally
            {

                _loadingService.IsLoading = false;
            }
        }

        public Result GenerateTableByBoroughDay()
        {
            _loadingService.LoadingMessage = Resources.loading_service_loading_here;
            _loadingService.IsLoading = true;
            try
            {
                if (SelectedBoroughs == null || SelectedBoroughs.Count == 0) // bring this error up to page level and zip too?
                {
                    _messagingService.ShowError("No boroughs selected for table!");
                    return Result.Failure("No boroughs selected for table!");
                }

                _messagingService.Clear(); // Clear old error if no boroughs selected

                var requestsTable = Requests
                    .Where(row => row.Status.Equals("closed", StringComparison.OrdinalIgnoreCase)
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
                            OpenTime = g.Sum(row => (row.ClosedDate.Value - row.CreatedDate.Value).TotalMinutes)
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
                    .Where(row => row.Status.Equals("closed", StringComparison.OrdinalIgnoreCase)
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
                            OpenTime = g.Sum(row => (row.ClosedDate.Value - row.CreatedDate.Value).TotalMinutes)
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
                return Result.Failure("No boroughs selected for table!");
            }

            ZipCodes = Requests
            .Where(r => !string.IsNullOrWhiteSpace(r.Borough)
                && SelectedBoroughs.Contains(r.Borough.ToProperCase())
                && !r.Borough.Equals("unspecified", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(r.IncidentZip))
            .Select(r => r.IncidentZip)
            .Distinct()
            .OrderBy(b => b)
            .ToList();

            // If any boroughs were newly added, add their zips to SelectedZipCodes
            var newlyAddedBoroughs = SelectedBoroughs.Except(SelectedZipBoroughs).ToList();
            if (newlyAddedBoroughs.Any())
            {
                var zipsToAdd = Requests
                    .Where(r => newlyAddedBoroughs.Contains(r.Borough) && !string.IsNullOrEmpty(r.IncidentZip))
                    .Select(r => r.IncidentZip)
                    .Distinct();

                if (SelectedZipCodes == null)
                {
                    SelectedZipCodes = ZipCodes.ToHashSet();
                }
                else
                {
                    foreach (var zip in zipsToAdd)
                    {
                        SelectedZipCodes.Add(zip);
                    }
                }
            }

            SelectedZipBoroughs = new HashSet<string>(SelectedBoroughs, StringComparer.OrdinalIgnoreCase);

            // If SelectedZipCodes is null (first load), set to all zip codes
            SelectedZipCodes ??= new HashSet<string>(ZipCodes, StringComparer.OrdinalIgnoreCase);

            if (SelectedZipCodes == null || SelectedZipCodes.Count == 0)
            {
                return Result.Failure("No zip codes selected for table!");
            }

            return Result.Success();
        }

        public async Task<Result<List<T>>> LoadData<T>(string url) where T : class
        {
            return await _httpService.GetAsync<List<T>>(url);
        }
    }
}
