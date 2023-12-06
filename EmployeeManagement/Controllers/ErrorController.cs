using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }

        //[Route("Error/{StatusCode}")]
        //public IActionResult HttpStatusCodeHandler(int StatusCode)
        //{
        //    var StatusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        //    switch (StatusCode)
        //    {
        //        case 404:
        //            { 
        //            ViewBag.ErrorMessage = "Sorry, the requested Resource could Not Found";
        //            logger.LogWarning($"404 error occured. path={StatusCodeResult.OriginalPath} and Query String={StatusCodeResult.OriginalQueryString}");
                  
        //            }
               
        ////    }

        //    return View("NotFound");
        //}


        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            logger.LogError($"The path {exceptionDetails.Path} threw an Exception {exceptionDetails.Error}");
            return View("Error");
        }
    }
}
