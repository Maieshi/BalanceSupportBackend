using System.Threading.Tasks;

namespace Balance_Support.Scripts.Controllers;

public interface IRequestController<T, TDependency1, TDependency2> where T : class
{
    TDependency1 Dependency1 { get; }
    TDependency2 Dependency2 { get; }

    public Task<IResult> HandleRequestAsync(T request);
}