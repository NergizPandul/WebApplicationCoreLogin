using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using NETCore.Encrypt.Extensions;
using System.Security.Claims;
using WebApplicationCoreLogin.Models;
using WebApplicationCoreLogin.Models.ViewModel;

namespace WebApplicationCoreLogin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private DatabaseContext db;
        private IConfiguration _configuration;

        public AccountController(DatabaseContext dbcontext,IConfiguration configuration)
        {
            db = dbcontext;
            _configuration = configuration;
        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string sifre = _configuration.GetValue<string>("Appsettings:sifre");
                sifre = model.Password + sifre;
                string md5sifre = sifre.MD5();

                User user = db.Users.FirstOrDefault(x => x.UserName.ToLower() == model.UserName.ToLower() && x.Password ==md5sifre);

                if (user!=null)
                {
                    List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()));
                    claims.Add(new Claim(ClaimTypes.Role,user.Role));
                    claims.Add(new Claim("Name", user.Name ?? String.Empty));
                    claims.Add(new Claim("UserName", user.UserName));

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Index", "Home");
                }
                else 
                {
                    ModelState.AddModelError("", "Kullanıcı adı ya da şifre hatalı");

                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(RegisterViewModel model) 
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(x=>x.UserName.ToLower() == model.UserName.ToLower()))
                {
                    ModelState.AddModelError(nameof(model.UserName), "Bu kullanıcı adı sistemdebulunuyor");
                    return View(model);
                }
                string sifre = _configuration.GetValue<string>("Appsettings:sifre");
               sifre= model.Password + sifre;
                string md5sifre = sifre.MD5();
                User user=new()
                {
                    UserName = model.UserName,
                    Password=md5sifre,
                   

                };
                db.Users.Add (user);
               
                if (db.SaveChanges()==0)
                {
                    ModelState.AddModelError("", "Kayıt eklenemedi");
                }
                else 
                {
                   return RedirectToAction("Login");
                }

            }

            return View(model);
        }
   
        public IActionResult Profil()
        {
            ProfilBilgiGoster();

            return View(); 

        }

        public IActionResult ProfilBilgiGoster()
        {
            Guid id = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = db.Users.SingleOrDefault(x => x.Id == id);
            ViewData["adsoyad"] = user.Name;
            ViewData["username"] = user.UserName;
            ViewData["password"] = user.Password;
            ViewData["image"] = user.ProfilResimDosyası;
            ViewData["mesaj"] = TempData["mesaj"];
        


            return View();

        }

        public IActionResult ProfilResmiKaydet(IFormFile resim)
        {
            if (ModelState.IsValid)
            {
                Guid id = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = db.Users.SingleOrDefault(x => x.Id == id);
                //f_123478556.jpg
                string filename = $"resim_{id}.jpg";
                Stream stream = new FileStream($"wwwroot/resim/{filename}", FileMode.OpenOrCreate);
                resim.CopyTo(stream);
                stream.Close();
                stream.Dispose();

                user.ProfilResimDosyası = filename;
                db.SaveChanges();
                ProfilBilgiGoster();
                return RedirectToAction("Profil");
            }
               
            return View("Profil");
        }

        [HttpPost]

        public IActionResult AdSoyadKaydet(string adsoyad)
        {
            if (ModelState.IsValid)
            {
                Guid id = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = db.Users.SingleOrDefault(x => x.Id == id);

                user.Name = adsoyad;
                db.SaveChanges();

                TempData["mesaj"] = "Nameupdate";
                return RedirectToAction("Profil");
            }
            return View("Profil");

        }

        public IActionResult PasswordSave(string password)
        {
            if (ModelState.IsValid)
            {
                Guid id = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = db.Users.SingleOrDefault(x => x.Id == id);

                string sifre = _configuration.GetValue<string>("Appsettings:sifre");
                sifre = password + sifre;
                string md5sifre = sifre.MD5();

                user.Password = md5sifre;
                db.SaveChanges();
            }
            ProfilBilgiGoster();
            return View("Profil");
        }
        public IActionResult Logout()
        {

            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");

        }
        public IActionResult UsernameSave(string username)
        {
            if (ModelState.IsValid)
            {
                ProfilBilgiGoster();
                Guid id = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = db.Users.SingleOrDefault(x => x.Id == id);

                if (db.Users.Any(x=>x.UserName.ToLower()==user.UserName.ToLower()&& x.Id!=id))
                {
                    ModelState.AddModelError("", "Bu kullanıcı adı sistemde bulunuyor");
                    return View("Profil");
                }

                user.UserName = username;
                db.SaveChanges();

                TempData["mesaj"] = "UserNameupdate";
                return RedirectToAction("Profil");
            }
            ProfilBilgiGoster();
            return View("Profil");

        }
    }
}
