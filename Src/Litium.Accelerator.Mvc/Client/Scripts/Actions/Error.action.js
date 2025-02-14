export const catchError = (ex, onError) => (dispatch) => {
    if (ex.response) {
        ex.response.json().then((error) => dispatch(onError(error)));
    } else {
        dispatch(onError(ex));
    }
};
