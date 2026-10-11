using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Subjects
{
    public class CreateModel : PageModel
    {
        private readonly ISubjectService _subjects;

        public CreateModel(ISubjectService subjects)
        {
            _subjects = subjects;
        }

        [BindProperty]
        public SubjectInput Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _subjects.CreateAsync(Input.ToDto());
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState, nameof(Input));
                return Page();
            }

            TempData["Success"] = $"Đã tạo môn học {result.Value!.Name}.";
            return RedirectToPage("Index");
        }
    }
}
