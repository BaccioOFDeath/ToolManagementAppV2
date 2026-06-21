using InventoryManagementApp.Utilities;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class InformationalTooltipServiceTests
    {
        [Fact]
        public void Apply_MovesStaticInstructionalCaptionToPreviousElementTooltip()
        {
            RunOnSta(() =>
            {
                var title = new TextBlock { Text = "Customers" };
                var helper = new TextBlock
                {
                    Text = "Select the exact company or contact before copying, printing, editing, or opening details.",
                    TextWrapping = TextWrapping.Wrap
                };
                var panel = new StackPanel();
                panel.Children.Add(title);
                panel.Children.Add(helper);

                InformationalTooltipService.Apply(panel);

                Assert.Equal(Visibility.Collapsed, helper.Visibility);
                Assert.Equal(helper.Text, title.ToolTip);
            });
        }

        [Fact]
        public void Apply_KeepsBoundCaptionTextVisible()
        {
            RunOnSta(() =>
            {
                var panel = new StackPanel { DataContext = new { Status = "12 visible records" } };
                var status = new TextBlock { TextWrapping = TextWrapping.Wrap };
                status.SetBinding(TextBlock.TextProperty, new Binding("Status"));
                panel.Children.Add(status);

                InformationalTooltipService.Apply(panel);

                Assert.Equal(Visibility.Visible, status.Visibility);
                Assert.Null(status.ToolTip);
            });
        }

        private static void RunOnSta(Action action)
        {
            Exception? exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
                throw exception;
        }
    }
}
