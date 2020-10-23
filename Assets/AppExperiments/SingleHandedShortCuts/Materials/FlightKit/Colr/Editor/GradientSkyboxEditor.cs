using UnityEngine;
using UnityEditor;

public class GradientSkyboxEditor : MaterialEditor {

    public override void OnInspectorGUI() {
        serializedObject.Update();

		var theShader = serializedObject.FindProperty ("m_Shader"); 

		if (isVisible && !theShader.hasMultipleDifferentValues && theShader.objectReferenceValue != null) {
            EditorGUI.BeginChangeCheck();

			base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck()) {
				var dirPitch = GetMaterialProperty(targets, "_DirectionPitch");
				var dirYaw = GetMaterialProperty(targets, "_DirectionYaw");

                var dirPitchRad = dirPitch.floatValue * Mathf.Deg2Rad;
                var dirYawRad = dirYaw.floatValue * Mathf.Deg2Rad;
                
                var direction = new Vector4(Mathf.Sin(dirPitchRad) * Mathf.Sin(dirYawRad), Mathf.Cos(dirPitchRad), 
				                            Mathf.Sin(dirPitchRad) * Mathf.Cos(dirYawRad), 0.0f);
                GetMaterialProperty(targets, "_Direction").vectorValue = direction;

                PropertiesChanged();
            }
        }
    }

}
