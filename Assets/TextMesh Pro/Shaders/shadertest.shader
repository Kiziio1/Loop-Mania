Shader "Custom/BalatroBackground"
{
    Properties
    {
        _Color1 ("Color Interior / Lineas", Color) = (0.62, 0.17, 0.42, 1.0) // Magenta
        _Color2 ("Color Fondo / Exterior", Color) = (0.11, 0.04, 0.17, 1.0) // Morado oscuro
        _Speed ("Velocidad de Expansion", Float) = 0.5
        _RingsDensity ("Cantidad de Anillos", Float) = 8.0
        _CenterDistort ("Deformacion Sutil", Range(0.0, 0.5)) = 0.08
        _Sharpness ("Nitidez de Borde", Range(0.01, 0.5)) = 0.15
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color1;
            float4 _Color2;
            float _Speed;
            float _RingsDensity;
            float _CenterDistort;
            float _Sharpness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Centrar coordenadas en el medio de la pantalla (0,0)
                float2 uv = i.uv - 0.5;

                // Deformación ondulada muy suave (para no perder el toque líquido sin ser caótico)
                float t = _Time.y * _Speed;
                float angle = atan2(uv.y, uv.x);
                float deform = sin(angle * 4.0 + t) * _CenterDistort;

                // Distancia radial al centro más el desplazamiento temporal
                float dist = length(uv) + deform;

                // Onda concéntrica expandiéndose hacia afuera
                float wave = sin((dist * _RingsDensity * 6.28318) - (t * 2.0));

                // Normalizar la onda a rango [0, 1]
                float mask = wave * 0.5 + 0.5;

                // Suavizar o endurecer la transición entre colores
                mask = smoothstep(0.5 - _Sharpness, 0.5 + _Sharpness, mask);

                // Mezclar la paleta
                float3 finalColor = lerp(_Color2.rgb, _Color1.rgb, mask);

                return float4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}