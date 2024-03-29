using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TG.Auth.Api.Application.Tokens;
using TG.Auth.Api.Config;
using TG.Auth.Api.Errors;
using TG.Auth.Api.Models.Request;
using TG.Auth.Api.Models.Response;
using TG.Core.App.Constants;
using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Controllers
{
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    // [ProducesJsonContent]
    [Route(ServiceConst.RoutePrefix)]
    public class TokensController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TokensController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("google")]
        public async Task<ActionResult<TokensResponse>> CreateByGoogleAuth([FromBody] TokenByGoogleAuthRequest request)
        {
            var cmd = new CreateTokensByGoogleAuthCommand(request.DeviceId, request.IdToken);
            var result = await _mediator.Send(cmd);
            return result.ToActionResult()
                .Forbidden(AppErrors.BanActive)
                .Created();
        }
        
        [HttpPost("facebook")]
        public async Task<ActionResult<TokensResponse>> CreateByFacebookAuth([FromBody] TokenByFacebookAuthRequest request)
        {
            var cmd = new CreateTokensByFacebookAuthCommand(request.DeviceId, request.AccessToken);
            var result = await _mediator.Send(cmd);
            return result.ToActionResult()
                .Forbidden(AppErrors.BanActive)
                .Created();
        }
        
        [HttpPost("admin")]
        public async Task<ActionResult<TokensResponse>> CreateByGoogleAdminAuth([FromBody] TokenByGoogleAuthRequest request)
        {
            var cmd = new CreateAdminTokensByGoogleAuthCommand(request.DeviceId, request.IdToken);
            var result = await _mediator.Send(cmd);
            return result.ToActionResult()
                .Forbidden(AppErrors.BanActive)
                .Created();
        }
        
        [HttpPost("apple")]
        public async Task<ActionResult<TokensResponse>> CreateByAppleAuth([FromBody] TokenByAppleAuthRequest request)
        {
            var cmd = new CreateTokensByAppleAuthCommand(request.DeviceId, request.Token);
            var result = await _mediator.Send(cmd);
            return result.ToActionResult()
                .Forbidden(AppErrors.BanActive)
                .Created();
        }
        
        [HttpPut("refresh")]
        public async Task<ActionResult<TokensResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            var cmd = new RefreshTokenCommand(request.RefreshToken);
            var result = await _mediator.Send(cmd);
            return result.ToActionResult()
                .Forbidden(AppErrors.BanActive)
                .Ok();
        }
    }
}