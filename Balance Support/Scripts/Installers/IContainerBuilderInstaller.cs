using Autofac;

namespace Balance_Support.Scripts.Installers;

public interface IContainerBuilderInstaller
{
    public Task Install(ContainerBuilder builder);
}