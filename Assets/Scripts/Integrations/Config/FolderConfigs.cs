﻿using System;
using System.Collections.Generic;

public class FolderConfigs : IConfig
{
    public string Type => "folder";
    public List<string> Prompts { get; set; }
    public string ReplayDirectory { get; set; }
    public int ReplayRate { get; set; }
    public int ReplaysPerBatch { get; set; }
}