namespace DVT.Core.Comparer
{
    public static class ObjectComparer
    {
        public static ObjectComparisonResult CompareObjects(object existing, object newObj, Type typeCompared, List<string> ignoredProperties)
        {
            bool changesDetected = false;
            var typeProperties = typeCompared.GetProperties();
            ObjectComparisonResult retVal = new ObjectComparisonResult(typeCompared);

            foreach (var property in typeProperties)
            {
                //Ignore certain properties
                if (ignoredProperties != null && ignoredProperties.Contains(property.Name))
                    continue;

                //Ignore lists
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition().Equals(typeof(ICollection<>)))
                    continue;

                var existingValue = property.GetValue(existing, null);
                var newValue = property.GetValue(newObj, null);

                //Don't do any comparison if both are null
                if (existingValue == null && newValue == null)
                    continue;

                //If it's a guid array
                if (property.PropertyType == typeof(Guid[]))
                {
                    var oldGuid = property.GetValue(existing, null) as Guid[];
                    var newGuid = property.GetValue(newObj, null) as Guid[];

                    //If both lists are not null, sort them so we can use SequenceEqual from Linq to compare them
                    if (oldGuid == null)
                    {
                        oldGuid = new Guid[0];
                    }
                    if (newGuid == null)
                    {
                        newGuid = new Guid[0];
                    }

                    Array.Sort(oldGuid);
                    Array.Sort(newGuid);

                    if (!oldGuid.SequenceEqual(newGuid))
                    {
                        changesDetected = true;
                        retVal.AddChangedField(property.Name, existingValue == null ? "" : string.Join(",", existingValue as Guid[]), newValue == null ? "" : string.Join(",", newValue as Guid[]));
                    }

                    continue;
                }

                if (existingValue == null && newValue != null && string.IsNullOrWhiteSpace(newValue.ToString()))
                    continue;

                if (newValue == null && existingValue != null && string.IsNullOrWhiteSpace(existingValue.ToString()))
                    continue;

                if ((existingValue != null && newValue == null) || (existingValue == null && newValue != null) || !existingValue.Equals(newValue))
                {
                    changesDetected = true;
                    retVal.AddChangedField(property.Name, existingValue, newValue);
                }
            }

            retVal.HasChanges = changesDetected;
            return retVal;
        }
    }
}