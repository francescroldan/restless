using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Renders a visible fan-shaped overlay on the vision cone for development clarity.
    /// Attach to the same GameObject as VisionCone. Disable or tint for final art.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VisionConeVisual : MonoBehaviour
    {
        [SerializeField] private float _radius = 8f;
        [SerializeField] private float _angle = 110f;
        [SerializeField] private int _segments = 24;
        [SerializeField] private Color _fillColor = new Color(1f, 1f, 0.4f, 0.08f);
        [SerializeField] private Color _edgeColor = new Color(1f, 1f, 0.4f, 0.35f);

        private Mesh _mesh;
        private MeshRenderer _renderer;

        private void Awake()
        {
            _mesh = new Mesh { name = "VisionConeMesh" };
            GetComponent<MeshFilter>().mesh = _mesh;

            _renderer = GetComponent<MeshRenderer>();
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _renderer.sortingOrder = 10;

            // Use Sprites/Default so it renders in URP 2D without lighting influence
            _renderer.material = new Material(Shader.Find("Sprites/Default"));
            _renderer.material.color = Color.white;

            BuildMesh();
        }

        private void BuildMesh()
        {
            int vertCount = _segments + 2; // origin + arc vertices
            var vertices  = new Vector3[vertCount];
            var colors    = new Color[vertCount];
            var triangles = new int[_segments * 3];

            vertices[0] = Vector3.zero;
            colors[0]   = _fillColor;

            float halfAngle = _angle * 0.5f;
            for (int i = 0; i <= _segments; i++)
            {
                float t   = (float)i / _segments;
                float deg = Mathf.Lerp(-halfAngle, halfAngle, t);
                float rad = deg * Mathf.Deg2Rad;
                // Cone points "up" (Unity Y+) before VisionCone rotates the transform
                vertices[i + 1] = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0f) * _radius;
                colors[i + 1]   = i == 0 || i == _segments ? _edgeColor : _fillColor;
            }

            for (int i = 0; i < _segments; i++)
            {
                triangles[i * 3]     = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            _mesh.vertices  = vertices;
            _mesh.colors    = colors;
            _mesh.triangles = triangles;
            _mesh.RecalculateBounds();
        }
    }
}
