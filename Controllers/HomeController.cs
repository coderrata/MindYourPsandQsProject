using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MindYourInjections.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace MindYourInjections.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MyContext _context;
        public HomeController(ILogger<HomeController> logger, MyContext dbcontext)
        {
            _logger = logger;
            _context = dbcontext;
        }

        //Login or Register page
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                _context.Add(newUser);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("UserId", newUser.UserId);
                HttpContext.Session.SetString("UserEmail", newUser.Email);
                return RedirectToAction("dashboard");
            } else {
                return View("Index");
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LogUser logUser)
        {
            if (ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(s => s.Email == logUser.LEmail);
                if (userInDb == null)
                {
                    ModelState.AddModelError("LPassword", "Incorrect user information, please register");
                    return View("Index");
                }
                PasswordHasher<LogUser> Hasher = new PasswordHasher<LogUser>();
                PasswordVerificationResult result = Hasher.VerifyHashedPassword(logUser, userInDb.Password, logUser.LPassword);
                if (result == 0)
                {
                    ModelState.AddModelError("LPassword", "Incorrect user information, please register");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                HttpContext.Session.SetString("UserEmail", userInDb.Email);
                return RedirectToAction("dashboard");
            } else {
                return View("Index");
            }
        }

        //add language and all Languages
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            if ( HttpContext.Session.GetString("UserEmail") != null)
            {
                ViewBag.RussianDoll = _context.Users.Include(p => p.Languages)
                .FirstOrDefault(p => p.UserId == HttpContext.Session.GetInt32("UserId"));
                ViewBag.userId = HttpContext.Session.GetInt32("UserId");
                return View();
            } else {
                return View("Index");
            }
        }

        [HttpPost("addLanguage")]
        public IActionResult addLanguage(Language newLanguage)
        {
            if(ModelState.IsValid)
            {
                _context.Languages.Add(newLanguage);
                _context.SaveChanges();
                return RedirectToAction("dashboard");
            } else {
                return View("Dashboard");
            }
        }

        //add objective and all objectives
        [HttpGet("language/{lid}")]
        public IActionResult Objectives(int lid)
        {
            if ( HttpContext.Session.GetString("UserEmail") != null)
            {
                HttpContext.Session.SetInt32("lid", lid);
                ViewBag.RussianDoll = _context.Languages.Include(p => p.Objectives)
                .FirstOrDefault(p => p.LanguageId == lid);
                ViewBag.languageId = lid;
                return View("Objectives");
            } else {
                return View("Index");
            }
        }

        [HttpPost("addObjective")]
        public IActionResult addObjective(int lid, Objective newObjective)
        {
            ViewBag.RussianDoll = _context.Languages
            .Include(p => p.Objectives)
            .FirstOrDefault(p => p.LanguageId == lid);
            if(ModelState.IsValid)
            {
                _context.Objectives.Add(newObjective);
                _context.SaveChanges();
                return Redirect($"language/{newObjective.LanguageId}");
            } else {
                return View("Objectives");
            }
        }

        //add step and all steps
        [HttpGet("objective/{oid}")]
        public IActionResult Steps(int oid)
        {
            if ( HttpContext.Session.GetString("UserEmail") != null)
            {
                HttpContext.Session.SetInt32("oid", oid);
                ViewBag.RussianDoll = _context.Objectives
                .Include(p => p.Steps)
                .FirstOrDefault(p => p.ObjectiveId == oid);
                ViewBag.objectiveId = oid;
                return View("Steps");
            } else {
                return View("Index");
            }
        }
        
        [HttpPost("addStep")]
        public IActionResult addStep(int oid, Step newStep)
        {
            ViewBag.RussianDoll = _context.Objectives.Include(p => p.Steps)
            .FirstOrDefault(p => p.ObjectiveId == oid);
            ViewBag.objectiveId = oid;
            if(ModelState.IsValid)
            {
                _context.Steps.Add(newStep);
                _context.SaveChanges();
                return Redirect($"objective/{newStep.ObjectiveId}");
            } else {
                ViewBag.RussianDoll = _context.Objectives.Include(p => p.Steps)
                .FirstOrDefault(p => p.ObjectiveId == oid);
                return View("Steps");
            }
        }

        //Views for editing 
        [HttpGet("edit/language/{lid}")]
        public IActionResult EditLanguage(int lid)
        {
            if ( HttpContext.Session.GetString("UserEmail") != null)
            {
                Language LanguageToEdit = _context.Languages
                .FirstOrDefault(p => p.LanguageId == lid);
                return View(LanguageToEdit);
            } else {
                return View("Index");
            }
        }

        [HttpGet("edit/objective/{oid}")]
        public IActionResult EditObjective(int oid)
        {
            if ( HttpContext.Session.GetString("UserEmail") != null)
            {
                Objective ObjectiveToEdit = _context.Objectives
                .FirstOrDefault(p => p.ObjectiveId == oid);
                return View(ObjectiveToEdit);
            } else {
                return View("Index");
            }
        }

        [HttpGet("edit/step/{sid}")]
        public IActionResult EditStep(int sid)
        {
            ViewBag.RussianDoll = _context.Steps
            .Include(p => p.Objective)
            .FirstOrDefault(p => p.StepId == sid);
            if ( HttpContext.Session.GetString("UserEmail") != null)
            {
                Step StepToEdit = _context.Steps
                .FirstOrDefault(p => p.StepId == sid);
                return View(StepToEdit);
            } else {
                return View("Index");
            }
        }

        //posts for updating
        [HttpPost("update/language/{lid}")]
        public IActionResult UpdateLanguage(int lid, Language updatedLanguage)
        {
            Language LanguageToEdit = _context.Languages
                .FirstOrDefault(p => p.LanguageId == lid);
            if(ModelState.IsValid)
            {
                LanguageToEdit.Name = updatedLanguage.Name;
                LanguageToEdit.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                return RedirectToAction("dashboard");
            } else {
                return View("EditLanguage", LanguageToEdit);
            }
        }

        [HttpPost("update/objective/{oid}")]
        public IActionResult UpdateObjective(int oid, Objective updatedObjective)
        {
            Objective ObjectiveToEdit = _context.Objectives
                .FirstOrDefault(p => p.ObjectiveId == oid);
            if(ModelState.IsValid)
            {
                ObjectiveToEdit.Name = updatedObjective.Name;
                ObjectiveToEdit.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                return Redirect($"/language/{updatedObjective.LanguageId}");
            } else {
                return View("EditObjective", ObjectiveToEdit);
            }
        }

        [HttpPost("update/step/{sid}")]
        public IActionResult UpdateStep(int sid, Step updatedStep)
        {
            Step StepToEdit = _context.Steps
                .FirstOrDefault(p => p.StepId == sid);
            if(ModelState.IsValid)
            {
                StepToEdit.AStep = updatedStep.AStep;
                StepToEdit.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                return Redirect($"/objective/{updatedStep.ObjectiveId}");
            } else {
                return View("EditStep", StepToEdit);
            }
        }

        //delete
        [HttpGet("deleteLanguage/{lid}")]
        public IActionResult DeleteLanguage(int lid)
        {
            if(HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Index");
            }
            Language ToDelete = _context.Languages
            .SingleOrDefault(p => p.LanguageId == lid);
            _context.Languages.Remove(ToDelete);
            _context.SaveChanges();
            return RedirectToAction("dashboard");
        }

        [HttpGet("deleteObjective/{oid}")]
        public IActionResult DeleteObjective(int oid)
        {
            if(HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Index");
            }
            var Language = HttpContext.Session.GetInt32("lid");
            Objective ToDelete = _context.Objectives
            .SingleOrDefault(p => p.ObjectiveId == oid);
            _context.Objectives.Remove(ToDelete);
            _context.SaveChanges();
            return Redirect($"/language/{Language}");
        }

        [HttpGet("deleteStep/{sid}")]
        public IActionResult DeleteStep(int sid)
        {
            if(HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Index");
            }
            var Objective = HttpContext.Session.GetInt32("oid");
            Step ToDelete = _context.Steps
            .SingleOrDefault(p => p.StepId == sid);
            _context.Steps.Remove(ToDelete);
            _context.SaveChanges();
            return Redirect($"/objective/{Objective}");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
