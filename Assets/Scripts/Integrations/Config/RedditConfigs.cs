﻿using System;
using System.Collections.Generic;

public class RedditConfigs : IConfig
{
    public string Type => "reddit";
    public List<string> Subreddits { get; set; }
    public int BatchMax { get; set; }
    public int BatchLifetimeMax { get; set; }
    public float BatchPeriodInMinutes { get; set; }
}