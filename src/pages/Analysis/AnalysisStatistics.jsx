import React from 'react'
import { DataGrid } from '@mui/x-data-grid'

function AnalysisStatistics(props) {
    const { statistics, fileType } = props;

    // Define row configurations for each file type
    const rowsConfig = {
        Vir: [
            {
                id: 1,
                fieldName: "Total Records",
                min: statistics.totalRecords,
                max: statistics.totalRecords
            },
            {
                id: 2,
                fieldName: "Quantity Ordered",
                min: statistics.quantityOrderedMin,
                max: statistics.quantityOrderedMax
            },
            {
                id: 3,
                fieldName: "Quantity Received",
                min: statistics.quantityReceivedMin,
                max: statistics.quantityReceivedMax
            },
            {
                id: 4,
                fieldName: "Date Received",
                min: statistics.dateReceivedMin,
                max: statistics.dateReceivedMax
            },
            {
                id: 5,
                fieldName: "Invoice Price Paid",
                min: statistics.invoicePricePaidMin,
                max: statistics.invoicePricePaidMax
            },
            {
                id: 6,
                fieldName: "Unit Price",
                min: statistics.unitPriceMin,
                max: statistics.unitPriceMax
            },
            {
                id: 7,
                fieldName: "Committed Date",
                min: statistics.committedDateMin,
                max: statistics.committedDateMax
            }
        ],
        Item: [
            {
                id: 1,
                fieldName: "Total Records",
                min: statistics.totalRecords,
                max: statistics.totalRecords
            },
            {
                id: 2,
                fieldName: "Standard Cost",
                min: statistics.standardCostMin,
                max: statistics.standardCostMax
            }
        ],
        Supplier: [
            {
                id: 1,
                fieldName: "Total Records",
                min: statistics.totalRecords,
                max: statistics.totalRecords
            }
        ],
        Inventory: [
            {
                id: 1,
                fieldName: "Total Records",
                min: statistics.totalRecords,
                max: statistics.totalRecords
            },
            {
                id: 2,
                fieldName: "Quantity",
                min: statistics.quantityMin,
                max: statistics.quantityMax
            },
            {
                id: 3,
                fieldName: "Standard Cost",
                min: statistics.standardCostMin,
                max: statistics.standardCostMax
            },
            {
                id: 4,
                fieldName: "Total Value",
                min: statistics.totalValueMin,
                max: statistics.totalValueMax
            },
            {
                id: 5,
                fieldName: "Inventory Date",
                min: statistics.inventoryDateMin,
                max: statistics.inventoryDateMax
            }

        ],
        Po: [
            {
                id: 1,
                fieldName: "Total Records",
                min: statistics.totalRecords,
                max: statistics.totalRecords
            },
            {
                id: 2,
                fieldName: "Order Date",
                min: statistics.orderDateMin,
                max: statistics.orderDateMax
            },
            {
                id: 3,
                fieldName: "Latest Amendment",
                min: statistics.latestAmendmentMin,
                max: statistics.latestAmendmentMax
            }
        ],
        Uom: [
            {
                id: 1,
                fieldName: "Total Records",
                min: statistics.totalRecords,
                max: statistics.totalRecords
            },
            {
                id: 2,
                fieldName: "Conversion Rate",
                min: statistics.conversionRateMin,
                max: statistics.conversionRateMax
            }
        ],
        Mpn: [
            {
                id: 1,
                fieldName: "Total Records",
                min: statistics.totalRecords,
                max: statistics.totalRecords
            }
        ],
        PoItem: [
            {
                id: 1,
                fieldName: "Total Records",
                min: statistics.totalRecords,
                max: statistics.totalRecords
            },
            {
                id: 2,
                fieldName: "Unit Cost",
                min: statistics.unitCostMin,
                max: statistics.unitCostMax
            },
            {
                id: 3,
                fieldName: "Ordered Value",
                min: statistics.orderedValueMin,
                max: statistics.orderedValueMax
            },
            {
                id: 4,
                fieldName: "Quantity Ordered",
                min: statistics.quantityOrderedMin,
                max: statistics.quantityOrderedMax
            },
            {
                id: 5,
                fieldName: "Quantity Returned",
                min: statistics.quantityReturnedMin,
                max: statistics.quantityReturnedMax
            },
            {
                id: 6,
                fieldName: "Committed Date",
                min: statistics.committedDateMin,
                max: statistics.committedDateMax
            },
            {
                id: 7,
                fieldName: "Requested Date",
                min: statistics.requestedDateMin,
                max: statistics.requestedDateMax
            },
            {
                id: 8,
                fieldName: "Qty Left to Receive",
                min: statistics.qtyLeftToReceiveMin,
                max: statistics.qtyLeftToReceiveMax
            },
            {
                id: 9,
                fieldName: "Value Left to Receive",
                min: statistics.valueLeftToReceiveMin,
                max: statistics.valueLeftToReceiveMax
            }

        ],
        // Add more file types here as needed
    };

    // Dynamically get rows based on fileType, fallback to empty array if type not found
    const rows = rowsConfig[fileType] || [];

    const columns = [
        { field: 'fieldName', headerName: 'Field Name', disableColumnMenu: true, flex: 1 },
        { field: 'min', headerName: 'Min', disableColumnMenu: true, flex: 1 },
        { field: 'max', headerName: 'Max', disableColumnMenu: true, flex: 2 },
    ];

    return (
        <div style={{ height: 400, width: '100%' }}>
            <DataGrid
                rows={rows}
                columns={columns}
                pageSize={5}
                disableSelectionOnClick
                initialState={{
                    pagination: {
                        paginationModel: {
                            pageSize: 15,
                        },
                    },
                }}
            />
        </div>
    )
}

export default AnalysisStatistics;
