using System;
using System.Threading;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class ToolSearchPageTests
    {
        [Fact]
        public void Constructor_LoadsWithoutException()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    if (System.Windows.Application.Current == null)
                        new System.Windows.Application();
                    var page = new ToolSearchPage();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadException != null)
            {
                throw threadException;
            }
        }
    }
}

