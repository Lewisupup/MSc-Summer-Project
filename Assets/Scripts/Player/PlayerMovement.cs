using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    public Vector2 Velocity;
    public bool IsNearWall;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0f);
        Velocity = new Vector2(moveX, moveY) * moveSpeed;

        transform.position += movement * moveSpeed * Time.deltaTime;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;

        IsNearWall =
            Mathf.Abs(clampedPosition.x - minX) < 0.5f ||
            Mathf.Abs(clampedPosition.x - maxX) < 0.5f ||
            Mathf.Abs(clampedPosition.y - minY) < 0.5f ||
            Mathf.Abs(clampedPosition.y - maxY) < 0.5f;
    }
}
