namespace asg2.BLL.Common
{
    /// <summary>
    /// Kết quả trả về của mọi service. Lỗi nghiệp vụ trả về qua Failure (không ném exception).
    /// Key của Errors là tên thuộc tính của DTO (để Web gắn vào ModelState), key rỗng = lỗi chung.
    /// </summary>
    public class ServiceResult
    {
        public bool IsSuccess { get; }
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        protected ServiceResult(bool isSuccess, IReadOnlyDictionary<string, string[]> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static ServiceResult Success() =>
            new(true, new Dictionary<string, string[]>());

        public static ServiceResult Failure(string key, params string[] messages) =>
            new(false, new Dictionary<string, string[]> { [key] = messages });

        public static ServiceResult Failure(IDictionary<string, string[]> errors) =>
            new(false, new Dictionary<string, string[]>(errors));
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Value { get; }

        private ServiceResult(bool isSuccess, T? value, IReadOnlyDictionary<string, string[]> errors)
            : base(isSuccess, errors)
        {
            Value = value;
        }

        public static ServiceResult<T> Success(T value) =>
            new(true, value, new Dictionary<string, string[]>());

        public new static ServiceResult<T> Failure(string key, params string[] messages) =>
            new(false, default, new Dictionary<string, string[]> { [key] = messages });

        public new static ServiceResult<T> Failure(IDictionary<string, string[]> errors) =>
            new(false, default, new Dictionary<string, string[]>(errors));
    }
}
