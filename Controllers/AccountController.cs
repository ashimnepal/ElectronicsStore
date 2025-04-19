using ElectronicsStore.Models;
using ElectronicsStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicsStore.Controllers
{
    /*
     * AccountController handles all user authentication and profile management including:
     * - User registration and login
     * - Admin registration (restricted to existing admins)
     * - User profile viewing and editing
     * - Password management
     */
    public class AccountController : Controller
    {
        // UserManager: Handles user operations like creating users and managing user data
        private readonly UserManager<ApplicationUser> _userManager;
        
        // SignInManager: Manages user sign-in, authentication, and sessions
        private readonly SignInManager<ApplicationUser> _signInManager;
        
        // RoleManager: Handles role-based operations for authorization
        private readonly RoleManager<IdentityRole> _roleManager;

        // Constructor: Initializes services via dependency injection
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // Displays the registration form for new users
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Processes the registration form submission
        // POST: /Account/Register
        // Collects user information, creates a new user account, assigns the Customer role
        // and automatically signs in the user upon successful registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new user object from the form data
                var user = new ApplicationUser
                {
                    UserName = model.Email,  // Using email as the username
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    PostalCode = model.PostalCode
                };

                // Attempt to create the user with the provided password
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign "Customer" role to every new registered user
                    await _userManager.AddToRoleAsync(user, "Customer");
                    
                    // Automatically sign in the newly registered user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // If there were errors creating the user, add them to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If ModelState is invalid or user creation failed, return to the form
            return View(model);
        }

        // Displays the login form
        // GET: /Account/Login
        // The returnUrl parameter allows redirecting back to the original page after login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Processes the login form submission
        // POST: /Account/Login
        // Authenticates the user credentials and establishes a session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Attempt to sign in the user with provided credentials
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // If there's a valid return URL, redirect there
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        // Otherwise, redirect to the home page
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    // If login fails, show an error message
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If ModelState is invalid, return to the login form
            return View(model);
        }

        // Signs the current user out of the application
        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // End the user's session
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Displays the admin registration form
        // GET: /Account/RegisterAdmin
        // Only accessible to users in the Admin role
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterAdmin()
        {
            return View("Register"); // Reuses the regular Register view
        }

        // Processes the admin registration form
        // POST: /Account/RegisterAdmin
        // Creates a new admin user and assigns the Admin role
        // Only accessible to existing admins
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdmin(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new user with admin flag
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    PostalCode = model.PostalCode,
                    IsAdmin = true  // Mark this user as an admin
                };

                // Attempt to create the new admin user
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign the "Admin" role to the new user
                    await _userManager.AddToRoleAsync(user, "Admin");
                    
                    // Show success message and redirect to admin dashboard
                    TempData["SuccessMessage"] = "Admin user created successfully!";
                    return RedirectToAction("Index", "Admin");
                }

                // If there were errors creating the admin, add them to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If ModelState is invalid or admin creation failed, return to the form
            return View("Register", model);
        }

        // Displays the user's profile information
        // GET: /Account/Profile
        // Requires authentication
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Get the user's roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Create profile view model with user data
            var model = new ProfileViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                PhoneNumber = user.PhoneNumber,
                Roles = userRoles
            };

            return View(model);
        }

        // Displays the profile edit form
        // GET: /Account/EditProfile
        // Requires authentication
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Create edit profile view model with current user data
            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        // Processes the profile edit form submission
        // POST: /Account/EditProfile
        // Updates the current user's profile information
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Update user data with form values
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.PostalCode = model.PostalCode;
            user.PhoneNumber = model.PhoneNumber;

            // Save the updated user data
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                // If update fails, add errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Success message and redirect to profile page
            TempData["SuccessMessage"] = "Your profile has been updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // Displays the change password form
        // GET: /Account/ChangePassword
        // Requires authentication
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Processes the change password form submission
        // POST: /Account/ChangePassword
        // Updates the current user's password
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Attempt to change the user's password
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                // If change fails, add errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Refresh the user's sign-in cookie
            await _signInManager.RefreshSignInAsync(user);
            
            // Success message and redirect to profile page
            TempData["SuccessMessage"] = "Your password has been changed successfully.";
            return RedirectToAction(nameof(Profile));
        }
    }
} 