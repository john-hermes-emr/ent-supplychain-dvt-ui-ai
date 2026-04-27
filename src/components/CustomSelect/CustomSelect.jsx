import React from 'react';
import Select from 'react-select';
import './CustomSelect.css';

const CustomSelect = ({
    options,
    value, // selected ID
    onChange,
    isClearable = true,
    isMulti = false,
    placeholder = 'Select...',
    className,
    ...rest
}) => {
    // Find the selected option object by ID
    const selectedOption = options.find(opt => opt.value === value) || null;

    // Handle change to return only the ID
    const handleChange = (option) => {
        if (isMulti) {
            onChange(option ? option.map(opt => opt.value) : []);
        } else {
            onChange(option ? option.value : '');
        }
    };

    return (
        <Select
            className={className ? `custom-select ${className}` : 'custom-select'}
            options={options}
            value={selectedOption}
            isClearable={isClearable}
            onChange={handleChange}
            isMulti={isMulti}
            placeholder={placeholder}
            classNamePrefix="custom-select"
            {...rest}
        />
    );
};

export default CustomSelect;
