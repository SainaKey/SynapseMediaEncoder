using System;
using System.Numerics;
using Reactive.Bindings;

namespace SynapseMediaEncoder;

public class EncodeInfo
{
    public ReactiveProperty<string> guid = new ReactiveProperty<string>();
    public ReactiveProperty<string> inputPath = new ReactiveProperty<string>();
    public ReactiveProperty<string> outputPath = new ReactiveProperty<string>();
    public ReactiveProperty<float> progress = new ReactiveProperty<float>();
    public bool isEncoded = false;

    public EncodeInfo(string inputPath, string outputPath)
    {
        this.guid.Value = Guid.NewGuid().ToString("N");
        this.inputPath.Value = inputPath;
        this.outputPath.Value = outputPath;
        this.progress.Value = 0.0f;
    }
}