using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static TreeEditor.TextureAtlas;

namespace CHARACTER
{
    public class Character_Sprite : VisualNovelCharater
    {
        private const string SPRITE_RENDERER_PARENT_NAME = "Renderers";
        private const string SPRITESHEET_DEFAULT_SHEETNAME = "Default";
        private const char SPRITESHEET_TEX_SPRITE_DELIMETER = '-';
        private CanvasGroup rootCG
        {
            get
            {
                if (root == null)
                    Debug.Log("root is null");
                Debug.Log("root" + root);
                return root.GetComponent<CanvasGroup>();
            }
        }
        public List<CharacterSpriteLayer> layers = new List<CharacterSpriteLayer>();

        private string artAssetsDirectory = "";

        public override bool isVisible
        {
            get { return isRevealing || rootCG.alpha == 1; }
            set { rootCG.alpha = value ? 1 : 0; }
        }

        public Character_Sprite(string name, CharacterConfigData config, GameObject prefab, string rootAssetFolder) : base(name, config, prefab)
        {
            rootCG.alpha = ENABLE_ON_START ? 1f : 0f;

            artAssetsDirectory = rootAssetFolder + "/Images";
            GetLayers();

            Debug.Log($"Created Sprite VisualNovelCharater: '{name}'");
        }

        private void GetLayers()
        {
            Transform renderRoot = animator.transform.Find( SPRITE_RENDERER_PARENT_NAME );

            if (renderRoot == null)
                return;

            for (int i = 0; i < renderRoot.transform.childCount; i++)
            {
                Transform child = renderRoot.transform.GetChild(i);

                Image rendererImage = child.GetComponentInChildren<Image>();

                if(rendererImage != null)
                {
                    CharacterSpriteLayer layer = new CharacterSpriteLayer(rendererImage, i);
                    layers.Add(layer);
                    child.name = $"Layer: {i}";
                }
                
            }

        }

        public void SetSprite(Sprite sprite, int layer = 0)
        {
            layers[layer].SetSprite(sprite);
        }

        public Sprite GetSprite(string spriteName)
        {
            if (config.characterType == CharacterType.SpriteSheet)
            {
                string[] data = spriteName.Split(SPRITESHEET_TEX_SPRITE_DELIMETER);
                Sprite[] spriteArray = new Sprite[0];
                
                
                if(data.Length == 2)
                {
                    string textureName = data[0];
                    spriteName = data[1];
                    spriteArray = Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{textureName}");

                }
                else
                {
                    spriteArray = Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{SPRITESHEET_DEFAULT_SHEETNAME}");
                }

                if (spriteArray.Length == 0)
                    Debug.LogWarning($"VisualNovelCharater  '{name}' does not have a default art asset called '{SPRITESHEET_DEFAULT_SHEETNAME}'");

                return Array.Find(spriteArray, sprite => sprite.name == spriteName);
            }
            else
            {
                return Resources.Load<Sprite>($"{artAssetsDirectory}/{spriteName}");
            }
        }

        public Coroutine TransitionSprite(Sprite sprite, int layer = 0, float speed = 1)
        {
            CharacterSpriteLayer spriteLayer = layers[layer];

            return spriteLayer.TransitionSprite(sprite, speed);
        }

        public override IEnumerator ShowingOrHiding(bool show, float speedMultiplier = 1f)
        {
            float targetAlpha = show ? 1.0f : 0.0f;
            CanvasGroup self = rootCG;

            while (self.alpha != targetAlpha)
            {
                self.alpha = Mathf.MoveTowards(self.alpha, targetAlpha, 3f * Time.deltaTime * speedMultiplier);
                yield return null;
            }

            co_revealing = null;
            co_hiding = null;
        }

        public override void SetColor(Color color)
        {
            base.SetColor(color);

            color = displayColor;

            foreach (CharacterSpriteLayer layer in layers)
            {
                layer.StopChangingColor();
                layer.SetColor(color);
            }
        }

        public override IEnumerator ChangingColor(Color color, float speed)
        {
            foreach(CharacterSpriteLayer layer in layers)
                layer.TransitionColor(color, speed);

            yield return null;

            while(layers.Any(l => l.isChangingColor))
                yield return null;

            co_changingColor = null;
        }

        public override IEnumerator Highlighting(float speedMultiplier, bool immediate)
        {
            Color targetColor = displayColor;

            foreach(CharacterSpriteLayer layer in layers)
            {
                if (immediate)
                    layer.SetColor(displayColor);
                else
                    layer.TransitionColor(targetColor, speedMultiplier);
            }

            yield return null;

            while (layers.Any(l => l.isChangingColor))
                yield return null;

            co_highlighting = null;

        }

        public override IEnumerator FaceDirection(bool faceLeft, float speedMultiplier, bool immediate)
        {
            foreach( CharacterSpriteLayer layer in layers)
            {
                layer.Flip(speedMultiplier, immediate);
            }

            yield return null;

            while (layers.Any(l => l.isFlipping))
                yield return null;

            co_fliping = null;
        }

        public override void OnReceiveCastingExpression(int layer, string expression)
        {
            Sprite sprite = GetSprite(expression);

            if(sprite == null)
            {
                Debug.LogWarning($"Sprite '{expression}' could not be found for character '{name}'");
                return;
            }

            TransitionSprite(sprite, layer);
        }
    }
}

