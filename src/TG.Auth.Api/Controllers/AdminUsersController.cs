using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TG.Auth.Api.Application.Users;
using TG.Auth.Api.Config;
using TG.Auth.Api.Errors;
using TG.Core.App.Constants;
using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Controllers
{
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route(ServiceConst.RoutePrefix)]
    [Authorize(Roles = TgUserRoles.Admin)]
    public class AdminUsersController
    {
        private readonly IMediator _mediator;

        public AdminUsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteUser([FromRoute] Guid userId)
        {
            var result = await _mediator.Send(new DeleteUserCommand(userId));
            return result.ToActionResult()
                .NotFound(AppErrors.NotFound)
                .NoContent();
        }
    }
}