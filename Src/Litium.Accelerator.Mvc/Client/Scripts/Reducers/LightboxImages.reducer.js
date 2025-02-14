import {
    LIGHTBOX_IMAGES_SET_CURRENT_IMAGE,
    LIGHTBOX_IMAGES_SHOW,
} from '../constants';

const defaultState = {
    index: 0,
    visible: false,
};

export const lightboxImages = (state = defaultState, action) => {
    const { type, payload } = action;
    switch (type) {
        case LIGHTBOX_IMAGES_SET_CURRENT_IMAGE:
        case LIGHTBOX_IMAGES_SHOW:
            return {
                ...state,
                ...payload,
            };
        default:
            return state;
    }
};
