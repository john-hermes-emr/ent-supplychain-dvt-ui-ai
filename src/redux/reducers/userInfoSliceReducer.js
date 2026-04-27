import { createSlice } from '@reduxjs/toolkit';

const initialState = {
    name: '',
    email: '',
    loggedIn: false,
};

const userInfoSlice = createSlice({
    name: 'userInfo',
    initialState,
    reducers: {
        setUserInfomation: (state, action) => {
            state.name = action.payload.name;
            state.email = action.payload.email;
            state.loggedIn = true;
        },
        clearUserInfomation: (state) => {
            state.name = '';
            state.email = '';
            state.loggedIn = false;
        },
    },
});

export const { setUserInfomation, clearUserInfomation } = userInfoSlice.actions;

export default userInfoSlice.reducer;