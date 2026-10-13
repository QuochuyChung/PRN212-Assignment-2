using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Questions
{
    public class CreateModel : PageModel
    {
        private readonly IQuestionService _questions;
        private readonly ISubjectService _subjects;

        public CreateModel(
            IQuestionService questions,
            ISubjectService subjects)
        {
            _questions = questions;
            _subjects = subjects;
        }

        [BindProperty]
        public QuestionInput Input { get; set; } = new();

        public IReadOnlyList<SubjectDto> Subjects { get; private set; } = Array.Empty<SubjectDto>();

        public async Task OnGetAsync(int? subjectId)
        {
            Subjects = await _subjects.GetAllAsync();
            if (subjectId.HasValue)
            {
                Input.SubjectId = subjectId.Value;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Subjects = await _subjects.GetAllAsync();
                return Page();
            }

            var result = await _questions.CreateAsync(Input.ToDto());
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState, nameof(Input));
                Subjects = await _subjects.GetAllAsync();
                return Page();
            }

            TempData["Success"] = "Đã tạo câu hỏi mới thành công.";
            return RedirectToPage("Index", new { subjectId = Input.SubjectId });
        }
    }
}
