//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class TileFab : MonoBehaviour
//{
//    public static Sprite spriteDefault;
//    public static Sprite sprite1;
//    public static Sprite sprite2;
//    public static Sprite sprite3;

//    private void Start()
//    {
//        CreateRuleTile();
//    }
//    static void CreateRuleTile()
//    {
//        RuleTile ruleTile = ScriptableObject.CreateInstance("RuleTile") as RuleTile;

//        AssetDatabase.CreateAsset(ruleTile, "Assets/" + UnityEngine.Random.Range(0, 999).ToString() + "MyRuleTile.asset");

//        Debug.Log(AssetDatabase.GetAssetPath(ruleTile));
//        ruleTile.m_DefaultSprite = spriteDefault;

//        RuleTile.TilingRule rule01 = new RuleTile.TilingRule();
//        rule01.m_Sprites[0] = sprite1;

//        RuleTile.TilingRule rule02 = new RuleTile.TilingRule();
//        rule02.m_Sprites[0] = sprite2;

//        RuleTile.TilingRule rule03 = new RuleTile.TilingRule();
//        rule03.m_Sprites[0] = sprite3;


//        ruleTile.m_TilingRules.Add(rule01);
//        ruleTile.m_TilingRules.Add(rule02);
//        ruleTile.m_TilingRules.Add(rule03);

//    }
//}