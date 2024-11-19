using Newtonsoft.Json;
using Google.TTS;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.Net.Http;
using System;

public class TextToSpeechGenerator : MonoBehaviour, ISubGenerator, IConfigurable<TTSConfigs>
{
    public static string GOOGLE_API_KEY;

    public void Configure(TTSConfigs config)
    {
        GOOGLE_API_KEY = config.GoogleApiKey;
    }

    private void Awake()
    {
        ConfigManager.Instance.RegisterConfig(typeof(TTSConfigs), "tts", (config) => Configure((TTSConfigs) config));
    }

    public async Task<Chat> Generate(Chat chat)
    {
        foreach (var node in chat.Nodes)
            await GenerateTextToSpeech(node);
        return chat;
    }

    private async Task GenerateTextToSpeech(ChatNode node, int delay = 1)
    {
        var url = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={GOOGLE_API_KEY}";
        var json = JsonConvert.SerializeObject(new Request(node.Text, node.Actor.Voice));

        var client = new HttpClient();
        var response = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            await Task.Delay(delay * 1000);
            await GenerateTextToSpeech(node, ++delay);
        }

        var text = await response.Content.ReadAsStringAsync();
        var output = JsonConvert.DeserializeObject<Output>(text);
        node.AudioData = output.AudioData;
    }
}

namespace Google.TTS
{
    class Request
    {
        public TextInput input { get; set; }
        public AudioConfig audioConfig { get; set; }
        public Voice voice { get; set; }

        public Request(string text, string name)
        {
            audioConfig = new AudioConfig();
            input = new TextInput() { text = text.Scrub() };
            voice = new Voice() { name = name };
        }
    }

    class TextInput
    {
        public string text { get; set; }
    }

    class AudioConfig
    {
        public string audioEncoding { get; set; } = "LINEAR16";
        public float sampleRateHertz { get; set; } = 48000;
        public float volumeGainDb { get; set; } = 1;
        public float pitch { get; set; } = 1;
        public float speakingRate { get; set; } = 1.1f;
    }

    class Voice
    {
        public string name { get; set; } = "en-US-Standard-D";
        public string languageCode { get; set; } = "en-US";
    }

    class Output
    {
        [JsonProperty("audioContent")]
        public string AudioData { get; set; }
    }
}