

import * as React from 'react';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';
import "./index.css";
export default function CircularIndeterminate() {
  return (
    <div class="loader-container">
      <div className="loader">
        <Box sx={{ display: 'flex' }}>
          <CircularProgress />
        </Box>
      </div>
    </div>
  );
}