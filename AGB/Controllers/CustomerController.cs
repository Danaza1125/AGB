using AparnaGoldBuyers.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace AparnaGoldBuyers.Controllers
{
    public class CustomerController : Controller
    {
        private readonly string conStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ActionResult List()
        {
            var customers = new List<CustomerModel>();

            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT CustomerId, FullName, Mobile, CreatedOn
                    FROM Customers
                    WHERE Active = 1
                    ORDER BY CreatedOn DESC, CustomerId DESC", con);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(new CustomerModel
                        {
                            CustomerId = Convert.ToInt32(reader["CustomerId"]),
                            FullName = reader["FullName"]?.ToString(),
                            Mobile = reader["Mobile"]?.ToString(),
                            CreatedOn = reader["CreatedOn"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(reader["CreatedOn"])
                        });
                    }
                }
            }

            return View(customers);
        }

        public ActionResult Create()
        {
            var model = new CustomerViewModel();
            model.Customer.CustomerDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(
            CustomerViewModel model,
            List<HttpPostedFileBase> OrnamentFiles,
            List<HttpPostedFileBase> ProofFiles)
        {
            if (model == null || model.Customer == null)
            {
                var emptyModel = new CustomerViewModel();
                emptyModel.Customer.CustomerDate = DateTime.Now.ToString("yyyy-MM-dd");
                return View(emptyModel);
            }

            model.Ornaments = model.Ornaments ?? new List<CustomerOrnamentModel>();
            model.Proofs = model.Proofs ?? new List<CustomerProofModel>();

            if (string.IsNullOrWhiteSpace(model.Customer.FullName))
            {
                ModelState.AddModelError("Customer.FullName", "Customer name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Customer.Mobile))
            {
                ModelState.AddModelError("Customer.Mobile", "Mobile number is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Customer.CustomerDate))
            {
                ModelState.AddModelError("Customer.CustomerDate", "Date is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ornaments = GetValidOrnaments(model.Ornaments, OrnamentFiles);
            var proofs = GetValidProofs(model.Proofs, ProofFiles);
            int customerId = 0;

            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Customers(FullName, Mobile, AccountDetails, CustomerDate, Active, CreatedOn)
                        VALUES(@n, @m, @a, @d, 1, GETDATE());
                        SELECT SCOPE_IDENTITY();", con, tx);

                    cmd.Parameters.AddWithValue("@n", model.Customer.FullName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@m", model.Customer.Mobile ?? string.Empty);
                    cmd.Parameters.AddWithValue("@a", model.Customer.AccountDetails ?? string.Empty);
                    cmd.Parameters.AddWithValue("@d", model.Customer.CustomerDate ?? string.Empty);

                    customerId = Convert.ToInt32(cmd.ExecuteScalar());

                    for (int i = 0; i < ornaments.Count; i++)
                    {
                        string filePath = string.Empty;

                        if (ornaments[i].File != null && ornaments[i].File.ContentLength > 0)
                        {
                            var file = ornaments[i].File;
                            EnsureUploadDirectory("~/Uploads/Ornaments");

                            filePath = "/Uploads/Ornaments/" + Guid.NewGuid() + Path.GetExtension(file.FileName);
                            file.SaveAs(Server.MapPath(filePath));
                        }

                        SqlCommand cmd2 = new SqlCommand(@"
                            INSERT INTO CustomersOrnament(CustomersId, OrnamentPath, grams, purity, buyingPrice, Active, CreatedOn)
                            VALUES(@cid, @path, @g, @p, @b, 1, GETDATE())", con, tx);

                        cmd2.Parameters.AddWithValue("@cid", customerId);
                        cmd2.Parameters.AddWithValue("@path", string.IsNullOrWhiteSpace(filePath) ? (object)DBNull.Value : filePath);
                        cmd2.Parameters.AddWithValue("@g", ornaments[i].Model.grams ?? string.Empty);
                        cmd2.Parameters.AddWithValue("@p", ornaments[i].Model.purity ?? string.Empty);
                        cmd2.Parameters.AddWithValue("@b", ornaments[i].Model.buyingPrice ?? string.Empty);

                        cmd2.ExecuteNonQuery();
                    }

                    for (int i = 0; i < proofs.Count; i++)
                    {
                        string filePath = string.Empty;

                        if (proofs[i].File != null && proofs[i].File.ContentLength > 0)
                        {
                            var file = proofs[i].File;
                            EnsureUploadDirectory("~/Uploads/Proofs");

                            filePath = "/Uploads/Proofs/" + Guid.NewGuid() + Path.GetExtension(file.FileName);
                            file.SaveAs(Server.MapPath(filePath));
                        }

                        SqlCommand cmd3 = new SqlCommand(@"
                            INSERT INTO CustomersProof(CustomersId, DocumentName, DocumentPath, Active, CreatedOn)
                            VALUES(@cid, @n, @path, 1, GETDATE())", con, tx);

                        cmd3.Parameters.AddWithValue("@cid", customerId);
                        cmd3.Parameters.AddWithValue("@n", proofs[i].Model.DocumentName ?? string.Empty);
                        cmd3.Parameters.AddWithValue("@path", string.IsNullOrWhiteSpace(filePath) ? (object)DBNull.Value : filePath);

                        cmd3.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            TempData["SuccessMessage"] = "Customer saved successfully.";
            return RedirectToAction("Create");
        }

        private List<IndexedOrnamentInput> GetValidOrnaments(List<CustomerOrnamentModel> ornaments, List<HttpPostedFileBase> files)
        {
            var validItems = new List<IndexedOrnamentInput>();

            for (int i = 0; i < ornaments.Count; i++)
            {
                var ornament = ornaments[i] ?? new CustomerOrnamentModel();
                var file = files != null && files.Count > i ? files[i] : null;

                var hasValues =
                    !string.IsNullOrWhiteSpace(ornament.grams) ||
                    !string.IsNullOrWhiteSpace(ornament.purity) ||
                    !string.IsNullOrWhiteSpace(ornament.buyingPrice) ||
                    (file != null && file.ContentLength > 0);

                if (hasValues)
                {
                    validItems.Add(new IndexedOrnamentInput
                    {
                        Model = ornament,
                        File = file
                    });
                }
            }

            return validItems;
        }

        private List<IndexedProofInput> GetValidProofs(List<CustomerProofModel> proofs, List<HttpPostedFileBase> files)
        {
            var validItems = new List<IndexedProofInput>();

            for (int i = 0; i < proofs.Count; i++)
            {
                var proof = proofs[i] ?? new CustomerProofModel();
                var file = files != null && files.Count > i ? files[i] : null;

                var hasValues =
                    !string.IsNullOrWhiteSpace(proof.DocumentName) ||
                    (file != null && file.ContentLength > 0);

                if (hasValues)
                {
                    validItems.Add(new IndexedProofInput
                    {
                        Model = proof,
                        File = file
                    });
                }
            }

            return validItems;
        }

        private void EnsureUploadDirectory(string relativePath)
        {
            var absolutePath = Server.MapPath(relativePath);
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }
        }

        private class IndexedOrnamentInput
        {
            public CustomerOrnamentModel Model { get; set; }
            public HttpPostedFileBase File { get; set; }
        }

        private class IndexedProofInput
        {
            public CustomerProofModel Model { get; set; }
            public HttpPostedFileBase File { get; set; }
        }
    }
}
