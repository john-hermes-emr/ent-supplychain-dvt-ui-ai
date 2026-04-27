import React from 'react';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
const { forwardRef, useImperativeHandle } = React;

const ToastifyModal = forwardRef((props, ref) => {
    useImperativeHandle(ref, () => ({
        getConfirmationMessage(message, status) {
            if (status === "error") {
                toast.error(message, {
                    position: toast.POSITION.BOTTOM_LEFT
                });
            } else {
                toast.success(message, {
                    position: toast.POSITION.BOTTOM_LEFT
                });
            }
        }
    }));

    return (
        <>
            <ToastContainer />
        </>
    );
});

export default ToastifyModal;
