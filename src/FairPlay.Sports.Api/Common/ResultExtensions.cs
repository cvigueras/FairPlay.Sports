using FairPlay.Sports.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace FairPlay.Sports.Api.Common;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => controller.NotFound(new { error = result.Error }),
            _ => controller.BadRequest(new { error = result.Error })
        };
    }

    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => controller.NotFound(new { error = result.Error }),
            _ => controller.BadRequest(new { error = result.Error })
        };
    }
}
