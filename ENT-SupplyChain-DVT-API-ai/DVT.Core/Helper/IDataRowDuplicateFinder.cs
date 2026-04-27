using DVT.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT.Core.Helper
{
    public static class IDataRowDuplicateFinder
    {
        public static List<int> FindDuplicatesRowNumbers(IEnumerable<IDataRow> dataRows)
        {
            //We're using an integer array for the value of the dictionary so that we can update it avoiding a secondary lookup
            //in the dictionary to update the value since it's a reference type it points directly to the value in the dictionary.
            Dictionary<string, int[]> distinctItems = new Dictionary<string, int[]>();
            List<int> duplicateRowNumbers = new List<int>();

            foreach (var row in dataRows)
            {
                if (distinctItems.TryGetValue(row.UniquenessKey, out int[]? existingRowNums))
                {
                    if (existingRowNums != null && existingRowNums.Length > 0)
                    {
                        //Since we will constantly find the first duplicate in the distinct items list, we need to
                        //also add the first occurrence of the record that we found in the distinct items list to the duplicate list if it's not already there.
                        //So that we do not keep adding the same existing record as duplicate, we will mark it as -1 in the distinct items list after
                        //we add it to the duplicate list the first time we find a duplicate, and we will check if it's marked before we add to the duplicate list.                    
                        if (existingRowNums[0] != -1)
                        {
                            duplicateRowNumbers.Add(existingRowNums[0]);
                            existingRowNums[0] = -1; //mark as added to duplicate list
                        }

                        //Add the duplicate we just found to the list of duplicates.
                        duplicateRowNumbers.Add(row.RowNumber);

                    }
                }
                else
                {
                    distinctItems.Add(row.UniquenessKey, new int[] { row.RowNumber });
                }
            }

            return duplicateRowNumbers;
        }
    }
}
