using System.Windows;
using System.Reflection;
using System.Threading.Tasks;

internal static class WpfTestHelper
{
    public static Application CreateApplication()
    {
        try
        {
            return new Application();
        }
        catch (InvalidOperationException)
        {
            ShutdownApplication();
            return new Application();
        }
    }

    public static void ShutdownApplication()
    {
        var app = Application.Current;
        if (app == null)
            return;

        try
        {
            if (app.Dispatcher.HasShutdownStarted || app.Dispatcher.HasShutdownFinished)
                return;

            if (app.Dispatcher.CheckAccess())
                app.Shutdown();
            else
                app.Dispatcher.Invoke(app.Shutdown);
        }
        catch (TaskCanceledException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        finally
        {
            ClearCurrentApplication();
        }
    }

    private static void ClearCurrentApplication()
    {
        var appType = typeof(Application);
        var instanceField = appType.GetField("_appInstance", BindingFlags.NonPublic | BindingFlags.Static);
        instanceField?.SetValue(null, null);

        var createdField = appType.GetField("_appCreatedInThisAppDomain", BindingFlags.NonPublic | BindingFlags.Static);
        createdField?.SetValue(null, false);
    }
}
