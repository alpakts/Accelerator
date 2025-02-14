import React from 'react';
import NavigationItem from './NavigationItem';
import { useSelector } from 'react-redux';

const Navigation = () => {
    const contentLinks = useSelector((state) => state.navigation.contentLinks);

    return <NavigationItem links={contentLinks} />;
};

export default Navigation;
