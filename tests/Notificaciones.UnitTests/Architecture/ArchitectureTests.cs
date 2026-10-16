using NetArchTest.Rules;
using Xunit;

namespace Notificaciones.UnitTests.Architecture;

public sealed class ArchitectureTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(Domain.Events.NotificationEvent).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(Application.Services.PrivacyMasker).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(Api.Controllers.NotificationsController).Assembly;

    [Fact]
    public void DomainMustNotHaveDependencyOnOtherProjects()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Notificaciones.Application",
                "Notificaciones.Infrastructure",
                "Notificaciones.Api",
                "Notificaciones.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer must have zero dependencies on outer layers.");
    }

    [Fact]
    public void ApplicationMustNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Notificaciones.Infrastructure",
                "Notificaciones.Api",
                "Notificaciones.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer must not depend on Infrastructure or Api layers.");
    }

    [Fact]
    public void InfrastructureMustNotDependOnApi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("Notificaciones.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, "Infrastructure must not depend on Api presentation entrypoint.");
    }
}
