using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    private Text tooltipText;

    // Start is called before the first frame update
    void Start()
    {
        tooltipText = GetComponentInChildren<Text>();
        gameObject.SetActive(false);
    }

    public void GenerateTooltip(ItemOld item)
    {
        string statText = "";
        if(item.GetType() == typeof(Weapon))
        {
            Weapon weapon = (Weapon)item;
            //Check if there are stats on the item
            if(weapon.Stats.Count > 0)
            {
                foreach(KeyValuePair<string, int> stat in weapon.Stats)
                {
                    statText += stat.Key.ToString() + ": " + stat.Value.ToString() + "\n";
                }
            }
            //Format the text to bold
            string tooltip = item.Title + "\n" + item.Description + "\n\n" + statText;

            tooltipText.text = tooltip;
            gameObject.SetActive(true);
        }
    }
}
