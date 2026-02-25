using Microsoft.AspNetCore.Mvc;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Utils.Results.Concrete;

namespace WebAPI.Controllers;

/// <summary>
/// Ortak hata response tiplerini taşıyan base controller.
/// </summary>
[ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ErrorDataResult<Exceptions.HttpProblemDetails.ValidationProblemDetails>),
    StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>),
    StatusCodes.Status500InternalServerError)]
public abstract class BaseMicroserviceController : ControllerBase { }