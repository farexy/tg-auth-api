using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TG.Auth.Api.Application.Tokens;
using TG.Auth.Api.Config;
using TG.Auth.Api.Models.Request;
using TG.Auth.Api.Models.Response;

namespace TG.Auth.Api.Controllers
{
    [ApiController]
    // [ApiVersion(ApiVersions.V1)]
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
            var cmd = new CreateTokensByGoogleAuthCommand(request.IdToken);
            var result = await _mediator.Send(cmd);
            return result;
        }
    }
}