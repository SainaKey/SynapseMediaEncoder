using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using Reactive.Bindings;

namespace SynapseMediaEncoder.View;

public class MediaView
{
    public string guid;
    public GroupBox groupBox { get; protected set; }
    public ProgressBar progressBar { get; protected set; }

    public MediaView(EncodeInfo encodeInfo)
    {
        guid = Guid.NewGuid().ToString("N");
        groupBox = new GroupBox();
        groupBox.Header = encodeInfo.inputPath.Value;
        
        var stackPanel = new StackPanel();
        stackPanel.Margin = new Thickness(10);
        stackPanel.Orientation = Orientation.Horizontal;
        
        var outputPathText = new TextBox();
        outputPathText.Width = 500;
        outputPathText.IsReadOnly = true;
        outputPathText.Margin = new Thickness(10);
        outputPathText.Text = encodeInfo.outputPath.Value;
        stackPanel.Children.Add(outputPathText);
        
        progressBar = new ProgressBar();
        progressBar.Width = 200;
        progressBar.Margin = new Thickness(10);
        progressBar.Value = 0;
        stackPanel.Children.Add(progressBar);

        groupBox.Content = stackPanel;
    }
}