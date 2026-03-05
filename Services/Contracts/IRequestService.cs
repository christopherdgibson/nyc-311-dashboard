using CSharpFunctionalExtensions;
using NYC311Dashboard.Models;
using NYC311Dashboard.Services.Models;
using System.Linq.Expressions;

namespace NYC311Dashboard.Services.Contracts
{
    public interface IRequestService
    {
        List<RequestModel> Requests { get; }

        List<string> Boroughs { get; }

        HashSet<string>? SelectedBoroughs { get; }

        HashSet<string>? SelectedZipCodes { get; }

        HashSet<string>? SelectedPrecincts { get; }

        List<BoroughDateTableRow> RequestsByBoroughDate { get; }

        List<ZipHourTableRow> RequestsByZipHour { get; }

        List<PrecinctTableRow> RequestsByPrecinct { get; }

        List<BoroughZipSelection> BoroughZipSelections { get; }

        List<BoroughPrecinctSelection> BoroughPrecinctSelections { get; }

        Task GetNYC311RequestsDataAsync(string? url = null);

        TableOptions<T> GetTableOptions<T>(Expression<Func<T, string>>? groupBy);

        Result GenerateTableByBoroughDay();

        Result GenerateTableByPrecinct();

        Result GenerateTableByZipHour();
    }
}
