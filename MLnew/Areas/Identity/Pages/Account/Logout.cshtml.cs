// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MLnew.Data;

namespace MLnew.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly ApplicationDbContext _context;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            var userEmail = User.Identity.Name;
            var logoutTime = DateTime.UtcNow;

            // Convert UTC to France time
            TimeZoneInfo franceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
            logoutTime = TimeZoneInfo.ConvertTimeFromUtc(logoutTime, franceTimeZone);

            // Find the last connection history record for the user with no logout time
            var connectionHistory = _context.ConnectionHistory
                .Where(c => c.UserEmail == userEmail && c.LogoutTime == null)
                .OrderByDescending(c => c.LoginTime)
                .FirstOrDefault();

            if (connectionHistory != null)
            {
                connectionHistory.LogoutTime = logoutTime;
                _context.ConnectionHistory.Update(connectionHistory);
                await _context.SaveChangesAsync();
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}