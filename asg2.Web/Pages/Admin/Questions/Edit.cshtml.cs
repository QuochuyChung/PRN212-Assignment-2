using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Questions
{
    public class EditModel : PageModel
    {
        private readonly IQuestionService _questions;
        private readonly ISubjectService _subjects;

        public EditModel(
            IQuestionService questions,
            ISubjectService subjects)
        {
            _questions = questions;
            _subjects = subjects;
        }

        [FromRoute]
        public int Id { get; set; }

        [BindProperty]
        public QuestionInput Input { get; set; } = new();

        public IReadOnlyList<SubjectDto> Subjects { get; private set; } = Array.Empty<SubjectDto>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;
            var question = await _questions.GetByIdAsync(id);
            if (question == null)
            {
                TempData["Error"] = "Không tìm thấy câu hỏi.";
                return RedirectToPage("Index");
            }

            Input = QuestionInput.FromDto(question);
            Subjects = await _subjects.GetAllAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Id = id;
            if (!ModelState.IsValid)
            {
                Subjects = await _subjects.GetAllAsync();
                return Page();
            }

            var result = await _questions.UpdateAsync(id, Input.ToDto());
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState, nameof(Input));
                Subjects = await _subjects.GetAllAsync();
                return Page();
            }

            TempData["Success"] = $"Đã cập nhật câu hỏi #{id}.";
            return RedirectToPage("Index", new { subjectId = Input.SubjectId });
        }
    }
}
