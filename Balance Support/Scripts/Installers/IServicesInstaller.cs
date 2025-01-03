namespace Balance_Support.Scripts.Installers;

public interface IServicesInstaller
{
    public Task Install(IServiceCollection services);
}