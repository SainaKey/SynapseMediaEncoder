using FFmpeg.NET;
using FFmpeg.NET.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SynapseMediaEncoder;

public class MediaEncoder
{
    private CancellationTokenSource cts = new CancellationTokenSource();
    
    public string ffmpegPath;

    public MediaEncoder()
    {
        
    }
    public async Task Encode(List<EncodeInfo> encodeInfoList,EncodePreset.Codec codec,EncodePreset.Resolution resolution)
    {
        cts = new CancellationTokenSource();
        await OutputHapVideos(cts.Token,encodeInfoList,codec.videoCodec,resolution.width,resolution.height);
    }

    public void Cancel()
    {
        cts.Cancel();
    }
    
    private async Task OutputHapVideos(CancellationToken cancellationToken,List<EncodeInfo> encodeInfoList,VideoCodec codec,int width,int height)
    {
        
        foreach (var inputFile in encodeInfoList)
        {
            if (inputFile.isEncoded == false)
            {
                var done = await OutputHapVideo(inputFile,cancellationToken,codec,width,height);
                if (done)
                {
                    MoveEncodedFile(inputFile.inputPath.Value);
                    inputFile.isEncoded = true;
                }
                else
                {
                    return;
                }
            }
        }
    }
    
    private async Task<bool> OutputHapVideo(EncodeInfo encodeInfo, CancellationToken cancellationToken,VideoCodec codec,int width,int height)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (Exception e)
        {
            Trace.WriteLine("Cancel");
            return false;
        }

        var inputFile = new InputFile(encodeInfo.inputPath.Value);
        var outputFile = new OutputFile(encodeInfo.outputPath.Value);
        
        var ffmpeg = new Engine(ffmpegPath);
        var conversionOptions = new ConversionOptions
        {
            VideoCodec = codec,
            VideoSize = VideoSize.Custom,
            CustomWidth = width,
            CustomHeight = height,
            VideoFps = 60,
        };
        ffmpeg.Progress += (sender, args) =>
        {
            var progress = args.ProcessedDuration / args.TotalDuration;
            encodeInfo.progress.Value = (float)progress;
        };
        
        CancellationToken fake = new CancellationToken();

        // ファイルが存在しない場合falseを返す
        if (!File.Exists(encodeInfo.inputPath.Value))
        {
            MessageBox.Show($"ファイルが存在しません\n{encodeInfo.inputPath.Value}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        await ffmpeg.ConvertAsync(inputFile, outputFile, conversionOptions,fake);
        
        return true;
    }
    
    
    public static string GetOutputPath(string inputPath)
    {
        var outputDirectory = Path.Combine(Path.GetDirectoryName(inputPath),"Outputs");
        var outputFileName = Path.GetFileNameWithoutExtension(inputPath);
        var outputPath = Path.Combine(outputDirectory, outputFileName+".mov");

        if (!File.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        return outputPath;
    }
    
    private void MoveEncodedFile(string filePath)
    {
        var encodedDirectory = Path.Combine(Path.GetDirectoryName(filePath),"OriginalFiles");
        var fileName = Path.GetFileName(filePath);
        if (!File.Exists(encodedDirectory))
        {
            Directory.CreateDirectory(encodedDirectory);
        }

        var movePath = Path.Combine(encodedDirectory, fileName);
        File.Move(filePath,movePath);
    }
}