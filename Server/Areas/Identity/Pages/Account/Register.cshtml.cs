#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SyncfusionHelpDeskClient.Server.Models;

namespace SyncfusionHelpDeskClient.Server.Areas.Identity.Pages
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        const string ADMINISTRATION_ROLE = "Administrators";
        const string ADMINISTRATOR_USERNAME = "Admin@email";

        private readonly 
            SignInManager<ApplicationUser> _signInManager;
        
        private readonly 
            UserManager<ApplicationUser> _userManager;
        
        private readonly 
            RoleManager<IdentityRole> _roleManager;
        public RegisterModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> 
            ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage =
                "The {0} must be at least {2} and " +
                "at max {1} characters long.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage =
                "The password and confirmation " +
                "password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? "";
            ExternalLogins =
                (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();
        }

        public async Task<IActionResult> OnPostAsync(
            string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins =
                (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                { UserName = Input?.Email, Email = Input?.Email };

                var result =
                    await _userManager.CreateAsync(
                        user, Input?.Password);

                if (result.Succeeded)
                {
                    // Set confirm Email for user.
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    // Ensure there is a ADMINISTRATION_ROLE
                    var RoleResult = await _roleManager
                        .FindByNameAsync(ADMINISTRATION_ROLE);

                    if (RoleResult == null)
                    {
                        // Create ADMINISTRATION_ROLE role.
                        await _roleManager
                            .CreateAsync(
                            new IdentityRole(ADMINISTRATION_ROLE));
                    }

                    if (user.UserName?.ToLower() ==
                        ADMINISTRATOR_USERNAME.ToLower())
                    {
                        // Put admin in Administrator role.
                        await _userManager
                            .AddToRoleAsync(
                            user, ADMINISTRATION_ROLE);
                    }

                    // Log user in.
                    await _signInManager.SignInAsync(
                        user, isPersistent: false);
                    
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form.
            return Page();
        }
    }
}