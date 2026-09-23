using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace MazeMath.Adventure
{
    /// <summary>
    /// Cute block-adventure theme: Minecraft-like block readability without copying Minecraft assets.
    /// Keeps the robot cast friendly while restoring stone, grass, wood and inventory-slot language.
    /// </summary>
    public sealed class AdventureTheme
    {
        public readonly Color Ink = new Color32(43,48,39,255);
        public readonly Color Muted = new Color32(83,91,75,255);
        public readonly Color BlockStone = new Color32(219,215,194,255);
        public readonly Color Stone = new Color32(241,237,218,255);
        public readonly Color Edge = new Color32(104,107,88,255);
        public readonly Color BlockBorder = new Color32(70,74,60,255);
        public readonly Color Grass = new Color32(105,153,72,255);
        public readonly Color Green = new Color32(82,143,68,255);
        public readonly Color Gold = new Color32(230,183,68,255);
        public readonly Color Mint = new Color32(151,199,153,255);
        public readonly Color Peach = new Color32(211,161,105,255);
        public readonly Color Sky = new Color32(166,201,213,255);
        public readonly Color Wood = new Color32(158,111,67,255);
        public readonly Color SlotDark = new Color32(62,67,56,255);
        public readonly Color Rail = new Color32(205,200,177,255);

        public Font Font { get; private set; }
        public bool Korean { get; private set; }
        private bool ownsFont;

        public AdventureTheme(Font supplied = null)
        {
            Font = supplied != null ? supplied : Resources.Load<Font>("MazeMathKorean");
#if !UNITY_WEBGL || UNITY_EDITOR
            if (Font == null)
            {
                Font = UnityEngine.Font.CreateDynamicFontFromOSFont(
                    new[]{"Malgun Gothic","Noto Sans CJK KR","Noto Sans KR","Apple SD Gothic Neo","sans-serif"},
                    22);
                ownsFont = Font != null;
            }
#endif
            if (Font != null)
            {
                Font.RequestCharactersInTexture("모험숫자제작가나다라마바사아자차카타파하",22,FontStyle.Bold);
                Korean = Font.HasCharacter('모') && Font.HasCharacter('험');
            }

            if (Font == null || !Korean)
            {
                if (ownsFont && Font != null) Object.Destroy(Font);
                ownsFont = false;
                Font = global::MazeMath.UI.RuntimeFontProvider.Get();
                Korean = false;
            }
        }

        public string T(string ko,string en) => Korean ? ko : en;
        public string Material(int i) => Korean
            ? new[]{"철 조각","철판","기어","에너지석","코어"}[i]
            : new[]{"Scrap","Iron","Gear","Crystal","Core"}[i];
        public string Tool(int i) => Korean
            ? new[]{"곡괭이 팔","파워 렌치","점프 부스터","에너지 실드","탐험 센서","가방"}[i]
            : new[]{"Mining arm","Wrench","Jump boots","Shield","Sensor","Bag"}[i];

        public string Effect(int tool,int variant)
        {
            string[] ko={
                "메아리 · 보물 표식","행운 · 보물 철 조각 +1",
                "빠른 수리 · 한 번에 수리","회로 감지 · 수리 위치 안내",
                "착지 보호 · 낙하 피해 없음","경로 탐색 · 높은 보물 위치",
                "재충전 · 퍼즐로 실드 회복","안정장 · 보스 예고 연장",
                "기억 · 방문한 경로 강조","길잡이 · 다음 목표 방향"
            };
            string[] en={
                "Echo · mark hidden loot","Lucky · extra scrap",
                "Quick fix · one-step repair","Circuit · repair marker",
                "Soft landing · no fall damage","Route scan · high loot",
                "Recharge · puzzle shield refill","Stable · longer boss warning",
                "Memory · highlight your trail","Guide · next objective"
            };
            return Korean ? ko[tool*2+variant] : en[tool*2+variant];
        }

        public RectTransform Rect(Transform parent,string name,float x0,float y0,float x1,float y1)
        {
            var go=new GameObject(name,typeof(RectTransform));
            go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();
            r.anchorMin=new Vector2(x0,y0);
            r.anchorMax=new Vector2(x1,y1);
            r.offsetMin=r.offsetMax=Vector2.zero;
            return r;
        }

        public Image Fill(RectTransform parent,Color c,bool raycast=false)
        {
            var image=parent.gameObject.AddComponent<Image>();
            image.color=c;
            image.raycastTarget=raycast;
            return image;
        }

        public RectTransform Box(Transform parent,string name,float x0,float y0,float x1,float y1)
        {
            var r=Rect(parent,name,x0,y0,x1,y1);
            var image=Fill(r,Stone);
            var outline=r.gameObject.AddComponent<Outline>();
            outline.effectColor=BlockBorder;
            outline.effectDistance=new Vector2(2,-2);
            outline.useGraphicAlpha=true;

            var top=Rect(r,"BlockTop",0,.91f,1,1);
            Fill(top,new Color(1f,1f,1f,.30f));
            var left=Rect(r,"BlockLeft",0,0,.035f,1);
            Fill(left,new Color(1f,1f,1f,.20f));
            var bottom=Rect(r,"BlockBottom",0,0,1,.07f);
            Fill(bottom,new Color(BlockBorder.r,BlockBorder.g,BlockBorder.b,.32f));
            return r;
        }

        public Text Label(Transform parent,string text,int size,TextAnchor align,float x0,float y0,float x1,float y1)
        {
            var r=Rect(parent,"Label",x0,y0,x1,y1);
            var label=r.gameObject.AddComponent<Text>();
            label.font=Font;
            label.text=text;
            label.fontSize=Mathf.Clamp(size,14,30);
            label.color=Ink;
            label.alignment=align;
            label.fontStyle=FontStyle.Bold;
            label.lineSpacing=1.08f;
            label.supportRichText=false;
            label.resizeTextForBestFit=false;
            label.horizontalOverflow=HorizontalWrapMode.Wrap;
            label.verticalOverflow=VerticalWrapMode.Truncate;
            label.raycastTarget=false;

            var outline=label.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(1f,1f,1f,.28f);
            outline.effectDistance=new Vector2(1,-1);
            outline.useGraphicAlpha=true;
            return label;
        }

        public Button Button(Transform parent,string text,UnityAction action,float x0,float y0,float x1,float y1,bool primary=false)
        {
            var r=Box(parent,string.IsNullOrEmpty(text)?"IconButton":text,x0,y0,x1,y1);
            var image=r.GetComponent<Image>();
            image.color=primary?Grass:Wood;
            image.raycastTarget=true;

            var button=r.gameObject.AddComponent<Button>();
            button.targetGraphic=image;
            button.onClick.AddListener(action);
            var colors=button.colors;
            colors.normalColor=Color.white;
            colors.highlightedColor=new Color(1.08f,1.08f,1.08f,1f);
            colors.pressedColor=new Color(.82f,.82f,.82f,1f);
            colors.selectedColor=Color.white;
            colors.disabledColor=new Color(.55f,.55f,.55f,.72f);
            colors.fadeDuration=.06f;
            button.colors=colors;

            var label=Label(r,text,18,TextAnchor.MiddleCenter,.05f,.07f,.95f,.93f);
            label.color=new Color32(255,250,228,255);
            var labelOutline=label.GetComponent<Outline>();
            if(labelOutline!=null)
            {
                labelOutline.effectColor=new Color(0,0,0,.58f);
                labelOutline.effectDistance=new Vector2(1,-1);
            }

            button.navigation=new Navigation{mode=Navigation.Mode.None};
            return button;
        }

        public void Dispose()
        {
            if(ownsFont && Font!=null) Object.Destroy(Font);
        }
    }
}
