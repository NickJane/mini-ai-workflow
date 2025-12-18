using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SuperFlowApi.Controllers
{
    [Authorize]
    public class BaseControllerController : WebApiBaseController
    {


    }
}
