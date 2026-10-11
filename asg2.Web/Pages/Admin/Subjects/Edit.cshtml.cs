using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Subjects
{
    public class EditModel : PageModel
    {
        private readonly ISubjectService _subjects;

        public EditModel(ISubjectService subjects)
        {
            _subjects = subjects;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public SubjectInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var subject = await _subjects.GetByIdAsync(Id);
            if (subject is null)
                return NotFound();

            Input = SubjectInput.From(subject);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _subjects.UpdateAsync(Id, Input.ToDto());
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState, nameof(Input));
                return Page();
            }

            TempData["Success"] = $"Đã cập nhật môn học {result.Value!.Name}.";
            return RedirectToPage("Index");
        }
    }
}
