import React from 'react';
import 'react-responsive-carousel/lib/styles/carousel.min.css';
import { Carousel } from 'react-responsive-carousel';

const CarouselSettings = {
    showStatus: false,
    showThumbs: false,
    infiniteLoop: true,
};

const Slider = ({ values }) => (
    <Carousel {...CarouselSettings}>
        {values.map((value, index) => (
            <div
                key={`figure${index}`}
                dangerouslySetInnerHTML={{
                    __html: value.html,
                }}
            ></div>
        ))}
    </Carousel>
);

export default Slider;
