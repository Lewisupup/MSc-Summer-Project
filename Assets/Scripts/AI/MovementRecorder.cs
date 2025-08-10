using System.Collections.Generic;
using UnityEngine;

public class SimpleMovementRecorder : MonoBehaviour
{
    public float sampleRateHz = 12f; // how many samples per second
    private List<string> movementData = new List<string>();
    private float timer;
    private bool recording;

    public void BeginRound()
    {
        movementData.Clear();
        timer = 0f;
        recording = true;
    }

    public List<string> EndRound()
    {
        recording = false;
        return new List<string>(movementData);
    }

    void Update()
    {
        if (!recording) return;

        timer += Time.deltaTime;
        if (timer >= 1f / sampleRateHz)
        {
            timer = 0f;
            movementData.Add(ReadDirectionToken());
        }
    }

    string ReadDirectionToken()
    {
        bool up = Input.GetKey(KeyCode.UpArrow)   || Input.GetKey(KeyCode.W);
        bool down = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        bool left = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.RightArrow)|| Input.GetKey(KeyCode.D);

        int vy = (up ? 1 : 0) - (down ? 1 : 0);
        int vx = (right ? 1 : 0) - (left ? 1 : 0);

        if (vx == 0 && vy == 0) return "s";     // still
        if (vx == 0 && vy > 0)  return "u";
        if (vx == 0 && vy < 0)  return "d";
        if (vx > 0 && vy == 0)  return "r";
        if (vx < 0 && vy == 0)  return "l";
        if (vx > 0 && vy > 0)   return "ur";
        if (vx < 0 && vy > 0)   return "ul";
        if (vx > 0 && vy < 0)   return "dr";
        if (vx < 0 && vy < 0)   return "dl";

        return "s";
    }

}
