using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Sessions
{
    public class DeleteModel : PageModel
    {
        private readonly IExamSessionService _sessions;

        public DeleteModel(IExamSessionService sessions)
        {
            _sessions = sessions;
        }

        public SessionDto? Session { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Session = await _sessions.GetByIdAsync(id);
            if (Session == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var session = await _sessions.GetByIdAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            var result = await _sessions.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState);
                Session = session;
                return Page();
            }

            TempData["Success"] = $"Đã xóa phiên thi \"{session.Title}\".";
            return RedirectToPage("Index");
        }
    }
}
