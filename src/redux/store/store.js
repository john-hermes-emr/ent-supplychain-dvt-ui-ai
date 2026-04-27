import { configureStore } from '@reduxjs/toolkit';
import counterReducer from '../reducers/counterSliceReducer';
import userPermissionsReducer from "../reducers/userPermissionsSliceReducer";
import userInfoReducer from "../reducers/userInfoSliceReducer";

export default configureStore({
    reducer: {
        counter: counterReducer,
        userPermissions: userPermissionsReducer,
        userInfo: userInfoReducer,
    }
});
