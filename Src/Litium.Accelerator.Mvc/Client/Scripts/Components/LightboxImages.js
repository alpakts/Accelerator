import React, { Fragment, useCallback } from 'react';
import Modal from 'react-modal';
import 'react-responsive-carousel/lib/styles/carousel.min.css';
import { Carousel as Lightbox } from 'react-responsive-carousel';
import { useSelector, useDispatch } from 'react-redux';
import { show, setCurrentIndex } from '../Actions/LightboxImages.action';

const customStyles = {
    content: {
        background: 'none',
        padding: 0,
        border: 'none',
    },
    overlay: {
        background: 'rgba(0, 0, 0, 0.8)',
        zIndex: 2003,
    },
};
const LightboxSettings = {
    showStatus: false,
    showIndicators: false,
    useKeyboardArrows: true,
    thumbWidth: 50,
    autoPlay: false,
    autoFocus: true,
};

const renderThumbnails = (children) => {
    const images = children.map((item, index) => {
        if (!item || !item.props || !item.props.children) {
            return undefined;
        }

        // find img to get source
        const imageElement = item.props.children.find(
            (ele) => ele.type === 'img'
        );
        if (!imageElement || !imageElement.props || !imageElement.props.src) {
            return undefined;
        }

        return (
            <div
                key={index}
                style={{
                    backgroundImage: 'url(' + imageElement.props.src + ')',
                }}
                className="thumbnail__image"
            />
        );
    });

    return images;
};

const LightboxImages = (props) => {
    const dispatch = useDispatch();
    const { visible, index } = useSelector((state) => state.lightboxImages);
    const onClose = useCallback(() => dispatch(show(false)), [dispatch]);
    const onClickThumbnail = (index) => {
        dispatch(show(true));
        dispatch(setCurrentIndex(index));
    };

    return !props.images || props.images.length < 1 ? (
        <Fragment />
    ) : (
        <Fragment>
            <div className="row product-images">
                <div className="small-12 large-9 columns large-order-1">
                    <figure className="product-detail__image-container">
                        <a
                            data-src={props.images[0].src}
                            href="#"
                            itemProp="url"
                            onClick={(e) => {
                                e.preventDefault();
                                onClickThumbnail(0);
                            }}
                            className="product-image"
                            rel="nofollow"
                            dangerouslySetInnerHTML={{
                                __html: props.images[0].html,
                            }}
                        ></a>
                    </figure>
                </div>
                <div className="small-12 large-3 columns medium-flex-dir-column">
                    <div className="row">
                        {props.images.map(
                            (image, index) =>
                                index > 0 && (
                                    <div
                                        className="product-detail__image-container columns large-12"
                                        key={index}
                                    >
                                        <a
                                            data-src={image.src}
                                            href="#"
                                            itemProp="url"
                                            onClick={(e) => {
                                                e.preventDefault();
                                                onClickThumbnail(index);
                                            }}
                                            className="product-image"
                                            rel="nofollow"
                                            dangerouslySetInnerHTML={{
                                                __html:
                                                    props.images[index].html,
                                            }}
                                        ></a>
                                    </div>
                                )
                        )}
                    </div>
                </div>
            </div>
            <Modal
                ariaHideApp={false}
                preventScroll={true}
                isOpen={visible}
                onRequestClose={onClose}
                style={customStyles}
                shouldCloseOnOverlayClick={false}
            >
                <Lightbox
                    {...LightboxSettings}
                    selectedItem={index}
                    className="light-box"
                    renderThumbs={renderThumbnails}
                >
                    {props.images.map((value, index) => (
                        <div
                            key={`figure${index}`}
                            className="light-box__container"
                        >
                            <button
                                onClick={onClose}
                                className="light-box__close-btn"
                            >
                                &times;
                            </button>
                            <img src={value.src} className="light-box__image" />
                        </div>
                    ))}
                </Lightbox>
            </Modal>
        </Fragment>
    );
};

export default LightboxImages;
