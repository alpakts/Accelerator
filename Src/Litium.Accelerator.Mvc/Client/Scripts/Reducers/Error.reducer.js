export const error = (state = {}, action) => {
    const { error } = action.payload;
    if (!error) {
        return state;
    }
    if (error.modelState) {
        return error.modelState;
    }
    if (error.name === 'ValidationError') {
        return {
            [error.path]: error.errors,
        };
    }
    if (typeof error === 'object') {
        const errorFormatted = {};
        Object.keys(error).forEach((key) => {
            errorFormatted[_camelCase(key)] = error[key];
        });
        return errorFormatted;
    }

    return state;
};

const _camelCase = (input) => {
    if (typeof input !== 'string') return '';
    return input.charAt(0).toLocaleLowerCase() + input.slice(1);
};
