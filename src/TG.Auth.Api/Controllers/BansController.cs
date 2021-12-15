using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TG.Auth.Api.Application.Bans;
using TG.Auth.Api.Config;
using TG.Auth.Api.Errors;
using TG.Auth.Api.Models.Request;
using TG.Auth.Api.Models.Response;
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
        
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BanResponse>>> Get([FromQuery] Guid? userId, [FromQuery] string? userLogin)
        {
            var result = await _mediator.Send(new GetBansQuery(userId, userLogin));
            return result.ToActionResult()
                .Ok();
        }

        [HttpPost]
        public async Task<ActionResult<BanResponse>> BanUser([FromBody] BanRequest request)
        {
            var result = await _mediator.Send(new BanUserCommand(request.UserId, request.BannedTill,
                request.Comment, request.Reason, User.GetLogin()));
            return result.ToActionResult()
                .NotFound(AppErrors.NotFound)
                .Created();
        }
        
        [HttpPatch("{id}/unban")]
        public async Task<ActionResult<BanResponse>> UnbanUser([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new UnbanUserCommand(id, User.GetLogin()));
            return result.ToActionResult()
                .NotFound(AppErrors.NotFound)
                .Ok();
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new DeleteBanCommand(id));
            return result.ToActionResult()
                .BadRequest(AppErrors.BanActive)
                .NotFound(AppErrors.NotFound)
                .NoContent();
        }
    }
}