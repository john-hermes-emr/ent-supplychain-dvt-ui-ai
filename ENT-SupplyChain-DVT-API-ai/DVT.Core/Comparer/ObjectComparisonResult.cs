using System.Text.Json;

namespace DVT.Core.Comparer
{
    public class ObjectComparisonResult
    {
        public ObjectComparisonResult()
        {
            HasChanges = false;
            ChangeSummaryObj = new ObjectCompareChangeSummary();
        }

        public ObjectComparisonResult(Type typeCompared)
        {
            TypeCompared = typeCompared;
            ChangeSummaryObj = new ObjectCompareChangeSummary();
        }

        public Type TypeCompared { get; set; }
        public bool HasChanges { get; set; }
        public string ChangeSummary
        {
            get
            {
                if (HasChanges && ChangeSummaryObj != null)
                {
                    return JsonSerializer.Serialize(ChangeSummaryObj);
                }

                return string.Empty;
            }
        }

        public ObjectCompareChangeSummary ChangeSummaryObj { get; set; }

        public string ChangeSummaryFriendly { get; set; }

        public void AddChangedField(string fieldName, object previousValue, object newValue)
        {
            ChangeSummaryObj.Fields.Add(
                new ObjectCompareChangeSummaryItem()
                {
                    Field = fieldName,
                    PrevValue = previousValue == null ? "NULL" : previousValue.ToString(),
                    NewValue = newValue == null ? "NULL" : newValue.ToString()
                }
                );
        }
    }

    public class ObjectCompareChangeSummary
    {
        public List<ObjectCompareChangeSummaryItem> Fields { get; set; } = new List<ObjectCompareChangeSummaryItem>();
    }

    public class ObjectCompareChangeSummaryItem
    {
        public string Field { get; set; }
        public string PrevValue { get; set; }
        public string NewValue { get; set; }
    }
}