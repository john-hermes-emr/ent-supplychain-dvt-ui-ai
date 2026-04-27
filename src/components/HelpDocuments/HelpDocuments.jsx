import React, { useState, useEffect, useRef } from 'react';
import { ButtonGroup, Button, Popper, Paper, MenuList, MenuItem, ClickAwayListener, Grow } from '@mui/material';
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';
import DownloadIcon from '@mui/icons-material/Download';
import axios from 'axios';
import { ApiHelper } from '../../dvt_api/ApiHelper';
import { DvtApiPaths } from '../../dvt_api/DvtApiPaths';
import { toast } from 'react-toastify';
import './HelpDocuments.css';
import { saveAs } from 'file-saver';

const FullPageLoader = React.lazy(() => import('../../components/FullPageLoader/index.jsx'));

const HelpDocuments = () => {
    const [loader, setLoader] = useState(false);
    const [open, setOpen] = useState(false);
    const [helpDocuments, setHelpDocuments] = useState([]);
    const anchorRef = useRef(null);

    useEffect(() => {
        fetchHelpDocuments();
    }, []);

    const fetchHelpDocuments = async () => {
        try {
            const response = await axios.get(
                ApiHelper.getApiUrl(DvtApiPaths.HelpDocuments.GetHelpDocuments),
                {
                    headers: {
                        Authorization: ApiHelper.getBearerToken(),
                    },
                }
            );
            setHelpDocuments(response.data || []);
        } catch (error) {
            console.error('Error fetching help documents:', error);
            toast.error('Failed to load help documents');
        }
    };

    const handleToggle = () => {
        setOpen((prevOpen) => !prevOpen);
    };

    const handleClose = (event) => {
        if (anchorRef.current && anchorRef.current.contains(event.target)) {
            return;
        }
        setOpen(false);
    };

    const handleDownload = async (document) => {
        try {
            setOpen(false);
            setLoader(true);
            const response = await axios.post(
                ApiHelper.getApiUrl(DvtApiPaths.HelpDocuments.DownloadHelpDocument),
                { name: document.name },
                {
                    responseType: 'blob',
                }
            );

            // Extract filename from Content-Disposition header
            const disposition = response.headers['content-disposition'];
            let filename = '';
            if (disposition && disposition.indexOf('filename=') !== -1) {
                filename = disposition
                    .split('filename=')[1].split(';')[0]
                    .replace(/['"]/g, '')
                    .trim();
            }
            let url = window.URL
                .createObjectURL(new Blob([response.data]));
            saveAs(url, filename);
            setLoader(false);
        } catch (error) {
            setLoader(false);
            setOpen(false);
            console.error('Error downloading document:', error);
        }
    };

    return (
        <div className="help-documents-container">
            <ButtonGroup ref={anchorRef} variant="text">
                <Button
                    onClick={handleToggle}
                    startIcon={<HelpOutlineIcon />}
                    sx={{
                        color: 'white',
                        textTransform: 'none',
                        '&:hover': {
                            backgroundColor: 'rgba(255, 255, 255, 0.1)',
                        },
                    }}
                >
                </Button>
            </ButtonGroup>
            <Popper
                open={open}
                anchorEl={anchorRef.current}
                role={undefined}
                placement="bottom-start"
                transition
                disablePortal
                sx={{ zIndex: 1300 }}
            >
                {({ TransitionProps, placement }) => (
                    <Grow
                        {...TransitionProps}
                        style={{
                            transformOrigin:
                                placement === 'bottom-start' ? 'left top' : 'left bottom',
                        }}
                    >
                        <Paper>
                            <ClickAwayListener onClickAway={handleClose}>
                                <MenuList autoFocusItem={open}>
                                    {helpDocuments.map((doc, index) => (
                                        <MenuItem
                                            key={index}
                                            onClick={() => handleDownload(doc)}
                                            sx={{
                                                minWidth: 200,
                                                display: 'flex',
                                                alignItems: 'center',
                                                gap: 1,
                                            }}
                                        >
                                            <DownloadIcon fontSize="small" />
                                            {doc.name}
                                        </MenuItem>
                                    ))}
                                </MenuList>
                            </ClickAwayListener>
                        </Paper>
                    </Grow>
                )}
            </Popper>
            <React.Suspense fallback={<div>Loading...</div>}>
                {loader && <FullPageLoader />}
            </React.Suspense>
        </div>
    );
};

export default HelpDocuments;
