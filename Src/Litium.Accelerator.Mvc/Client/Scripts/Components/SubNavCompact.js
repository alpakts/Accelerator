import React, { Fragment } from 'react';

const SubNavCompact = ({ subNavigation }) => (
    <nav className="subnav subnav--compact" role="navigation">
        {displaySubNav(subNavigation)}
    </nav>
);

const displaySubNav = (subnav) =>
    subnav && (
        <ul className="subnav__list">
            {subnav.map(({ name, url, isSelected, links }, index) => (
                <li
                    className={`subnav__item ${
                        links && links.length > 0 ? 'has-children' : ''
                    } ${isSelected ? 'active' : ''}`}
                    key={`subnavCompact-${index}`}
                >
                    <a
                        className="subnav__link"
                        href={url ?? '#'}
                        rel={url && url.length > 0 ? '' : 'nofollow'}
                    >
                        {name}
                    </a>
                    {displaySubNav(links)}
                </li>
            ))}
        </ul>
    );

export default SubNavCompact;
