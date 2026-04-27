export const getUpdatedBy = () => {
    let oktaInformation = JSON.parse(localStorage.getItem("okta-token-storage"))
    const hasEmail = oktaInformation && oktaInformation.idToken && oktaInformation.idToken.claims && oktaInformation.idToken.claims.email
    let updatedby = hasEmail ? oktaInformation.idToken.claims.email : null
    return updatedby
}

export const getTokenStorage = () => {
    let oktaTokenStorage = JSON.parse(localStorage.getItem("okta-token-storage"))
    return oktaTokenStorage && Object.keys(oktaTokenStorage).length ? oktaTokenStorage : null
}
