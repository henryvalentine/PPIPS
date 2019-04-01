using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ImportPermitPortal.Models
{
    public class ApplicationRole : IdentityRole<string, ApplicationDbInitializer.ApplicationUserRole>, IRole<string>
    {

        public string Description { get; set; }
        
        public ApplicationRole() { }

        public ApplicationRole(string name)

            : this()
        {

            Name = name;

        }
        
        public ApplicationRole(string name, string description)

            : this(name)
        {

            Description = description;

        }

    }
}