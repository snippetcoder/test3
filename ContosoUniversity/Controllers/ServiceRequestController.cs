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
using System.Web.Security;
using ContosoUniversity.CustomAuthentication;
using ContosoUniversity.DataAccess;
using Newtonsoft.Json;

namespace ContosoUniversity.Controllers
{
    [Authorize]
    public class ServiceRequestController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: ServiceRequest
        public ActionResult Index()
        {
            var serviceRequests = from s in db.ServiceRequests.Where(s => s.IsDeleted == false)
                                  select s;

            return View(serviceRequests);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ServiceName")] ServiceRequest ServiceRequest)
        {
            if (ModelState.IsValid)
            {
                ServiceRequest.IsVisible = true;
                db.ServiceRequests.Add(ServiceRequest);
                await db.SaveChangesAsync();

                string query = "UPDATE ServiceRequest SET CreatedDate = GETDATE(), ServiceRequestCreatedByUserId = (SELECT UserId FROM Users WHERE Username = '" + HttpContext.User.Identity.Name + "') WHERE ServiceRequestCreatedByUserId = 0";
                db.Database.ExecuteSqlCommand(query);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(ServiceRequest);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceRequest ServiceRequest = await db.ServiceRequests.FindAsync(id);
            if (ServiceRequest == null)
            {
                return HttpNotFound();
            }

            return View(ServiceRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int? id, string[] selectedServiceRequest)
        {
            string[] fieldsToBind = new string[] { "ServiceName", "IsVisible" };

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var ServiceRequestToUpdate = await db.ServiceRequests.FindAsync(id);
            if (ServiceRequestToUpdate == null)
            {
                ServiceRequest deletedServiceRequest = new ServiceRequest();
                TryUpdateModel(deletedServiceRequest, fieldsToBind);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The Service Request was deleted by another user.");
                return View(deletedServiceRequest);
            }

            if (TryUpdateModel(ServiceRequestToUpdate, fieldsToBind))
            {
                try
                {
                    string query = "UPDATE ServiceRequest SET LastUpdatedDate = GETDATE(), LastUpdatedByUserId = (SELECT UserId FROM Users WHERE Username = '" + HttpContext.User.Identity.Name + "') WHERE ServiceRequestId = {0}";
                    ViewBag.RowsAffected = db.Database.ExecuteSqlCommand(query, id);
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (ServiceRequest)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. The department was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (ServiceRequest)databaseEntry.ToObject();

                        if (databaseValues.ServiceName != clientValues.ServiceName)
                            ModelState.AddModelError("ServiceName", "Current value: " + databaseValues.ServiceName);

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

            return View(ServiceRequestToUpdate);
        }

        public async Task<ActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceRequest ServiceRequest = await db.ServiceRequests.FindAsync(id);
            if (ServiceRequest == null)
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

            return View(ServiceRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? id)
        {
            string[] fieldsToBind = new string[] { "isDeleted" };

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string query = "UPDATE ServiceRequest SET isDeleted = 1, IsVisible = 0, DeletedDate = GETDATE(), DeletedByUserId = (SELECT UserId FROM Users WHERE Username = '" + HttpContext.User.Identity.Name + "') WHERE ServiceRequestId = {0}";

            ViewBag.RowsAffected = db.Database.ExecuteSqlCommand(query, id);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}