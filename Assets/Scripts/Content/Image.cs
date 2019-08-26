using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public class Image
{
    public readonly string origin;
    public readonly Vector2i size;

    byte[] data;
    Texture2D texture = null;
    bool isInitialized = false;

    public Texture2D GetTexture()
    {
        if (!isInitialized)
        {
            throw new Exception("Tried to get a texture2d from a NON INITIALIZED image");
        }
        return texture;
    }

    public Image(string origin, bool autoLoad = true)
    {
        this.origin = origin;
        var name = Path.GetFileNameWithoutExtension(origin);
        var parts = name.Split('_');
        var sizePart = parts[parts.Length - 1];
        var sizeParts = sizePart.Split('x');
        var size = new Vector2i(
            Convert.ToInt32(sizeParts[0]),
            Convert.ToInt32(sizeParts[1])
        );

        if (autoLoad) Initialize();
    }

    public void Initialize()
    {
        texture = new Texture2D(size.x, size.y);

        data = Disk.ReadAllBytes(origin);

        ImageConversion.LoadImage(texture, data);

        isInitialized = true;
    }
}