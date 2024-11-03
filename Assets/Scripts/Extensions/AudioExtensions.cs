﻿using System;
using UnityEngine;

public static class AudioExtensions
{
    public static float GetAmplitude(this AudioSource source, int size = 16)
    {
        if (!source.isPlaying || source.clip == null || source.timeSamples + size > source.clip.samples)
            return 0;
        var samples = new float[size];
        source.clip.GetData(samples, source.timeSamples);

        var sum = 0f;
        for (var i = 0; i < samples.Length; i++)
            sum += Mathf.Abs(samples[i]);
        return sum / samples.Length;
    }

    public static AudioClip ToAudioClip(this string data, int frequency = 48000)
    {
        var bytes = Convert.FromBase64String(data);
        var samples = new float[bytes.Length / 2];
        for (int i = 0; i < samples.Length; i++)
            samples[i] = BitConverter.ToInt16(bytes, i * 2) / 32768f;

        var clip = AudioClip.Create(
            string.Empty,
            samples.Length, 1, frequency, false);
        clip.SetData(samples, 0);

        return clip;
    }
}