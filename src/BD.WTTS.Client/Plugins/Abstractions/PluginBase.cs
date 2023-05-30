using dotnetCampus.Ipc.Pipes;

namespace BD.WTTS.Plugins.Abstractions;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract class PluginBase<TPlugin> : IPlugin where TPlugin : PluginBase<TPlugin>
{
    string DebuggerDisplay => Name;

    public abstract string Name { get; }

    readonly Lazy<string> mVersion = new(() =>
    {
        var assembly = typeof(TPlugin).Assembly;
        string? version = null;
        for (int i = 0; string.IsNullOrWhiteSpace(version); i++)
        {
            version = i switch
            {
                0 => assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? string.Empty,
                1 => assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? string.Empty,
                2 => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty,
                _ => null,
            };
            if (version == null) return string.Empty;
        }
        return version;
    });

    public virtual string Version => mVersion.Value;

    public virtual IEnumerable<TabItemViewModel>? GetMenuTabItems() => null;

    public virtual IEnumerable<Action<IConfiguration, IServiceCollection>>? GetConfiguration(
        ConfigurationBuilder builder,
        bool directoryExists) => null;

    public virtual ValueTask OnInitializeAsync() => ValueTask.CompletedTask;

    public virtual void ConfigureDemandServices(
        IServiceCollection services,
        Startup startup)
    {

    }

    public virtual void ConfigureRequiredServices(
        IServiceCollection services,
        Startup startup)
    {

    }

    public virtual void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }

    public virtual void OnUnhandledException(Exception ex, string name, bool? isTerminating = null)
    {

    }

    public virtual bool ExplicitHasValue()
    {
        return true;
    }

    public virtual ValueTask OnExit()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 将参数解析为字符串
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static string DecodeArgs(string args)
        => HttpUtility.UrlDecode(args);

    /// <summary>
    /// 将参数解析为字符串数组
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static string[] DecodeToArrayArgs(string args)
        => HttpUtility.UrlDecode(args).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    public virtual async Task<int> RunSubProcessMainAsync(string moduleName, string pipeName, string processId, string encodedArgs)
    {
        var subProcessBootConfiguration = GetSubProcessBootConfiguration(encodedArgs ?? string.Empty);
        if (subProcessBootConfiguration == default)
            return (int)CommandExitCode.GetSubProcessBootConfigurationFail;

        var exitCode = await IPCSubProcessService.MainAsync(moduleName,
            subProcessBootConfiguration.configureServices,
            subProcessBootConfiguration.configureIpcProvider,
            new[] { pipeName, processId });
        return exitCode;
    }

    /// <summary>
    /// 获取子进程 IPC 程序启动配置
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    protected virtual (Action<IServiceCollection>? configureServices, Action<IpcProvider>? configureIpcProvider) GetSubProcessBootConfiguration(string args)
    {
        return default;
    }
}
