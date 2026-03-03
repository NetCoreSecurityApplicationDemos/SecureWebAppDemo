using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace SecureWebAppDemo.Controllers
{
    public class DebugController : Controller
    {
        [Authorize]
        public async Task<IActionResult> Ticket()
        {

            var outPut = new StringBuilder();

            var result = await HttpContext.AuthenticateAsync();

            if (!result.Succeeded)
                return Content("Not Authenticated");

            var ticket = result.Ticket;
            var principal = result.Principal;
            var properties = result.Properties;

            outPut.AppendLine("===SCHEME===");
            outPut.AppendLine(ticket.AuthenticationScheme);
            outPut.AppendLine("\n===CLAIMS===");
            foreach (var claim in principal.Claims)
            {
                outPut.AppendLine($"{claim.Type} = {claim.Value}");
            }

            outPut.AppendLine("\n====PROPERTIES====");
            outPut.AppendLine($"IssuedUtc : {properties.IssuedUtc}");
            outPut.AppendLine($"ExpreUtc : {properties.ExpiresUtc}");
            outPut.AppendLine($"IsPersistent : {properties.IsPersistent}");

            

            return Content(outPut.ToString(), "text/plain");
        }
    }
}
