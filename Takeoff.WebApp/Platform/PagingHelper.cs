using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Takeoff
{

    /// <summary>
    /// Handles the simple math associated with pagination.
    //'under the covers, this stores only 3 values:  page size, total number of items, and the current page index
    /// </summary>
    public class PagingHelper
    {
        int? pageSize, totalItemCount, indexOfFirstItemOnPage;//use indexOfFirstItemOnPage so if the page size changes after setting the current page, the CurrentPage will automatically update

        /// <summary>
        /// The number of available pages.  If there are no items or pagesize hasn't been set, it will return 0.
        /// </summary>
        public int PageCount
        {
            get
            {
                if (pageSize.HasValue && totalItemCount.HasValue)
                {
                    return (int)Math.Ceiling((totalItemCount.Value + 0.0) / (pageSize.Value + 0.0));
                }
                return 0;
            }
            set
            {
                if (!pageSize.HasValue)
                    throw new InvalidOperationException("You must set PageSize before PageCount.");
                TotalItemCount = value * PageSize;
            }
        }


        /// <summary>
        /// The maximum number of items per page.
        /// </summary>
        public int PageSize
        {
            get
            {
                return pageSize.GetValueOrDefault(0);
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException();

                pageSize = value;
            }
        }


        /// <summary>
        /// Gets or sets the index of the currently displayed page. 
        /// </summary>
        /// <remarks>This is 0 based, so when displaying PageIndex in user messages, you should typically add 1 to it.</remarks>
        public int PageIndex
        {
            get
            {
                int pageSize = PageSize;
                if (pageSize <= 0)
                    return 0;

                return (FirstItemIndex / pageSize);
            }
            set
            {
                if (value < 0 || value >= PageCount)
                    throw new ArgumentOutOfRangeException();
                FirstItemIndex = value * PageCount;
            }
        }

        /// <summary>
        /// Page index + 1
        /// </summary>
        public int PageNumber
        {
            get
            {
                return PageIndex + 1;
            }
        }

        /// <summary>
        /// The total number of items in all pages.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public int TotalItemCount
        {
            get
            {
                return totalItemCount.GetValueOrDefault(0);
            }
            set
            {
                totalItemCount = value;

                if (value < indexOfFirstItemOnPage.GetValueOrDefault())
                {
                    throw new ArgumentOutOfRangeException("TotalItemCount must be less than FirstItemIndex");
                }
            }
        }

        /// <summary>
        /// The number of records in the current page.  This could be less than the PageSize and is a more exact number.
        /// </summary>
        public int CurrentPageSize
        {
            get
            {
                //there is no page size set, so the actual page size is all the objects
                if (PageSize <= 0) 
                    return TotalItemCount;

                //There are fewer items in this page than it can hold, so return the last item index
                if (FirstItemIndex + PageSize > TotalItemCount)
                    return TotalItemCount - FirstItemIndex;
                else
                    return PageSize;
            }
        }


        /// <summary>
        /// The zero-based data index of the first item on the current page across all pages.  So if we are on page 1 (actually the second page) and page size is 20, this will return 20.
        /// </summary>
        public int FirstItemIndex
        {
            get
            {
                return indexOfFirstItemOnPage.GetValueOrDefault(-1);
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
                indexOfFirstItemOnPage = value;
            }
        }


        public bool IsLastPage
        {
            get
            {
                return PageIndex == PageCount - 1;
            }
        }

        /// <summary>
        /// Gets the data index of an item on this page.
        /// </summary>
        /// <param name="currPageItemIndex">The 0-based index of the item in this page.</param>
        /// <returns></returns>
        internal int GetDataIndex(int currPageItemIndex)
        {
            if (currPageItemIndex < 0 || currPageItemIndex >= PageSize)
                throw new ArgumentException("Must be between 0 and PageSize-1.", "currPageItemIndex");

            return this.FirstItemIndex + currPageItemIndex;
        }

    }

}