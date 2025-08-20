using System;
using System.Linq;
using System.Threading;
using ToolManagementAppV2.Views.Pages;
using ToolManagementAppV2.Views.Windows;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Tests;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class ItemSearchPageTests
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
                    var page = new ItemSearchPage();
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

        [Fact]
        public void SearchButton_UsesItemLabel()
        {
            Exception? threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    LabelProvider.Instance.UpdateLabels("Widget", "Widgets");
                    if (System.Windows.Application.Current == null)
                        new System.Windows.Application();
                    var page = new ItemSearchPage();
                    var button = TestHelpers.FindVisualChildren<System.Windows.Controls.Button>(page)
                        .First(b => (string)b.Content! == "Search Widgets");
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
                finally
                {
                    LabelProvider.Instance.UpdateLabels("Item", "Items");
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadException != null)
                throw threadException;
        }
    }
}

