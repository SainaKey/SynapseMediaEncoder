using System.Collections.Generic;
using System.Numerics;
using FFmpeg.NET.Enums;

namespace SynapseMediaEncoder;

public static class EncodePreset
{
    public class Preset
    {
        public string name;
    }
    public class Codec : Preset
    {
        public VideoCodec videoCodec;

        public Codec(string name, VideoCodec videoCodec)
        {
            this.name = name;
            this.videoCodec = videoCodec;
        }
    }
    
    public class Resolution : Preset
    {
        public int width;
        public int height;

        public Resolution(string name,int witdh,int height)
        {
            this.name = name;
            this.width = witdh;
            this.height = height;
        }
    }

    public static List<Preset> codecs = new List<Preset>();
    public static List<Preset> resolutions = new List<Preset>();

    static EncodePreset()
    {
        var hap = new Codec("hap",VideoCodec.hap);
        codecs.Add(hap);
        
        var resolution = new Resolution("1080p", 1920, 1080);
        resolutions.Add(resolution);
        var resolution2 = new Resolution("720p", 1280, 720);
        resolutions.Add(resolution2);
    }

    public static string[] GetNameArray(List<Preset> presets)
    {
        var codecArray = new string[presets.Count];
        var index = 0;
        foreach (var preset in presets)
        {
            codecArray[index] = preset.name;
            index++;
        }

        return codecArray;
    }

}