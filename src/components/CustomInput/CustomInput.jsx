import React from 'react';
import './CustomInput.css';

const CustomInput = ({
    type = 'text',
    value,
    onChange,
    placeholder = '',
    name,
    max,
    min,
    ...rest
}) => {
    const handleChange = (e) => {
        if (type === 'number') {
            const val = e.target.value;
            // Allow empty string or valid number
            if (val === '' || /^-?\d*\.?\d*$/.test(val)) {
                let numVal = val === '' ? '' : Number(val);
                // Enforce min/max if value is not empty
                if (val !== '') {
                    if (min !== undefined && numVal < min) {
                        numVal = min;
                    }
                    if (max !== undefined && numVal > max) {
                        numVal = max;
                    }
                    // If value was clamped, update the input value directly
                    if (numVal !== Number(val)) {
                        e.target.value = numVal;
                    }
                }
                onChange && onChange(e);
            }
        } else {
            onChange && onChange(e);
        }
    };

    return (
        <input
            className='custom-input'
            type={type}
            value={value}
            onChange={handleChange}
            placeholder={placeholder}
            name={name}
            max={type === 'number' ? max : undefined}
            min={type === 'number' ? min : undefined}
            {...rest}
        />
    );
};

export default CustomInput;