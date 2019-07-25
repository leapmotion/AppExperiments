using UnityEngine;
using UnityEditor;

public class ShapeConfigurationAsset
{
	[MenuItem("Assets/Create/PointCloud/Shape Configuration")]
	public static void CreateAsset ()
	{	
		ShapeConfiguration asset = ScriptableObject.CreateInstance<ShapeConfiguration>();

		ProjectWindowUtil.CreateAsset(asset, "New " + typeof(ShapeConfiguration).Name + ".asset");
	}
}

