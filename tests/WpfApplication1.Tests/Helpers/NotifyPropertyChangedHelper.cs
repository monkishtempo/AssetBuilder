using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Asset_Builder.Tests.Helpers
{
    [ExcludeFromCodeCoverage]
    public class NotifyPropertyChangedHelper
    {
        public NotifyPropertyChangedHelper(INotifyPropertyChanged viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel", "Argument cannot be null.");
            }

            Changes = new List<string>();

            viewModel.PropertyChanged += viewModel_PropertyChanged;
        }

        void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Changes.Add(e.PropertyName);
        }

        public List<string> Changes { get; private set; }

        public void AssertChange(int changeIndex, string expectedPropertyName)
        {
            Assert.NotNull(Changes);

            Assert.True(changeIndex < Changes.Count);

            Assert.Equal(expectedPropertyName, Changes[changeIndex]);
        }
    }
}