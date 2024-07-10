using Microsoft.AspNetCore.Identity;
using OnRails;
using OnRails.Models;
using OnRails.ResultDetails.Errors.BadRequest;

namespace AuthApi.Helpers.OnRails;

public static class ResultHelpers {
    public static Result MapToResult(this IdentityResult identityResult) {
        if (identityResult.Succeeded) return Result.Ok();

        var errors = identityResult.Errors
            .Select(error => new KeyValue<object?>(error.Code, error.Description))
            .ToList();
        return Result.Fail(new BadRequestError(errors));
    }
}