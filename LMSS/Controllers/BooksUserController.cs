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
    public class BooksUserController : Controller
    {
        private readonly LibraryManagementSystemContext _context;
        private readonly IAccountRepo _accountRepo;

        public BooksUserController(LibraryManagementSystemContext context, IAccountRepo accountRepo)
        {
            _context = context;
            _accountRepo = accountRepo;
        }

        // GET: BooksUser
        public async Task<IActionResult> User_dash(string searchstring)
        {
            var username = HttpContext.Session.GetString("username");
            var user = _accountRepo.GetAccountByName(username);



            var lMS_ProjectContext = _context.Books.Include(b => b.Author).Include(b => b.Publisher);
            if (!string.IsNullOrEmpty(searchstring))
            {
                lMS_ProjectContext = _context.Books.Where(b => b.Author.AuthorName.Contains(searchstring) ||
                                 b.Category.Contains(searchstring) ||
                                 b.BookTitle.Contains(searchstring)).Include(b => b.Author).Include(b => b.Publisher);
            }
            return View(await lMS_ProjectContext.ToListAsync());

        }
        public async Task<IActionResult> Index()
        {
            var libraryManagementSystemContext = _context.Books.Include(b => b.Author).Include(b => b.Publisher);
            return View(await libraryManagementSystemContext.ToListAsync());
        }

        // GET: BooksUser/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: BooksUser/Create
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors, "AuthorId", "AuthorId");
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "PublisherId", "PublisherId");
            return View();
        }

        // POST: BooksUser/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,BookTitle,NoOfCopies,AuthorId,PublisherId,Category,ImageUrl,IssuedBooks,IsAvailable")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "AuthorId", "AuthorId", book.AuthorId);
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "PublisherId", "PublisherId", book.PublisherId);
            return View(book);
        }

        // GET: BooksUser/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "AuthorId", "AuthorId", book.AuthorId);
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "PublisherId", "PublisherId", book.PublisherId);
            return View(book);
        }

        // POST: BooksUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,BookTitle,NoOfCopies,AuthorId,PublisherId,Category,ImageUrl,IssuedBooks,IsAvailable")] Book book)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookId))
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
            ViewData["AuthorId"] = new SelectList(_context.Authors, "AuthorId", "AuthorId", book.AuthorId);
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "PublisherId", "PublisherId", book.PublisherId);
            return View(book);
        }

        // GET: BooksUser/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: BooksUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}
