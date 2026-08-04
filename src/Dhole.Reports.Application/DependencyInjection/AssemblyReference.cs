using System.Reflection;

namespace Dhole.Reports.Application.DependencyInjection;

internal static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
