using Forum.Models;
using Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class ContaController : Controller
    {

        private UserManager<UsuarioAplicacao> _userManager;
        public UserManager<UsuarioAplicacao> UserManager
        {
            get
            {
                if(_userManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _userManager = contextOwin.GetUserManager<UserManager<UsuarioAplicacao>>();
                }
                return _userManager;
            }
            set
            {
                _userManager = value;
            }
        }


        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Registrar(ContaRegistrarViewModel modelo)
        {
            if (ModelState.IsValid)
            {

                var novoUsuario = new UsuarioAplicacao();
                novoUsuario.UserName = modelo.Username;
                novoUsuario.NomeCompleto = modelo.NomeCompleto;
                novoUsuario.Email = modelo.Email;

                var usuario = await UserManager.FindByEmailAsync(modelo.Email);
                var usuarioExiste = usuario != null;

                if(usuarioExiste)
                    return View("AguardandoConfirmacao");

                var resultado =  await UserManager.CreateAsync(novoUsuario, modelo.Senha);

                if (resultado.Succeeded)
                {
                    await EnviarEmailDeConfirmacaoAsync(novoUsuario);
                    return View("AguardandoConfirmacao");
                }
                else
                {
                    AdicionaErros(resultado);
                }
               
            }
            return View(modelo);
        }

        private async Task EnviarEmailDeConfirmacaoAsync(UsuarioAplicacao usuario)
        {

            var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);

            var linkDeCallBack = Url.Action(
                "ConfirmacaoEmail", "Conta", new { usuarioId = usuario.Id, token = token },
                Request.Url.Scheme);

            await UserManager.SendEmailAsync(
                usuario.Id,
                "Fórum - Confirmação de Email",
                $"Bem vindo ao Fórum, clique aqui {linkDeCallBack} confirmar seu endereço de email!"
                );
        }

        public async Task<ActionResult> ConfirmacaoEmail(string usuarioId, string token)
        {
            if (usuarioId == null || token == null)
                return View("Error");

            var  resultado = await UserManager.ConfirmEmailAsync(usuarioId, token);

            if (resultado.Succeeded)
                return RedirectToAction("Index", "Home");
            else
                return View("Error");

        }

        private void AdicionaErros(IdentityResult resultado)
        {
           foreach(var erro in resultado.Errors)
                ModelState.AddModelError("", erro);
            
        }
    }
}