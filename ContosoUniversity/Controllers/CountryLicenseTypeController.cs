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
    public class CountryLicenseTypeController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: CountryLicenseType
        public async Task<ActionResult> Index()
        {
            var countrylicensetype = from d in db.CountryLicenseTypes
                                     where d.IsDeleted != true
                                     orderby d.CountryLicenseTypeId
                                     select d;
            
            return View(await countrylicensetype.ToListAsync());
        }

        // GET: Department/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CountryLicenseTypeName, IsVisible, CountryLicenseTypeCreatedByUserId, CreatedDate")] CountryLicenseType countrylicensetype)
        {
            if (ModelState.IsValid)
            {
                countrylicensetype.IsVisible = true;
                db.CountryLicenseTypes.Add(countrylicensetype);
                await db.SaveChangesAsync();

                string query = "UPDATE CountryLicenseType SET CreatedDate = GETDATE(), CountryLicenseTypeCreatedByUserId = (SELECT UserId FROM Users WHERE Username = '" + HttpContext.User.Identity.Name + "') WHERE CountryLicenseTypeCreatedByUserId = 0";
                db.Database.ExecuteSqlCommand(query);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            
            return View(countrylicensetype);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CountryLicenseType countrylicensetype = await db.CountryLicenseTypes.FindAsync(id);
            if (countrylicensetype == null)
            {
                return HttpNotFound();
            }
            
            return View(countrylicensetype);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int? id, string[] selectedCountryLicense)
        {
            string[] fieldsToBind = new string[] { "CountryLicenseTypeName", "IsVisible" };

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var countrylicenseToUpdate = await db.CountryLicenseTypes.FindAsync(id);
            if (countrylicenseToUpdate == null)
            {
                CountryLicenseType deletedcountrylicense = new CountryLicenseType();
                TryUpdateModel(deletedcountrylicense, fieldsToBind);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The Lead was deleted by another user.");

                return View(deletedcountrylicense);
            }
            if (TryUpdateModel(countrylicenseToUpdate, fieldsToBind))
            {
                try
                {
                    string query = "UPDATE CountryLicenseType SET LastUpdatedDate = GETDATE(), LastUpdatedByUserId = (SELECT UserId FROM Users WHERE Username = '" + HttpContext.User.Identity.Name + "') WHERE LastUpdatedByUserId = {0}";
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
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            
            return View(countrylicenseToUpdate);
        }

        // GET: CountryLicenseType/Delete/5
        public async Task<ActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CountryLicenseType countrylicensetype = await db.CountryLicenseTypes.FindAsync(id);
            if (countrylicensetype == null)
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

            return View(countrylicensetype);
        }
        // POST: CountryLicenseType/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? id)
        {
            string[] fieldsToBind = new string[] { "isDeleted" };

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string query = "UPDATE CountryLicenseType SET IsDeleted = 1, IsVisible = 0, DeletedDate = GETDATE(), DeletedByUserId = (SELECT UserId FROM Users WHERE Username = '" + HttpContext.User.Identity.Name + "') WHERE CountryLicenseTypeId = {0}";

            ViewBag.RowsAffected = db.Database.ExecuteSqlCommand(query, id);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}