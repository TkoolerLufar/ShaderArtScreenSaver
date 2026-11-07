// My Shader Art
Shader "Unlit/MyArtShader"
{
    Properties
    {
        [HideInInspector] _MainTex ("(Unused)", 2D) = "black" {}
        _aspectRatio ("Aspect Ratio", float) = 1.
        _lightPower ("Light Power", float) = .002
        [MainColor] _lowColor ("Low-intense Color", Color) = (0., 0., 1., 1.)
        _midColor ("Medium-intense Color", Color) = (0., 1., 0., 1.)
        _hiColor ("High-intense Color", Color) = (1., 0., 0., 1.)
    }

    // It has only one sub shader
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float _aspectRatio;
            float _lightPower;
            fixed4 _lowColor;
            fixed4 _midColor;
            fixed4 _hiColor;

            float sq(float x)
            {
                return x * x;
            }

            float easeInOutCubic(float t)
            {
                return (-2 * t + 3) * t * t;
            }

            // main process to make the art
            fixed4 frag (v2f input) : SV_Target
            {
                float2 p = (input.uv - float2(.5, .5)) * 2.;
                // Fit to shorter bound
                if (_aspectRatio < 1) {
                    p /= _aspectRatio;
                }
                else {
                    p *= _aspectRatio;
                }
                // Use square value instead plain to optimize for performance
                float radiusSq = (sq(p.x) + sq(p.y));
                float angle = atan(p.y / p.x);

                // Decide the intensity
                float f = (abs(sin( 3. * angle + radians((_Time.y - 1.) * 6.))) + 1.) * 32.5 / 100.;
                float g = .65;
                float h = (abs(cos(15. * angle - radians((_Time.y - 1.) * 6.))) * 16. + 65.) / 100.;
                float i = .81;
                fixed intensity = 0;
                if (_Time.y >= 1. || degrees(atan2(p.x, p.y)) + 180. < easeInOutCubic(_Time.y) * 360.) {
                    intensity += _lightPower / abs(g * g - radiusSq) * 1.;
                }
                if (_Time.y >= 1. || degrees(atan2(-p.x, -p.y)) + 180. < easeInOutCubic(_Time.y) * 360.) {
                    intensity += _lightPower / abs(i * i - radiusSq) * 1.;
                }
                if (_Time.y >= 2. || degrees(atan2(-p.x, -p.y)) + 180. < (_Time.y - 1.) * 360.) {
                    intensity += _lightPower / abs(h * h - radiusSq) * 1.;
                }
                if (_Time.y >= 2.) {
                    intensity += _lightPower * easeInOutCubic(smoothstep(2., 3., _Time.y)) / sqrt(abs(f * f - radiusSq)) * 20.;
                }

                // Set the color by the intensity
                return
                    _lowColor * smoothstep(0., 1., intensity) +
                    _midColor * smoothstep(1., 2., intensity) +
                    _hiColor * smoothstep(2., 3., intensity);
            }
            ENDCG
        }
    }
}
