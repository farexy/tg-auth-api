using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TG.Auth.Api.Application.Users;
using TG.Auth.Api.Config;
using TG.Auth.Api.Errors;
using TG.Auth.Api.Models.Request;
using TG.Core.App.Constants;
using TG.Core.App.Extensions;
using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Controllers
{
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route(ServiceConst.RoutePrefix)]
    [Authorize(Roles = TgUserRoles.Admin)]
    public class BansController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BansController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult> BanUser([FromBody] BanRequest request)
        {
            var result = await _mediator.Send(new BanUserCommand(request.UserId, request.BannedTill,
                request.Comment,request.Reason, User.GetUserId()));
            return result.ToActionResult()
                .NotFound(AppErrors.NotFound)
                .NoContent();
        }
    }
}