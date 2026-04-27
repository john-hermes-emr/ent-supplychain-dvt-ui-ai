import React, { useRef, useEffect, useState, lazy, Suspense } from 'react';
import { Link, NavLink, useLocation } from 'react-router-dom';
import { useOktaAuth } from '@okta/okta-react';
import { useDispatch, useSelector } from 'react-redux';
import { setUserInfomation, clearUserInfomation } from '../../redux/reducers/userInfoSliceReducer';
import AccountCircleOutlinedIcon from '@mui/icons-material/AccountCircleOutlined';
import style from './navbar.module.css';
import './navbar.css';
import axios from 'axios';
import HelpDocuments from '../../components/HelpDocuments/HelpDocuments';

const CustomDialog = lazy(() => import('../../components/CustomDialog/CustomDialog'));


const Navbar = () => {
    const { authState, oktaAuth } = useOktaAuth();
    const location = useLocation();
    const dispatch = useDispatch();
    const [userInfo, setUserInfo] = useState(null);
    const [dialogOpen, setDialogOpen] = useState(false);

    useEffect(() => {
        if (!authState || !authState.isAuthenticated) {
            setUserInfo(null);
            dispatch(clearUserInfomation());
        } else {
            oktaAuth.getUser().then((info) => {
                setUserInfo(info);
                dispatch(setUserInfomation(info));
            });
        }
    }, [authState, oktaAuth]);

    if (!authState) {
        return null;
    }

    const handleDialogClose = () => {
        setDialogOpen(false);
        window.location.href = '/home';
    };

    // Helper to determine if current page is Analysis
    const isOnAnalysis = location.pathname === '/analysis';

    return (
        <>
            <ul className='navbarContainer'>
                <li>
                    <img src="../asset/emerson-logo.png" className={style.logo} width="100px" />
                </li>
                {authState.isAuthenticated && (
                    <li>
                        <NavLink
                            to={{
                                pathname: '/home',
                                state: isOnAnalysis ? { fromAnalysis: true } : undefined
                            }}
                            activeClassName={style.active}
                        >
                            Home
                        </NavLink>
                    </li>
                )}
                {authState.isAuthenticated && (
                    <li className={style.dropdown + ' ' + style.firstDropdown}>
                        <span className={style.firstMenu}>Tools</span>
                        <div className={`${style.dropdownContent} ${style.animatedDropdown}`}>
                            <NavLink to={'/changePaths'}>Change Paths</NavLink>
                        </div>
                    </li>
                )}
                {/* {authState.isAuthenticated && (
                    <li>
                        <NavLink to={'/help'} activeClassName={style.active}>
                            Help
                        </NavLink>
                    </li>
                )} */}
                {authState.isAuthenticated && (
                    <div className={style.rightMenu}>
                        <HelpDocuments />
                        <div className={style.dropdown}>
                            <AccountCircleOutlinedIcon className={style.icon} />
                            <div className={`${style.dropdownContent} ${style.animatedDropdown} ${style.userInfo}`}>
                                <a onClick={() => oktaAuth.signOut()}>Logout</a>
                            </div>
                        </div>
                    </div>
                )}
            </ul>
            <Suspense fallback={<div></div>}>
                <CustomDialog
                    className='dialogOpen'
                    open={dialogOpen}
                    handleClose={handleDialogClose}
                    content={`You don't have permission to access this page. Please contact your administrator.`}
                    actions={[{ label: 'OK', onClick: handleDialogClose }]}
                />
            </Suspense>
        </>
    );
};

export default Navbar;