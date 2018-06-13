Shader "Portal/PortalSurfaceGrid"
{
	Properties
	{
    _PortalMask("Portal Mask", Float) = 0
    _Color ("Color", Color) = (1, 1, 1, 1)
    _GridSizeAndRowColCount ("Grid Size And Row Col Count", Vector) = (1, 1, 3, 3)
    _OffsetAndPopState ("Offset 2D (XY), PopState (Z), LerpedPopState(W)", Vector) = (0, 0, 0, 0)
    _SurfaceGlowOffset ("Offset For Fingertip Glow", Vector) = (0, 0, 0, 0)
    _PopStateBackup ("Pop State Backup", Vector) = (0, 0, 0, 0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "DisableBatching"="True" }
		LOD 100

		Stencil{
			Ref[_PortalMask]
			ReadMask 3
			Comp equal
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
      #include "Assets/AppModules/TodoUMward/Shader Hand Data/Resources/HandData.cginc"

      // Vert / Frag Structs

			struct appdata
			{
				float4 vertex : POSITION; 
				float2 uv : TEXCOORD0;
        float id : TEXCOORD1;
        float4 blendshapeDelta : TEXCOORD2;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
        //uint id : TEXCOORD1; // ID debug
			};

      // Public Material Properties
			float4 _Color;
      float4 _GridSizeAndRowColCount;
      float4 _OffsetAndPopState;
      float4 _SurfaceGlowOffset;
      float4 _PopStateBackup;

      // Hidden Material Properties
      //float4x4 _WorldToObjectMatrix;

      // plane displacement stuff, not even started really
      //float3 Leap_FingertipsDepthInPlane_AssumeHandsInLocalPlaneSpace() {
      //  // Find the position that offers the greatest finger displacement.
      //  //float3 fingertipPos = float3(0, 0, 0);
      //  //float 
      //  //for (int i = 0; i < 5; i++) {
      //  //  _Leap_LH_Fingertips[i]
      //  //}
      //
      //}
      // Assumes fingertips can be retrieved in LOCAL space relative to the plane-- that is,
      // that the fingertips' planar projected position is their X and Y coordinates, and their
      // depth from the plane is their distance along the plane normal.
      //float3 getDistortionFromFingertips(float4 vertex) {
      //  // Closest fingertip to plane.
      //  float3 closestFingertip = Leap_ClosestFingertipToPlane_AssumeHandsInLocalSpace();
      //}

			v2f vert (appdata v)
			{
        uint numRows = (uint)(_GridSizeAndRowColCount.z + 0.1),
             numCols = (uint)(_GridSizeAndRowColCount.w + 0.1);

        float gridW = _GridSizeAndRowColCount.x,
              gridH = _GridSizeAndRowColCount.y;
        float halfW = gridW / 2.0,
              halfH = gridH / 2.0;
        float cellW = gridW / numCols,
              cellH = gridH / numRows;
        float halfCW = cellW / 2.0,
              halfCH = cellH / 2.0;

        // Get expected grid cell based on ID.
        // Grid placement is left-to-right, then top-to-bottom.
        uint id = (uint)(v.id.x + 0.1);
        uint gridX = id % numCols;
        uint gridY = id / numCols;

        // Get theoretical grid position to be clamped.
        float x = cellW * gridX;
        float y = cellH * gridY;

        // Remember original offset of input vertex from its grid point.
        float gridToVertXOffset = v.vertex.x - x;
        float gridToVertYOffset = v.vertex.y - (-y); // -y cheat makes X/Y modulus logic identical

        // Offset vertex position.
        x += _OffsetAndPopState.x;
        y -= _OffsetAndPopState.y;
        
        {
          // Cell offset for wrapping.
          x += halfCW;
          y += halfCH;
        
          {
            // Flip handling when offsets are negative.
            int flipX = 1, flipY = 1;
            if (x < 0) {
              x = -x;
              flipX = -1;
            }
            if (y < 0) {
              y = -y;
              flipY = -1;
            }

            {
              // Apply X/Y wrapping
              x = x % gridW;
              y = y % gridH;
            }

            // Undo flip state, and if we were flipped, add an offset
            if (flipX < 0) {
              x -= gridW;
              x = flipX * x;
            }
            if (flipY < 0) {
              y -= gridH;
              y = flipY * y;
            }
          }
        
          // Undo cell offset.
          x -= halfCW;
          y -= halfCH;
        }

        // Convert final grid point back into final vertex position.
        v.vertex.x = x + gridToVertXOffset;
        v.vertex.y = (-y) + gridToVertYOffset; // see: -y cheat above


        // Apply blendshape based on distance from fingertips.

        // First, set preprocess matrix for hand data (fingertips in this case) to convert them
        // from world space to the space of this object.
        Leap_HandData_Preprocess_Matrix = unity_WorldToObject;

        float sqrDist = Leap_SqrDistToFingertips_WithScale(v.vertex + _SurfaceGlowOffset, float3(0.5, 0.5, 2));
        
        float pierceDisplacement = -0.03;
        float distortionSpreadAmount = 0.26;

        // Displacement _sideways_ experiment. Problems when this is activated are pretty tricky to get around.
        //float maxSidewaysDisplacementDepth = -0.10;
        //float maxNormalwiseDisplacementDistance = 10;
        //
        //float4 sideOffsetX = float4(0.005, 0, 0, 0), sideOffsetY = float4(0, 0.005, 0, 0);
        //float pushPlaneDisplacementX0 = getMinDisplacement(float3(0, 0, -1), v.vertex + _SurfaceGlowOffset - sideOffsetX,
        //                                                   0.0, 0.32);
        //float pushPlaneDisplacementX1 = getMinDisplacement(float3(0, 0, -1), v.vertex + _SurfaceGlowOffset + sideOffsetX,
        //                                                   0.0, 0.32);
        //float pushPlaneDisplacementY0 = getMinDisplacement(float3(0, 0, -1), v.vertex + _SurfaceGlowOffset - sideOffsetY,
        //                                                   0.0, 0.32);
        //float pushPlaneDisplacementY1 = getMinDisplacement(float3(0, 0, -1), v.vertex + _SurfaceGlowOffset + sideOffsetY,
        //                                                   0.0, 0.32);
        //float displacementX = pushPlaneDisplacementX1 - pushPlaneDisplacementX0;
        //float displacementY = pushPlaneDisplacementY1 - pushPlaneDisplacementY0;
        //float3 displacementNormal = float3(displacementX, displacementY, 0);
        //
        //float normalwiseDisplacementAmount = 5;
        //float4 sidewaysDisplace = float4(displacementNormal.x * normalwiseDisplacementAmount,
        //                                 displacementNormal.y * normalwiseDisplacementAmount,
        //                                 0, 0);

        
        float4 offsetFromPierceDisplacement_cheatMatchGlowDepth = float4(0, 0, pierceDisplacement, 0);
        //v.vertex += offsetFromPierceDisplacement_cheatMatchGlowDepth;

        // Distort vertices based on finger position on the plane.
        //float lerpedPopState = _OffsetAndPopState.z; 
        float lerpedPopState = _OffsetAndPopState.z;
        distortionSpreadAmount = 0.26 * (lerpedPopState * 5.0 + 1.0);
        float pushPlaneDisplacement = getMinDisplacement(float3(0, 0, -1), v.vertex + _SurfaceGlowOffset - 0.02,
                                                         0.0, distortionSpreadAmount);

        
        float4 gridPoint = float4(x + halfCW, -y - halfCH, 0, 0)
                           + _SurfaceGlowOffset;
                           //+ offsetFromPierceDisplacement_cheatMatchGlowDepth;

        gridPoint = v.vertex - float4(gridToVertXOffset, gridToVertYOffset, 0, 0)
                             - float4(halfW, -halfH, 0, 0)
                             + float4(halfCW, -halfCH, 0, 0);

        float gridPointDisplacement = getMinDisplacement(float3(0, 0, -1), gridPoint, 0.0, distortionSpreadAmount);
        float4 displacedGridPoint = gridPoint + float4(0, 0, -1, 1) * gridPointDisplacement;

        // Sideways distortion is pretty cool with some vanishing in there.
        //int vanish = 0;
        //if (pushPlaneDisplacement < pierceDisplacement) {
        //  vanish = 1;
        //}
        float vanish = Leap_Map(pushPlaneDisplacement, pierceDisplacement - 0.01, pierceDisplacement + 0.01, 1, 0);
        float gridPointVanish = Leap_Map(gridPointDisplacement, pierceDisplacement - 0.01, pierceDisplacement + 0.01, 1, 0);

        //float sidewaysDisplaceAmount = Leap_Map(vanish, 0.5, 0.8, 0, 1);
        //float gridSidewaysDisplaceAmount = Leap_Map(gridPointVanish, 0.5, 0.8, 0, 1);
        //v.vertex           += sidewaysDisplace * sidewaysDisplaceAmount;
        //displacedGridPoint += sidewaysDisplace * gridSidewaysDisplaceAmount;

        pushPlaneDisplacement = clamp(pushPlaneDisplacement, pierceDisplacement, 0);

        //if (_OffsetAndPopState.z >= 0.9) {
        //  pushPlaneDisplacement = 0;
        //  vanish = 0;
        //}

        //vanish = max(vanish, lerp(vanish, 0.9, lerpedPopState));

        v.vertex += float4(0, 0, -1, 1) * pushPlaneDisplacement;
        //displacedGridPoint += float4(0, 0, -1, 1) * pushPlaneDisplacement;

        // Finally, apply blendshape, suppressed 
        float maxRange = 0.04;
        float mapping = Leap_Map(sqrDist, 0, maxRange * maxRange, 0, 1);
        v.vertex = v.vertex + (v.blendshapeDelta * mapping);

        //if (vanish == 1) {
        //  v.vertex = float4(v.vertex.x, v.vertex.y, v.vertex.z + 1000, 1.0);
        //}
        v.vertex = lerp(v.vertex, displacedGridPoint, vanish);

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        //o.id = id; // ID debug
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
        fixed4 color = _Color;

        // Color-based ID debug
        //if (i.id / 3 == 0) {
        //  color = fixed4(1, 0, 0, 1);
        //  
        //  if (i.id % 3 == 0) {
        //    color = fixed4(1, 0, 0, 1);
        //  }
        //  if (i.id % 3 == 1) {
        //    color = fixed4(0.66, 0, 0, 1);
        //  }
        //  if (i.id % 3 == 2) {
        //    color = fixed4(0.33, 0, 0, 1);
        //  }
        //}
        //if (i.id / 3 == 1) {
        //  color = fixed4(0, 1, 0, 1);
        //  
        //  if (i.id % 3 == 0) {
        //    color = fixed4(0, 1, 0, 1);
        //  }
        //  if (i.id % 3 == 1) {
        //    color = fixed4(0, 0.66, 0, 1);
        //  }
        //  if (i.id % 3 == 2) {
        //    color = fixed4(0, 0.33, 0, 1);
        //  }
        //}
        //if (i.id / 3 == 2) {
        //  color = fixed4(0, 0, 1, 1);
        //  
        //  if (i.id % 3 == 0) {
        //    color = fixed4(0, 0, 1, 1);
        //  }
        //  if (i.id % 3 == 1) {
        //    color = fixed4(0, 0, 0.66, 1);
        //  }
        //  if (i.id % 3 == 2) {
        //    color = fixed4(0, 0, 0.33, 1);
        //  }
        //}

				return color;
			}
			ENDCG
		}
	}
}
