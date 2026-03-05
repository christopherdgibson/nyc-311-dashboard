namespace NYC311Dashboard.Models
{
    public class RequestTableRow
    {

        public string UniqueKey { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        public double Duration { get; set; }

        public string Agency { get; set; }

        public string AgencyName { get; set; }

        public string Problem { get; set; }

        public string ProblemDetail { get; set; }

        public string AdditionalDetails { get; set; }

        public string Status { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? ResolutionActionUpdatedDate { get; set; }

        public string ResolutionDescription { get; set; }

        public string LocationType { get; set; }

        public string IncidentZip { get; set; }

        public string IncidentAddress { get; set; }

        public string StreetName { get; set; }

        public string CrossStreet1 { get; set; }

        public string CrossStreet2 { get; set; }

        public string IntersectionStreet1 { get; set; }

        public string IntersectionStreet2 { get; set; }

        public string AddressType { get; set; }

        public string City { get; set; }

        public string Landmark { get; set; }

        public string FacilityType { get; set; }

        public string CommunityBoard { get; set; }

        public string CouncilDistrict { get; set; }

        public string PolicePrecinct { get; set; }

        public string BBL { get; set; }

        public string Borough { get; set; }

        public string XCoordinateStatePlane { get; set; }

        public string YCoordinateStatePlane { get; set; }

        public string OpenDataChannelType { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string Type { get; set; }

        public List<double> Coordinates { get; set; }

        public string ParkFacilityName { get; set; }

        public string ParkBorough { get; set; }

        public string VehicleType { get; set; }

        public string TaxiCompanyBorough { get; set; }

        public string TaxiPickUpLocation { get; set; }

        public string BridgeHighwayName { get; set; }

        public string BridgeHighwayDirection { get; set; }

        public string RoadRamp { get; set; }

        public string BridgeHighwaySegment { get; set; }
    }
}
