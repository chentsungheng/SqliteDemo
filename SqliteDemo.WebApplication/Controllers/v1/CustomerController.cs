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

        /// <summary>
        /// 依指定ID查詢顧客資料
        /// </summary>
        /// <param name="ID">識別碼</param>
        /// <returns></returns>
        [HttpGet("{ID}"), Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<Customer?>> GetCustomersAsync(string ID)
        {
            try
            {
                var data = await _factory.GetLogic<ICustomerLogic>().GetCustomersAsync(ID, null, null, null, Request.HttpContext.RequestAborted);

                return Ok(data.SingleOrDefault());
            }
            catch (Exception ex)
            {
                return new ExceptionObjectResult(ex);
            }
        }

        /// <summary>
        /// 新增顧客資料
        /// </summary>
        /// <param name="NewCustomer">新顧客資料</param>
        /// <returns></returns>
        [HttpPost(""), Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<Customer>> AddCustomerAsync(Customer NewCustomer)
        {
            try
            {
                return Ok(await _factory.GetLogic<ICustomerLogic>().AddCustomerAsync(NewCustomer, Request.HttpContext.RequestAborted));
            }
            catch (Exception ex)
            {
                return new ExceptionObjectResult(ex);
            }
        }

        /// <summary>
        /// 更新顧客資料
        /// </summary>
        /// <param name="ID">識別碼</param>
        /// <param name="ExistCustomer">顧客資料</param>
        /// <returns></returns>
        [HttpPatch("{ID}"), Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<Customer>> UpdateCustomerAsync(string ID, CustomerForUpdate ExistCustomer)
        {
            try
            {
                return Ok(await _factory.GetLogic<ICustomerLogic>().UpdateCustomerAsync(ID, ExistCustomer, Request.HttpContext.RequestAborted));
            }
            catch (Exception ex)
            {
                return new ExceptionObjectResult(ex);
            }
        }

        /// <summary>
        /// 刪除顧客資料
        /// </summary>
        /// <param name="ID">識別碼</param>
        /// <returns></returns>
        [HttpDelete("{ID}"), Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<CustomerDeleted>> DeleteCustomerAsync(string ID)
        {
            try
            {
                return Ok(await _factory.GetLogic<ICustomerLogic>().DeleteCustomerAsync(ID, Request.HttpContext.RequestAborted));
            }
            catch (Exception ex)
            {
                return new ExceptionObjectResult(ex);
            }
        }
    }
}
