import React from 'react';

function InfoField({ values }) {
    return (
        <div className="small-12 medium-12 columns">
            {values.map((item, idx) => (
                <>
                    <span key={idx}>{item}</span>&nbsp;
                </>
            ))}
        </div>
    );
}

export default InfoField;
