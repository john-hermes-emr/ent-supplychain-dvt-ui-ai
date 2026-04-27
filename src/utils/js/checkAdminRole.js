import axios from 'axios';
import { ApiHelper } from '../../dvt_api/ApiHelper';
import { DvtApiPaths } from '../../dvt_api/DvtApiPaths';
import { setIsAdmin } from '../../redux/reducers/userPermissionsSliceReducer';

export const checkAdminRole = async (email, dispatch, setLoader, setDialogOpen, currentPath) => {
    try {
        if (setLoader) setLoader(true);
        const response = await axios.get(ApiHelper.getApiUrlWithId(DvtApiPaths.Users.GetUserRole, email));
        const isAdmin = response.data;
        if (dispatch) dispatch(setIsAdmin(isAdmin));
        if (!isAdmin && currentPath === '/users' && setDialogOpen) {
            setDialogOpen(true);
        }
        if (setLoader) setLoader(false);
    } catch (error) {
        if (setLoader) setLoader(false);
        console.error('Error checking user role:', error);
    }
};
