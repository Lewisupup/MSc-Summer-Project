using System;
using System.Collections.Generic;

[Serializable]
public class RadialBurstConfig
{
    public int bulletCount;
    public float[] speeds;
    public float[] angles;
    public float damage;
    public float cooldown;
}

[Serializable]
public class RadialBurstModeData
{
    public List<ModeEntry> modes;
}

[Serializable]
public class ModeEntry
{
    public string key;
    public RadialBurstConfig value;
}
