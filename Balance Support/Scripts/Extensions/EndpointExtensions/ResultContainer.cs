using System.Diagnostics.Eventing.Reader;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Balance_Support.Scripts.Extensions;
namespace Balance_Support.Scripts.Extensions;

// public static class EndpointExtensions
// {
//     public static async Task<IResult> HandleWithValidation<TModel, TValidator>(
//         TModel model,
//         Func<TModel, Task<IResult>> action)
//         where TValidator : IValidator<TModel>, new()
//     {
//         var validator = new TValidator();
//         var validationResult = validator.Validate(model);
//
//         if (!validationResult.IsValid)
//         {
//             return Results.BadRequest(validationResult.Errors);
//         }
//
//         return await action(model);
//     }
// }

// public static class EndpointHandler
// {
//     public static ResultContainer StartHandling() => new ResultContainer();
//
//     public static ResultContainer ValidateInputData<TModel, TValidator>(this ResultContainer container,
//         TModel model)
//         where TValidator : IValidator<TModel>, new()
//     {
//         if (container.IsCancelled) return container;
//         
//         var validator = new TValidator();
//         var validationResult = validator.Validate(model);
//         
//         if (!validationResult.IsValid)
//         {
//             container.IsCancelled = true;
//             container.Result =  Results.BadRequest(validationResult.Errors);
//         }
//         else
//         {
//             container.Result = Results.Ok("Data successfully validated");
//         }
//         
//         return container;
//     }
//     
//     public static  ResultContainer IsUserAuthorized(this ResultContainer container, HttpContext httpContext)
//     {
//         if (container.IsCancelled) return container;
//         
//         if (httpContext == null || httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
//         {
//             container.IsCancelled = true;
//             container.Result =  Results.Unauthorized();
//             return container;
//         }
//         var user = httpContext.User;
//         var sessionStartTime = DateTime.MinValue;
//         var expiresUtc = DateTime.MinValue;
//
//         if (user.Identity.IsAuthenticated && user.HasClaim(c => c.Type == "FirebaseToken"))
//         {
//             var sessionStartClaim = user.FindFirst("SessionStartTime");
//             var expiresUtcClaim = user.FindFirst("ExpiresUtc");
//
//             if (sessionStartClaim != null && DateTime.TryParse(sessionStartClaim.Value, out sessionStartTime) &&
//                 expiresUtcClaim != null && DateTime.TryParse(expiresUtcClaim.Value, out expiresUtc))
//             {
//                 // Compare the current time with the expiration time
//                 if (DateTime.UtcNow <= expiresUtc)
//                 {
//                     container.Result =  Results.Ok("User is authorised");
//                     return container;
//                 }
//             }
//         }
//         container.IsCancelled = true;
//         container.Result =  Results.Unauthorized();
//         return container;
//     }
//     
//     public static ResultContainer HandleProviderOptions(this ResultContainer container, Func<ResultContainer,ResultContainer> action)
//     {
//         if (container.IsCancelled) return container;
//
//         return action.Invoke(container);
//     }
//
//     public static IResult GetResult(this ResultContainer container) => container.Result;
// }


public class ResultContainer
{
    private bool _isCancelled;
    private IResult _result;
    private bool _isResultRetrieved;

    // private T _data;
    // private readonly HttpContext _httpContext;

    // private ResultContainer(T data, HttpContext httpContext)
    // {
    //     _data = data;
    //     _httpContext = httpContext;
    // }

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
            _result = Results.Unauthorized();
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