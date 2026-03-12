using System.Windows;
using System.Windows.Controls;

namespace Sts2AiCompanion.Wpf;

public partial class MainWindow : Window
{
    private readonly ShellViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new ShellViewModel();
        DataContext = _viewModel;
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync(Dispatcher);
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        await _viewModel.DisposeAsync();
    }

    private async void AnalyzeNow_OnClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.AnalyzeNowAsync();
    }

    private void ToggleAutoAdvice_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ToggleAutoAdvice();
    }

    private async void RetryLast_OnClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.AnalyzeNowAsync();
    }

    private void RefreshKnowledge_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.RefreshKnowledge();
    }

    private void OpenArtifacts_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenArtifacts();
    }

    private void ModelSelection_OnChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is string option)
        {
            _viewModel.SetSelectedModelOption(option);
        }
    }

    private void ReasoningSelection_OnChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is string option)
        {
            _viewModel.SetSelectedReasoningOption(option);
        }
    }
}
