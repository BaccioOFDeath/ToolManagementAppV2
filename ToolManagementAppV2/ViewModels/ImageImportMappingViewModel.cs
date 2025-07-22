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

        public Func<ToolModel, IEnumerable<string>> BuildSelector()
        {
            return t =>
            {
                var keys = new List<string>();
                if (UseToolNumber && !string.IsNullOrWhiteSpace(t.ToolNumber))
                    keys.Add(t.ToolNumber.Trim().ToUpperInvariant());
                if (UsePartNumber && !string.IsNullOrWhiteSpace(t.PartNumber))
                    keys.Add(t.PartNumber.Trim().ToUpperInvariant());
                if (UseNameDescription && !string.IsNullOrWhiteSpace(t.NameDescription))
                    keys.Add(t.NameDescription.Trim().ToUpperInvariant());
                return keys;
            };
        }
    }
}
