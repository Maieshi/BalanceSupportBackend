using Balance_Support.Scripts.Extensions.AuthorizationStrategies;
using FluentValidation;

namespace Balance_Support.Scripts.Extensions.EndpointExtensions;

public class ResultContainer
{
    private bool isCancelled;
    private IResult? result;
    private bool isResultRetrieved;

    public static ResultContainer Start() => new();

    public ResultContainer Validate<TData,TValidator>(TData data) where TValidator : IValidator<TData>, new()
    {
        if (isCancelled) return this;

        var validator = new TValidator();
        var validationResult = validator.Validate(data);

        if (!validationResult.IsValid)
        {
            isCancelled = true;
            result = Results.BadRequest(validationResult.Errors);
        }

        return this;
    }

    public ResultContainer Authorize(HttpContext context, ContextStrategy? strategy =null)
    {
        if (isCancelled) return this;

        bool isAuthorized = strategy != null
            ? strategy.IsUserAuthorized(context)
            : new DefaultContextStrategy().IsUserAuthorized(context);

        if (!isAuthorized)
        {
            isCancelled = true;
            result = Results.Unauthorized();
        }

        return this;
    }

    public ResultContainer Process(Func<Task<IResult>> processFunc)
    {
        if (isCancelled) return this;

        if (processFunc == null)
            throw new ArgumentNullException(nameof(processFunc));

        try
        {
            result = processFunc.Invoke().Result;
        }
        catch (Exception ex)
        {
            isCancelled = true;
            result = Results.Problem(detail: ex.Message);
        }

        return this;
    }
    
    public async Task<ResultContainer> ProcessAsync(Func<Task<IResult>> processFunc)
    {
        if (isCancelled) return this;

        if (processFunc == null)
            throw new ArgumentNullException(nameof(processFunc));

        try
        {
            result = await processFunc.Invoke();
        }
        catch (Exception ex)
        {
            isCancelled = true;
            result = Results.Problem(detail: ex.Message);
        }

        return this;
    }

    public IResult GetResult()
    {
        if (isResultRetrieved)
            throw new InvalidOperationException("Result has already been retrieved.");

        isResultRetrieved = true;
        return result ?? Results.Problem("No result was produced.");
    }
}