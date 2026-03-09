using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 柠檬 Buff 视觉控制器：角色周围上升的黄色箭头粒子 + 角色淡黄色染色。
    /// 响应式监听 Global.LemonDamageBuffBonus，buff 生效时自动激活，到期后自动关闭。
    /// 由 Player.Start() 动态挂载到玩家对象上。
    /// </summary>
    public class LemonBuffVisualController : MonoBehaviour
    {
        // ── 颜色常量 ──
        internal static readonly Color LemonTintColor = new Color(1f, 0.95f, 0.5f, 1f);
        private const float TintBlend = 0.18f; // 与 base color 混合比例

        // ── 粒子相关 ──
        private GameObject _vfxGo;
        private ParticleSystem _ps;
        private ParticleSystemRenderer _psRenderer;
        private bool _isActive;

        // ── 静态缓存（全局共用一份材质和纹理） ──
        private static Material sCachedMat;
        private static Texture2D sCachedTex;

        #region 生命周期

        void Start()
        {
            BuildParticleSystem();

            Global.LemonDamageBuffBonus.RegisterWithInitValue(bonus =>
            {
                if (bonus > 0f)
                    Activate();
                else
                    Deactivate();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnDestroy()
        {
            Deactivate();
            if (_vfxGo) Destroy(_vfxGo);
        }

        #endregion

        #region 激活 / 关闭

        private void Activate()
        {
            if (_isActive) return;
            _isActive = true;

            if (_ps && !_ps.isPlaying)
                _ps.Play(true);

            // 通知 Player 开启染色
            if (Player.Default)
                Player.Default.LemonTintActive = true;
        }

        private void Deactivate()
        {
            if (!_isActive) return;
            _isActive = false;

            // 停止发射，让已有粒子自然消散
            if (_ps && _ps.isPlaying)
                _ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            if (Player.Default)
                Player.Default.LemonTintActive = false;
        }

        #endregion

        #region 粒子系统构建（纯代码，无预制体依赖）

        private void BuildParticleSystem()
        {
            _vfxGo = new GameObject("LemonBuffArrows");
            _vfxGo.transform.SetParent(transform, false);
            _vfxGo.transform.localPosition = Vector3.zero;

            _ps = _vfxGo.AddComponent<ParticleSystem>();
            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // 关闭默认自动播放
            var main = _ps.main;
            main.playOnAwake = false;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 2.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.28f);
            main.startColor = new Color(1f, 0.92f, 0.23f, 0.9f); // 柠檬金黄
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 14;
            main.gravityModifier = 0f;
            main.startRotation = 0f; // 箭头保持朝上

            // ── 发射 ──
            var emission = _ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 7f;

            // ── 形状：角色周围的圆形 ──
            var shape = _ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.35f;
            shape.radiusThickness = 0.85f;
            shape.rotation = new Vector3(-90f, 0f, 0f); // 粒子朝上发射

            // ── 速度：确保向上 + 轻微水平抖动 ──
            var vel = _ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            vel.y = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            // ── 尺寸随生命缩小 ──
            var sol = _ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            // ── 颜色随生命渐隐 ──
            var col = _ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0.7f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            // ── 渲染 ──
            _psRenderer = _vfxGo.GetComponent<ParticleSystemRenderer>();
            _psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            _psRenderer.material = GetOrCreateMaterial();
            _psRenderer.sortingOrder = 100; // 确保在角色上方
        }

        #endregion

        #region 材质与纹理（静态缓存，程序化生成）

        private static Material GetOrCreateMaterial()
        {
            if (sCachedMat) return sCachedMat;

            // 优先使用 URP 粒子 shader
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (!shader) shader = Shader.Find("Particles/Standard Unlit");
            if (!shader) shader = Shader.Find("Sprites/Default");

            sCachedMat = new Material(shader)
            {
                name = "LemonBuffArrowMat"
            };

            // 设置纹理
            var tex = GetOrCreateArrowTexture();
            sCachedMat.mainTexture = tex;
            if (sCachedMat.HasProperty("_BaseMap")) sCachedMat.SetTexture("_BaseMap", tex);
            if (sCachedMat.HasProperty("_BaseColor")) sCachedMat.SetColor("_BaseColor", Color.white);

            // 透明混合
            if (sCachedMat.HasProperty("_Surface"))
            {
                sCachedMat.SetFloat("_Surface", 1f); // Transparent
                sCachedMat.SetFloat("_Blend", 0f);   // Alpha
            }
            sCachedMat.renderQueue = 3100;

            return sCachedMat;
        }

        /// <summary>
        /// 程序化生成 32×32 的上箭头纹理（白色像素，最终颜色由 startColor 控制）
        /// </summary>
        private static Texture2D GetOrCreateArrowTexture()
        {
            if (sCachedTex) return sCachedTex;

            const int size = 32;
            sCachedTex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "LemonArrowTex"
            };

            var pixels = new Color[size * size];
            // 全部透明
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;

            var cx = size / 2f;

            // --- 绘制向上的箭头 ---
            // 三角形头部：行 18~31（顶端宽度 0，底端宽度渐宽）
            for (var y = 18; y < size; y++)
            {
                var t = (float)(y - 18) / (size - 1 - 18); // 0 at bottom of head, 1 at top
                var halfW = Mathf.Lerp(7f, 0.5f, t);       // 底部宽 → 顶部窄
                var xMin = Mathf.FloorToInt(cx - halfW);
                var xMax = Mathf.CeilToInt(cx + halfW);
                xMin = Mathf.Max(0, xMin);
                xMax = Mathf.Min(size - 1, xMax);
                for (var x = xMin; x <= xMax; x++)
                {
                    var dist = Mathf.Abs(x - cx);
                    var edge = Mathf.Clamp01(1f - (dist - halfW + 1f)); // 边缘抗锯齿
                    pixels[y * size + x] = new Color(1f, 1f, 1f, edge);
                }
            }

            // 矩形尾部（箭柄）：行 2~17
            const float stemHalfW = 2.5f;
            for (var y = 2; y < 18; y++)
            {
                var xMin = Mathf.FloorToInt(cx - stemHalfW);
                var xMax = Mathf.CeilToInt(cx + stemHalfW);
                xMin = Mathf.Max(0, xMin);
                xMax = Mathf.Min(size - 1, xMax);
                for (var x = xMin; x <= xMax; x++)
                {
                    var dist = Mathf.Abs(x - cx);
                    var edge = Mathf.Clamp01(1f - (dist - stemHalfW + 0.5f));
                    pixels[y * size + x] = new Color(1f, 1f, 1f, edge);
                }
            }

            sCachedTex.SetPixels(pixels);
            sCachedTex.Apply(false, true); // makeNoLongerReadable = true 节省内存
            return sCachedTex;
        }

        #endregion

        #region 公开工具

        /// <summary>
        /// 对给定的 baseColor 叠加柠檬色调，返回混合后的颜色（保留原始 alpha）。
        /// 由 Player.SetPlayerAlpha() 在渲染时调用。
        /// </summary>
        public static Color ApplyLemonTint(Color baseColor)
        {
            return new Color(
                Mathf.Lerp(baseColor.r, LemonTintColor.r, TintBlend),
                Mathf.Lerp(baseColor.g, LemonTintColor.g, TintBlend),
                Mathf.Lerp(baseColor.b, LemonTintColor.b, TintBlend),
                baseColor.a
            );
        }

        #endregion
    }
}
