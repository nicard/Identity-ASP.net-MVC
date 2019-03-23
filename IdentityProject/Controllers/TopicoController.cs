using IdentityProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IdentityProject.Controllers
{
    [Authorize]
    public class TopicoController : Controller
    {
        // GET: Topico
        public ActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Criar(CriarViewModel model)
        {
            return View();
        }
    }
}