using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static GameLogger;
using UnityEngine;

public class Image
{
    public readonly string origin;
    public readonly Vector2Int size;

    byte[] data;
    Sprite sprite;
    bool isInitialized = false;

    public Sprite GetSprite()
    {
        if (!isInitialized)
        {
            throw new Exception("Tried to get a texture2d from a NON INITIALIZED image");
        }
        return sprite;
    }

    public Image(string origin, bool autoLoad = true) : this(origin, ReadTextureSize(origin), autoLoad){ }

    public Image(string origin, Vector2Int size, bool autoLoad=true)
    {
        this.origin = origin;
        this.size = size;

        if (autoLoad) Initialize();
    }

    public void Initialize()
    {
        var texture = new Texture2D(size.x, size.y);
        logger.Debug("Created new image of size " + size);

        data = Disk.ReadAllBytes(origin);

        ImageConversion.LoadImage(texture, data);

        sprite = Sprite.Create(texture, new Rect(new Vector2(0f, 0f), size), new Vector2(size.x / 2f, size.y / 2f));
        sprite.texture.filterMode = FilterMode.Point;

        isInitialized = true;
    }

    static Vector2Int ReadTextureSize(string path)
    {

        var name = Path.GetFileNameWithoutExtension(path);
        var parts = name.Split('_');
        var sizePart = parts[parts.Length - 1];
        var sizeParts = sizePart.Split('x');
        var size = new Vector2Int(
            Convert.ToInt32(sizeParts[0]),
            Convert.ToInt32(sizeParts[1])
        );

        return size;
    }
}