import React from 'react';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';
import style from './CustomDialog.module.css';
import './CustomDialog.css';

const CustomDialog = ({ open, handleClose, title, content, htmlContent, actions, maxWidth, customWidth, className }) => {
    return (
        <Dialog
            className={className ? "customDialog" + ' ' + `${className}` : "customDialog"}
            open={open}
            onClose={handleClose}
            maxWidth={typeof maxWidth === 'string' ? maxWidth : false}
            fullWidth
            PaperProps={
                customWidth
                    ? { sx: { width: customWidth, maxWidth: customWidth } }
                    : undefined
            }
        >
            <DialogTitle className={style.dialogTitle}>{title}</DialogTitle>
            <DialogContent>
                {htmlContent ? (
                    <div className={style.dialogContent}>
                        {htmlContent}
                    </div>
                ) : (
                    <DialogContentText className={style.dialogContent}>
                        {content}
                    </DialogContentText>
                )}
            </DialogContent>
            <DialogActions className='dialogActions'>
                {actions.map((action, index) => (
                    <Button key={index} disabled={action.disabled} title={action.title} onClick={action.onClick} className='dialogButton'>
                        {action.label}
                    </Button>
                ))}
            </DialogActions>
        </Dialog>
    );
};

export default CustomDialog;
