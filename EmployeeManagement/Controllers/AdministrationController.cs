using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EmployeeManagement.Controllers
{
    [Authorize(Policy = "AccessPolicy")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager,
                                        ILogger<AdministrationController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await CreateIdentityRoleAsync(model);
                if (ResultValidation(result))  return RedirectToAction("ListRoles"); 
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await GetRoleByIdAsync(id);
            if (VerificationObj(role)) { MessageNF(id); }
            var model = await EditRoleVMAsync(role);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await GetRoleByIdAsync(model.Id);
            if (VerificationObj(role)) { MessageNF(model.Id); }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if (ResultValidation(result)) { return RedirectToAction("ListRoles"); }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUserInRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role = await GetRoleByIdAsync(roleId);
            if (VerificationObj(role)) { MessageNF(roleId); }
            var model = await EditUserInRoleVMAsync(role);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserInRole(List<UserRoleViewModel> models, string roleId)
        {
            var role = await GetRoleByIdAsync(roleId);
            if (VerificationObj(role)) { MessageNF(roleId); }
            var res = await EditUserInRoleVMAsync(models, role);
            if (res) { return RedirectToAction("EditRole", new { Id = role.Id }); } else { return RedirectToAction("EditRole", new { Id = roleId }); }
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users.ToList();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await GetUserByIdAsync(id);
            if (VerificationObj(user)) { MessageNF(id); }
            var model = await EditUserVMAsync(user);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await GetUserByIdAsync(model.Id);
            if (VerificationObj(user)) { MessageNF(model.Id); }
            else
            {
                user.Email = model.Email; user.City = model.City; user.UserName = model.UserName;
                var result = await userManager.UpdateAsync(user);
                if (ResultValidation(result)) { return RedirectToAction("ListUsers"); }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await GetUserByIdAsync(id);
            if (VerificationObj(user)) { MessageNF(id); }
            else
            {
                var result = await userManager.DeleteAsync(user);
                if (ResultValidation(result)) { return RedirectToAction("ListUsers"); }
            }
            return View("ListUsers");
        }

        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await GetRoleByIdAsync(id);
            if (VerificationObj(role)) { MessageNF(id); }
            try
            {
                var result = await roleManager.DeleteAsync(role);
                if (ResultValidation(result)) { return RedirectToAction("ListRoles"); }
                return View("ListRoles");
            }
            catch (Exception ex)
            {
                if (RoleDeleteError(role, ex)) { return View("Error"); }
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;
            var user = await GetUserByIdAsync(userId);
            if (VerificationObj(user)) { MessageNF(userId); }
            var model = new List<UserRolesViewModel>();
            foreach (var roles in roleManager.Roles.ToList())
            {
                var userRolesViewModel = new UserRolesViewModel { RoleId = roles.Id, RoleName = roles.Name };
                if (await GetBoolRoleAsync(user, roles.Name)) { userRolesViewModel.IsSelected = true; }
                else { userRolesViewModel.IsSelected = false; }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (VerificationObj(user)) { MessageNF(userId); }
            var roles = await GetRolesByUser(user);
            var result = await RemoveUserFromRoleAsync(user, roles);
            if (!result.Succeeded) { ModelState.AddModelError("", "Cannot remove user from exsisting roles"); return View(model); }
            result = await userManager.AddToRolesAsync(user, model.Where(x => x.IsSelected).Select(y => y.RoleName));
            if (!result.Succeeded) { ModelState.AddModelError("", "Cannot add user to selected roles"); return View(model); }
            return RedirectToAction("EditUser", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string id)
        {
            var user = await GetUserByIdAsync(id);
            if (VerificationObj(user)) { MessageNF(id); }
            var exsistingUserClaims = await GetClaimsByUser(user);
            var model = new UserClaimsViewModel { UserId = id };
            foreach (var claim in ClaimsStore.AllClaims.ToList())
            {
                UserClaim userClaim = new UserClaim { ClaimType = claim.Type };
                if (exsistingUserClaims.Any(c => c.Type == claim.Type && c.Value == "true")) { userClaim.IsSelected = true; }
                model.Claims.Add(userClaim);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await GetUserByIdAsync(model.UserId);
            if (VerificationObj(user)) { MessageNF(model.UserId); }
            var claims = await GetClaimsByUser(user);
            var result = await RemoveClaimsByUserAndClaim(user, claims);
            if (!result.Succeeded) { ModelState.AddModelError("", "Cannot remove user from exsisting Claim"); return View(model); }
            result = await userManager.AddClaimsAsync(user, model.Claims.Select(c => new Claim(c.ClaimType, c.IsSelected ? "true" : "false")));
            if (!result.Succeeded) { ModelState.AddModelError("", "Cannot Add select claims for user"); return View(model); }
            return RedirectToAction("EditUser", new { id = model.UserId });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<IdentityResult> RemoveClaimsByUserAndClaim(ApplicationUser user, IList<Claim> claims)
        {
            var result = await userManager.RemoveClaimsAsync(user, claims);
            if (result.Succeeded) { return result; } else { return null; }
        }

        private async Task<IList<Claim>> GetClaimsByUser(ApplicationUser user)
        {
            var claim = await userManager.GetClaimsAsync(user);
            if (claim != null) { return claim; } else { return null; }
        }

        private bool RoleDeleteError(IdentityRole role, Exception ex)
        {
            if (role != null && ex != null)
            {
                logger.LogError($"Error deleting role {ex}");
                ViewBag.ErrorTitle = $"{role.Name} is in use";
                ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted as there are users in this role. " +
                    $"If you want to delete this role, please remove the users from the role and then try to delete";
                return true;
            }
            else { return false; }
        }

        private async Task<EditRoleViewModel> EditRoleVMAsync(IdentityRole role)
        {
            var model = new EditRoleViewModel { Id = role.Id, RoleName = role.Name };
            if (model != null)
            {
                foreach (var user in userManager.Users.ToList())
                { if (await GetBoolRoleAsync(user, role.Name)) { model.Users.Add(user.UserName); } }
                return model;
            }
            else { return null; }
        }

        private async Task<List<UserRoleViewModel>> EditUserInRoleVMAsync(IdentityRole role)
        {
            var model = new List<UserRoleViewModel>();
            foreach (var user in userManager.Users.ToList())
            {
                var userRoleViewModel = CreateUserRolesVM(user);
                if (await GetBoolRoleAsync(user, role.Name)) { userRoleViewModel.IsSelected = true; }
                else { userRoleViewModel.IsSelected = false; }
                model.Add(userRoleViewModel);
            }
            if (model != null) { return model; } else { return null; }
        }

        private UserRoleViewModel CreateUserRolesVM(ApplicationUser user)
        {
            var roles = new UserRoleViewModel { UserId = user.Id, UserName = user.UserName };
            if (roles != null) { return roles; }
            else { return null; }
        }

        private bool ResultValidation(IdentityResult result)
        {
            if (result.Succeeded) { return true; }
            else { foreach (var error in result.Errors) { ModelState.AddModelError("", error.Description); } return false; }
        }

        private async Task<bool> EditUserInRoleVMAsync(List<UserRoleViewModel> models, IdentityRole role)
        {
            IdentityResult result = null;
            for (int i = 0; i < models.Count; i++)
            {
                var user = await GetUserByIdAsync(models[i].UserId);
                if (models[i].IsSelected && !(await GetBoolRoleAsync(user, role.Name))) { result = await AddRoleAsync(user, role.Name); }
                else if (!models[i].IsSelected && await GetBoolRoleAsync(user, role.Name)) { result = await RemoveUserFromRoleByRoleNameAsync(user, role.Name); }
                else { continue; }
                if (result.Succeeded)
                {
                    if (i < (models.Count - 1)) { continue; }
                    else { return true; }
                }
            }
            return false;
        }

        private async Task<IdentityResult> AddRoleAsync(ApplicationUser user, string roleName)
        {
            IdentityResult result = null;
            result = await userManager.AddToRoleAsync(user, roleName);
            if (result != null) { return result; }
            else { return null; }
        }

        private async Task<IdentityResult> RemoveUserFromRoleAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            var result = await userManager.RemoveFromRolesAsync(user, roles);
            if (result != null) { return result; }
            else { return null; }
        }

        private async Task<IdentityResult> RemoveUserFromRoleByRoleNameAsync(ApplicationUser user, string roleName)
        {
            IdentityResult result = null;
            result = await userManager.RemoveFromRoleAsync(user, roleName);
            if (result != null) { return result; }
            else { return null; }
        }

        private async Task<IdentityResult> CreateIdentityRoleAsync(CreateRoleViewModel model)
        {
            IdentityRole identityRole = new IdentityRole { Name = model.RoleName };
            if (identityRole != null) { var result = await roleManager.CreateAsync(identityRole); return result; } else { return null; }
        }

        private async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null) { return user; }
            else { return null; }
        }

        private async Task<IList<string>> GetRolesByUser(ApplicationUser user)
        {
            var role = await userManager.GetRolesAsync(user);
            if (role != null) { return role; }
            else { return null; }
        }

        private async Task<IdentityRole> GetRoleByIdAsync(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role != null) { return role; }
            else { return null; }
        }

        private async Task<bool> GetBoolRoleAsync(ApplicationUser user, string name)
        {
            bool res = await userManager.IsInRoleAsync(user, name);
            if (res) { return true; }
            else { return false; }
        }

        private async Task<EditUserViewModel> EditUserVMAsync(ApplicationUser user)
        {
            var userClaims = await GetClaimsByUser(user);
            var userRoles = await GetRolesByUser(user);
            var model = new EditUserViewModel { Id = user.Id, UserName = user.UserName, Email = user.Email, City = user.City, Claims = userClaims.Select(c => c.Type + " : " + c.Value).ToList(), Roles = userRoles };
            if (model != null) { return model; } else { return null; }
        }

        private bool VerificationObj(System.Object obj) { if (obj == null) { return true; } else { return false; } }

        private IActionResult MessageNF(string id) { ViewBag.ErrorMessage = $"Request with id {id}, cannot be proccessed please try again later"; return View("NotFound"); }
    }
}