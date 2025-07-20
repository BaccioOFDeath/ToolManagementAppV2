using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.ViewModels
{
    public class ImageImportMappingViewModel : ObservableObject
    {
        bool _useToolNumber = true;
        public bool UseToolNumber { get => _useToolNumber; set => SetProperty(ref _useToolNumber, value); }

        bool _usePartNumber;
        public bool UsePartNumber { get => _usePartNumber; set => SetProperty(ref _usePartNumber, value); }

        bool _useNameDescription;
        public bool UseNameDescription { get => _useNameDescription; set => SetProperty(ref _useNameDescription, value); }

        public Func<ToolModel, string> BuildSelector()
        {
            return t =>
            {
                var parts = new List<string>();
                if (UseToolNumber) parts.Add(t.ToolNumber);
                if (UsePartNumber) parts.Add(t.PartNumber);
                if (UseNameDescription) parts.Add(t.NameDescription);
                return string.Join("_", parts).Trim().ToUpperInvariant();
            };
        }
    }
}
