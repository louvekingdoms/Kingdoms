using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Color
{
    public byte R = 0;
    public byte G = 0;
    public byte B = 0;

    public float r { get { return byteToFloat(R); } }
    public float g { get { return byteToFloat(G); } }
    public float b { get { return byteToFloat(B); } }

    public static Color white { get { return new Color(1f, 1f, 1f); } }

    public Color(float r, float g, float b) : this(floatToByte(r), floatToByte(g), floatToByte(b)) { }

    public Color(byte R, byte G, byte B)
    {
        this.R = R;
        this.G = G;
        this.B = B;
    }

    public static Color FromHSV(float h, float s, float v)
    {
        return new Color(
            r: HSVHelper(h, s, v, 5),
            g: HSVHelper(h, s, v, 3),
            b: HSVHelper(h, s, v, 1)
        );
    }

    static float HSVHelper(float h, float s, float v, float n)
    {
        var H = floatToByte(h);
        var S = floatToByte(s);
        var V = floatToByte(v);

        var k = (n + H * 360f / (255f * 60f)) % 6;
        return V / 255f - (V / 255f) * (S / 255f) * Math.Max(Math.Min(k, Math.Min(4 - k, 1)), 0);
    }

    static byte floatToByte(float f)
    {
        return Convert.ToByte(Math.Floor(f * 255));
    }

    static float byteToFloat(byte b)
    {
        return b / 255f;
    }

    public override string ToString()
    {
        return "R{0} G{1} B{2}".Format(R, G, B);
    }

    public UnityEngine.Color ToUnity()
    {
        return new UnityEngine.Color(r, g, b);
    }

    public System.Drawing.Color ToNET()
    {
        return System.Drawing.Color.FromArgb(R, G, B);
    }
}

