using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Agent;
using Libplanet.Crypto;
using Mimir.Services;
using Mimir.Util;

namespace Mimir.Controllers;

[ApiController]
[Route("agent")]
public class AccountController : ControllerBase
{
    [HttpGet("{agentAddress}/avatars")]
    public async Task<AvatarsResponse> GetAvatars(string agentAddress, IStateService stateService)
    {
        var stateGetter = new StateGetter(stateService);
        var avatars = await stateGetter.GetAvatarStatesAsync(new Address(agentAddress));
        if (avatars is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return new AvatarsResponse([]);
        }

        return new AvatarsResponse(avatars
            .Select(e => new Avatar(e.address.ToString(), e.name, e.level))
            .ToList());
    }
}
