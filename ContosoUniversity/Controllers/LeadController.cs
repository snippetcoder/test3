using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ContosoUniversity.DAL;
using ContosoUniversity.Models;
using System.Data.Entity.Infrastructure;
using PagedList;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.IO;
using System.Data.SqlClient;
using ContosoUniversity.Code;
using ContosoUniversity.CustomAuthentication;
using System.Security.Principal;

namespace ContosoUniversity.Controllers
{
    [Authorize]
    public class LeadController : Controller
    {
        private SchoolContext db = new SchoolContext();
        //var identity = ((CustomPrincipal)HttpContext.Current.User);

        // GET: Lead
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.EmailDateSortParm = sortOrder == "emaildate" ? "emaildate_desc" : "emaildate";
            ViewBag.SourceSortParm = sortOrder == "source" ? "source_desc" : "source";
            ViewBag.RequestDateSortParm = sortOrder == "requestdate" ? "requestdate_desc" : "requestdate";
            ViewBag.CaseAttendedBySortParm = sortOrder == "caseattendedby" ? "caseattendedby_desc" : "caseattendedby";
            ViewBag.ClientNameSortParm = sortOrder == "client" ? "cliente_desc" : "client";
            ViewBag.CompanyNameSortParm = sortOrder == "company" ? "company_desc" : "company";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var leads = from s in db.Leads.Where(s => s.isDeleted == false)
                        select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                leads = leads.Where(s => s.Email.Contains(searchString)
                                    || s.CompanyName.Contains(searchString)
                                    || s.ClientName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "emaildate":
                    leads = leads.OrderBy(s => s.EmailDate);
                    break;
                case "emaildate_desc":
                    leads = leads.OrderByDescending(s => s.EmailDate);
                    break;
                case "source":
                    leads = leads.OrderBy(s => s.Source);
                    break;
                case "source_desc":
                    leads = leads.OrderByDescending(s => s.Source);
                    break;
                case "requestdate":
                    leads = leads.OrderBy(s => s.RequestDate);
                    break;
                case "requestdate_desc":
                    leads = leads.OrderByDescending(s => s.RequestDate);
                    break;
                case "caseattendedby":
                    leads = leads.OrderBy(s => s.CaseAttendedBy);
                    break;
                case "caseattendedby_desc":
                    leads = leads.OrderByDescending(s => s.CaseAttendedBy);
                    break;
                case "client":
                    leads = leads.OrderBy(s => s.ClientName);
                    break;
                case "client_desc":
                    leads = leads.OrderByDescending(s => s.ClientName);
                    break;
                case "company":
                    leads = leads.OrderBy(s => s.CompanyName);
                    break;
                case "company_desc":
                    leads = leads.OrderByDescending(s => s.CompanyName);
                    break;
                default:  // Name ascending 
                    leads = leads.OrderBy(s => s.LeadId);
                    break;
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(leads.ToPagedList(pageNumber, pageSize));
        }

        // GET: Lead/Create
        public ActionResult Create()
        {
            var servicerequests = from d in db.ServiceRequests
                                  where d.IsVisible == true && d.IsDeleted != true
                                  orderby d.ServiceRequestId
                                  select d;
            var countrylicensetype =    from d in db.CountryLicenseTypes
                                        where d.IsVisible == true && d.IsDeleted != true
                                        orderby d.CountryLicenseTypeId
                                        select d;

            ViewBag.ServiceRequestID = new SelectList(servicerequests, "ServiceRequestId", "ServiceName");
            ViewBag.CountryLicenseTypeID = new SelectList(countrylicensetype, "CountryLicenseTypeID", "CountryLicenseTypeName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "LeadId, EmailDate, Source, RequestDate, CaseAttendedBy, ClientName, CompanyName, Email, ContactNumber1, ContactNumber2, ServiceRequestID, CountryLicenseTypeID, FollowUpDate, FollowUpDate2, Remarks, LeadCreatedByUserId, CreatedDate")] Lead lead)
        {
            if (ModelState.IsValid)
            {
                db.Leads.Add(lead);
                await db.SaveChangesAsync();

                string query = "UPDATE Lead SET CreatedDate = GETDATE(), LeadCreatedByUserId = (SELECT UserId FROM Users WHERE Username = '" + HttpContext.User.Identity.Name + "') WHERE LeadCreatedByUserId = 0";
                db.Database.ExecuteSqlCommand(query);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            var servicerequests = from d in db.ServiceRequests
                                  where d.IsVisible == true && d.IsDeleted != true
                                  orderby d.ServiceRequestId
                                  select d;
            var countrylicensetype = from d in db.CountryLicenseTypes
                                     where d.IsVisible == true && d.IsDeleted != true
                                     orderby d.CountryLicenseTypeId
                                     select d;

            ViewBag.ServiceRequestID = new SelectList(servicerequests, "ServiceRequestId", "ServiceName", lead.ServiceRequestID);
            ViewBag.CountryLicenseTypeID = new SelectList(countrylicensetype, "CountryLicenseTypeID", "CountryLicenseTypeName", lead.CountryLicenseTypeID);
            return View(lead);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lead lead = await db.Leads.FindAsync(id);
            if (lead == null)
            {
                return HttpNotFound();
            }

            var servicerequests = from d in db.ServiceRequests
                                  where d.IsVisible == true && d.IsDeleted != true
                                  orderby d.ServiceRequestId
                                  select d;
            var countrylicensetype = from d in db.CountryLicenseTypes
                                     where d.IsVisible == true && d.IsDeleted != true
                                     orderby d.CountryLicenseTypeId
                                     select d;

            ViewBag.ServiceRequestID = new SelectList(servicerequests, "ServiceRequestId", "ServiceName", lead.ServiceRequestID);
            ViewBag.CountryLicenseTypeID = new SelectList(countrylicensetype, "CountryLicenseTypeID", "CountryLicenseTypeName", lead.CountryLicenseTypeID);
            return View(lead);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int? id, string[] selectedLead)
        {
            string[] fieldsToBind = new string[] { "EmailDate", "Source", "RequestDate", "CaseAttendedBy", "ClientName", "CompanyName", "ContactNumber1", "ContactNumber2", "ServiceRequestID", "CountryLicenseTypeID", "FollowUpDate", "FollowUpDate2", "Remarks" };

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var leadToUpdate = await db.Leads.FindAsync(id);
            if (leadToUpdate == null)
            {
                Lead deletedLead = new Lead();
                TryUpdateModel(deletedLead, fieldsToBind);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The Lead was deleted by another user.");
                ViewBag.ServiceRequestID = new SelectList(db.ServiceRequests, "ServiceRequestId", "ServiceName", deletedLead.ServiceRequestID);
                ViewBag.CountryLicenseTypeID = new SelectList(db.CountryLicenseTypes, "CountryLicenseTypeID", "CountryLicenseTypeName", deletedLead.CountryLicenseTypeID);

                return View(deletedLead);
            }
            if (TryUpdateModel(leadToUpdate, fieldsToBind))
            {
                try
                {
                    string query = "UPDATE Lead SET LastUpdatedDate = GETDATE(), LeadLastUpdatedByUserId = (SELECT UserId FROM Users WHERE Username = '" + HttpContext.User.Identity.Name + "') WHERE LeadId = {0}";
                    ViewBag.RowsAffected = db.Database.ExecuteSqlCommand(query, id);
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (Lead)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. The department was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Lead)databaseEntry.ToObject();

                        if (databaseValues.EmailDate != clientValues.EmailDate)
                            ModelState.AddModelError("EmailDate", "Current value: " + databaseValues.EmailDate);
                        if (databaseValues.Source != clientValues.Source)
                            ModelState.AddModelError("Source", "Current value: " + String.Format("{0:c}", databaseValues.Source));
                        if (databaseValues.RequestDate != clientValues.RequestDate)
                            ModelState.AddModelError("RequestDate", "Current value: " + String.Format("{0:d}", databaseValues.RequestDate));
                        if (databaseValues.CaseAttendedBy != clientValues.CaseAttendedBy)
                            ModelState.AddModelError("CaseAttendedBy", "Current value: " + db.Leads.Find(databaseValues.CaseAttendedBy).CaseAttendedBy);
                        if (databaseValues.ClientName != clientValues.ClientName)
                            ModelState.AddModelError("ClientName", "Current value: " + db.Leads.Find(databaseValues.ClientName).ClientName);
                        if (databaseValues.CompanyName != clientValues.CompanyName)
                            ModelState.AddModelError("CompanyName", "Current value: " + db.Leads.Find(databaseValues.CompanyName).CompanyName);
                        if (databaseValues.ContactNumber1 != clientValues.ContactNumber1)
                            ModelState.AddModelError("ContactNumber1", "Current value: " + db.Leads.Find(databaseValues.ContactNumber1).ContactNumber1);
                        if (databaseValues.ContactNumber2 != clientValues.ContactNumber2)
                            ModelState.AddModelError("ContactNumber2", "Current value: " + db.Leads.Find(databaseValues.ContactNumber2).ContactNumber2);
                        if (databaseValues.ServiceRequestID != clientValues.ServiceRequestID)
                            ModelState.AddModelError("ServiceRequestID", "Current value: " + db.Leads.Find(databaseValues.ServiceRequestID).ServiceRequestID);
                        if (databaseValues.CountryLicenseTypeID != clientValues.CountryLicenseTypeID)
                            ModelState.AddModelError("CountryLicenseTypeID", "Current value: " + db.Leads.Find(databaseValues.CountryLicenseTypeID).CountryLicenseTypeID);
                        if (databaseValues.FollowUpDate != clientValues.FollowUpDate)
                            ModelState.AddModelError("FollowUpDate", "Current value: " + db.Leads.Find(databaseValues.FollowUpDate).FollowUpDate);
                        if (databaseValues.FollowUpDate2 != clientValues.FollowUpDate2)
                            ModelState.AddModelError("FollowUpDate2", "Current value: " + db.Leads.Find(databaseValues.FollowUpDate2).FollowUpDate2);
                        if (databaseValues.Remarks != clientValues.Remarks)
                            ModelState.AddModelError("Remarks", "Current value: " + db.Leads.Find(databaseValues.Remarks).Remarks);

                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                            + "was modified by another user after you got the original value. The "
                            + "edit operation was canceled and the current values in the database "
                            + "have been displayed. If you still want to edit this record, click "
                            + "the Save button again. Otherwise click the Back to List hyperlink.");
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            ViewBag.ServiceRequestID = new SelectList(db.ServiceRequests, "ServiceRequestId", "ServiceName", leadToUpdate.ServiceRequestID);
            ViewBag.CountryLicenseTypeID = new SelectList(db.CountryLicenseTypes, "CountryLicenseTypeID", "CountryLicenseTypeName", leadToUpdate.CountryLicenseTypeID);
            return View(leadToUpdate);
        }
        // GET: Lead/Delete/5
        public async Task<ActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lead lead = await db.Leads.FindAsync(id);
            if (lead == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction("Index");
                }
                return HttpNotFound();
            }

            if (concurrencyError.GetValueOrDefault())
            {
                ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                    + "was modified by another user after you got the original values. "
                    + "The delete operation was canceled and the current values in the "
                    + "database have been displayed. If you still want to delete this "
                    + "record, click the Delete button again. Otherwise "
                    + "click the Back to List hyperlink.";
            }

            return View(lead);
        }
        // POST: Lead/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? id)
        {
            string[] fieldsToBind = new string[] { "isDeleted" };

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string query = "UPDATE Lead SET isDeleted = 1, DeletedDate = GETDATE(), DeletedByUserId = (SELECT UserId FROM Users WHERE Username = '" + HttpContext.User.Identity.Name + "') WHERE LeadId = {0}";

            ViewBag.RowsAffected = db.Database.ExecuteSqlCommand(query, id);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult ExportToExcel()
        {
            //Get the data from database into datatable
            var leads = from s in db.Leads
                        where s.isDeleted == false
                        select new
                        {
                            Email_Sent_Date = s.EmailDate,
                            Source = s.Source,
                            Date_of_Request = s.RequestDate,
                            Case_Attended_By = s.CaseAttendedBy,
                            Client_Name = s.ClientName,
                            Company_Name = s.CompanyName,
                            Email_Address = s.Email,
                            Contact_Number_1 = s.ContactNumber1,
                            Contact_Number_2 = s.ContactNumber2,
                            Service_Request = s.ServiceRequester.ServiceName,
                            Country_License_Type = s.CountryLicense.CountryLicenseTypeName,
                            Follow_Up_Date = s.FollowUpDate,
                            Follow_Up_Date2 = s.FollowUpDate2,
                            Remarks = s.Remarks
                        };

            var list_leads = leads.ToList();
            byte[] filecontent = ExcelExportHelper.ExportExcel(list_leads, "Leads");
            return File(filecontent, ExcelExportHelper.ExcelContentType, "Leads.xlsx");
        }
    }
}