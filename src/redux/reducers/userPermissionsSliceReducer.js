import { createSlice } from '@reduxjs/toolkit';

const initialState = {
    permissions: [],
    isAdmin: false,
};

const userPermissionsSlice = createSlice({
    name: 'userPermissions',
    initialState,
    reducers: {
        setUserPermissions(state, action) {
            state.permissions = action.payload;
        },
        setIsAdmin(state, action) {
            state.isAdmin = action.payload;
        },
    },
});

export const {
    setUserPermissions,
    setIsAdmin
} = userPermissionsSlice.actions;

export default userPermissionsSlice.reducer;