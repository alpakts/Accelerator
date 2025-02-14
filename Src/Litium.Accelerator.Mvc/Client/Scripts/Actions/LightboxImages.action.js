import {
    LIGHTBOX_IMAGES_SET_CURRENT_IMAGE,
    LIGHTBOX_IMAGES_SHOW,
} from '../constants';

export const setCurrentIndex = (index) => ({
    type: LIGHTBOX_IMAGES_SET_CURRENT_IMAGE,
    payload: {
        index,
    },
});

export const show = (visible) => ({
    type: LIGHTBOX_IMAGES_SHOW,
    payload: {
        visible,
    },
});
