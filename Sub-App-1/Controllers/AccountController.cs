namespace Sub_App_1.Controllers
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Sub_App_1.Models;

    /*
* TODO: Consider using ASP.NET Core Identity instead.
* Oppdatere Index til Accountindex på grunn av rename av cshtml filen 
*/ 
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // /Account/Index
        public IActionResult Index()
        {
            return View();
        }

        // /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            //Egen variabler for resultatet slik at det kan sjekke med if-setning
            var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Finn bruker variabler 
                var user = await _userManager.FindByNameAsync(username);
                var roles = await _userManager.GetRolesAsync(user);

                // Redirect basert på bruker type direkte til riktig view for brukeren 
                // Kan vi legge inn flere basert på roller
                //Problem etter logget inn forsvinner FoodProducer Dashboard, ikke klart å løses enda
                if (roles.Contains("FoodProducer"))
                {
                    return RedirectToAction("Dashboard", "FoodProducer");
                }
                else
                {
                    return RedirectToAction("Index", "Home"); // Default for andre brukere/roller eventuelt gjestebruker?
                }
            }
            ViewBag.Error = "Invalid username or password.";
            return View("Index");
        }

       // /Account/Logout
    public async Task<IActionResult> Logout() {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(string username, string password, string role) {
        if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
            ModelState.AddModelError(string.Empty, "Username and password cannot be null or empty.");
            return View("Index", ModelState);
        }

        var user = new IdentityUser {
            UserName = username
        };
        var result = await _userManager.CreateAsync(user, password); // create user (attempt)
        
        if(result.Succeeded) {
            if(string.IsNullOrEmpty(role)) {
                await _userManager.AddToRoleAsync(user, UserRoles.RegularUser); // default role
            } else {
                if(await _roleManager.RoleExistsAsync(role)) {
                    await _userManager.AddToRoleAsync(user, role);
                } else {
                    ModelState.AddModelError(string.Empty, "Invalid role specified.");
                    ViewBag.Error = "Error during registration.";
                    return View("Index", ModelState);
                }
            }
    
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");   
        }

        foreach (var error in result.Errors) {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        ViewBag.Error = "Error during registration.";
        return View("Index", ModelState);
    }

    // /Account/BrowseAsGuest
    public IActionResult BrowseAsGuest() {
        // todo ..
        // continue browsing without an account
        return RedirectToAction("Index", "Home");
    }
}