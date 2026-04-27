import { getTokenStorage } from "../utils/js/globalFunctions";
export const ApiHelper = {
    getApiUrl: function (url) {
        return process.env.REACT_APP_DVT_API_ROOT_PATH + "/api/v1" + url;
    },
    getApiUrlWithId: function (url, id) {
        return process.env.REACT_APP_DVT_API_ROOT_PATH + "/api/v1" + url.replace("{id}", id);
    },
    getApiUrlWithSearchKey: function (url, searchString) {
        return process.env.REACT_APP_DVT_API_ROOT_PATH + "/api/v1" + url.replace("{searchString}", searchString);
    },
    //Get Access token from local storage, build the beare token and return to calling function    
    getBearerToken: function () {
        const oktaTokenStorage = getTokenStorage();
        if (oktaTokenStorage != null) {
            const tokenType = oktaTokenStorage.accessToken.tokenType;
            const tokenValue = oktaTokenStorage.accessToken.accessToken;
            return tokenType + " " + tokenValue;
        }
    }
}