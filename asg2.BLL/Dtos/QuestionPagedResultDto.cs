namespace asg2.BLL.Dtos
{
    public class QuestionPagedResultDto
    {
        public IReadOnlyList<QuestionDto> Items { get; set; } = Array.Empty<QuestionDto>();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
