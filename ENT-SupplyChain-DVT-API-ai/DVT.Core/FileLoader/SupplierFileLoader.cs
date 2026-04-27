using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Models.DataRowEntities;
using DVT.Core.Services;
using System.Collections.Generic;
using static DVT.Core.Constants;

namespace DVT.Core.FileLoader
{
    public class SupplierFileLoader : IFileLoader
    {
       
        public async Task<FileLoadResult> LoadFileAsync(FileLoadRequest request, IStorageService storageService)
        {
            var result = new FileLoadResult();
            try
            {
                request.FlatFileContent = await storageService.GetWorkingFileContentsAsync(request.JobId, request.FileName);

                if (string.IsNullOrWhiteSpace(request.FlatFileContent))
                {
                    throw new ArgumentException(StardardMessages.FileContentIsEmpty, nameof(request.FilePath));
                }
                
                var loadedFile = ParseSupplierFileDataWithSpans(request);   
                result.JobFileId = request.JobFileId;
                result.Operation = "LoadFile: " + request.FileName;
                result.Success = true;
                result.Message = $"Uploaded {loadedFile.DataRows.Count} rows from {request.FileName}";
                result.FileHeader = loadedFile.FileHeader.Where(header => !string.IsNullOrWhiteSpace(header)).ToList();
                result.DataRows = loadedFile.DataRows;
            }
            catch (Exception ex)
            {
                result.Operation = "LoadFile: " + request.FileName;
                result.Success = false;
                result.Message = $"Errors loading file: {ex.Message}";
            }
            return result;
        }

        private IJobFileModel ParseSupplierFileDataWithSpans(FileLoadRequest request)
        {
            JobFileModel supplierDataFile = new JobFileModel()
            {
                JobFileId = request.JobFileId,
                FileType = Constants.FileTypes.Supplier,
                FileName = request.FileName,
                DataRows = new List<IDataRow>()
            };

            var parser = new EfficientCsvParser(request.FlatFileContent);
            int rowNumber = 0;

            while (parser.TryReadRow(out var row))
            {
                if(rowNumber == 0)
                {
                    supplierDataFile.FileHeader.AddRange(row.GetAsSplitStringList());
                    rowNumber++;
                    continue;
                }

                //If the row is empty, create an empty object and move on
                if (row.IsEmptyLine)
                {
                    supplierDataFile.DataRows.Add(new SupplierDataRow());
                    rowNumber++;
                    continue;
                }

                supplierDataFile.DataRows.Add(new SupplierDataRow
                {
                    RowNumber = rowNumber,
                    DivisionId = row.GetFieldOrDefault(SupplierColumns.DivisionId).ToString(),
                    LocalSiteId = row.GetFieldOrDefault(SupplierColumns.LocalSiteId).ToString(),
                    SupplierId = row.GetFieldOrDefault(SupplierColumns.SupplierId).ToString(),
                    SupplierName = row.GetFieldOrDefault(SupplierColumns.SupplierName).ToString(),
                    DUNS = row.GetFieldOrDefault(SupplierColumns.DUNS).ToString(),
                    ActiveInactive = row.GetFieldOrDefault(SupplierColumns.ActiveInactive).ToString(),
                    DirectIndirect = row.GetFieldOrDefault(SupplierColumns.DirectIndirect).ToString(),
                    AddressDescr = row.GetFieldOrDefault(SupplierColumns.AddressDescr).ToString(),
                    Street = row.GetFieldOrDefault(SupplierColumns.Street).ToString(),
                    Suite = row.GetFieldOrDefault(SupplierColumns.Suite).ToString(),
                    City = row.GetFieldOrDefault(SupplierColumns.City).ToString(),
                    State = row.GetFieldOrDefault(SupplierColumns.State).ToString(),
                    PostalCode = row.GetFieldOrDefault(SupplierColumns.PostalCode).ToString(),
                    County = row.GetFieldOrDefault(SupplierColumns.County).ToString(),
                    Country = row.GetFieldOrDefault(SupplierColumns.Country).ToString(),
                    Addr1 = row.GetFieldOrDefault(SupplierColumns.Addr1).ToString(),
                    Addr2 = row.GetFieldOrDefault(SupplierColumns.Addr2).ToString(),
                    Addr3 = row.GetFieldOrDefault(SupplierColumns.Addr3).ToString(),
                    Addr4 = row.GetFieldOrDefault(SupplierColumns.Addr4).ToString(),
                    CountryCode = row.GetFieldOrDefault(SupplierColumns.CountryCode).ToString(),
                    GlobalFlag = row.GetFieldOrDefault(SupplierColumns.GlobalFlag).ToString(),
                    MainTelephone = row.GetFieldOrDefault(SupplierColumns.MainTelephone).ToString(),
                    TollFree = row.GetFieldOrDefault(SupplierColumns.TollFree).ToString(),
                    Fax = row.GetFieldOrDefault(SupplierColumns.Fax).ToString(),
                    WebSite = row.GetFieldOrDefault(SupplierColumns.WebSite).ToString(),
                    SupplierType = row.GetFieldOrDefault(SupplierColumns.SupplierType).ToString(),
                    IncorrectColumnCount = row.FieldCount != SupplierColumns.TotalColumns
                });
                rowNumber++;
            }

            return supplierDataFile;

        }        

        private static class SupplierColumns
        {
            public const int DivisionId = 0;
            public const int LocalSiteId = 1;
            public const int SupplierId = 2;
            public const int SupplierName = 3;
            public const int DUNS = 4;
            public const int ActiveInactive = 5;
            public const int DirectIndirect = 6;
            public const int AddressDescr = 7;
            public const int Street = 8;
            public const int Suite = 9;
            public const int City = 10;
            public const int State = 11;
            public const int PostalCode = 12;
            public const int County = 13;
            public const int Country = 14;
            public const int Addr1 = 15;
            public const int Addr2 = 16;
            public const int Addr3 = 17;
            public const int Addr4 = 18;
            public const int CountryCode = 19;
            public const int GlobalFlag = 20;
            public const int MainTelephone = 21;
            public const int TollFree = 22;
            public const int Fax = 23;
            public const int WebSite = 24;
            public const int SupplierType = 25;
            public const int TotalColumns = 26;
        }
    }
}