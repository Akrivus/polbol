using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class AvatarGenerator : MonoBehaviour
{
    [SerializeField]
    private Camera Camera;

    private Actor[] Actors;
    private Sentiment[] Sentiments;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Actors = Resources.LoadAll<Actor>("Actors");
        Sentiments = Resources.LoadAll<Sentiment>("Faces");
        StartCoroutine(GenerateAvatars());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator GenerateAvatars()
    {
        foreach (var actor in Actors)
        {
            foreach (var sentiment in Sentiments)
            {
                var path = $"WWW/{sentiment.Name}-{actor.Name.ToFileSafeString()}.png";
                if (File.Exists(path))
                    continue;

                var gameObject = Instantiate(actor.Prefab);
                var actorController = gameObject.GetComponent<ActorController>();
                actorController.Context = new ActorContext(actor);
                actorController.Sentiment = sentiment;

                yield return new WaitForEndOfFrame();

                var texture = new Texture2D(256, 256);

                Camera.targetTexture = new RenderTexture(256, 256, 24);
                Camera.Render();
                RenderTexture.active = Camera.targetTexture;
                texture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
                texture.Apply();

                var bytes = texture.EncodeToPNG();
                Destroy(texture);
                Destroy(gameObject);

                Debug.Log($"Generated avatar for {sentiment.Name}-{actor.Name}");

                if (!File.Exists(path))
                    File.Create(path).Dispose();
                File.WriteAllBytes(path, bytes);


            }
        }
    }

}
