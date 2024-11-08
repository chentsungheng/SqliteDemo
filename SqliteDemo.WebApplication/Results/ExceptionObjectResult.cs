using Microsoft.AspNetCore.Mvc;

namespace SqliteDemo.WebApplication.Results
{
    /// <summary>
    /// 例外結果
    /// </summary>
    public class ExceptionObjectResult : ObjectResult
    {
        /// <summary>
        /// 例外結果
        /// </summary>
        /// <param name="ex">例外物件</param>
        /// <param name="HttpStatusCode">HTTP狀態碼, 預設為500</param>
        public ExceptionObjectResult(Exception ex, int HttpStatusCode = StatusCodes.Status500InternalServerError) : base(ex.Message)
        {
            StatusCode = HttpStatusCode;
            Value = new
            {
                Type = ex.GetType().FullName,
                ex.Message,
                ex.StackTrace
            };
        }
    }
}
