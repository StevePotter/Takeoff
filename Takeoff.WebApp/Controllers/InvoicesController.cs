using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mediascend.Web;
using Takeoff.Data;
using Takeoff.ViewModels;
using System.Net;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Recurly;

namespace Takeoff.Controllers
{
    /// <summary>
    /// Lets customers view their invoices.  Part of Account
    /// </summary>
    [SubController("/account/", false)]
    [RestrictIdentity(RequireAccountStatus = new[]{ AccountStatus.Subscribed } )]
    public class InvoicesController : BasicController
    {

        public ActionResult Index()
        {
            var user = this.UserThing();
            var account = user.Account;

            XDocument xDoc = null;
            RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Get, "/accounts/" + account.Id.ToInvariant() + "/invoices", (reader =>
                {
                    xDoc = XDocument.Load(reader);
                }));

            var invoices = xDoc.Descendants("invoice").Select(i => new Invoices_InvoiceSummary
            {
                Date = i.Element("date").Value.ConvertTo<DateTime>(),
                Id = i.Element("id").Value,
                Number = i.Element("invoice_number").Value.ConvertTo<int>(),
                Total = i.Element("total_in_cents").Value.ConvertTo<double>() / 100.0,
            }).ToArray();

            return View(invoices);
        }


        public ActionResult Details(string id)
        {
            var user = this.UserThing();
            var account = user.Account;

            var invoice = GetRecurlyInvoice(id);
            if (!invoice.AccountCode.EqualsCaseSensitive(account.Id.ToInvariant()))
                throw new ArgumentException("Invoice ID is not from this account.");
            return View(invoice);
        }

        /// <summary>
        /// Recurly .net api's RecurlyInvoice.Get was missing a bunch of fields.  So I had to build my own.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static Invoices_Invoice GetRecurlyInvoice(string id)
        {
            XDocument xDoc = null;
            RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Get, "/invoices/" + id, (reader =>
            {
                xDoc = XDocument.Load(reader);
            }));

            var invoice = new Invoices_Invoice();
            foreach (var invoiceElement in xDoc.Root.Elements())
            {
                switch (invoiceElement.Name.LocalName)
                {
                    case "id":
                        invoice.Id = invoiceElement.Value;
                        break;
                    case "account_code":
                        invoice.AccountCode = invoiceElement.Value;
                        break;
                    case "date":
                        invoice.Date = invoiceElement.Value.ConvertTo<DateTime>();
                        break;
                    case "invoice_number":
                        invoice.Number = invoiceElement.Value.ConvertTo<int>();
                        break;
                    case "subtotal_in_cents":
                        invoice.Subtotal = invoiceElement.Value.ConvertTo<double>() / 100.0;
                        break;
                    case "total_in_cents":
                        invoice.Total = invoiceElement.Value.ConvertTo<double>() / 100.0;
                        break;
                    case "paid_in_cents":
                        invoice.Paid = invoiceElement.Value.ConvertTo<double>() / 100.0;
                        break;
                    case "total_due_in_cents":
                        invoice.TotalDue = invoiceElement.Value.ConvertTo<double>() / 100.0;
                        break;
                    case "line_items":
                        invoice.LineItems = invoiceElement.Elements().Select(lineItemElement =>
                        {
                            var lineItem = new Invoices_InvoiceLineItem();
                            foreach (var lineItemField in lineItemElement.Elements())
                            {
                                switch (lineItemField.Name.LocalName)
                                {
                                    case "amount_in_cents":
                                        lineItem.Amount = lineItemField.Value.ConvertTo<double>() / 100.0;
                                        if (lineItemElement.Element("type").Value.EqualsCaseSensitive("credit"))
                                            lineItem.Amount *= -1;
                                        break;
                                    case "start_date":
                                        lineItem.StartDate = lineItemField.Value.ConvertTo<DateTime>();
                                        break;
                                    case "end_date":
                                        lineItem.EndDate = lineItemField.Value.ConvertTo<DateTime>();
                                        break;
                                    case "description":
                                        lineItem.Description = lineItemField.Value;
                                        break;
                                    case "id":
                                        lineItem.Id = lineItemField.Value;
                                        break;
                                }
                            }
                            return lineItem;
                        });
                        break;
                    case "payments":
                        invoice.Payments = invoiceElement.Elements().Select(paymentElement =>
                        {
                            var payment = new Invoices_InvoicePayment();
                            foreach (var paymentField in paymentElement.Elements())
                            {
                                switch (paymentField.Name.LocalName)
                                {
                                    case "amount_in_cents":
                                        payment.Amount = paymentField.Value.ConvertTo<double>() / 100.0;
                                        break;
                                    case "date":
                                        payment.Date = paymentField.Value.ConvertTo<DateTime>();
                                        break;
                                    case "message":
                                        payment.Message = paymentField.Value;
                                        break;
                                    case "id":
                                        payment.Id = paymentField.Value;
                                        break;
                                }
                            }
                            return payment;
                        });
                        break;
                }
            }
            return invoice;
        }

    }
}


//<invoice>
//  <id>c2eb688147ed4d5282d3e1867217ac19</id>
//  <account_code>22916</account_code>
//  <date type="datetime">2011-02-10T23:04:14Z</date>
//  <invoice_number type="integer">1032</invoice_number>
//  <vat_number></vat_number>
//  <status>closed</status>
//  <subtotal_in_cents type="integer">15405</subtotal_in_cents>
//  <total_in_cents type="integer">15405</total_in_cents>
//  <vat_amount_in_cents type="integer">0</vat_amount_in_cents>
//  <paid_in_cents type="integer">15405</paid_in_cents>
//  <total_due_in_cents type="integer">0</total_due_in_cents>
//  <discount_in_cents type="integer">0</discount_in_cents>
//  <line_items type="array">
//    <line_item>
//      <id>dacefd36308c4010812ed0b35cedc0cd</id>
//      <type>charge</type>
//      <amount_in_cents type="integer">19876</amount_in_cents>
//      <start_date type="datetime">2011-02-10T23:04:13Z</start_date>
//      <end_date type="datetime">2011-03-10T18:55:15Z</end_date>
//      <description>Prorating for remainder of subscription term</description>
//      <created_at type="datetime">2011-02-10T23:04:14Z</created_at>
//      <applied_coupon_code></applied_coupon_code>
//    </line_item>
//    <line_item>
//      <id>d6e54bef0a0e498496c851ca6df7cd08</id>
//      <type>credit</type>
//      <amount_in_cents type="integer">-4471</amount_in_cents>
//      <start_date type="datetime">2011-02-10T23:04:13Z</start_date>
//      <end_date type="datetime">2011-03-10T18:55:15Z</end_date>
//      <description>Remaining value for subscription</description>
//      <created_at type="datetime">2011-02-10T23:04:15Z</created_at>
//      <applied_coupon_code></applied_coupon_code>
//    </line_item>
//  </line_items>
//  <payments type="array">
//    <payment>
//      <id>13797108958e445ea244c3b05f87de44</id>
//      <date type="datetime">2011-02-11T00:00:08Z</date>
//      <amount_in_cents type="integer">15405</amount_in_cents>
//      <message>Test Gateway: Successful test transaction</message>
//      <reference>12345</reference>
//    </payment>
//  </payments>
//</invoice>
