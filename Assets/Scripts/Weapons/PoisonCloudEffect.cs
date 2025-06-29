using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PoisonCloudEffect : MonoBehaviour
{
    public float duration = 0.5f;
    private float radius;
    private float angle;
    private float damage;
    private Vector2 origin;
    private Vector2 direction;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public void Initialize(float radius, float angle, float damage, Vector2 origin, Vector2 direction)
    {
        this.radius = radius;
        this.angle = angle;
        this.damage = damage;
        this.origin = origin;
        this.direction = direction;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        AdjustVisual();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0f, 1f, 0f, 0.3f);
        meshRenderer.material = mat;

        StartCoroutine(ApplyDamage());
        Destroy(gameObject, duration);
    }

    void AdjustVisual()
    {
        Mesh mesh = new Mesh();

        int segments = 30;
        int pointCount = segments + 2;
        Vector3[] vertices = new Vector3[pointCount];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        float angleStep = angle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = Mathf.Deg2Rad * (-angle / 2f + i * angleStep);
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[i + 1] = new Vector3(x, y, 0f);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    IEnumerator ApplyDamage()
    {
        yield return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector2 toTarget = ((Vector2)hit.transform.position - origin).normalized;
                float angleToTarget = Vector2.Angle(direction, toTarget);

                if (angleToTarget <= angle / 2f)
                {
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(Mathf.RoundToInt(damage));
                        Debug.Log($"Poison Cloud hit {enemy.name} for {Mathf.RoundToInt(damage)}");
                    }
                }
            }
        }
    }
}
