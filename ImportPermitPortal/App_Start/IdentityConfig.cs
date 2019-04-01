using System.Security.Claims;
using ImportPermitPortal.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Net.Mail;
using ImportPermitPortal.DataObjects.Helpers;

namespace ImportPermitPortal.Models
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    // *** PASS IN TYPE ARGUMENT TO BASE CLASS:

    public class ApplicationUserManager : UserManager<ApplicationUser, string>
    {

        // *** ADD string TYPE ARGUMENT TO CONSTRUCTOR CALL:

        public ApplicationUserManager(IUserStore<ApplicationUser, string> store)

            : base(store)
        {

        }



        public static ApplicationUserManager Create(

            IdentityFactoryOptions<ApplicationUserManager> options,

            IOwinContext context)
        {

            // *** PASS CUSTOM APPLICATION USER STORE AS CONSTRUCTOR ARGUMENT:

            var manager = new ApplicationUserManager(

                new ApplicationUserStore(context.Get<ApplicationDbContext>()));

            // Configure validation logic for usernames

            // *** ADD string TYPE ARGUMENT TO METHOD CALL:

            manager.UserValidator = new UserValidator<ApplicationUser, string>(manager)

            {
                AllowOnlyAlphanumericUserNames = false,

                RequireUniqueEmail = true

            };



            // Configure validation logic for passwords

            manager.PasswordValidator = new PasswordValidator

            {

                RequiredLength = 8,

                RequireNonLetterOrDigit = true,

                RequireDigit = true,

                RequireLowercase = true,

                RequireUppercase = true,

            };



            // Configure user lockout defaults

            manager.UserLockoutEnabledByDefault = true;

            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);

            manager.MaxFailedAccessAttemptsBeforeLockout = 5;



            // Register two factor authentication providers. 

            // This application uses Phone and Emails as a step of receiving a 

            // code for verifying the user You can write your own provider and plug in here.

            // *** ADD string TYPE ARGUMENT TO METHOD CALL:

            manager.RegisterTwoFactorProvider("PhoneCode",

                new PhoneNumberTokenProvider<ApplicationUser, string>

                {
                    MessageFormat = "Your security code is: {0}"

                });



            // *** ADD string TYPE ARGUMENT TO METHOD CALL:

            manager.RegisterTwoFactorProvider("EmailCode",

                new EmailTokenProvider<ApplicationUser, string>

                {

                    Subject = "SecurityCode",

                    BodyFormat = "Your security code is {0}"

                });



            manager.EmailService = new EmailService();

            manager.SmsService = new SmsService();

            var dataProtectionProvider = options.DataProtectionProvider;

            if (dataProtectionProvider != null)
            {
                // *** ADD string TYPE ARGUMENT TO METHOD CALL:

                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser, string>(
                        dataProtectionProvider.Create("ASP.NET Identity"));

            }

            return manager;

        }

    }


    // Configure the RoleManager used in the application. RoleManager is defined in the ASP.NET Identity core assembly
    // PASS CUSTOM APPLICATION ROLE AND string AS TYPE ARGUMENTS TO BASE:

    public class ApplicationRoleManager : RoleManager<ApplicationRole, string>
    {

        // PASS CUSTOM APPLICATION ROLE AND string AS TYPE ARGUMENTS TO CONSTRUCTOR:

        public ApplicationRoleManager(IRoleStore<ApplicationRole, string> roleStore)

            : base(roleStore)
        {

        }

        // PASS CUSTOM APPLICATION ROLE AS TYPE ARGUMENT:

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options,
            IOwinContext context)
        {
            return new ApplicationRoleManager(new ApplicationRoleStore(context.Get<ApplicationDbContext>()));

        }

    }

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
                var settings = (System.Net.Configuration.MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                if (settings != null)
                {
                    // Credentials:
                    var credentialUserName = settings.Smtp.Network.UserName;
                    var sentFrom = new MailAddress(settings.Smtp.From);
                    var pwd = settings.Smtp.Network.Password;

                    // Configure the client:
                    System.Net.Mail.SmtpClient client =
                        new System.Net.Mail.SmtpClient("smtp.gmail.com");

                    client.Port = 587;
                    client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;

                    // Create the credentials:
                    System.Net.NetworkCredential credentials =
                        new System.Net.NetworkCredential(credentialUserName, pwd);

                    client.EnableSsl = true;
                    client.Credentials = credentials;

                    // Create the message:
                    var mail = new MailMessage(sentFrom, new MailAddress(message.Destination));

                    mail.Subject = message.Subject;
                    mail.Body = message.Body;
                    mail.IsBodyHtml = true;
                    // Send:
                    return client.SendMailAsync(mail);
                }
                return null;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // This is useful if you do not want to tear down the database each time you run the application.
    // public class ApplicationDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    // This example shows you how to create a new database if the Model changes
    public class ApplicationDbInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            InitializeIdentityForEF(context);
            base.Seed(context);
        }

        //Create User=Admin@Admin.com with password=Admin@123456 in the Admin role        
        public static void InitializeIdentityForEF(ApplicationDbContext db)
        {
            
        }

        public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
        {
            public ApplicationSignInManager(ApplicationUserManager userManager,
                IAuthenticationManager authenticationManager) :
                    base(userManager, authenticationManager)
            {
            }

            public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
            {
                return user.GenerateUserIdentityAsync((ApplicationUserManager) UserManager);
            }

            public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options,
                IOwinContext context)
            {
                return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(),
                    context.Authentication);
            }
        }

        public class ApplicationUserLogin : IdentityUserLogin<string>
        {
        }

        public class ApplicationUserClaim : IdentityUserClaim<string>
        {
        }

        public class ApplicationUserRole : IdentityUserRole<string>
        {
        }
    }
}