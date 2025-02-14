import { PaginationOptions } from '../constants';

export const calculatePager = (
    totalCount = 0,
    currentPage = 1,
    options = {}
) => {
    const {
        pageSize = PaginationOptions.PageSize,
        displayedEntries = PaginationOptions.DisplayedEntries,
        edgeEntries = PaginationOptions.EdgeEntries,
    } = options;

    const pageCount =
        pageSize != 0
            ? parseInt(Math.ceil(parseFloat(totalCount) / pageSize))
            : 0;
    const interval = getInterval(pageCount, currentPage, displayedEntries);

    return {
        totalCount,
        pageSize,
        currentPageIndex: currentPage,
        pageCount,
        edgeEntries,
        intervalStart: interval[0],
        intervalEnd: interval[1],
    };
};

const getInterval = (pageCount, currentPageIndex, displayedEntries) => {
    const internalPageIndex = currentPageIndex - 1;
    const half = parseInt(Math.ceil(parseFloat(displayedEntries / 2)));
    const upperLimit = pageCount - displayedEntries;
    const start =
        internalPageIndex > half
            ? Math.max(Math.min(internalPageIndex - half, upperLimit), 0)
            : 0;
    const end =
        internalPageIndex > half
            ? Math.min(internalPageIndex + half, pageCount)
            : Math.min(displayedEntries, pageCount);

    return [start, end];
};
