using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nop.Infrastructure;
using Nop.WebApiFramework.OpenTelemetry;

namespace Nop.WebApiFramework
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class WebApiBaseController : ControllerBase
    {
        [NonAction]
        public new ActionResult Ok()
        {
            return base.Ok(new JsonResponse(requestId: Request.HttpContext.GetRequestId()));
        }


        [NonAction]
        public ActionResult Ok<T>(T data, int errCode = 0, string errMessage = "ok")
        {
            return base.Ok(new JsonResponse<T>(data, errCode, errMessage, Request.HttpContext.GetRequestId()));
        }
        [NonAction]
        public JsonResponse<T> Ok2<T>(T data, int errCode = 0, string errMessage = "ok")
        {
            return new JsonResponse<T>(data, errCode, errMessage, Request.HttpContext.GetRequestId());
        }

        [NonAction]
        public JsonResponse Error2(string errMessage, int errCode = 500)
        {
            return new JsonResponse(errCode, errMessage, Request.HttpContext.GetRequestId());
        }

        [NonAction]
        public ActionResult Error(string errMessage, int errCode = 500)
        {
            return base.Ok(new JsonResponse(errCode, errMessage, Request.HttpContext.GetRequestId()));
        }

        [NonAction]
        public ActionResult Ok(JsonResponse response)
        {
            return base.Ok(response);
        }

        [NonAction]
        public ActionResult Ok<T>(JsonResponse<T> response) where T : class
        {
            return base.Ok(response);
        }
        public long UserId
        {
            get
            {
                var userId = Request.HttpContext.GetUserId();
                return userId == 0 ? 0 : userId;
            }
        }

    }
}
