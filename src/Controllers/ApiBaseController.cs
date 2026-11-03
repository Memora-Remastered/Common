namespace Common.Controllers;

[ApiController]
public abstract class ApiBaseController : ControllerBase
{
    protected async Task<T> SendAsync<T>(IRequest<T> request)
    {
        var mediator = HttpContext.RequestServices.GetRequiredService<ISender>();
        return await mediator.Send(request, HttpContext.RequestAborted);
    }
}
