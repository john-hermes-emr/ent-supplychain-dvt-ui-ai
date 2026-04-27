import React from 'react';
import { useOktaAuth } from '@okta/okta-react';
import Home from '../Home/Home';

export default function Landing() {
    const { authState, oktaAuth } = useOktaAuth();

    return (
        <div>
            {authState && authState.isAuthenticated && <Home />}
            {/* {authState && !authState.isAuthenticated && <h1>Click on login to visit website</h1>} */}
        </div>
    );
}
