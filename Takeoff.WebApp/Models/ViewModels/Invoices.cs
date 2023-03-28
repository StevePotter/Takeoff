using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;

namespace Takeoff.ViewModels
{
    /// <summary>
    /// Shown in the list of invoices for a given customer.  
    /// </summary>
    public class Invoices_InvoiceSummary
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public double Total { get; set; }
    }


    public class Invoices_Invoice
    {
        public string Id { get; set; }
        public string AccountCode { get; set; }
        public DateTime Date { get; set; }
        public int Number { get; set; }
        public double Subtotal { get; set; }
        public double Total { get; set; }
        public double Paid { get; set; }
        public double TotalDue { get; set; }
        public IEnumerable<Invoices_InvoiceLineItem> LineItems { get; set; }
        public IEnumerable<Invoices_InvoicePayment> Payments { get; set; }
    }


    public class Invoices_InvoiceLineItem
    {
        public double Amount { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class Invoices_InvoicePayment
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Id { get; set; }
        public string Message { get; set; }
    }
}


