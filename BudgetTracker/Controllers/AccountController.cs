using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.Utils;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // GET: Account/AccountDetails
    [TypeFilter(typeof(AuthorizationFilter))]
    public async Task<IActionResult> AccountDetails()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
        {
            return RedirectToAction("Login", "Account");
        }
            
        var user = await _context.User
            .FirstOrDefaultAsync(m => m.UserId == currentUserId);

        return View(user);
    }
    
    // GET: Account/ChangePassword
    [TypeFilter(typeof(AuthorizationFilter))]
    public async Task<IActionResult> ChangePassword()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.User.FindAsync(currentUserId);
        if (user == null)
        {
            return NotFound();
        }

        return View();
    }

    // POST: User/ChangePassword/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [TypeFilter(typeof(AuthorizationFilter))]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        var user = await _context.User
            .FirstOrDefaultAsync(m => m.UserId == currentUserId);
        if (user == null)
        {
            return NotFound();
        }
            
        if (ModelState.IsValid)
        {
            user.PasswordHash = HashEngine.ComputeMd5Hash(model.Password);
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AccountDetails));
        }
        
        return View(model);
    }
    
    // GET: User/Edit/5
    [TypeFilter(typeof(AuthorizationFilter))]
    public async Task<IActionResult> Edit()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.User.FindAsync(currentUserId);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    // POST: User/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [TypeFilter(typeof(AuthorizationFilter))]
    public async Task<IActionResult> Edit([Bind("UserId,Username,Email,Name,Surname,IsAdmin,RegistrationDate,PasswordHash,ApiToken")] User user)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.User.Any(e => e.UserId == currentUserId))
                {
                    return NotFound();
                }
            }
                
            return RedirectToAction(nameof(AccountDetails));
        }
        return View(user);
    }
    
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(NewUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (await _context.User.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "Account with this username already exists");
                return View(model);
            }
            if (await _context.User.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Account with this email already exists");
                return View(model);
            }
            
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
                PasswordHash = HashEngine.ComputeMd5Hash(model.Password),
                IsAdmin = false,
                ApiToken = ApiTokenProvider.Generate(),
                RegistrationDate = DateTime.Now
            };
            
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }
        
        return View(model);
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string login, string password)
    {
        var user = _context.User.FirstOrDefault(u => u.Username == login);
        string hashedPassword = HashEngine.ComputeMd5Hash(password);
            
        if (user != null && user.PasswordHash == hashedPassword)
        {
            HttpContext.Session.SetString("IsLoggedIn", "True");
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
            Console.WriteLine(user.IsAdmin.ToString());
            return RedirectToAction("Index", "Home");
        }
            
        ViewBag.Error = "Nieprawidłowy login lub hasło";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("IsLoggedIn");
        HttpContext.Session.Remove("UserId");
        HttpContext.Session.Remove("IsAdmin");
        return RedirectToAction("Index", "Home");
    }
}