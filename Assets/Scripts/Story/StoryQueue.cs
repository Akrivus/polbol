using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;

public class StoryQueue : MonoBehaviour
{
    public static StoryQueue Instance => _instance ??= FindObjectOfType<StoryQueue>();
    private static StoryQueue _instance;

    public Action OnQueueOpen;
    public Action OnQueueClosed;

    public ChatNodeTree Chat { get; private set; }
    public CountryManager CountryManager { get; private set; }
    public StoryGenerator Generator { get; private set; }

    [SerializeField]
    private TextMeshProUGUI titleCard;

    [SerializeField]
    private TextMeshProUGUI titleName;

    private ConcurrentQueue<Story> queue = new ConcurrentQueue<Story>();
    private Stopwatch stopwatch = new Stopwatch();

    private void Awake()
    {
        Generator = GetComponent<StoryGenerator>();
        CountryManager = GetComponent<CountryManager>();
        Chat = GetComponent<ChatNodeTree>();

        StartCoroutine(PlayQueue());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Generate(string story)
    {
        Generator.AddIdeaToQueue(story);
    }

    public void AddStoryToQueue(Story story)
    {
        queue.Enqueue(story);
    }

    private IEnumerator PlayQueue()
    {
        yield return new WaitForSeconds(1);

        OnQueueOpen?.Invoke();

        titleCard.text = "Suggest topics in the chat.";
        stopwatch.Start();

        yield return new WaitUntil(() => queue.Count > 0
            || stopwatch.Elapsed.TotalSeconds >= 30);
        yield return Interstitual.Activate();

        stopwatch.Reset();

        if (queue.TryDequeue(out var story))
        {
            titleCard.text = story.Title;
            OnQueueClosed?.Invoke();
            yield return PlayStory(story);
        }
        else
        {
            Story.LoadOrGenerate();
        }
        yield return PlayQueue();
    }

    public IEnumerator PlayStory(Story story)
    {
        var countries = story.Countries.Select((n) => CountryManager[n]).ToArray();
        CountryManager.SpawnCountries(countries);

        foreach (var node in story.Nodes)
            Chat.Add(new ChatNode(CountryManager, node));

        titleCard.text = "";
        titleName.text = story.Title;

        yield return Chat.Play();
        yield return new WaitForSeconds(1);

        titleName.text = "";

        CountryManager.DespawnCountries();
    }
}