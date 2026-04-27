import React from 'react';
import { useHistory } from 'react-router-dom';

const NotFound = () => {
    const history = useHistory();
    return (
        <div
            style={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                height: '60vh',
                color: '#444',
                fontFamily: 'Segoe UI, Arial, sans-serif'
            }}
        >
            <h1 style={{ fontSize: 64, margin: 0 }}>404</h1>
            <h2 style={{ margin: '10px 0 20px 0', fontWeight: 400 }}>Page Not Found</h2>
            <p style={{ marginBottom: 24, color: '#888' }}>
                Sorry, the page you are looking for does not exist.
            </p>
            <button
                style={{
                    padding: '10px 24px',
                    background: '#1976d2',
                    color: '#fff',
                    border: 'none',
                    borderRadius: 4,
                    cursor: 'pointer',
                    fontSize: 16
                }}
                onClick={() => history.push('/home')}
            >
                Go Home
            </button>
        </div>
    );
};

export default NotFound;
