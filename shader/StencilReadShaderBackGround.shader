Shader "Unlit/StencilReadShaderBackGround"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    _MainColor("_MainColor",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull off

        Pass
        {
            Stencil
            {
                Ref 2
                Comp Equal
            }

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            uniform fixed4 _MainColor;
            fixed4 frag(v2f_img i) : SV_Target
            {
               fixed4 col = tex2D(_MainTex, i.uv);
               return fixed4(col.r * _MainColor.r, col.g * _MainColor.g, col.b * _MainColor.b, col.a * _MainColor.a);
            }
            
            ENDCG
        }

        Pass
        {
            Stencil
            {
                Ref 0
                Comp Equal
            }

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            uniform fixed4 _MainColor;
            fixed4 frag(v2f_img i) : SV_Target
             {
                 fixed4 col = tex2D(_MainTex, i.uv);
                 return fixed4(col.r, col.g, col.b, 0);
             }
            ENDCG
        }
    }
}
