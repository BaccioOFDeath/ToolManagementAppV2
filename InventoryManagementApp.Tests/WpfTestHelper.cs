using System.Windows;
using System.Reflection;

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

        if (app.Dispatcher.CheckAccess())
            app.Shutdown();
        else
            app.Dispatcher.Invoke(app.Shutdown);

        ClearCurrentApplication();
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
