using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VLO.Models;

namespace VLO.Controllers
{
    public class AccountController : Controller
    {
        Context db = new Context();
        public ActionResult Index()
        {
            if (Session["Id"] != null)
            {
                return View(db.Usuarios.ToList());

            }
            else
            {
                return View("Login");
            }
        }
        public ActionResult Login()
        {
            ViewBag.alerta = "hidden";

            //TimeZoneInfo zona = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            //DateTime elSalvador = TimeZoneInfo.ConvertTime(DateTime.Now, zona);
            //ViewBag.fecha = elSalvador.ToLongDateString();
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string contra)
        {
            
            var Consulta = (from a in db.Usuarios where a.Username == username && a.Password == contra select a).FirstOrDefault();
            if (Consulta != null)
            {
                Session["Id"] = Consulta.IdUser;
                Session["nombre"] = Consulta.Nombre;
                ViewBag.User = Session["nombre"];
                Session["tipo"] = Consulta.IdRol;
                ViewBag.TipoUsuario = Session["tipo"];
                return RedirectToAction("Ordenes", "Ordenes");
            }

            ViewBag.alerta = "vissible";

            ViewBag.error = "Usuario o contraseña incorrectos";
            return View();


        }

        public ActionResult LogOut()
        {

            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

    }
}