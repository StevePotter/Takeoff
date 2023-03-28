using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CommandLine;
using Takeoff.Controllers;
using System.IO;
using System.Web;
using Moq;
using System.Security.Principal;
using System.Collections;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Specialized;
using System.Web.WebPages;
using System.Web.Hosting;
using System.Reflection;
using Ninject;
using System.Threading;
using System.Linq.Expressions;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Resources;
using Takeoff.ThingRepositories;
using Takeoff.ViewModels;

namespace Takeoff.WebApp.Ripper
{
    partial class Program
    {

        private static void RenderViewsForAccount()
        {
            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Index(),
                User = User.SubscribedFreelance,
                Model = Account.SubscribedFreelance,
                View = "Index-HasAccount",
                IsDefaultViewForAction = true,
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Index(),
                //Model = new Account_Login(),
                User = User.TrialFreelance,
                Model = Account.TrialFreelanceBeta,
                Scenario = "Trial",
                View = "Index-HasAccount",
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Index(),
                User = User.Guest,
                Scenario = "NoAccount",
                View = "Index-NoAccount",
            }.Render();


            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Login(null),
                Model = new Account_Login(),
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Login(null),
                View = "Login-MustVerify",
                Scenario = "MustVerify",
                Model = new Account_Verify_Request
                {
                    Email = User.Guest.Email
                }
            }.Render();


            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Logout(null),
                View = @"Signout-Confirm",
                Model = new Account_Login(),
                User = User.SubscribedFreelance,
                Scenario = "Confirm",
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Logout(null),
                View = @"Signout-Done",
                Scenario = "Done",
            }.Render();


            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.PasswordReset(null),
                Model = new Account_PasswordReset()
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.PasswordReset(null),
                View = "PasswordReset-InvalidLink",
                Scenario = "InvalidLink",
            }.Render();



            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.PasswordReset(null),
                View = "PasswordReset-Request",
                Scenario = "Request",
                Model = new Account_PasswordReset()
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.PasswordReset(null),
                View = "PasswordReset-Request-UserNotFound",
                Scenario = "Request-UserNotFound",
                Model = new Account_PasswordReset()
            }.Render();


            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Verify(null),
                View = "Verify-Request",
                IsDefaultViewForAction = true,
                Model = new Account_Verify_Request()
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Verify(null),
                Scenario = "InvalidLink",
                View = "Verify-InvalidLink",
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Verify(null),
                User = User.Guest,
                Scenario = "AlreadyDone",
                View = "Verify-AlreadyDone",
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Verify(null),
                User = User.Guest,
                View = "Verify-Success",
                Scenario = "Success",
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = c => c.NecessaryInfo((string)null),
                Model = new Account_NecessaryInfo(),
                User = User.Guest,
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = c => c.Logo(),
                Model = new Account_Logo
                {
                    //CurrentLogoUrl = ""
                },
                User = User.SubscribedFreelance,
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = c => c.Logo(),
                Model = new Account_Logo
                {
                    CurrentLogoUrl = "DUH.PNG"
                },
                Scenario = "HasLogo",
                User = User.SubscribedFreelance,
            }.Render();


            var subscriber = User.SubscribedFreelance;
            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.MainInfo(null),
                User = subscriber,
                View = "MainInfo",
                IsDefaultViewForAction = true,
                Model = new Account_MainInfo
                {
                    FirstName = subscriber.FirstName,
                    LastName = subscriber.LastName,
                    Email = subscriber.Email,
                },
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = c => c.Billing(),
                User = subscriber,
                Model = new Account_Subscription
                {
                },
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = c => c.Billing(),
                User = subscriber,
                View = "Billing-Success",
                Scenario = "UpdateSuccess",
            }.Render();


            new ViewRenderer<AccountNotificationsController>
            {
                ActionExpr = c => c.Index(),
                Model = new Account_Notifications
                {
                    DigestEmailFrequency =
                        EmailDigestFrequency.BeforeDuringWorkDay,
                    NotifyWhenNewFile = true,
                },
                User = subscriber,
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = c => c.Privacy(),
                Model = new Account_Privacy
                {
                    EnableMembershipRequests = true,
                    AutoResponses = new Account_Privacy.AutoResponse[]
                        {
                            new Account_Privacy.AutoResponse
                                {
                                    Accept = true, 
                                    Id=5,
                                    IsInvitation = true,
                                    TargetUserEmail = "bill@bill.com",
                                    TargetUserName = "Billy Boy",
                                },

                            new Account_Privacy.AutoResponse
                                {
                                    Accept = false, 
                                    Id=8,
                                    IsInvitation = true,
                                    TargetUserEmail = "ron@bill.com",
                                    TargetUserName = "Ronny Boy",
                                },

                        }
                },
                User = subscriber,
            }.Render();


            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Close(),
                User = subscriber,
                IsDefaultViewForAction = true,
                View = "Close-Guest",
            }.Render();
            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Close(),
                User = subscriber,
                Scenario = "Subscriber",
                View = "Close-AccountHolder",
                FillViewData = d =>
                {
                    d["IsSubscribed"] = true;
                }
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Close(),
                View = "Closed-Guest",
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Close(),
                View = "Closed-Subscriber",
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Close(),
                View = "Closed-Trial",
            }.Render();

            new ViewRenderer<AccountController>
            {
                ActionExpr = a => a.Close(),
                View = "Closed",
            }.Render();

        }

        private static void RenderViewsForAccountMembers()
        {
            new ViewRenderer<AccountMembersController>
            {
                ActionExpr = c => c.Index(),
                User = User.SubscribedFreelance,
                Model = new AccountMembers_Index
                {
                    Memberships = new AccountMembers_Membership[]
                                                      {
                                                          new AccountMembers_Membership
                                                              {
                                                                  MembershipId = 1,
                                                                  Role = "Staff",
                                                                  MemberEmail = User.View1.Email,
                                                                  MemberName = User.View1.Name,
                                                                  MemberId = User.View1.Id,
                                                              },
                                                          new AccountMembers_Membership
                                                              {
                                                                  MembershipId = 2,
                                                                  Role = "Client",
                                                                  MemberEmail = User.View2.Email,
                                                                  MemberName = User.View2.Name,
                                                                  MemberId = User.View2.Id,
                                                              }
                                                      }
                },
            }.Render();

            //account/members/{id}
            new ViewRenderer<AccountMembersController>
            {
                ActionExpr = c => c.Details(0),
                User = User.SubscribedFreelance,
                Model = new AccountMembers_Details
                {
                    Membership = new AccountMembers_Membership
                    {
                        MembershipId = 2,
                        Role = "Client",
                        MemberEmail = User.View2.Email,
                        MemberName = User.View2.Name,
                        MemberId = User.View2.Id,
                    },
                    Productions = new AccountMembers_Details.Production[]
                                                      {
                                                          new AccountMembers_Details.Production
                                                              {
                                                                  Id = 3,
                                                                  Title = "First One"
                                                              },
                                                          new AccountMembers_Details.Production
                                                              {
                                                                  Id = 4,
                                                                  Title = "Another One"
                                                              }
                                                      }
                },
            }.Render();
        }

        private static void RenderViewsForAccountMemberships()
        {
            //no data
            new ViewRenderer<AccountMembershipsController>
            {
                ActionExpr = c => c.Index(),
                User = User.Guest,
                Scenario = "nodata",
                Model = new AccountMemberships_Index
                {
                }
            }.Render();

            new ViewRenderer<AccountMembershipsController>
            {
                ActionExpr = c => c.Index(),
                User = User.Guest,
                Model = new AccountMemberships_Index
                {
                    AccountsUserBelongsTo = new AccountMemberships_AccountsUserBelongsTo[]
                                                {
                                                    new AccountMemberships_AccountsUserBelongsTo
                                                        {
                                                            AccountId = 443,
                                                            AccountOwnerName = DummyData.Names[0],
                                                            Id = 4,
                                                        },
                                                    new AccountMemberships_AccountsUserBelongsTo
                                                        {
                                                            AccountId = 4423,
                                                            AccountOwnerName = DummyData.Names[1],
                                                            Id = 21,
                                                        },
                                                }
                }
            }.Render();

            new ViewRenderer<AccountMembershipsController>
            {
                ActionExpr = c => c.Details(0),
                User = User.Guest,
                Model = new AccountMemberships_Details
                {
                    Id = 3234,
                    JoinedAccountOn = DummyData.Date1.ToLocalTime(),
                    AccountOwner = new UserSummary
                    {
                        Email = User.View1.Email,
                        Id = User.View1.Id,
                        Name = User.View1.Name,
                    },
                    Productions = new AccountMemberships_Details.Production[]
                                      {
                                          new AccountMemberships_Details.Production
                                              {
                                                  Id = 4,
                                                  Title = "Bill's Production"
                                              },
                                          new AccountMemberships_Details.Production
                                              {
                                                  Id = 5,
                                                  Title = "Ron's Production"
                                              },
                                          new AccountMemberships_Details.Production
                                              {
                                                  Id = 6,
                                                  Title = "Frank's Production"
                                              },
                                      }
                }
            }.Render();

            ////account/members/{id}
            //new ViewRenderer<AccountMembersController>
            //{
            //    ActionExpr = c => c.Details(0),
            //    User = User.SubscribedFreelance,
            //    Model = new AccountMembers_Details
            //    {
            //        Membership = new AccountMembers_Membership
            //        {
            //            MembershipId = 2,
            //            Role = "Client",
            //            MemberEmail = User.View2.Email,
            //            MemberName = User.View2.Name,
            //            MemberId = User.View2.Id,
            //        },
            //        Productions = new AccountMembers_Details.Production[]
            //                                          {
            //                                              new AccountMembers_Details.Production
            //                                                  {
            //                                                      Id = 3,
            //                                                      Title = "First One"
            //                                                  },
            //                                              new AccountMembers_Details.Production
            //                                                  {
            //                                                      Id = 4,
            //                                                      Title = "Another One"
            //                                                  }
            //                                          }
            //    },
            //}.Render();
        }

        private static void RenderViewsForAccountInvoices()
        {

            new ViewRenderer<InvoicesController>
            {
                ActionExpr = c => c.Index(),
                User = User.SubscribedFreelance,
                Model = new Invoices_InvoiceSummary[]
                    {
                        new Invoices_InvoiceSummary
                            {
                                Date = DummyData.Date1,
                                Id = "1",
                                Number = 1001,
                                Total = 10.99,
                            },
                        new Invoices_InvoiceSummary
                            {
                                Date = DummyData.Date1.AddMonths(1),
                                Id = "2",
                                Number = 1002,
                                Total = 10.99,
                            }
                    },
            }.Render();


            new ViewRenderer<InvoicesController>
            {
                ActionExpr = c => c.Details("one"),
                User = User.SubscribedFreelance,
                Model = new Invoices_Invoice
                {
                    Id = "2",
                    AccountCode = "accountcode",
                    Date = DummyData.Date1,
                    Number = 1002,
                    Paid = 10.99,
                    Subtotal = 10.99,
                    Total = 10.99,
                    TotalDue = 0,
                    LineItems = new Invoices_InvoiceLineItem[]
                                        {
                                            new Invoices_InvoiceLineItem
                                                {
                                                    Amount = 10.99,
                                                    Description = "Takeoff Freelance",
                                                    StartDate = DummyData.Date1.AddMonths(-1),
                                                    EndDate = DummyData.Date1,
                                                    Id = "123",
                                                }
                                        },
                    Payments = new Invoices_InvoicePayment[]
                                        {
                                            new Invoices_InvoicePayment
                                                {
                                                    Id            = "2",
                                                    Amount = 10.99,
                                                    Date = DummyData.Date1,
                                                    Message = "Thanks!"
                                                }
                                        }
                },
            }.Render();

        }

        private static void RenderViewsForAccountPlan()
        {

            new ViewRenderer<AccountPlanController>
            {
                ActionExpr = c => c.Index(),
                User = User.SubscribedFreelance,
                Model = new AccountPlan_Index
                {
                    Plan = Plan.Freelance,
                    AvailablePlans = Plan.PlansForSale,
                },
            }.Render();

            new ViewRenderer<AccountPlanController>
            {
                ActionExpr = c => c.Edit(null),
                User = User.SubscribedFreelance,
                View = "Edit-Upgrade",
                IsDefaultViewForAction = true,
                Model = new AccountPlan_Edit
                {
                    CurrentPlan = Plan.Freelance,
                    NewPlan = Plan.Studio,
                },
            }.Render();

            new ViewRenderer<AccountPlanController>
            {
                ActionExpr = c => c.Edit(null),
                User = User.SubscribedFreelance,
                View = "Edit-Downgrade",
                AddViewNameToFileName = false,
                Scenario = "Downgrade",
                Model = new AccountPlan_Edit
                {
                    CurrentPlan = Plan.Studio,
                    NewPlan = Plan.Freelance,
                },
            }.Render();

            new ViewRenderer<AccountPlanController>
            {
                ActionExpr = c => c.Edit(null),
                User = User.Demo,
                View = "Edit-DowngradeForbidden",
                AddViewNameToFileName = false,
                Scenario = "DowngradeForbidden",
            }.Render();

            new ViewRenderer<AccountPlanController>
            {
                ActionExpr = c => c.Edit(null),
                User = User.SubscribedFreelance,
                View = "Edit-DowngradeInvalid",
                AddViewNameToFileName = false,
                Scenario = "DowngradeInvalid",
                Model = new AccountPlan_Edit_DowngradeInvalid
                {
                    LimitsExceeded = new string[] { "Productions", "Videos" }
                }
            }.Render();

        }

    }
}
