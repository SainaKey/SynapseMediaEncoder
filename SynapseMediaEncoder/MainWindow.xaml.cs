using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SynapseMediaEncoder.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using Path = System.IO.Path;

namespace SynapseMediaEncoder
{
    public partial class MainWindow : Window
    {
        private ReactiveCollection<string> dropPathList = new ReactiveCollection<string>();
        private List<EncodeInfo> encodeInfos = new List<EncodeInfo>();
        private List<MediaView> mediaViews = new List<MediaView>();
        private MediaEncoder mediaEncoder = new MediaEncoder();

        private List<IDisposable> disposables = new List<IDisposable>();
        
        public MainWindow()
        {
            InitializeComponent();
            SetupUI();
            InitDropEvents();
        }

        private void SetupUI()
        {
            var defaultFFmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg-5.1.2-full_build-shared",
                "bin", "ffmpeg.exe");
            
            var ffmpegPath = "";
            var xmlPath = Directory.GetCurrentDirectory() + "\\" + "ffmpegPath.xml";
            if (File.Exists(xmlPath))
            {
                XmlSerializer serializer = new XmlSerializer(ffmpegPath.GetType());
                using (FileStream fs = new FileStream(xmlPath, FileMode.Open))
                    ffmpegPath = (string)serializer.Deserialize(fs);
                FFmpegPathText.Text = "FFmpegPath:"+ffmpegPath;
                mediaEncoder.ffmpegPath = ffmpegPath;
            }
            else
            {
                if (File.Exists(defaultFFmpegPath))
                {
                    FFmpegPathText.Text = defaultFFmpegPath;
                    mediaEncoder.ffmpegPath = defaultFFmpegPath;
                }
                else
                {
                    NoteIsFFmpegPathIsNull();
                }
            }
            
            
            CodecBox.ItemsSource = EncodePreset.GetNameArray(EncodePreset.codecs);
            CodecBox.SelectedIndex = 0;
            SizeBox.ItemsSource = EncodePreset.GetNameArray(EncodePreset.resolutions);
            SizeBox.SelectedIndex = 0;
        }

        public void NoteIsFFmpegPathIsNull()
        {
            MessageBox.Show("ffmpeg.exeのパスを指定してください。");
            mediaEncoder.ffmpegPath = "";
        }

        private void InitDropEvents()
        {
            dropPathList.CollectionChangedAsObservable().Subscribe(x =>
            {
                encodeInfos.Clear();
                mediaViews.Clear();
                foreach (var disposable in disposables)
                {
                    disposable.Dispose();
                }
                
                disposables.Clear();

                List<GroupBox> groupBoxes = new List<GroupBox>();
                
                foreach (var dropPath in dropPathList)
                {
                    EncodeInfo encodeInfo = new EncodeInfo(dropPath,MediaEncoder.GetOutputPath(dropPath));
                    MediaView mediaView = new MediaView(encodeInfo);

                    encodeInfo.progress.AsObservable().Subscribe(x =>
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            mediaView.progressBar.Value = x * 100;
                        }));
                    }).AddTo(disposables);
                    
                    encodeInfos.Add(encodeInfo);
                    mediaViews.Add(mediaView);
                    
                    groupBoxes.Add(mediaView.groupBox);
                }
                
                EncodeList.ItemsSource = groupBoxes;
                
            });
        }

        
        private async void EncodeButton_Click(object sender, RoutedEventArgs e)
        {
            EncodeButton.IsEnabled = false;
            CodecBox.IsEnabled = false;
            SizeBox.IsEnabled = false;
            CancelButton.IsEnabled = true;
            await mediaEncoder.Encode(encodeInfos,EncodePreset.codecs[CodecBox.SelectedIndex] as EncodePreset.Codec
                ,EncodePreset.resolutions[SizeBox.SelectedIndex] as EncodePreset.Resolution);
            
            EncodeButton.IsEnabled = true;
            CodecBox.IsEnabled = true;
            SizeBox.IsEnabled = true;
            CancelButton.IsEnabled = true;
            
            MessageBox.Show("エンコードが終了しました。");
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            mediaEncoder.Cancel();
            CancelButton.IsEnabled = false;
            MessageBox.Show("現在のエンコードが終わり次第中断します。");
        }

        private void SearchPathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "実行ファイル (*.exe)|*.exe|全てのファイル (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                FFmpegPathText.Text = "FFmpegPath:" + dialog.FileName;
                mediaEncoder.ffmpegPath = dialog.FileName;
                
                XmlSerializer serializer = new XmlSerializer(mediaEncoder.ffmpegPath.GetType());
                using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + "ffmpegPath.xml", FileMode.Create))
                    serializer.Serialize(fs, mediaEncoder.ffmpegPath);
            }
        }

        
        #region DragAndDrop
        private void FileListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var pathArray = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var path in pathArray)
                {
                    var isDirectory = File.GetAttributes(path.ToString()).HasFlag(FileAttributes.Directory);
                    if (isDirectory)
                    {
                        var files = Directory.GetFiles(path, "*", System.IO.SearchOption.TopDirectoryOnly);
                        foreach (var filePath in files)
                        {
                            dropPathList.Add(filePath);
                        }
                    }
                    else
                    {
                        var filePath = path;
                        dropPathList.Add(filePath);
                    }
                }

            }
        }
 
        private void FileListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        
        #endregion


        protected virtual void Close_Click(object sender, CancelEventArgs e)
        {
            // encodeInfos内のisEncodedが全てtrueの際何もせずに閉じる
            var completed = encodeInfos.Where(x => x.isEncoded).Count() == encodeInfos.Count;
            if (completed) return;

            var running = encodeInfos.Any(x => x.isEncoded == false && x.progress.Value > 0);
            if (running)
            {
                if (MessageBoxResult.No == MessageBox.Show("エンコード中のファイルが存在します\n終了しますか？", "Alert",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning))
                {
                    e.Cancel = true;
                }
            }
        }
    }
}