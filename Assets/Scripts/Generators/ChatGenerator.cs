﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ChatGenerator : MonoBehaviour
{
    public event Func<Chat, Task> OnGeneration;

    [SerializeField]
    private TextAsset _prompt;

    private string slug => name.Replace(' ', '-').ToLower();

    private ISubGenerator[] generators => _generators ?? (_generators = GetComponentsInChildren<ISubGenerator>());
    private ISubGenerator[] _generators;

    private ConcurrentQueue<Idea> queue = new ConcurrentQueue<Idea>();

    private void Awake()
    {
        StartCoroutine(UpdateQueue());
        ServerIntegration.AddApiRoute<Idea, string>("POST", $"/generate?with={slug}", HandleRequest);
    }

    private IEnumerator UpdateQueue()
    {
        var idea = default(Idea);
        yield return new WaitUntilTimer(() => queue.TryDequeue(out idea));

        if (idea != null)
        {
            var task = Generate(idea);
            yield return new WaitUntilTimer(() => task.IsCompleted, 1800);
            var chat = task.Result;

            ChatManager.Instance.AddToPlayList(chat);
        }

        yield return UpdateQueue();
    }

    public async Task<string> HandleRequest(Idea idea)
    {
        await Task.Run(() => AddIdeaToQueue(idea));
        return "OK.";
    }

    public void AddIdeaToQueue(Idea idea)
    {
        queue.Enqueue(idea);
    }

    public async Task<Chat> Generate(Idea idea)
    {
        var options = string.Join(", ", GetCharacterNames());
        var prompt = _prompt.Format(idea.Prompt, options);
        var chat = new Chat(idea);

        chat.Messages = await ChatClient.ChatAsync(prompt);
        chat.Topic = chat.Messages.Last().Content.ToString();

        foreach (var generator in generators)
            await generator.Generate(chat);
        if (OnGeneration != null)
            await OnGeneration(chat);

        chat.Lock();
        chat.Save();
        return chat;
    }

    private string[] GetCharacterNames()
    {
        return Actor.All.List
            .Select(actor => string.Format("{0} ({1})", actor.Name, actor.Pronouns.Chomp()))
            .ToArray();
    }
}
