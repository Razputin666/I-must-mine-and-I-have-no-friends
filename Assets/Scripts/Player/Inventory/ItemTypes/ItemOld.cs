using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemOld
{
    protected int id;
    protected string title;
    protected string description;
    protected Sprite icon;

    public ItemOld(int id, string title, string description)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.icon = Resources.Load<Sprite>("Sprites/Items/" + title);
    }

    public ItemOld(ItemOld item)
    {
        this.id = item.id;
        this.title = item.title;
        this.description = item.description;
        this.icon = item.icon;
    }

    public int ID
    {
        get 
        {
            return this.id; 
        }
        set
        {
            this.id = value;
        }
    }
    public string Title
    {
        get
        {
            return this.title;
        }
        set
        {
            this.title = value;
        }
    }
    public string Description
    {
        get
        {
            return this.description;
        }
        set
        {
            this.description = value;
        }
    }
    public Sprite Icon
    {
        get
        {
            return this.icon;
        }
        set
        {
            this.icon = value;
        }
    }
}
