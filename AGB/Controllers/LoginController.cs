using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using AparnaGoldBuyers.Models;

namespace AparnaGoldBuyers.Controllers
{
    public class LoginController : Controller
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult Login(LoginModel model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var user = GetUser(model.Username, model.Password);

            if (user != null)
            {
                // Set auth cookie
                FormsAuthentication.SetAuthCookie(user.Username, false);

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Index", "Dashboard"),
                    userDetails = new
                    {
                        user.Username,
                        user.FullName
                    }
                });
            }
            else
            {
                return Json(new { success = false, message = "Invalid credentials" });
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        // Fetch full user details from DB
        private LoginModel GetUser(string username, string password)
        {
            string hashed = GetMd5Hash(password);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT Username, FullName FROM Users WHERE Username=@u AND Password=@p AND IsActive=1";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", hashed);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new LoginModel
                        {
                            Username = reader["Username"].ToString(),
                            FullName = reader["FullName"].ToString(),
                            Password = null // optional: do not return password
                        };
                    }
                    else
                    {
                        return null; // user not found
                    }
                }
            }
        }

        private string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
