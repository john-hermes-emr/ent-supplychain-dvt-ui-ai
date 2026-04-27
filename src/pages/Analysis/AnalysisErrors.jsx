import React from 'react'
import { DataGrid } from '@mui/x-data-grid'
import { useHistory } from 'react-router-dom';
import Constant from '../../utils/constant/index.json';
import { Typography } from '@mui/material';
import Box from '@mui/material/Box';

const noValidationErrors = Constant.Analysis.noValidationErrors;


function AnalysisErrors(props) {
    const { errors, jobId, jobFileId, filename, errorDate } = props;
    const history = useHistory();

    const rows = errors.map((error, index) => ({
        id: index + 1,
        messageType: error.messageType,
        field: error.field,
        count: error.count,
        error: error.error,
        errorDetails: error.details,
    }));

    const columns = [
        { field: 'messageType', headerName: 'Message Type', disableColumnMenu: true, flex: 1 },
        { field: 'field', headerName: 'Field', disableColumnMenu: true, flex: 1 },
        { field: 'count', headerName: 'Count', disableColumnMenu: true, flex: 1 },
        { field: 'error', headerName: 'Error', disableColumnMenu: true, flex: 2 },
    ];

    const handleRowDoubleClick = (params) => {
        const { errorDetails } = params.row;
        let errorDetailsInfo = {}
        errorDetailsInfo.errorDetails = errorDetails;
        errorDetailsInfo.filename = filename;
        errorDetailsInfo.errorDate = errorDate;

        sessionStorage.setItem('errorDetailsInfo', JSON.stringify(errorDetailsInfo)); // or the specific array
        window.open(`/analysisErrorDetails?jobFileId=${jobFileId}&jobId=${jobId}&type=error`, '_blank');
    };

    const NoRowsOverlay = () => (
        <Box
            sx={{
                height: '100%',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                flexDirection: 'column',
            }}
        >
            <Typography variant="body1" color="textSecondary">
                {noValidationErrors}
            </Typography>
        </Box>
    );

    return (
        <div style={{ height: 400, width: '100%' }}>
            <DataGrid
                rows={rows}
                columns={columns}
                pageSize={5}
                onRowDoubleClick={handleRowDoubleClick}
                disableSelectionOnClick
                initialState={{
                    pagination: {
                        paginationModel: {
                            pageSize: 15,
                        },
                    },
                }}
                slots={{
                    noRowsOverlay: NoRowsOverlay,
                }}
            />
        </div>
    )
}

export default AnalysisErrors
