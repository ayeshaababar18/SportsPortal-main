using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsPortal.Data;
using SportsPortal.Models;

namespace SportsPortal.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public GalleryController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Gallery
        [AllowAnonymous]
        public async Task<IActionResult> Index(string category = "All")
        {
            var images = _context.GalleryImages.AsQueryable();

            if (category != "All" && !string.IsNullOrEmpty(category))
            {
                images = images.Where(i => i.Category == category);
            }

            ViewBag.CurrentCategory = category;
            ViewBag.Categories = await _context.GalleryImages.Select(i => i.Category).Distinct().ToListAsync();

            return View(await images.OrderByDescending(i => i.UploadDate).ToListAsync());
        }

        // GET: Gallery/Upload
        [Authorize(Roles = "Organizer")]
        public IActionResult Upload()
        {
            return View();
        }

        // POST: Gallery/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Upload(GalleryImage galleryImage, IFormFile? ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Basic Validation
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
                    return View(galleryImage);
                }

                // Generate Path
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                fileName = fileName + "_" + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(wwwRootPath + "/images/gallery/", fileName);

                // Create directory if not exists
                string dir = Path.GetDirectoryName(path)!;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Save File
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                // Save to DB
                galleryImage.ImagePath = "/images/gallery/" + fileName;
                galleryImage.UploadDate = DateTime.Now;

                _context.Add(galleryImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("ImageFile", "Please select an image to upload.");
            }

            return View(galleryImage);
        }

        // POST: Gallery/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _context.GalleryImages.FindAsync(id);
            if (image != null)
            {
                // Delete file
                var imagePath = Path.Combine(_hostEnvironment.WebRootPath, image.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _context.GalleryImages.Remove(image);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
