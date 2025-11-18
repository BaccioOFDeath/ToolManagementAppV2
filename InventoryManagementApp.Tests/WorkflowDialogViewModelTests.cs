using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class WorkflowDialogViewModelTests
    {
        [Fact]
        public void MaintenanceEditViewModel_UsesExpectedTitle()
        {
            var vm = new MaintenanceEditViewModel(new MaintenanceRecord(), isNew: false, () => { }, () => { });
            Assert.Equal("Edit Maintenance", vm.Title);
        }

        [Fact]
        public void CalibrationEditViewModel_SaveInvokesAction()
        {
            var record = new CalibrationRecord();
            var invoked = false;
            var vm = new CalibrationEditViewModel(record, true, () => invoked = true, () => { });
            vm.SaveCommand.Execute(null);
            Assert.True(invoked);
        }

        [Fact]
        public void ReservationEditViewModel_TitleReflectsMode()
        {
            var vm = new ReservationEditViewModel(new Reservation(), true, () => { }, () => { });
            Assert.Equal("New Reservation", vm.Title);
        }

        [Fact]
        public void KitEditViewModel_SaveCommandTriggers()
        {
            var triggered = false;
            var vm = new KitEditViewModel(new Kit(), true, () => triggered = true, () => { });
            vm.SaveCommand.Execute(null);
            Assert.True(triggered);
        }

        [Fact]
        public void KitItemEditViewModel_CancelCommandTriggers()
        {
            var cancelled = false;
            var vm = new KitItemEditViewModel(new KitItem(), false, () => { }, () => cancelled = true);
            vm.CancelCommand.Execute(null);
            Assert.True(cancelled);
        }

        [Fact]
        public void InputDialogViewModel_EnforcesRequiredInput()
        {
            var okCalled = false;
            var vm = new InputDialogViewModel("Title", "Prompt", true, () => okCalled = true, () => { });
            vm.OkCommand.Execute(null);
            Assert.False(okCalled);
            vm.InputText = "Ready";
            vm.OkCommand.Execute(null);
            Assert.True(okCalled);
        }
    }
}
