import React from 'react';
import './Footer.css'
const currentYear = new Date().getFullYear();
const environmentName = window.location.host
const environmentsMaping = {
    'cds-appinventory-dev.emerson.com': 'Development Environment',
    'cds-appinventory-stage.emerson.com': 'Staging Environment',
}
const displayEnvironmentName = environmentsMaping[environmentName]
console.log(process.env)
const Footer = () => {
    return (
        <div>
            <h1 className='footer'>
                <span className='environment-name'>{displayEnvironmentName}</span>
                ©{currentYear} Emerson Electric Co. All rights reserved.
                <p>
                    Consider It Solved.</p>
            </h1>
        </div>
    );
}

export default Footer;