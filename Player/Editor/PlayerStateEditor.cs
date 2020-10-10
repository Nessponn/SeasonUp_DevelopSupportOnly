using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PlayerStatusController))]

public class PlayerStateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerStatusController PSC = target as PlayerStatusController;
        //obj.freq = EditorGUILayout.Slider("Frequency", obj.freq, 0.0f, 10.0f);
        //EditorUtility.SetDirty(target);
        /*
        obj.foldOut = EditorGUILayout.Foldout(obj.foldOut, "FoldOut");
        if (obj.foldOut == true)
        {
            obj.boundField = EditorGUILayout.BoundsField("BoundsField", obj.boundField);
            obj.color = EditorGUILayout.ColorField("Color", obj.color);
            obj.animationCurve = EditorGUILayout.CurveField("CurveField", obj.animationCurve);
            obj.enumMask = (ObjectGenerator.EnumHoge)EditorGUILayout.EnumMaskField("EnumMaskField", obj.enumMask);
            //obj.enumMask2 = (ObjectGenerator.EnumHoge)EditorGUILayout.EnumPopup("EnumPopup", obj.enumMask2);
            //obj.floatField = EditorGUILayout.FloatField("FloatField", obj.floatField);
        }
        */
        PSC.ButtonfoldOut = EditorGUILayout.Foldout(PSC.ButtonfoldOut, "ボタンオブジェクト");
        if (PSC.ButtonfoldOut)
        {
            //ボタンのインスペクター
            PSC.BlueButton = EditorGUILayout.ObjectField("青ボタン",PSC.BlueButton,typeof(GameObject),true)as GameObject;
            PSC.GreenButton = EditorGUILayout.ObjectField("黄色ボタン", PSC.GreenButton, typeof(GameObject), true) as GameObject;
            PSC.YellowButton = EditorGUILayout.ObjectField("緑ボタン", PSC.YellowButton, typeof(GameObject), true) as GameObject;
            PSC.RedButton = EditorGUILayout.ObjectField("赤ボタン", PSC.RedButton, typeof(GameObject), true) as GameObject;

            //PSC.Lifestate = (PlayerStatusController.LifeState)EditorGUILayout.EnumMaskField("ライフの状態", PSC.Lifestate);
        }

        PSC.StatusfoldOut = EditorGUILayout.Foldout(PSC.StatusfoldOut, "ライフステータス変更");
        if (PSC.StatusfoldOut)
        {
            PSC.Blue = EditorGUILayout.Toggle("青、ライフ４、全快", !PSC.Green && !PSC.Yellow && !PSC.Red && !PSC.NoLife);
            PSC.Green = EditorGUILayout.Toggle("緑、ライフ３、軽傷", !PSC.Blue && !PSC.Yellow && !PSC.Red && !PSC.NoLife);
            PSC.Yellow = EditorGUILayout.Toggle("黄色、ライフ２、重症", !PSC.Green && !PSC.Blue && !PSC.Red && !PSC.NoLife);
            PSC.Red = EditorGUILayout.Toggle("赤、ライフ１、瀕死", !PSC.Green && !PSC.Yellow && !PSC.Blue && !PSC.NoLife);
            PSC.NoLife = EditorGUILayout.Toggle("無、ライフ０、リタイア", !PSC.Green && !PSC.Yellow && !PSC.Red && !PSC.Blue);
        }
    }
}
