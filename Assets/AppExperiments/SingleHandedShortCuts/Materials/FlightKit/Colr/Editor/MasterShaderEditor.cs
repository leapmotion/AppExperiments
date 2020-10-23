using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;

public enum ColorMode
{
    OFF = 0,
    SOLID = 1,
    GRADIENT = 2
}

public class MasterShaderEditor : MaterialEditor
{
    private static string FRONT_SOLID_ON = "FRONT_SOLID_ON";
    private static string BACK_SOLID_ON = "BACK_SOLID_ON";
    private static string LEFT_SOLID_ON = "LEFT_SOLID_ON";
    private static string RIGHT_SOLID_ON = "RIGHT_SOLID_ON";

    private static string FRONT_GRADIENT_ON = "FRONT_GRADIENT_ON";
    private static string BACK_GRADIENT_ON = "BACK_GRADIENT_ON";
    private static string LEFT_GRADIENT_ON = "LEFT_GRADIENT_ON";
    private static string RIGHT_GRADIENT_ON = "RIGHT_GRADIENT_ON";

    private static string LIGHTMAP_COLR_ON = "LIGHTMAP_COLR_ON";

    private static string FOG_BOTTOM = "FOG_BOTTOM";
    
    private static string INDEPENDENT_SIDES = "INDEPENDENT_SIDES";

    private AnimBool showFront;
    private AnimBool showBack;
    private AnimBool showLeft;
    private AnimBool showRight;

    private ColorMode frontMode;
    private ColorMode backMode;
    private ColorMode leftMode;
    private ColorMode rightMode;
    
    private bool independentSides;
    private bool fogBottom;
    private bool lightmapOn;

    public override void OnEnable()
    {
        base.OnEnable();

        Material targetMat = target as Material;
        string[] keywordsCurrent = targetMat.shaderKeywords;

        // ---- FRONT ----
        bool frontSolidOn = ArrayUtility.Contains(keywordsCurrent, FRONT_SOLID_ON);
        bool frontGradientOn = ArrayUtility.Contains(keywordsCurrent, FRONT_GRADIENT_ON);
        showFront = new AnimBool(frontSolidOn || frontGradientOn);
        showFront.valueChanged.AddListener(Repaint);
        frontMode = frontGradientOn ? ColorMode.GRADIENT : (frontSolidOn ? ColorMode.SOLID : ColorMode.OFF);

        // ---- BACK ----
        bool backSolidOn = ArrayUtility.Contains(keywordsCurrent, BACK_SOLID_ON);
        bool backGradientOn = ArrayUtility.Contains(keywordsCurrent, BACK_GRADIENT_ON);
        showBack = new AnimBool(backSolidOn || backGradientOn);
        showBack.valueChanged.AddListener(Repaint);
        backMode = backGradientOn ? ColorMode.GRADIENT : (backSolidOn ? ColorMode.SOLID : ColorMode.OFF);

        // ---- LEFT ----
        bool leftSolidOn = ArrayUtility.Contains(keywordsCurrent, LEFT_SOLID_ON);
        bool leftGradientOn = ArrayUtility.Contains(keywordsCurrent, LEFT_GRADIENT_ON);
        showLeft = new AnimBool(leftSolidOn || leftGradientOn);
        showLeft.valueChanged.AddListener(Repaint);
        leftMode = leftGradientOn ? ColorMode.GRADIENT : (leftSolidOn ? ColorMode.SOLID : ColorMode.OFF);

        // ---- RIGHT ----
        bool rightSolidOn = ArrayUtility.Contains(keywordsCurrent, RIGHT_SOLID_ON);
        bool rightGradientOn = ArrayUtility.Contains(keywordsCurrent, RIGHT_GRADIENT_ON);
        showRight = new AnimBool(rightSolidOn || rightGradientOn);
        showRight.valueChanged.AddListener(Repaint);
        rightMode = rightGradientOn ? ColorMode.GRADIENT : (rightSolidOn ? ColorMode.SOLID : ColorMode.OFF);

        lightmapOn = ArrayUtility.Contains(keywordsCurrent, LIGHTMAP_COLR_ON);

        independentSides = ArrayUtility.Contains(keywordsCurrent, INDEPENDENT_SIDES);
        fogBottom = ArrayUtility.Contains(keywordsCurrent, FOG_BOTTOM);
        
        // If old version of Colr and local space, replace shader.
        if (!ArrayUtility.Contains(targetMat.shaderKeywords, "COLR_1_2") && 
            !ArrayUtility.Contains(targetMat.shaderKeywords, "WORLD_SPACE_ON"))
        {
            targetMat.shader = Shader.Find("Colr/Master Shader Local Space");
            Repaint();
        }
        
        ApplyProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Material targetMat = target as Material;
        
        // Update inspector if user changed shader.
        if (targetMat.shader.name != "Colr/Master Shader")
        {
            Repaint();
            EditorUtility.SetDirty(target);
            return;
        }

        var shdr = serializedObject.FindProperty("m_Shader");

        if (isVisible && !shdr.hasMultipleDifferentValues && shdr.objectReferenceValue != null)
        {
            string[] keywordsCurrent = targetMat.shaderKeywords;
            EditorGUI.BeginChangeCheck();
            
            if (targets == null || targets.Length == 0)
            {
                return;
            }

            TextureProperty(GetMaterialProperty(targets, "_MainTex"), "Texture");
            EditorGUILayout.Separator();

            ColorProperty(GetMaterialProperty(targets, "_TopColor"), "Top Color");
            ColorProperty(GetMaterialProperty(targets, "_BottomColor"), "Bottom Color");
            EditorGUILayout.Separator();

            // ---- FRONT ----
            frontMode = (ColorMode)EditorGUILayout.EnumPopup("Front color override", frontMode);
            showFront.target = frontMode != ColorMode.OFF;
            using (var group = new EditorGUILayout.FadeGroupScope(showFront.faded))
            {
                if (group.visible)
                {
                    EditorGUI.indentLevel++;

                    if (frontMode == ColorMode.GRADIENT)
                    {
                        ColorProperty(GetMaterialProperty(targets, "_FrontTopColor"), "Front Top Color");
                        ColorProperty(GetMaterialProperty(targets, "_FrontBottomColor"), "Front Bottom Color");
                    }
                    else
                    {
                        ColorProperty(GetMaterialProperty(targets, "_FrontTopColor"), "Color");
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.Separator();

            // ---- BACK ----
            backMode = (ColorMode)EditorGUILayout.EnumPopup("Back color override", backMode);
            showBack.target = backMode != ColorMode.OFF;
            using (var group = new EditorGUILayout.FadeGroupScope(showBack.faded))
            {
                if (group.visible)
                {
                    EditorGUI.indentLevel++;

                    if (backMode == ColorMode.GRADIENT)
                    {
                        ColorProperty(GetMaterialProperty(targets, "_BackTopColor"), "Back Top Color");
                        ColorProperty(GetMaterialProperty(targets, "_BackBottomColor"), "Back Bottom Color");
                    }
                    else
                    {
                        ColorProperty(GetMaterialProperty(targets, "_BackTopColor"), "Color");
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.Separator();

            // ---- LEFT ----
            leftMode = (ColorMode)EditorGUILayout.EnumPopup("Left color override", leftMode);
            showLeft.target = leftMode != ColorMode.OFF;
            using (var group = new EditorGUILayout.FadeGroupScope(showLeft.faded))
            {
                if (group.visible)
                {
                    EditorGUI.indentLevel++;

                    if (leftMode == ColorMode.GRADIENT)
                    {
                        ColorProperty(GetMaterialProperty(targets, "_LeftTopColor"), "Left Top Color");
                        ColorProperty(GetMaterialProperty(targets, "_LeftBottomColor"), "Left Bottom Color");
                    }
                    else
                    {
                        ColorProperty(GetMaterialProperty(targets, "_LeftTopColor"), "Color");
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.Separator();

            // ---- RIGHT ----
            rightMode = (ColorMode)EditorGUILayout.EnumPopup("Right color override", rightMode);
            showRight.target = rightMode != ColorMode.OFF;
            using (var group = new EditorGUILayout.FadeGroupScope(showRight.faded))
            {
                if (group.visible)
                {
                    EditorGUI.indentLevel++;

                    if (rightMode == ColorMode.GRADIENT)
                    {
                        ColorProperty(GetMaterialProperty(targets, "_RightTopColor"), "Right Top Color");
                        ColorProperty(GetMaterialProperty(targets, "_RightBottomColor"), "Right Bottom Color");
                    }
                    else
                    {
                        ColorProperty(GetMaterialProperty(targets, "_RightTopColor"), "Color");
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.Separator();

            // ---- FOG ----
            fogBottom = EditorGUILayout.Toggle("Fade bottom to fog", fogBottom);
            EditorGUILayout.Separator();
            
            // ---- ADDITIVE ----
            if (fogBottom)
            {
                independentSides = true;
            }
            else
            {
                independentSides = EditorGUILayout.Toggle("Do not mix colors of sides", independentSides);
                EditorGUILayout.Separator();
            }
            
            // ---- GRADIENT ----
            if (frontMode != ColorMode.SOLID || backMode != ColorMode.SOLID ||
                leftMode != ColorMode.SOLID || rightMode != ColorMode.SOLID ||
                fogBottom)
            {
                FloatProperty(GetMaterialProperty(targets, "_GradientYStartPos"), "Gradient start Y");
                float h = FloatProperty(GetMaterialProperty(targets, "_GradientHeight"), "Gradient height");
                if (h < 0) {
                    targetMat.SetFloat("_GradientHeight", 0);
                }
                EditorGUILayout.Separator();
            }

            ColorProperty(GetMaterialProperty(targets, "_LightTint"), "Light Color");
            EditorGUILayout.Separator();

            ColorProperty(GetMaterialProperty(targets, "_AmbientColor"), "Ambient Color");
            RangeProperty(GetMaterialProperty(targets, "_AmbientPower"), "Ambient Power");
            EditorGUILayout.Separator();

            lightmapOn = ArrayUtility.Contains(keywordsCurrent, LIGHTMAP_COLR_ON);
            using (var group = new EditorGUILayout.ToggleGroupScope("Enable Lightmap", lightmapOn))
            {
                lightmapOn = group.enabled;
                ColorProperty(GetMaterialProperty(targets, "_LightmapColor"), "Lightmap Color");
                RangeProperty(GetMaterialProperty(targets, "_LightmapPower"), "Lightmap Power");
            }
            EditorGUILayout.Separator();

            RangeProperty(GetMaterialProperty(targets, "_Rotation"), "Gradient Angle");
            GetMaterialProperty(targets, "_Offset").vectorValue = 
                    EditorGUILayout.Vector3Field("Angle Origin", 
                    GetMaterialProperty(targets, "_Offset").vectorValue);
            EditorGUILayout.Separator();

            // If a value changed.
            if (EditorGUI.EndChangeCheck())
            {
                ApplyProperties();
            }
        }
    }

    private void ApplyProperties()
    {
        Material targetMat = target as Material;
        var keywordsUpdated = new List<string>() { "COLR_1_2" };

        switch (frontMode)
        {
            case ColorMode.GRADIENT:
                keywordsUpdated.Add(FRONT_GRADIENT_ON);
                break;
            case ColorMode.SOLID:
                keywordsUpdated.Add(FRONT_SOLID_ON);
                GetMaterialProperty(targets, "_FrontBottomColor").colorValue =
                    GetMaterialProperty(targets, "_FrontTopColor").colorValue;
                break;
            case ColorMode.OFF:
                GetMaterialProperty(targets, "_FrontTopColor").colorValue =
                    GetMaterialProperty(targets, "_TopColor").colorValue;
                GetMaterialProperty(targets, "_FrontBottomColor").colorValue =
                    GetMaterialProperty(targets, "_BottomColor").colorValue;
                break;
        }

        switch (backMode)
        {
            case ColorMode.GRADIENT:
                keywordsUpdated.Add(BACK_GRADIENT_ON);
                break;
            case ColorMode.SOLID:
                keywordsUpdated.Add(BACK_SOLID_ON);
                GetMaterialProperty(targets, "_BackBottomColor").colorValue =
                    GetMaterialProperty(targets, "_BackTopColor").colorValue;
                break;
            case ColorMode.OFF:
                GetMaterialProperty(targets, "_BackTopColor").colorValue =
                    GetMaterialProperty(targets, "_TopColor").colorValue;
                GetMaterialProperty(targets, "_BackBottomColor").colorValue =
                    GetMaterialProperty(targets, "_BottomColor").colorValue;
                break;
        }

        switch (leftMode)
        {
            case ColorMode.GRADIENT:
                keywordsUpdated.Add(LEFT_GRADIENT_ON);
                break;
            case ColorMode.SOLID:
                keywordsUpdated.Add(LEFT_SOLID_ON);
                GetMaterialProperty(targets, "_LeftBottomColor").colorValue =
                    GetMaterialProperty(targets, "_LeftTopColor").colorValue;
                break;
            case ColorMode.OFF:
                GetMaterialProperty(targets, "_LeftTopColor").colorValue =
                    GetMaterialProperty(targets, "_TopColor").colorValue;
                GetMaterialProperty(targets, "_LeftBottomColor").colorValue =
                    GetMaterialProperty(targets, "_BottomColor").colorValue;
                break;
        }

        switch (rightMode)
        {
            case ColorMode.GRADIENT:
                keywordsUpdated.Add(RIGHT_GRADIENT_ON);
                break;
            case ColorMode.SOLID:
                keywordsUpdated.Add(RIGHT_SOLID_ON);
                GetMaterialProperty(targets, "_RightBottomColor").colorValue =
                    GetMaterialProperty(targets, "_RightTopColor").colorValue;
                break;
            case ColorMode.OFF:
                GetMaterialProperty(targets, "_RightTopColor").colorValue =
                    GetMaterialProperty(targets, "_TopColor").colorValue;
                GetMaterialProperty(targets, "_RightBottomColor").colorValue =
                    GetMaterialProperty(targets, "_BottomColor").colorValue;
                break;
        }

        if (lightmapOn)
        {
            keywordsUpdated.Add(LIGHTMAP_COLR_ON);
        }
        
        // Turn off independentSides when user turns off fog.
        if (!fogBottom && ArrayUtility.Contains(targetMat.shaderKeywords, FOG_BOTTOM))
        {
            independentSides = false;
        }
        
        if (independentSides)
        {
            keywordsUpdated.Add(INDEPENDENT_SIDES);
        }

        if (fogBottom)
        {
            keywordsUpdated.Add(FOG_BOTTOM);
        }
        
        targetMat.shaderKeywords = keywordsUpdated.ToArray();
        EditorUtility.SetDirty(targetMat);
        
    }

    public void OnInspectorUpdate() {
        this.Repaint();
    }
}
