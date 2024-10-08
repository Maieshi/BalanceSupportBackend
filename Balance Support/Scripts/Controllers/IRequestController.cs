namespace Balance_Support.Scripts.Controllers;

public interface IRequestController<T> where T:class
{
    public Task<IResult> HandleRequestAsync(T request);
}