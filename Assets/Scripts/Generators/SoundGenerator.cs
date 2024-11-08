﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SoundGenerator : MonoBehaviour, ISubGenerator
{
    public static string[] SoundGroups;

    [SerializeField]
    private TextAsset _prompt;

    private string context;

    private void Awake()
    {
        if (SoundGroups == null)
            SoundGroups = Resources.LoadAll<SoundGroup>("SoundGroups")
                .Select(t => t.name)
                .ToArray();
    }

    public async Task<Chat> Generate(Chat chat)
    {
        var names = chat.Headline.Names;
        var topic = chat.Headline.Topic;

        context = "";
        foreach (var node in chat.Nodes)
            context += string.Format("{0}: {1}\n", node.Actor.Name, node.Line);

        var soundGroups = await SelectSoundGroup(chat, names, topic);
        foreach (var soundGroup in soundGroups)
            chat.Contexts.Get(soundGroup.Key).SoundGroup = soundGroup.Value;

        return chat;
    }

    private async Task<Dictionary<Actor, string>> SelectSoundGroup(Chat chat, string[] names, string topic)
    {
        var options = string.Join(", ", SoundGroups);
        var characters = string.Join("\n- ", names);
        var prompt = _prompt.Format(options, characters, topic, context);
        var messages = await ChatClient.ChatAsync(prompt, true);
        var message = messages[1];

        var lines = message.Content.ToString().Parse(names);

        return lines
            .Where(line => names.Contains(line.Key))
            .ToDictionary(
                line => chat.Actors.Get(line.Key),
                line => line.Value);
    }
}
