using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AppMVC.ExtendMethods
{
    public static class ModelStateExtend
    {
        public static void AddModelError(this ModelStateDictionary ModelState, string msg)
        {
            ModelState.AddModelError(string.Empty, msg);
        }
        public static void AddModelError(this ModelStateDictionary ModelState, IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Description);
            }
        }
    }
}