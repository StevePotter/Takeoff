using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;
using Takeoff.Models;

namespace Takeoff.ViewModels
{
    /// <summary>
    /// /comments/create.
    /// </summary>
    public class Comments_Create
    {
        public int VideoId { get; set; }
        [HasCharacters(CharsThatMatter.NonWhitespace)]
        public string Body { get; set; }
        public string UserName { get; set; }
        public double? StartTime { get; set; }
    }

 
}


