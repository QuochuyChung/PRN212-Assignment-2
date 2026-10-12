using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Subjects
{
    public class DeleteModel : PageModel
    {
        private readonly ISubjectService _subjects;

        public DeleteModel(ISubjectService subjects)
        {
            _subjects = subjects;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public SubjectDto? Subject { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Subject = await _subjects.GetByIdAsync(Id);
            return Subject is null ? NotFound() : Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _subjects.DeleteAsync(Id);

            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState);
                Subject = await _subjects.GetByIdAsync(Id);
                return Subject is null ? NotFound() : Page();
            }

            TempData["Success"] = "Đã xóa môn học.";
            return RedirectToPage("Index");
        }
    }
}
