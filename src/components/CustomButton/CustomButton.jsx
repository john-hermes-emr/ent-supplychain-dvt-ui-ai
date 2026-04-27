import React from 'react';
import Button from '@mui/material/Button';
import './CustomButton.css';

const CustomButton = ({ children, variant = 'contained', color = 'primary', ...props }) => {
    // Custom colors that aren't part of Material-UI's default palette
    const customColors = ['success', 'danger', 'info'];

    // Check if it's a custom color
    const isCustomColor = customColors.includes(color);

    // For custom colors, we'll use default MUI color and add our own class
    const muiColor = isCustomColor ? 'primary' : color;
    const customClass = isCustomColor ? `custom-button custom-button-${color}` : 'custom-button';

    return (
        <Button
            className={customClass}
            variant={variant}
            color={muiColor}
            {...props}
        >
            {children}
        </Button>
    );
};

export default CustomButton;