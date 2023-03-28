//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Xml.Linq;
//using Recurly;
//using Takeoff.Models;

//namespace Takeoff.DataTools.Commands
//{

//    /// <summary>
//    /// Updates first/last name and email in recurly if it's different in Takeoff. 
//    /// </summary>
//    public class SyncAccountsWithRecurly : BaseCommand
//    {
//        public SyncAccountsWithRecurly()
//        {
//            EnableXmlReport = true;
//            NotifyOnErrors = true;
//            LogJobInDatabase = true;
//        }

//        protected override void Perform(string[] commandLineArgs)
//        {
//            List<int> accountIdsInRecurly = null;
//            Step("GetRecurlyAccountIds", () =>
//                                             {
//                                                 accountIdsInRecurly = GetRecurlyAccounts(null);
//                                                 this.AddReportAttribute("RecurlyAccountCount", accountIdsInRecurly.Count());
//                                             });
//            Step("SyncAccounts", () =>
//            {
//                foreach (var accountId in accountIdsInRecurly)
//                {
//                    Step(new StepParams
//                             {
//                                 Name = "ProcessAccount",
//                                 RunIfErrorOccured = true,
//                                 Work = () =>
//                                            {
//                                                AddReportAttribute("AccountId", accountId);
//                                                var localAccount = Things.Get<AccountThing>(accountId);
//                                                var owner = localAccount.Owner;
//                                                RecurlyAccount recurlyAcccount = null;
//                                                Step("GetRecurlyAccount", false, 2, TimeSpan.FromSeconds(1), () =>
//                                                {
//                                                    recurlyAcccount = RecurlyAccount.Get(accountId.ToInvariant());
//                                                });
//                                                if (recurlyAcccount != null)
//                                                {
//                                                    bool update = false;
//                                                    if (!owner.FirstName.EqualsCaseInsensitive(recurlyAcccount.FirstName))
//                                                    {
//                                                        recurlyAcccount.FirstName = owner.FirstName;
//                                                        update = true;
//                                                    }
//                                                    if (!owner.LastName.EqualsCaseInsensitive(recurlyAcccount.LastName))
//                                                    {
//                                                        recurlyAcccount.LastName = owner.LastName;
//                                                        update = true;
//                                                    }
//                                                    if (!owner.Email.EqualsCaseInsensitive(recurlyAcccount.Email))
//                                                    {
//                                                        recurlyAcccount.Email = owner.Email;
//                                                        update = true;
//                                                    }
//                                                    if (update)
//                                                    {
//                                                        AddReportAttribute("Update", true);
//                                                        Step("UpdateRecurlyAccount", false, 2, TimeSpan.FromSeconds(1),
//                                                             () => recurlyAcccount.Update());
//                                                    }
//                                                }
//                                            }
//                             });
//                }
//            });

//        }

//        private List<int> GetRecurlyAccounts(string show)
//        {
//            PagingHelper paging = null;
//            var accountIds = new List<int>();
//            while (paging == null || !paging.IsLastPage)
//            {
//                var pageParam = "?page=" + (paging == null ? "1" : paging.PageNumber.ToInvariant());
//                XDocument xDoc = null;//todo: append page
//                RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Get, "/accounts" + pageParam + (show.HasChars() ? "&show=" + show : string.Empty), (reader =>
//                {
//                    xDoc = XDocument.Load(reader);
//                }));
//                if (paging == null)
//                {
//                    paging = new PagingHelper
//                                 {
//                                     PageSize = xDoc.Descendants("per_page").First().Value.ToInt(),
//                                     TotalItemCount = xDoc.Descendants("total_entries").First().Value.ToInt()
//                                 };
//                }

//                paging.PageIndex = xDoc.Descendants("current_page").First().Value.ToInt() - 1;//their paging is 1-based
//                WriteLine("Received Page " + paging.PageIndex.ToInvariant(), true);
//                accountIds.AddRange(xDoc.Descendants("account_code").Select(e => e.Value.ToInt()).ToArray());
//            }
//            return accountIds;
//        }

//    }

//}

