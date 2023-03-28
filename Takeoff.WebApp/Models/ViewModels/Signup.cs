using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;
using Takeoff.Models;
using Takeoff.Data;

namespace Takeoff.ViewModels
{

    public class Signup_DirectPurchase
    {
  
        [LocalizedDisplayName("Account_ViewModel_FirstName")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string FirstName { get; set; }

        [LocalizedDisplayName("Account_ViewModel_LastName")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string LastName { get; set; }

        [LocalizedDisplayName("Account_ViewModel_Email")]
        [Email(ErrorMessageResourceName = "Validation_InvalidEmail", ErrorMessageResourceType = typeof(Resources.Strings))]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Email { get; set; }

        [LocalizedDisplayName("Signup_Account_ViewModel_Password")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Password { get; set; }

        public string PlanId { get; set; }

        /// <summary>
        /// The number of hours from UTC, provided by javascript, for the user.  This allows us to convert the database's UTC times into local ones.
        /// </summary>
        public int? TimezoneOffset { get; set; }

        /// <summary>
        /// Optional url that started the signup.  This way they can be sent to it when the signup process is complete.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// The value of the "how did you hear about us" dropdown.
        /// </summary>
        public string SignupSource { get; set; }

        /// <summary>
        /// If they choose "Other" for SignupSource, this is the custom entered value.
        /// </summary>
        public string SignupSourceOther { get; set; }

    }

    public class Signup_Subscription : BillingInfo
    {
        ///// <summary>
        ///// A coupon code that is used mostly to track affiliates.
        ///// </summary>
        //public string CouponCode { get; set; }

        /// <summary>
        /// Optional url that started the signup.  This way they can be sent to it when the signup process is complete.
        /// </summary>
        public string PostSignupUrl { get; set; }

        public string PlanId { get; set; }

        public IPlan Plan { get; set; }

        /// <summary>
        /// The account that is owned by the person signing up.
        /// </summary>
        public IAccount Account { get; set; }

    }

    public class Signup_Account_SubscribeChoice
    {
        /// <summary>
        /// The link used for when the user decides to subscribe later.  
        /// </summary>
        public string SubscribeLaterUrl { get; set; }

    }

    public class Signup_Subscribe_Success
    {
        /// <summary>
        /// The url that is pointed to in the "get started" link on the page.  This will default to either the "would you like a sample" page or their dashboard.
        /// </summary>
        public string GetStartedUrl { get; set; }

    }


    public class Signup_Sample
    {
        /// <summary>
        /// The url pointed to when the user declines to create a sample.  This will default to their dashboard.
        /// </summary>
        public string DeclineUrl { get; set; }

    }


    public class Signup_Guest
    {

        [LocalizedDisplayName("Account_ViewModel_FirstName")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string FirstName { get; set; }

        [LocalizedDisplayName("Account_ViewModel_LastName")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string LastName { get; set; }

        [LocalizedDisplayName("Account_ViewModel_Email")]
        [Email(ErrorMessageResourceName = "Validation_InvalidEmail", ErrorMessageResourceType = typeof(Resources.Strings))]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Email { get; set; }

        [LocalizedDisplayName("Signup_Account_ViewModel_Password")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Password { get; set; }

        /// <summary>
        /// The number of hours from UTC, provided by javascript, for the user.  This allows us to convert the database's UTC times into local ones.
        /// </summary>
        public int? TimezoneOffset { get; set; }

    }


    public class Signup_Trial
    {
        public string ReturnUrl { get; set; }

        [LocalizedDisplayName("Account_ViewModel_FirstName")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string FirstName { get; set; }

        [LocalizedDisplayName("Account_ViewModel_LastName")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string LastName { get; set; }

        [LocalizedDisplayName("Account_ViewModel_Email")]
        [Email(ErrorMessageResourceName = "Validation_InvalidEmail", ErrorMessageResourceType = typeof(Resources.Strings))]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Email { get; set; }

        [LocalizedDisplayName("Signup_Account_ViewModel_Password")]
        [Required(ErrorMessageResourceName = "Validation_Required", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string Password { get; set; }

        /// <summary>
        /// The number of hours from UTC, provided by javascript, for the user.  This allows us to convert the database's UTC times into local ones.
        /// </summary>
        public int? TimezoneOffset { get; set; }

    }

    public class Signup_Trial_Success
    {
        public string ReturnUrl { get; set; }

    }
}


