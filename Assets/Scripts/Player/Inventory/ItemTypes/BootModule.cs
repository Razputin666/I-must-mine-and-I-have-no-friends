using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootModule : ItemOld
{
    private float jumpMultiplier;

    public BootModule(int id, string title, string description, float jumpMultiplier) : base(id, title, description)
    {
        this.jumpMultiplier = jumpMultiplier;
    }

    public float JumpMultiplier
    {
        get
        {
            return this.jumpMultiplier;
        }
    }
}
