import React, { useState, useMemo } from 'react';

export const AccordionPanel = (props) => props;

export const Accordion = ({ children, className }) => {
    const [index, setIndex] = useState(-1);
    const handleClickHeader = (index) => {
        setIndex((prevIndex) => (prevIndex === index ? -1 : index));
    };

    const activeClass = (itemIndex) => (itemIndex === index ? 'active' : '');

    const accordions = useMemo(() => React.Children.toArray(children), [
        children,
    ]);

    const headers = accordions.map((accordion, index) => (
        <div className="columns" key={`accordion__header-${index}`}>
            <div
                className={`accordion__header ${activeClass(index)} ${
                    accordion.props.icon || ''
                }`}
                onClick={() => handleClickHeader(index)}
            >
                {accordion.props.header || ''}
            </div>
        </div>
    ));

    const panels = accordions.map((accordion, index) => (
        <div
            className={`accordion__panel ${activeClass(index)}`}
            key={`accordion__panel-${index}`}
        >
            {accordion.props.children}
        </div>
    ));

    return (
        <div className={className}>
            <nav className={`accordion__header-container`}>{headers}</nav>
            {panels}
        </div>
    );
};
