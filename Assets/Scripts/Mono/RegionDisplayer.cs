using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Geometry;

public class RegionDisplayer : MaskableGraphic, IPointerEnterHandler, IPointerExitHandler
{
    public Polygon polygon = new Polygon();
    public Vector2 centroid = new Vector2();
    public event System.Action onMouseEnter;
    public event System.Action onMouseExit;

    [SerializeField]
    Texture m_Texture;

    // make it such that unity will trigger our ui element to redraw whenever we change the texture in the inspector
    public Texture texture {
        get {
            return m_Texture;
        }
        set {
            if (m_Texture == value)
                return;

            m_Texture = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }
    public override Texture mainTexture {
        get {
            return m_Texture == null ? s_WhiteTexture : m_Texture;
        }
    }

    public void SetColor(Color c)
    {
        color = c;
    }



    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
        SetMaterialDirty();
    }
    
    void MakePolygon(VertexHelper vh, Polygon polygon, Vector2 centroid)
    {
        vh.Clear();
        UIVertex vert = new UIVertex();
        vert.color = this.color;  // Do not forget to set this, otherwise 

        vert.position = centroid;
        vh.AddVert(vert);

        if (polygon.Count < 3) {
            return;
        }

        foreach (var segment in polygon) {
            vert.position = segment.a;
            vh.AddVert(vert);
            vert.position = segment.b;
            vh.AddVert(vert);
            var i = vh.currentVertCount;
            vh.AddTriangle(0, i - 2, i - 1);
        }
    }

    void UpdateCollider(Polygon polygon)
    {
        var collider = GetComponent<PolygonCollider2D>();
        var points = new List<Vector2>();
        foreach(var segment in polygon) {
            points.Add(segment.a);
            points.Add(segment.b);
        }
        collider.points = points.ToArray();
    }
    
    // actually update our mesh
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        MakePolygon(vh, polygon, centroid);
    }

    public void OnMouseEnter()
    {
        print("MOUSE IS ENTERING");
        try { onMouseEnter.Invoke(); } catch { }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //try { onMouseEnter.Invoke(); } catch { }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //try { onMouseExit.Invoke(); } catch { }
    }
}
