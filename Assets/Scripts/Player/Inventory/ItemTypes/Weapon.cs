using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    private Dictionary<string, int> stats;

    public Weapon(int id, string title, string description, Dictionary<string, int> stats) : base(id, title, description)
    {
        this.stats = stats;
    }

    public Weapon(Weapon weapon) : base(weapon)
    {
        this.stats = weapon.stats;
    }

    public Dictionary<string, int> Stats
    {
        get
        {
            return this.stats;
        }
        set
        {
            this.stats = value;
        }
    }
}
