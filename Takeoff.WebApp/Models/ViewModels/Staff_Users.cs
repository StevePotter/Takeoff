using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;
using Takeoff.Models;
using AutoMapper;

namespace Takeoff.ViewModels
{

    public class Staff_Users_Details
    {
        public int Id { get; set; }

        [DisplayName("Account Id")]
        public int? AccountId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public bool IsVerified { get; set; }

        public string VerificationKey { get; set; }

    }


    public class Staff_Users_Edit
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Email")]
        public string Email { get; set; }

        [DisplayName("New Password")]
        public string Password { get; set; }

        [DisplayName("Is Email Verified")]
        public bool IsVerified { get; set; }
    }

}


