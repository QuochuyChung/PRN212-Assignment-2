namespace asg2.BLL.Common
{
    /// <summary>
    /// Tiêu chuẩn định lượng thang đo cấp độ tư duy Bloom (Bloom's Taxonomy) cho câu hỏi vấn đáp.
    /// Chuẩn hóa hiển thị tên, màu sắc, từ khóa hành động và thang tiêu chí đánh giá.
    /// </summary>
    public static class BloomDescriptor
    {
        public sealed record BloomDetail(
            int Level,
            string Code,
            string VietnameseName,
            string EnglishName,
            string BadgeClass,
            string ColorHex,
            string Description,
            IReadOnlyList<string> ActionKeywords,
            string EvaluationRubric
        );

        private static readonly IReadOnlyDictionary<int, BloomDetail> Details = new Dictionary<int, BloomDetail>
        {
            [1] = new BloomDetail(
                Level: 1,
                Code: "BLOOM_L1_REMEMBER",
                VietnameseName: "Nhận biết",
                EnglishName: "Remember / Knowledge",
                BadgeClass: "bg-secondary text-white",
                ColorHex: "#6c757d",
                Description: "Khả năng ghi nhớ, nhận diện và nhắc lại các định nghĩa, cú pháp, khái niệm cơ bản đã học.",
                ActionKeywords: new[] { "Liệt kê", "Định nghĩa", "Nhắc lại", "Nêu tên", "Nhận diện", "Trình bày sơ lược" },
                EvaluationRubric: "Thí sinh trình bày đúng từ khóa, nêu đúng định nghĩa hoặc cú pháp chuẩn. Đánh giá tối đa 100% điểm nếu không nhầm lẫn thuật ngữ."
            ),
            [2] = new BloomDetail(
                Level: 2,
                Code: "BLOOM_L2_UNDERSTAND",
                VietnameseName: "Thông hiểu",
                EnglishName: "Understand / Comprehension",
                BadgeClass: "bg-info text-white",
                ColorHex: "#0dcaf0",
                Description: "Khả năng diễn giải, giải thích nguyên lý hoạt động, phân biệt và lấy ví dụ minh họa bằng ngôn ngữ của bản thân.",
                ActionKeywords: new[] { "Giải thích", "So sánh", "Minh họa", "Phân biệt", "Tóm tắt", "Diễn giải cơ chế" },
                EvaluationRubric: "Thí sinh diễn giải được bản chất cơ chế hoạt động, so sánh được ưu/nhược điểm cơ bản, đưa ra được ít nhất 1 ví dụ thực tế chính xác."
            ),
            [3] = new BloomDetail(
                Level: 3,
                Code: "BLOOM_L3_APPLY",
                VietnameseName: "Vận dụng",
                EnglishName: "Apply / Application",
                BadgeClass: "bg-warning text-dark",
                ColorHex: "#ffc107",
                Description: "Khả năng vận dụng kiến thức, thuật toán, mô hình thiết kế vào việc giải quyết bài toán kỹ thuật cụ thể.",
                ActionKeywords: new[] { "Áp dụng", "Viết mã nguồn", "Triển khai", "Giải quyết vấn đề", "Xây dựng kịch bản", "Xử lý lỗi" },
                EvaluationRubric: "Thí sinh viết hoặc mô tả được luồng xử lý mã nguồn chính xác, áp dụng đúng cấu trúc thiết kế, xử lý đúng các trường hợp ngoại lệ phát sinh."
            ),
            [4] = new BloomDetail(
                Level: 4,
                Code: "BLOOM_L4_ANALYZE",
                VietnameseName: "Phân tích & Đánh giá",
                EnglishName: "Analyze & Evaluate",
                BadgeClass: "bg-danger text-white",
                ColorHex: "#dc3545",
                Description: "Khả năng phân tích sâu cấu trúc hệ thống, đánh giá hiệu năng, bảo mật, tối ưu hóa và đưa ra quyết định kỹ thuật.",
                ActionKeywords: new[] { "Phân tích sâu", "Đánh giá hiệu năng", "Tối ưu hóa", "So sánh kiến trúc", "Đề xuất giải pháp scale-out", "Phản biện" },
                EvaluationRubric: "Thí sinh chỉ rõ được nút thắt cổ chai (bottleneck), phân tích được độ phức tạp thời gian/không gian, bảo mật, và phản biện được các phương án thay thế."
            )
        };

        public static BloomDetail GetDetail(int level)
        {
            if (Details.TryGetValue(level, out var detail))
            {
                return detail;
            }

            return new BloomDetail(
                Level: level,
                Code: "BLOOM_UNKNOWN",
                VietnameseName: $"Mức {level}",
                EnglishName: "Unknown Level",
                BadgeClass: "bg-light text-dark",
                ColorHex: "#adb5bd",
                Description: "Mức độ Bloom chưa xác định.",
                ActionKeywords: Array.Empty<string>(),
                EvaluationRubric: "Tiêu chí đánh giá chung."
            );
        }

        public static string GetVietnameseName(int level) => GetDetail(level).VietnameseName;

        public static string GetBadgeClass(int level) => GetDetail(level).BadgeClass;

        public static IReadOnlyList<BloomDetail> GetAllLevels() => Details.Values.OrderBy(d => d.Level).ToList();
    }
}
