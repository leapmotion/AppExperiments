using UnityEngine;

public class ShapeConfiguration : ScriptableObject
{
	[System.Serializable]
	public enum Shape
	{
		Sphere,
		Cube,			
		FFT,
		Path
	}

	[Header("Shape Configuration")]
	public Shape 				shape;
	public Color 				color;
	public bool 				fill;
	public bool 				grid;
	public int 					point_count;
	public float 				point_size;
	public float 				curvature;
	public float 				density;
  public bool         hand_interaction;
  public Vector4[] 		control_point;		

	public ShapeConfiguration()
	{
		shape 					  = Shape.Sphere; 		
		color 					  = Color.white;	
		point_size				= 1.0f;
		point_count 			= 65536;
		fill 					    = false;
		curvature 				= 0.01f;
		density 				  = 1.0f;
    hand_interaction  = false;
    control_point 		= new Vector4[8];
		for(int i = 0; i < control_point.Length;i++)
		{
			control_point[i]	= new Vector3(0.0f, 0.0f, (float)i);
		}
	}
}
