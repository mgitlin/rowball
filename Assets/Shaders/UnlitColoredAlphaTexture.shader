//UnlitColoredAlphaTexture.shader
//Created by Aaron C Gaudette on 18.03.16
//Renders an unlit colored texture

Shader "Custom/Unlit Colored Texture (Transparent)"{
	Properties{
		_Color("Color",Color) = (1.0,1.0,1.0,1.0)
		_MainTex("Texture",2D) = "white"{}
	}
	SubShader{
        Tags{"Queue"="Transparent" "RenderType"="Transparent"}
        
        ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		
        Pass{
            Lighting Off
            SetTexture[_MainTex]{
            	constantColor [_Color]
            	combine constant*texture
            }
        }
    }
}