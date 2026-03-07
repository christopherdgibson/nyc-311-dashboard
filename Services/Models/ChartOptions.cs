using System.Text.Json.Serialization;

namespace NYC311Dashboard.Services.Models
{
    public class ChartOptions
    {
        [JsonPropertyName("chart")]
        public Chart Chart { get; set; } = new Chart();

        [JsonPropertyName("dataLabels")]
        public DataLabels? DataLabels { get; set; } = new DataLabels();

        [JsonPropertyName("plotOptions")]
        public PlotOptions? PlotOptions { get; set; } = new PlotOptions();

        [JsonPropertyName("tooltip")]
        public Tooltip? Tooltip { get; set; } = new Tooltip();

        [JsonPropertyName("yaxis")]
        public YAxis YAxis { get; set; } = new YAxis();

        //public bool HasData =>
        //    (XAxis?.Categories?.Count ?? 0) > 0
        //    && (Series?.Any(s => s.Data?.Count > 0) ?? false);
    }

    public class ApexDataGroup
    {
        [JsonPropertyName("groupKey")]
        public string? GroupKey { get; set; }

        [JsonPropertyName("dataset")]
        public ApexDataSet Dataset { get; set; } = new();
    }

    public class ApexDataSet
    {
        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        [JsonPropertyName("series")]
        public List<ApexSeries> Series { get; set; } = new();
    }

    public class ApexSeries
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("data")]
        public List<double>? Data { get; set; }
    }

    public class BarOptions
    {
        [JsonPropertyName("borderRadius")]
        public int BorderRadius { get; set; } = 1;

        [JsonPropertyName("dataLabelsPosition")]
        public string? DataLabelsPosition { get; set; } = "top";
    }

    public class Chart
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "bar";

        [JsonPropertyName("width")]
        public object Width { get; set; } = "100%";

        [JsonPropertyName("height")]
        public object Height { get; set; } = "100%";
    }

    public class DataLabels
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("seriesFormatters")]
        public List<string> SeriesFormatters { get; set; } = new() { "integer", "decimal:2" };

        [JsonPropertyName("offsetY")]
        public int OffsetY { get; set; } = -20;

        [JsonPropertyName("style")]
        public DataLabelStyle Style { get; set; } = new DataLabelStyle();
    }

    public class DataLabelStyle
    {
        [JsonPropertyName("fontSize")]
        public string FontSize { get; set; } = "12px";

        [JsonPropertyName("colors")]
        public List<string> Colors { get; set; } = new() { "#304758" };
    }

    public class PlotOptions
    {
        [JsonPropertyName("bar")]
        public BarOptions Bar { get; set; } = new BarOptions();
    }

    public class Tooltip
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("seriesFormatters")]
        public List<string> SeriesFormatters { get; set; } = new() { "integer", "decimal:2" };
    }

    public class YAxis
    {
        [JsonPropertyName("labelsFormatter")]
        public string LabelsFormatter { get; set; } = "integer";
    }
}
