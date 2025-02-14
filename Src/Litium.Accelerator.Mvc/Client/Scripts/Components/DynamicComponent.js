import React, { lazy, Suspense } from 'react';

const DynamicComponent = ({ loader, loading = <div></div> }) => {
    const Component = lazy(loader);
    return (props) => (
        <Suspense fallback={loading}>
            <Component {...props} />
        </Suspense>
    );
};

export default DynamicComponent;
