using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;

namespace Takeoff.ViewModels
{
    /// <summary>
    /// Used by views that allow users to enter a credit card.  
    /// </summary>
    public class BillingInfo
    {
        [LocalizedDisplayName("Subscribe_FirstName_Label")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string FirstName { get; set; }

        [LocalizedDisplayName("Subscribe_LastName_Label")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string LastName { get; set; }

        [LocalizedDisplayName("Subscribe_PostalCode_Label")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string PostalCode { get; set; }

        [LocalizedDisplayName("Subscribe_CardNumber_Label")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string CreditCardNumber { get; set; }

        [LocalizedDisplayName("Subscribe_CardNumber_Label")]
        [Range(1, 12)]
        public int CreditCardMonth { get; set; }
        
        [MethodValidaton()]
        public int CreditCardYear { get; set; }

        /// <summary>
        /// The months to populate the credit card year dropdown with.
        /// </summary>
        public IEnumerable<SelectListItem> CreditCardMonths
        {
            get
            {
                var dateCulture = System.Globalization.DateTimeFormatInfo.InvariantInfo;
                return 1.UpTo(12).Select(i =>
                new SelectListItem
                {
                    Value = i.ToInvariant(),
                    Text = i.ToString("D2") + " - " + dateCulture.GetMonthName(i),
                });
            }
        }

        /// <summary>
        /// The years to populate the credit card year dropdown with.
        /// </summary>
        public IEnumerable<SelectListItem> CreditCardYears
        {
            get
            {
                return (DateTime.Today.Year).UpTo(DateTime.Today.Year + 10).Select(i =>
                new SelectListItem
                {
                    Value = i.ToInvariant(),
                    Text = i.ToInvariant(),
                });
            }
        }


        [DisplayName("CVV Code")]
        public string CreditCardVerificationCode { get; set; }

        public bool IsCreditCardYearValid(int value)
        {
            return value >= DateTime.UtcNow.Year;
        }
    }

    /// <summary>
    /// Used to show plans in pricing and upgrade pages.
    /// </summary>
    public class PlanForSale
    {
        public PlanForSale(Takeoff.Data.IPlan plan)
        {
            this.Id = plan.Id;
            this.Title = plan.Title;
            this.VideosPerBillingCycleMax = plan.VideosPerBillingCycleMax;
            this.AssetsTotalMaxSize = plan.AssetsTotalMaxSize;
            this.ProductionLimit = plan.ProductionLimit;
            this.Price = plan.PriceInCents/100.0;
        }

        public string Id { get; set; }

        public string Title { get; set; }

        public int? VideosPerBillingCycleMax { get; set; }

        public FileSize? AssetsTotalMaxSize { get; set; }

        public int? ProductionLimit { get; set; }

        public double Price { get; set; }
    }

    public class UserSummary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class AppLogo
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class DataTableParams
    {
        /// <summary>
        /// A value passed from the client to here and then back to identify the request and verify integrity
        /// </summary>
        public string Echo { get; set; }

        public int DisplayStart { get; set; }
        public int DisplayLength { get; set; }

        public string Search { get; set; }

        /// <summary>
        /// A list of strings that contain column names and sort direction.  Example: "Name ASC", "IsHere DESC"
        /// </summary>
        public string[] SortBy { get; set; }
    }


    public class DataTableResponse
    {
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public string sEcho { get; set; }
       // public string sColumns { get; set; }
        public object aaData { get; set; }
    }

    /// <summary>
    /// Parameters passed in from the datatables jquery plugin.
    /// </summary>
    public class DataTablesParam
    {
        public int iDisplayStart { get; set; }
        public int iDisplayLength { get; set; }
        public int iColumns { get; set; }
        public string sSearch { get; set; }
        public bool bEscapeRegex { get; set; }
        public int iSortingCols { get; set; }
        public string sEcho { get; set; }
        public List<bool> bSortable { get; set; }
        public List<bool> bSearchable { get; set; }
        public List<string> sSearchColumns { get; set; }
        public List<int> iSortCol { get; set; }
        public List<string> sSortDir { get; set; }
        public List<bool> bEscapeRegexColumns { get; set; }

        public DataTablesParam()
        {
            bSortable = new List<bool>();
            bSearchable = new List<bool>();
            sSearchColumns = new List<string>();
            iSortCol = new List<int>();
            sSortDir = new List<string>();
            bEscapeRegexColumns = new List<bool>();
        }
    }

}


