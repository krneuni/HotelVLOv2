using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VLO.Models;

namespace VLO.Controllers
{
    public class OrdenesController : Controller
    {
        private Context db = new Context();

        // GET: Ordenes
        public ActionResult Index()
        {
            OrdenesViewModel All = new OrdenesViewModel();
            All.DetallePedido = db.DetallePedido.ToList();
            All.Pedido = db.Pedido.ToList();
            All.Mesa = db.Mesa.ToList();
            return View(All);
        }

        public ActionResult Ordenes()
        {
            var orden = db.Pedido.Where(x=>x.Estado==1).ToList();
            var detalle = db.DetallePedido.ToList();
            CocinaViewModel cvm = new CocinaViewModel();
            cvm.pedidos = orden;
            cvm.detalle = detalle;
            cvm.menus = db.Menus.ToList();
            

            //var orden = (from o in db.Pedido join me in db.Mesa on o.IdMesa equals me.IdMesa select o).FirstOrDefault();
            return View(cvm);
        }
        

        [HttpGet]
        public ActionResult TerminarOrdenCocina(int idpedido)
        {
            Pedido d = db.Pedido.Find(idpedido);
            d.Estado = 2;
            db.Entry(d).State = EntityState.Modified;
            db.SaveChanges();

            return Redirect("/Ordenes/Ordenes");
        }

        public ActionResult OrdenesTerminadas()
        {
            var orden = db.Pedido.Where(x => x.Estado == 2).ToList();
            var detalle = db.DetallePedido.ToList();
            CocinaViewModel cvm = new CocinaViewModel();
            cvm.pedidos = orden;
            cvm.detalle = detalle;
            cvm.menus = db.Menus.ToList();
            return View(cvm);
        }

        public ActionResult OrdenesMeseros(int idpedido)
        {
            Pedido p = db.Pedido.Find(idpedido);
            p.Estado = 3;
            db.Entry(p).State = EntityState.Modified;
            db.SaveChanges();

            return Redirect("/Ordenes/Pagos");
        }


        public ActionResult Pagos(int? idpedido)
        {
            var orden = db.Pedido.Where(x => x.Estado == 3).ToList();
            var queryOrd = db.DetallePedido.Where(d => d.IdPedido == idpedido).ToList();
            CocinaViewModel cvm = new CocinaViewModel();
            cvm.pedidos = orden;
            cvm.detalle = queryOrd;
            cvm.menus = db.Menus.ToList();
            return View(cvm);
        }

        

        //VistaMenu
        public ActionResult Menu(int? id, int? idPedido)
        {
            ViewBag.mesa = id;
            var queryOrd = db.DetallePedido.Where(d => d.IdPedido == idPedido).ToList();
            OrdenesViewModel ovm = new OrdenesViewModel();
            ovm.DetallePedido = queryOrd;
            ovm.Menus = db.Menus.ToList();
            ovm.TiposMenu = db.TipoMenus.ToList();
            return View(ovm);
        }


        //Agregar orden y guardar datos en las diferentes tablas
        [HttpPost]
        public async Task<ActionResult> AddOrden(AddOrdenViewModel aovm)
        {
            Usuarios e = db.Usuarios.Where(x => x.IdUser == 1).FirstOrDefault();

            Pedido p = new Pedido();
            p.Cantidad = aovm.numpersonas;
            p.Cliente = aovm.cliente;
            p.Usuarios = e;
            p.Estado = 1;
            p.IdMesa = aovm.mesa;
            

            db.Pedido.Add(p);
            await db.SaveChangesAsync();

            var lastPedido = db.Pedido.OrderByDescending(x=>x.IdPedido).First();
            for(var i= 0;i < aovm.id.Count;i++)
            {
                DetallePedido dp = new DetallePedido();
                dp.IdMenu = aovm.id[i];
                dp.cantidad = aovm.cantidad[i];
                dp.IdPedido = lastPedido.IdPedido;
                db.DetallePedido.Add(dp);
                await db.SaveChangesAsync();

                //Encontrar el menu
                var menu = db.Menus.Find(dp.IdMenu);
                //Buscar los menus en la receta
                var recmenu = (from u in db.Receta where u.IdMenu == menu.IdMenu select u).ToList();
                //Recorrer
                foreach (var io in recmenu)
                {
                    //Encontrar los productos que se utilizan
                    Productos de = db.Productos.Find(io.IdProducto);
                    
                    //Resta de la cantidad que se pide menos la cantidad utilizada

                    var Descuento = io.CantidadUtilizada * dp.cantidad;
                    de.Cantidad =de.Cantidad - Descuento;
                    db.Entry(de).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            //Encontrar las mesas
            Mesa d = db.Mesa.Find(aovm.mesa);
            //Cambia el estado de la mesa
            d.Estado = false;
            db.Entry(d).State = EntityState.Modified;
            db.SaveChanges();

            return Redirect("Index");
        }

        public async Task<ActionResult> DeleteDetalle(string key, string key2)
        {
            try
            {
                var header = new Dictionary<string, string>()
                {
                    {"IdDetalle",key }
                };
                DetallePedido dp = db.DetallePedido.Find(key);
                db.DetallePedido.Remove(dp);
                await db.SaveChangesAsync();
            }
            catch { }
            return RedirectToAction("Menu", new { IdMesa = key2 });
        }
    }
}