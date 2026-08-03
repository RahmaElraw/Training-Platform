using Training_Platform.ViewModels;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    public class CategoriesController : Controller
    {
        private readonly IRepository<Category> _categoryRepository;

        private const int PageSize = 6;

        public CategoriesController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }


        public async Task<IActionResult> Index(string? query, int page = 1)
        {
            var categories = await _categoryRepository.GetAsync(
                includes: [c => c.Courses],
                tracked: false);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();

                categories = categories.Where(c =>
                    c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            categories = categories.OrderBy(c => c.Name);

            int totalCount = categories.Count();

            var model = new CategoryWithRelatedVM
            {
                Categories = categories
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize),

                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize),
                Query = query
            };

            return View(model);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = await _categoryRepository.GetOneAsync(
                c => c.Name.ToLower().Trim() == model.Name.ToLower().Trim());
            if (exists != null)
            {
                ModelState.AddModelError(nameof(model.Name),
                    "Category name already exists.");

                return View(model);
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim()
            };

            await _categoryRepository.AddAsync(category);

            if (await _categoryRepository.CommitAsync() > 0)
            {
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Something went wrong.";
        
                return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            var model = new CategoryVM
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = await _categoryRepository.GetOneAsync(
                c => c.Name.ToLower().Trim() == model.Name.ToLower().Trim()
                  && c.Id != model.Id);

            if (exists != null)
            {
                ModelState.AddModelError(nameof(model.Name),
                    "Category name already exists.");

                return View(model);
            }

            var category = await _categoryRepository.GetOneAsync(c => c.Id == model.Id);

            if (category == null)
                return NotFound();

            category.Name = model.Name.Trim();
            category.Description = model.Description?.Trim();

            _categoryRepository.Update(category);

            if (await _categoryRepository.CommitAsync() > 0)
            {
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Something went wrong.";

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetOneAsync(
                c => c.Id == id,
                includes: [c => c.Courses]);

            if (category == null)
                return NotFound();

            if (category.Courses.Count != 0)
            {
                TempData["Error"] =
                    "Cannot delete category because it contains courses.";

                return RedirectToAction(nameof(Index));
            }

            _categoryRepository.Delete(category);

            if (await _categoryRepository.CommitAsync() > 0)
            {
                TempData["Success"] = "Category deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Something went wrong.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ViewCourses(int id)
        {
            var category = await _categoryRepository.GetOneAsync(
                c => c.Id == id,
                includes: [c => c.Courses]);

            if (category == null)
                return NotFound();

            return View(category);
        }
    }
}