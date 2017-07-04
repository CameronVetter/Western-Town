using UnityEngine;
using System.Collections.Generic;

public class BoxDrawer
{
    // Consts
    public const float DefaultLineWidth = 0.001f;
    public const float DefaultBasisLength = 0.2f;

    private readonly GameObject _callingObject;

    public BoxDrawer(GameObject callingObject)
    {
        _callingObject = callingObject;
    }

    // Structs
    public class Line
    {
        // Functions
        public Line()
        {
        }

        public Line(Vector3 p0, Vector3 p1, Color c0, Color c1, float lineWidth = DefaultLineWidth)
        {
            P0 = p0;
            P1 = p1;
            C0 = c0;
            C1 = c1;
            LineWidth = lineWidth;
            IsValid = true;
        }

        public bool Set_IfDifferent(Vector3 p0, Vector3 p1, Color c0, Color c1, float lineWidth)
        {
            IsValid = true;
            if ((P0 != p0) || (P1 != p1) || (C0 != c0) || (C1 != c1) || (LineWidth != lineWidth))
            {
                P0 = p0;
                P1 = p1;
                C0 = c0;
                C1 = c1;
                LineWidth = lineWidth;
                return true;
            }
            return false;
        }

        // Data
        public Vector3 P0;
        public Vector3 P1;
        public Color C0;
        public Color C1;
        public float LineWidth;
        public bool IsValid;
    }

    public class LineData
    {
        public int LineIndex;
        public List<Line> Lines = new List<Line>();
        public MeshRenderer Renderer;
        public MeshFilter Filter;
    }

    public class AnimationCurve3
    {
        public void AddKey(float time, Vector3 pos)
        {
            CurveX.AddKey(time, pos.x);
            CurveY.AddKey(time, pos.y);
            CurveZ.AddKey(time, pos.z);
        }
        public Vector3 Evaluate(float time)
        {
            return new Vector3(CurveX.Evaluate(time), CurveY.Evaluate(time), CurveZ.Evaluate(time));
        }

        public AnimationCurve CurveX = new AnimationCurve();
        public AnimationCurve CurveY = new AnimationCurve();
        public AnimationCurve CurveZ = new AnimationCurve();
    }

    public class Box
    {
        public const float InitialPositionForwardMaxDistance = 2.0f;
        public const float AnimationTime = 2.5f;
        public const float DelayPerItem = 0.35f;

        public Box(
            Vector3 center,
            Quaternion rotation,
            Color color,
            Vector3 halfSize,
            float lineWidth = DefaultLineWidth * 3.0f)
        {
            Center = center;
            Rotation = rotation;
            Color = color;
            HalfSize = halfSize;
            LineWidth = lineWidth;
        }


        public Vector3 Center;
        public Quaternion Rotation;
        public Color Color;
        public Vector3 HalfSize;
        public float LineWidth;

    }

    // Config
    public Material MaterialLine;

    // Privates
    private readonly LineData _lineData = new LineData();

    // Functions
    protected bool DrawBox(Box box)
    {
        // Animation is done, just pass through
        return Draw_Box(box.Center, box.Rotation, box.Color, box.HalfSize, box.LineWidth);

    }

    protected bool Draw_Box(Vector3 center, Quaternion rotation, Color color, Vector3 halfSize, float lineWidth = DefaultLineWidth)
    {
        bool needsUpdate = false;

        Vector3 basisX = rotation * Vector3.right;
        Vector3 basisY = rotation * Vector3.up;
        Vector3 basisZ = rotation * Vector3.forward;
        Vector3[] pts =
        {
            center + basisX * halfSize.x + basisY * halfSize.y + basisZ * halfSize.z,
            center + basisX * halfSize.x + basisY * halfSize.y - basisZ * halfSize.z,
            center - basisX * halfSize.x + basisY * halfSize.y - basisZ * halfSize.z,
            center - basisX * halfSize.x + basisY * halfSize.y + basisZ * halfSize.z,

            center + basisX * halfSize.x - basisY * halfSize.y + basisZ * halfSize.z,
            center + basisX * halfSize.x - basisY * halfSize.y - basisZ * halfSize.z,
            center - basisX * halfSize.x - basisY * halfSize.y - basisZ * halfSize.z,
            center - basisX * halfSize.x - basisY * halfSize.y + basisZ * halfSize.z
        };

        // Bottom
        needsUpdate |= Draw_Line(pts[0], pts[1], color, color, lineWidth);
        needsUpdate |= Draw_Line(pts[1], pts[2], color, color, lineWidth);
        needsUpdate |= Draw_Line(pts[2], pts[3], color, color, lineWidth);
        needsUpdate |= Draw_Line(pts[3], pts[0], color, color, lineWidth);

        // Top
        needsUpdate |= Draw_Line(pts[4], pts[5], color, color, lineWidth);
        needsUpdate |= Draw_Line(pts[5], pts[6], color, color, lineWidth);
        needsUpdate |= Draw_Line(pts[6], pts[7], color, color, lineWidth);
        needsUpdate |= Draw_Line(pts[7], pts[4], color, color, lineWidth);

        // Vertical lines
        needsUpdate |= Draw_Line(pts[0], pts[4], color, color, lineWidth);
        needsUpdate |= Draw_Line(pts[1], pts[5], color, color, lineWidth);
        needsUpdate |= Draw_Line(pts[2], pts[6], color, color, lineWidth);
        needsUpdate |= Draw_Line(pts[3], pts[7], color, color, lineWidth);

        return needsUpdate;
    }

    protected bool Draw_Line(Vector3 start, Vector3 end, Color colorStart, Color colorEnd, float lineWidth = DefaultLineWidth)
    {
        // Create up a new line (unless it's already created)
        while (_lineData.LineIndex >= _lineData.Lines.Count)
        {
            _lineData.Lines.Add(new Line());
        }

        // Set it
        bool needsUpdate = _lineData.Lines[_lineData.LineIndex].Set_IfDifferent(_callingObject.transform.InverseTransformPoint(start), _callingObject.transform.InverseTransformPoint(end), colorStart, colorEnd, lineWidth);

        // Inc out count
        ++_lineData.LineIndex;

        return needsUpdate;
    }

    private void Lines_LineDataToMesh()
    {
        // Alloc them up
        Vector3[] verts = new Vector3[_lineData.Lines.Count * 8];
        int[] tris = new int[_lineData.Lines.Count * 12 * 3];
        Color[] colors = new Color[verts.Length];

        // Build the data
        for (int i = 0; i < _lineData.Lines.Count; ++i)
        {
            // Base index calcs
            int vert = i * 8;
            int v0 = vert;
            int tri = i * 12 * 3;

            // Setup
            Vector3 dirUnit = (_lineData.Lines[i].P1 - _lineData.Lines[i].P0).normalized;
            Vector3 normX = Vector3.Cross((Mathf.Abs(dirUnit.y) >= 0.99f) ? Vector3.right : Vector3.up, dirUnit).normalized;
            Vector3 normy = Vector3.Cross(normX, dirUnit);

            // Verts
            verts[vert] = _lineData.Lines[i].P0 + normX * _lineData.Lines[i].LineWidth + normy * _lineData.Lines[i].LineWidth; colors[vert] = _lineData.Lines[i].C0; ++vert;
            verts[vert] = _lineData.Lines[i].P0 - normX * _lineData.Lines[i].LineWidth + normy * _lineData.Lines[i].LineWidth; colors[vert] = _lineData.Lines[i].C0; ++vert;
            verts[vert] = _lineData.Lines[i].P0 - normX * _lineData.Lines[i].LineWidth - normy * _lineData.Lines[i].LineWidth; colors[vert] = _lineData.Lines[i].C0; ++vert;
            verts[vert] = _lineData.Lines[i].P0 + normX * _lineData.Lines[i].LineWidth - normy * _lineData.Lines[i].LineWidth; colors[vert] = _lineData.Lines[i].C0; ++vert;

            verts[vert] = _lineData.Lines[i].P1 + normX * _lineData.Lines[i].LineWidth + normy * _lineData.Lines[i].LineWidth; colors[vert] = _lineData.Lines[i].C1; ++vert;
            verts[vert] = _lineData.Lines[i].P1 - normX * _lineData.Lines[i].LineWidth + normy * _lineData.Lines[i].LineWidth; colors[vert] = _lineData.Lines[i].C1; ++vert;
            verts[vert] = _lineData.Lines[i].P1 - normX * _lineData.Lines[i].LineWidth - normy * _lineData.Lines[i].LineWidth; colors[vert] = _lineData.Lines[i].C1; ++vert;
            verts[vert] = _lineData.Lines[i].P1 + normX * _lineData.Lines[i].LineWidth - normy * _lineData.Lines[i].LineWidth; colors[vert] = _lineData.Lines[i].C1; ++vert;

            // Indices
            tris[tri + 0] = (v0 + 0); tris[tri + 1] = (v0 + 5); tris[tri + 2] = (v0 + 4); tri += 3;
            tris[tri + 0] = (v0 + 1); tris[tri + 1] = (v0 + 5); tris[tri + 2] = (v0 + 0); tri += 3;

            tris[tri + 0] = (v0 + 1); tris[tri + 1] = (v0 + 6); tris[tri + 2] = (v0 + 5); tri += 3;
            tris[tri + 0] = (v0 + 2); tris[tri + 1] = (v0 + 6); tris[tri + 2] = (v0 + 1); tri += 3;

            tris[tri + 0] = (v0 + 2); tris[tri + 1] = (v0 + 7); tris[tri + 2] = (v0 + 6); tri += 3;
            tris[tri + 0] = (v0 + 3); tris[tri + 1] = (v0 + 7); tris[tri + 2] = (v0 + 2); tri += 3;

            tris[tri + 0] = (v0 + 3); tris[tri + 1] = (v0 + 7); tris[tri + 2] = (v0 + 4); tri += 3;
            tris[tri + 0] = (v0 + 3); tris[tri + 1] = (v0 + 4); tris[tri + 2] = (v0 + 0); tri += 3;

            tris[tri + 0] = (v0 + 0); tris[tri + 1] = (v0 + 3); tris[tri + 2] = (v0 + 2); tri += 3;
            tris[tri + 0] = (v0 + 0); tris[tri + 1] = (v0 + 2); tris[tri + 2] = (v0 + 1); tri += 3;

            tris[tri + 0] = (v0 + 5); tris[tri + 1] = (v0 + 6); tris[tri + 2] = (v0 + 7); tri += 3;
            tris[tri + 0] = (v0 + 5); tris[tri + 1] = (v0 + 7); tris[tri + 2] = (v0 + 4); tri += 3;
        }

        // Create up the components
        if (_lineData.Filter == null)
        {
            _lineData.Filter = _callingObject.AddComponent<MeshFilter>();
        }
        if (_lineData.Renderer == null)
        {
            _lineData.Renderer = _callingObject.AddComponent<MeshRenderer>();
            _lineData.Renderer.material = MaterialLine;
        }

        // Create or clear the mesh
        Mesh mesh;
        if (_lineData.Filter.mesh != null)
        {
            mesh = _lineData.Filter.mesh;
            mesh.Clear();
        }
        else
        {
            mesh = new Mesh();
            mesh.name = "LineDrawer.Lines_LineDataToMesh";
        }

        // Set them into the mesh
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.colors = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        _lineData.Filter.mesh = mesh;

        // If no tris, hide it
        _lineData.Renderer.enabled = (_lineData.Lines.Count != 0);

        // Line index reset
        _lineData.LineIndex = 0;
    }

    public void UpdateBoxes(List<Box> boxes)
    {

        // Lines: Begin
        LineDraw_Begin();

        // Drawers
        bool needsUpdate = false;
        needsUpdate |= Draw_LineBoxList(boxes);

        // Lines: Finish up
        LineDraw_End(needsUpdate);
    }

    private bool Draw_LineBoxList(List<Box> boxes)
    {
        bool needsUpdate = false;
        foreach (var box in boxes)
        {
            needsUpdate |= DrawBox(box);
        }
        return needsUpdate;
    }

    protected void LineDraw_Begin()
    {
        _lineData.LineIndex = 0;
        for (int i = 0; i < _lineData.Lines.Count; ++i)
        {
            _lineData.Lines[i].IsValid = false;
        }
    }

    protected void LineDraw_End(bool needsUpdate)
    {
        if (_lineData == null)
        {
            return;
        }

        // Check if we have any not dirty
        int i = 0;
        while (i < _lineData.Lines.Count)
        {
            if (!_lineData.Lines[i].IsValid)
            {
                needsUpdate = true;
                _lineData.Lines.RemoveAt(i);
                continue;
            }
            ++i;
        }

        // Do the update (if needed)
        if (needsUpdate)
        {
            Lines_LineDataToMesh();
        }
    }
}