using System.Diagnostics.Eventing.Reader;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Balance_Support.Scripts.Extensions;
namespace Balance_Support.Scripts.Extensions;

public class ResultContainer
{
    private bool _isCancelled;
    private IResult _result;
    private bool _isResultRetrieved;

    public static ResultContainer Start() => new ResultContainer();

    public ResultContainer Validate<TData,TValidator>(TData data) where TValidator : IValidator<TData>, new()
    {
        if (_isCancelled) return this;

        var validator = new TValidator();
        var validationResult = validator.Validate(data);

        if (!validationResult.IsValid)
        {
            _isCancelled = true;
            _result = Results.BadRequest(validationResult.Errors);
        }

        return this;
    }

    public ResultContainer Authorize(HttpContext context, ContextStrategy strategy =null)
    {
        if (_isCancelled) return this;

        bool isAuthorized = strategy != null
            ? strategy.IsUserAuthorized(context)
            : new DefaultContextStrategy().IsUserAuthorized(context);

        if (!isAuthorized)
        {
            _isCancelled = true;
            _result = Results.BadRequest(context.User?.Claims);
        }

        return this;
    }

    public ResultContainer Process(Func<Task<IResult>> processFunc)
    {
        if (_isCancelled) return this;

        if (processFunc == null)
            throw new ArgumentNullException(nameof(processFunc));

        try
        {
            _result = processFunc.Invoke().Result;
        }
        catch (Exception ex)
        {
            _isCancelled = true;
            _result = Results.Problem(detail: ex.Message);
        }

        return this;
    }

    public IResult GetResult()
    {
        if (_isResultRetrieved)
            throw new InvalidOperationException("Result has already been retrieved.");

        _isResultRetrieved = true;
        return _result ?? Results.Problem("No result was produced.");
    }
}