﻿namespace Agronomist.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using ViewModels;

    public sealed partial class LiveCard : UserControl
    {
        public static DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(LiveCardViewModel), typeof(UserControl), new PropertyMetadata(null));

        public LiveCard()
        {
            InitializeComponent();
            Bindings.Update();
        }

        /// <summary>
        ///     Dep prop for setting the ViewModel in an ItemTemplate.
        /// </summary>
        public LiveCardViewModel ViewModel
        {
            get { return (LiveCardViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}