using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
    public class Dissolve : MonoBehaviour
        {
            public Material Material;
			private static readonly int color =Shader.PropertyToID("_Color");
			private static readonly int Fade =Shader.PropertyToID("_Fade");
			public Color DissovleColor;

			private void Start()
			{
				var material = Instantiate(Material);
				GetComponent<SpriteRenderer>().material = material;

				material.SetColor(color,DissovleColor);
                ActionKit.Lerp(1, 0, 0.5f, (fade) =>
					{
						material.SetFloat(Fade,fade);
						this.LocalScale(1+(1-fade)*0.5f);
					})
					.Start(this, () =>
					{
						Destroy(material);
						this.DestroyGameObjGracefully();
					});
			}
        }
}