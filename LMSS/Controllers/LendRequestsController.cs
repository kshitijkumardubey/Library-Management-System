using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LMSS.Models;
using Microsoft.AspNetCore.Http;

namespace LMSS.Controllers
{
    public class LendRequestsController : Controller
    {
        private readonly LibraryManagementSystemContext _context;
        private readonly IAccountRepo _accountRepo;

        public LendRequestsController(LibraryManagementSystemContext context, IAccountRepo accountRepo)
        {
            _context = context;
            _accountRepo = accountRepo;
        }

        // GET: LendRequests
        
public async Task<IActionResult> LendBooks() { 
            var libraryManagementSystemContext = _context.LendRequests.Include(l => l.Book).Include(l => l.User);
            return View(await libraryManagementSystemContext.ToListAsync()); 
        }

        public async Task<IActionResult> LendHistory() { 
            var libraryManagementSystemContext = _context.LendRequests.Include(l => l.Book); 
            return View(await libraryManagementSystemContext.ToListAsync()); 
        }


        public async Task<IActionResult> UserRecords()
        {
            var username = HttpContext.Session.GetString("username");
            var user = _context.Accounts.Where(b => b.UserName == username).FirstOrDefault();
            var Library_Management_SystemContext = _context.LendRequests.Where(b => b.UserId == user.UserId).Include(l => l.Book).Include(l => l.User);
            return View(await Library_Management_SystemContext.ToListAsync());
        }
        public ActionResult Approved(int LendId, int BookId)
        {
            var abc = _context.LendRequests.FirstOrDefault(b => b.LendId == LendId);
            abc.LendStatus = "Approved";
            abc.ReturnDate = DateTime.Now.AddDays(7);
            _context.Books.SingleOrDefault(b => b.BookId == BookId).IssuedBooks++;
            if (System.DateTime.Now.Subtract(abc.ReturnDate).TotalDays > 0 && abc.LendStatus == "Approved")
            {
                abc.FineAmount += 100;
            }
            _context.SaveChanges();
            //ViewBag.abc = abc.LendStatus;
            return RedirectToAction("Admin_dash", "LendRequests");
        }

        public ActionResult Rejected(int LendId)
        {
            var abc = _context.LendRequests.FirstOrDefault(b => b.LendId == LendId);
                abc.LendStatus = "Declined";
            _context.SaveChanges();
            return RedirectToAction("Admin_dash","LendRequests");
        }
        public ActionResult Lend(int bookId)
        {
            var username = HttpContext.Session.GetString("username");
            var user = _accountRepo.GetAccountByName(username);
            var noofcopies = _context.Books.SingleOrDefault(b => b.BookId == bookId).NoOfCopies;
            if (noofcopies <= 0)
            {
                return View("RequestedError");
            }
            var alreadyrequested = _context.LendRequests.FirstOrDefault(b => b.BookId == bookId && b.UserId == user.UserId && b.LendStatus == "Requested");



            if (alreadyrequested != null && alreadyrequested.LendStatus == "Requested")
            {
                return View("AlreadyRequestError");
            }
            _context.Books.SingleOrDefault(b => b.BookId == bookId).NoOfCopies--;
            LendRequest lendRequest = new LendRequest()
            {
                LendStatus = "Requested",
                LendDate = System.DateTime.Now,
                BookId = bookId,
                UserId = user.UserId,
                Book = _context.Books.SingleOrDefault(b => b.BookId == bookId),
                User = _context.Accounts.SingleOrDefault(u => u.UserId == user.UserId),
            };
            _context.LendRequests.Add(lendRequest);
            _context.SaveChanges();



            return RedirectToAction("Requested");
        }
        public ActionResult Requested()
        {
            return View();
        }
        public ViewResult Admin_dash()
        {
            var allreq = _context.LendRequests.Include(l => l.Book).Include(l => l.User);

            return View(allreq.ToList());
        }
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _accountRepo.GetAccountByName(username);
            var User = _context.LendRequests.Where(u => u.UserId == user.UserId).Include(i => i.Book).Include(i => i.User);


            return View(await User.ToListAsync());
        }

        public async Task<IActionResult> IssuedBooks()
        {
            var username = HttpContext.Session.GetString("username");
            var user = _context.Accounts.Where(b => b.UserName == username).FirstOrDefault();
            var Library_Management_SystemContext = _context.LendRequests.Where(b => b.UserId == user.UserId && b.LendStatus.Equals("Approved")).Include(l => l.Book).Include(l => l.User);
            return View(await Library_Management_SystemContext.ToListAsync());
        }
        public ActionResult ReturnQuery(int bookId)
        {
            var returnBook = _context.LendRequests.FirstOrDefault(b => b.BookId == bookId);
            returnBook.LendStatus = "Returned";
            returnBook.ReturnDate = System.DateTime.Now;
            _context.Books.SingleOrDefault(b => b.BookId == bookId).NoOfCopies++;
            _context.Books.SingleOrDefault(b => b.BookId == bookId).IssuedBooks--;
            _context.SaveChanges();
            return RedirectToAction("IssuedBooks", "LendRequests");
        }

        // GET: LendRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lendRequest = await _context.LendRequests
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LendId == id);
            if (lendRequest == null)
            {
                return NotFound();
            }

            return View(lendRequest);
        }

        // GET: LendRequests/Create
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId");
            ViewData["UserId"] = new SelectList(_context.Accounts, "UserId", "Password");
            return View();
        }

        // POST: LendRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LendId,LendStatus,LendDate,ReturnDate,UserId,BookId,FineAmount")] LendRequest lendRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lendRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", lendRequest.BookId);
            ViewData["UserId"] = new SelectList(_context.Accounts, "UserId", "Password", lendRequest.UserId);
            return View(lendRequest);
        }

        // GET: LendRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lendRequest = await _context.LendRequests.FindAsync(id);
            if (lendRequest == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", lendRequest.BookId);
            ViewData["UserId"] = new SelectList(_context.Accounts, "UserId", "Password", lendRequest.UserId);
            return View(lendRequest);
        }

        // POST: LendRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LendId,LendStatus,LendDate,ReturnDate,UserId,BookId,FineAmount")] LendRequest lendRequest)
        {
            if (id != lendRequest.LendId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lendRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LendRequestExists(lendRequest.LendId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", lendRequest.BookId);
            ViewData["UserId"] = new SelectList(_context.Accounts, "UserId", "Password", lendRequest.UserId);
            return View(lendRequest);
        }

        // GET: LendRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lendRequest = await _context.LendRequests
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LendId == id);
            if (lendRequest == null)
            {
                return NotFound();
            }

            return View(lendRequest);
        }

        // POST: LendRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lendRequest = await _context.LendRequests.FindAsync(id);
            _context.LendRequests.Remove(lendRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LendRequestExists(int id)
        {
            return _context.LendRequests.Any(e => e.LendId == id);
        }
    }
}
