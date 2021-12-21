using Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Forum.App_Start.Identity;

[assembly: OwinStartup(typeof(Forum.Startup))]

namespace Forum
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder.CreatePerOwinContext<DbContext>(() => new IdentityDbContext<UsuarioAplicacao>("DefaultConnection"));
            builder.CreatePerOwinContext<IUserStore<UsuarioAplicacao>>(
                (opcoes, contextoOwin) =>
                {
                    var dbContext = contextoOwin.Get<DbContext>();
                    return new UserStore<UsuarioAplicacao>(dbContext);
                }
                );

            builder.CreatePerOwinContext<UserManager<UsuarioAplicacao>>(
                (opcoes, contextoOwin) =>
            {
                var userStore = contextoOwin.Get<IUserStore<UsuarioAplicacao>>();
                var userManager = new UserManager<UsuarioAplicacao>(userStore);
                var useValidator = new UserValidator<UsuarioAplicacao>(userManager);
                useValidator.RequireUniqueEmail = true;

                userManager.UserValidator = useValidator;
                userManager.PasswordValidator = new SenhaValidador()
                {
                    TamanhoRequerido = 6,
                    ObrigatorioCaracteresEspeciais = true,
                    ObrigatorioDigitos = true,
                    ObrigatorioLowerCase = true,
                    ObrigatorioUpperCase = true,
                };


                userManager.EmailService = new EmailServico();

                var dataProtectionProvider = opcoes.DataProtectionProvider;
                var dataProtectionProviderCreate = dataProtectionProvider.Create("Forum");

                userManager.UserTokenProvider = new DataProtectorTokenProvider<UsuarioAplicacao>(dataProtectionProviderCreate);


                return userManager;
            }
                );
        }
    }
}