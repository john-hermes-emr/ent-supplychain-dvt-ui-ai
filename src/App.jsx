import React, { useState, lazy, Suspense } from 'react';
import { Route, Switch, useHistory } from 'react-router-dom';
import { OktaAuth, toRelativeUrl } from '@okta/okta-auth-js';
import { Security, SecureRoute, LoginCallback } from '@okta/okta-react';
import axios from "axios";
import { ApiHelper } from './dvt_api/ApiHelper';
import config from './config';
import { Provider } from 'react-redux';
import store from './redux/store/store';
import Navbar from './pages/Navbar/Navbar';
import PageFooter from './pages/Footer/Footer';
import Home from './pages/Home/Home';
import Landing from './pages/Landing/Landing';
import ChangePaths from './pages/Tools/ChangePaths/ChangePaths';
import Help from './pages/Help/Help';
import Analysis from './pages/Analysis/Analysis';
import AnalysisErrorDetails from './pages/Analysis/AnalysisErrorDetails';
import NotFound from './pages/NotFound/NotFound';
import './App.css';

const CustomDialog = lazy(() => import('./components/CustomDialog/CustomDialog'));

// ErrorBoundary to catch errors in Suspense/lazy components
class ErrorBoundary extends React.Component {
    constructor(props) {
        super(props);
        this.state = { hasError: false, error: null };
    }
    static getDerivedStateFromError(error) {
        return { hasError: true, error };
    }
    componentDidCatch(error, errorInfo) {
        // Optionally log errorInfo
    }
    render() {
        if (this.state.hasError) {
            return (
                <div style={{ color: 'red', padding: 20 }}>
                    <h2>Something went wrong.</h2>
                    <pre>{this.state.error && this.state.error.toString()}</pre>
                </div>
            );
        }
        return this.props.children;
    }
}

const oktaAuth = new OktaAuth(config.oidc);

const App = () => {
    const [validationdialogOpen, setValidationDialogOpen] = useState(false);
    const [validationMessage, setValidationMessage] = useState('');
    const history = useHistory();

    const restoreOriginalUri = async (_oktaAuth, originalUri) => {
        history.replace(toRelativeUrl(originalUri || '/', window.location.origin));
    };

    const validationDialogActions = [
        { label: 'OK', onClick: () => { handleDialogClose(); } },
    ];
    const handleDialogClose = () => {
        setValidationDialogOpen(false);
    };

    const style = {
        position: "relative",
        padding: "0px 40px 100px",
    };

    axios.interceptors.request.use((config) => {
        config.headers.Authorization = ApiHelper.getBearerToken();
        config.withCredentials = false;
        return config;
    });

    axios.interceptors.response.use(
        (response) => response,
        (error) => {
            if (error && error.response) {
                const { response: { status, data } } = error;
                if (status === 401) {
                    oktaAuth.tokenManager.clear();
                    history.push('/');
                } else if (status === 400 || status === 500) {
                    const errorMessage = data.exceptionMessage || data.ExceptionMessage || '';
                    setValidationDialogOpen(true);
                    setValidationMessage(errorMessage);
                }
            }
            return Promise.reject(error);
        }
    );

    return (
        <Provider store={store}>
            <Suspense fallback={<div></div>}>
                <Security oktaAuth={oktaAuth} restoreOriginalUri={restoreOriginalUri}>
                    <Navbar />
                    <ErrorBoundary>
                        <div style={style}>
                            <Switch>
                                <SecureRoute path="/home" component={Home} />
                                <SecureRoute path="/" exact={true} component={Landing} />
                                <Route path="/login/callback" component={LoginCallback} />
                                <SecureRoute path="/changePaths" component={ChangePaths} />
                                <SecureRoute path="/help" component={Help} />
                                <SecureRoute path="/analysis" component={Analysis} />
                                <SecureRoute path="/analysisErrorDetails" component={AnalysisErrorDetails} />
                                <Route path="*" component={NotFound} />
                            </Switch>
                        </div>
                    </ErrorBoundary>
                </Security>
                <PageFooter />
                <CustomDialog
                    className='validationdialogOpen'
                    open={validationdialogOpen}
                    handleClose={handleDialogClose}
                    content={validationMessage}
                    actions={validationDialogActions}
                />
            </Suspense>
        </Provider>
    );
};

export default App;
