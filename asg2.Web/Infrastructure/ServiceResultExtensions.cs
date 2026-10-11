using asg2.BLL.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace asg2.Web.Infrastructure
{
    public static class ServiceResultExtensions
    {
        /// <summary>
        /// Chuyển lỗi của ServiceResult vào ModelState. Key lỗi là tên thuộc tính của DTO; truyền prefix
        /// (ví dụ "Input") khi form bind vào một InputModel. Key rỗng thành lỗi chung (asp-validation-summary).
        /// </summary>
        public static void AddToModelState(this ServiceResult result, ModelStateDictionary modelState, string? prefix = null)
        {
            foreach (var (key, messages) in result.Errors)
            {
                var field = string.IsNullOrEmpty(key) ? string.Empty
                    : string.IsNullOrEmpty(prefix) ? key
                    : $"{prefix}.{key}";

                foreach (var message in messages)
                    modelState.AddModelError(field, message);
            }
        }
    }
}
