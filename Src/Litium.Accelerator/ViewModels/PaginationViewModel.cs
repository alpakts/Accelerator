using System;

namespace Litium.Accelerator.ViewModels
{
    /// <summary>
    /// Class PaginationViewModel.
    /// </summary>
    public class PaginationViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationViewModel" /> class.
        /// </summary>
        /// <param name="totalCount">The total count.</param>
        /// <param name="currentPage">The current page.</param>
        /// <param name="pageSize">Size of the page.</param>
        public PaginationViewModel(int totalCount, int currentPage, int pageSize = 10)
        {
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPageIndex = currentPage;
            PageCount = pageSize != 0 ? Convert.ToInt32(Math.Ceiling(Convert.ToDouble(totalCount) / PageSize)) : 0;

            var interval = GetInterval();
            IntervalStart = interval[0];
            IntervalEnd = interval[1];
        }

        /// <summary>
        /// Gets the index of the current page.
        /// </summary>
        /// <value>The index of the current page.</value>
        public int CurrentPageIndex { get; }
        /// <summary>
        /// Specifies how many links to show excluding possible EdgeEntries
        /// </summary>
        /// <value> The displayed entries. </value>
        public int DisplayedEntries { get; set; } = 4;
        /// <summary>
        /// Specifies how many links from beginning and end to show ex. 1 2 3 ... 10 20 30 ... 97 98 99 where "1 2 3" and "97 98 99" are edge entries
        /// </summary>
        /// <value> Number of edge entries </value>
        public int EdgeEntries { get; set; } = 2;
        /// <summary>
        /// Gets the interval end.
        /// </summary>
        /// <value>The interval end.</value>
        public int IntervalEnd { get; }
        /// <summary>
        /// Gets the interval start.
        /// </summary>
        /// <value>The interval start.</value>
        public int IntervalStart { get; }
        /// <summary>
        /// Gets the page count.
        /// </summary>
        /// <value>The page count.</value>
        public int PageCount { get; }
        /// <summary>
        /// Gets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        public int PageSize { get; }
        /// <summary>
        /// Gets the total count.
        /// </summary>
        /// <value>The total count.</value>
        public int TotalCount { get; }

        private int[] GetInterval()
        {
            var internalPageIndex = CurrentPageIndex - 1;
            var half = Math.Ceiling(Convert.ToDouble(DisplayedEntries / 2));
            double upperLimit = PageCount - DisplayedEntries;
            var start = internalPageIndex > half ? Math.Max(Math.Min(internalPageIndex - half, upperLimit), 0) : 0;
            var end = internalPageIndex > half ? Math.Min(internalPageIndex + half, PageCount) : Math.Min(DisplayedEntries, PageCount);

            return new[] { Convert.ToInt32(start), Convert.ToInt32(end) };
        }
    }
}
