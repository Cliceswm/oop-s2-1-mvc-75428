using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        // Constructor - gets RoleManager from dependency injection
        public AdminController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // GET: Admin/Roles
        // This shows the list of roles
        public IActionResult Roles()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        // POST: Admin/CreateRole
        // This creates a new role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                // Check if role already exists
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    // Create the new role
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    TempData["Success"] = $"Role '{roleName}' created successfully!";
                }
                else
                {
                    TempData["Error"] = $"Role '{roleName}' already exists.";
                }
            }
            else
            {
                TempData["Error"] = "Role name cannot be empty.";
            }

            return RedirectToAction("Roles");
        }

        // POST: Admin/DeleteRole
        // This deletes a role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    await _roleManager.DeleteAsync(role);
                    TempData["Success"] = $"Role '{roleName}' deleted successfully!";
                }
            }

            return RedirectToAction("Roles");
        }
    }
}