#define TARGET_ITER 3
#define TOP_LEVEL_RAD 90
#define WINDOW_WIDTH 320
#define WINDOW_HEIGHT 180
#define CENTER_X 320 / 2
#define CENTER_Y 180 / 2
#define OFFSET_FACTOR 1.618f
#define SHRINK_FACTOR 2.618f
#define SKID_SWAP_FREQ 2
#define TAU 6.283

uniform float4x4 World;
uniform float theta;
uniform float skid;
uniform float zoom;

float snoise(float p) {
  return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float getJumps(float id, float iter) {
  return step(1, fmod(ceil(id / pow(6, 0)), 6)) * step(0, iter) +
         step(1, fmod(ceil(id / pow(6, 1)), 6)) * step(1, iter) +
         step(1, fmod(ceil(id / pow(6, 2)), 6)) * step(2, iter);
        //  step(1, fmod(ceil(id / pow(6, 3)), 6)) * step(3, iter) +
        //  step(1, fmod(ceil(id / pow(6, 4)), 6)) * step(4, iter);
        //  step(1, fmod(ceil(id / pow(6, 5)), 6)) * step(5, iter) +
        //  step(1, fmod(ceil(id / pow(6, 6)), 6)) * step(6, iter);
}

float4 SpritePixelShader(float polyId : TEXCOORD0, float index : TEXCOORD1,
                         float4 position : SV_Position)
    : COLOR0 {
//   return float4(snoise(polyId), snoise(polyId), 0.5, 1.0);

//   return float4(1.0, 1.0, 0.5, 1.0);

    return lerp(float4(0, 0.25, 0.25, 0.25), float4(0.25, 0, 0.25, 0.25), getJumps(polyId, TARGET_ITER - 1) / TARGET_ITER);
}

void SpriteVertexShader(inout float polyId : TEXCOORD0,
                        inout float index : TEXCOORD1,
                        inout float4 position : SV_Position) {
  float polyRadius = TOP_LEVEL_RAD / pow(SHRINK_FACTOR, TARGET_ITER) * zoom;
  position = float4(CENTER_X, CENTER_Y, 0, 1);

  // ok, just do one iteration. until it works.

  //   float jumps = getJumps(polyId, i);
  for (float i = 0; i < TARGET_ITER; i += 1) {
    float acc = ceil(polyId / pow(6, i));
    float onesDigit = fmod(acc, 6);
    // float jumps = step(1, fmod(ceil(polyId / pow(6, 0)), 6)) * step(0, i);
    float jumps = getJumps(polyId, i);
    float angle = TAU * onesDigit * 0.2 + TAU * 0.1 * fmod(jumps + i, 2);
    float d = TOP_LEVEL_RAD / pow(SHRINK_FACTOR, i + 1) * OFFSET_FACTOR;
    float neighborOffset = (step(1, fmod(i, SKID_SWAP_FREQ)) - 0.5) * 2;
    float neighborDigit = fmod(5 + onesDigit + neighborOffset, 5);
    float neighborAngle =
        TAU * neighborDigit * 0.2 + TAU * 0.1 * fmod(jumps + i, 2);

    position.xy += lerp(float2(cos(angle), sin(angle)),
                        float2(cos(neighborAngle), sin(neighborAngle)), skid) *
                   d * step(1, onesDigit);
  }
//   {
//     float i = 1;
//     float acc = ceil(polyId / pow(6, i));
//     float onesDigit = fmod(acc, 6);
//     // float jumps = step(1, fmod(ceil(polyId / pow(6, 0)), 6)) * step(0, i);
//     float jumps = getJumps(polyId, i);
//     float angle = TAU * onesDigit * 0.2 + TAU * 0.1 * fmod(jumps + i, 2);
//     float d = TOP_LEVEL_RAD / pow(SHRINK_FACTOR, i + 1) * OFFSET_FACTOR;
//     float neighborOffset = (step(1, fmod(i, SKID_SWAP_FREQ)) - 0.5) * 2;
//     float neighborDigit = fmod(5 + onesDigit + neighborOffset, 5);
//     float neighborAngle =
//         TAU * neighborDigit * 0.2 + TAU * 0.1 * fmod(jumps + i, 2);

//     position.xy += lerp(float2(cos(angle), sin(angle)),
//                         float2(cos(neighborAngle), sin(neighborAngle)), skid) *
//                    d * step(1, onesDigit);
//   }

  float indexAngle = TAU * 0.2 * index + TAU;
  indexAngle += TAU * 0.1 * fmod(1 + TARGET_ITER + getJumps(polyId, TARGET_ITER - 1), 2);
  indexAngle -= theta * 2;

  position.xy -= float2(CENTER_X, CENTER_Y);
  position.xy = mul(position.xy, float2x2(cos(theta), sin(-theta), sin(theta), cos(theta)));
  position.xy *= zoom;
  position.xy += float2(CENTER_X, CENTER_Y);

  position.xy += float2(cos(indexAngle + theta), sin(indexAngle + theta)) * polyRadius;
  // position.xy += float2(cos(indexAngle), sin(indexAngle)) * polyRadius;

  //   position.x += polyId;

  position = mul(position, World);
}

technique Shader {
  pass pass0 {
    VertexShader = compile vs_3_0 SpriteVertexShader();
    PixelShader = compile ps_3_0 SpritePixelShader();
  }
}
