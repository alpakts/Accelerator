import React, { useEffect, useRef } from 'react';

const scriptPattern = /<script\b[^>]*>([\s\S]*?)<\/script>/gi;
const scriptFilePattern = /<script.*?src=["'](.*?)["']/gi;

const extractScripts = (domString) => {
    let matches,
        html = domString;
    const scripts = [],
        scriptFiles = [];
    while ((matches = scriptPattern.exec(domString)) !== null) {
        html = html.replace(matches[0], '');
        matches[1] && matches[1].trim() !== '' && scripts.push(matches[1]);
    }
    while ((matches = scriptFilePattern.exec(domString)) !== null) {
        matches[1] && matches[1].trim() !== '' && scriptFiles.push(matches[1]);
    }

    return {
        html,
        scripts,
        scriptFiles,
    };
};

const executeScript = (domId, scriptContent) => {
    const script = document.createElement('script');
    script.type = 'text/javascript';
    try {
        script.appendChild(document.createTextNode(scriptContent));
    } catch (e) {
        // to support IE
        script.text = scriptContent;
    }
    document.getElementById(domId).appendChild(script);
};

const includeScript = (domId, srciptUrl) => {
    const script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = srciptUrl;
    document.getElementById(domId).appendChild(script);
};

const CheckoutWidget = ({ responseString, id }) => {
    const extractScriptsResult = extractScripts(responseString);
    const firstRender = useRef(true);

    useEffect(() => {
        // Make sure it is executed only once.
        // firstRender is used to prevent eslint warning when having empty dependencies array
        if (!firstRender.current) {
            return;
        }
        firstRender.current = false;
        extractScriptsResult.scripts &&
            extractScriptsResult.scripts.forEach((script) =>
                executeScript(id, script)
            );
        extractScriptsResult.scriptFiles &&
            extractScriptsResult.scriptFiles.forEach((url) =>
                includeScript(id, url)
            );
    }, [extractScriptsResult, id]);

    return (
        <div
            id={id}
            dangerouslySetInnerHTML={{ __html: extractScriptsResult.html }}
        />
    );
};

export default CheckoutWidget;
