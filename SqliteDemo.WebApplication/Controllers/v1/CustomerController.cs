using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqliteDemo.Logic;
using SqliteDemo.Model;
using SqliteDemo.WebApplication.Results;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace SqliteDemo.WebApplication.Controllers.v1
{
    /// <summary>
    /// 顧客API
    /// </summary>
    [ApiController, ApiVersion("1.0", Deprecated = false), Route("api/v{version:apiVersion}/customer"), AllowAnonymous]
    [SwaggerTag("顧客資訊")]
    public class CustomerController : ControllerBase
    {
        private readonly IBusinessLogicFactory _factory;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="Factory"></param>
        public CustomerController(IBusinessLogicFactory Factory)
        {
            _factory = Factory;
        }

        /// <summary>
        /// 查詢顧客資料
        /// </summary>
        /// <param name="CompanyName">公司名稱</param>
        /// <param name="Region">地區</param>
        /// <param name="PostalCode">郵遞區號</param>
        /// <returns></returns>
        [HttpGet(""), Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomersAsync(string? CompanyName, string? Region, string? PostalCode)
        {
            try
            {
                return Ok(await _factory.GetLogic<ICustomerLogic>().GetCustomersAsync(null, CompanyName, Region, PostalCode, Request.HttpContext.RequestAborted));
            }
            catch (Exception ex)
            {
                return new ExceptionObjectResult(ex);
            }
        }
    }
}
