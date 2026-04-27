namespace DVT.Core.Models
{
    public class FileCalculateStatistics_POItem
    {
        public int TotalRecords { get; set; }
        public string UnitCostMin { get; set; }
        public string UnitCostMax { get; set; }
        public string OrderedValueMin { get; set; }
        public string OrderedValueMax { get; set; }
        public string QuantityOrderedMin { get; set; }
        public string QuantityOrderedMax { get; set; }
        public string QuantityReturnedMin { get; set; }
        public string QuantityReturnedMax { get; set; }
        public string CommittedDateMin { get; set; }
        public string CommittedDateMax { get; set; }
        public string RequestedDateMin { get; set; }
        public string RequestedDateMax { get; set; }
        public string QtyLeftToReceiveMin { get; set; }
        public string QtyLeftToReceiveMax { get; set; }
        public string ValueLeftToReceiveMin { get; set; }
        public string ValueLeftToReceiveMax { get; set; }
    }
}
