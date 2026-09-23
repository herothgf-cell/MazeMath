using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace MazeMath.Adventure
{
    /// <summary>Cozy robot expedition: paper cards, sea-glass mint, apricot accents and quiet typography.</summary>
    public sealed class AdventureTheme
    {
        public readonly Color Ink = new Color32(42,65,77,255), Muted = new Color32(107,128,138,255),
            Stone = new Color32(255,252,246,255), Edge = new Color32(211,229,225,255),
            Green = new Color32(42,151,127,255), Gold = new Color32(170,98,48,255),
            Mint = new Color32(206,239,224,255), Peach = new Color32(255,224,199,255),
            Sky = new Color32(218,237,248,255), Rail = new Color32(244,249,246,255);
        public Font Font { get; private set; }
        public bool Korean { get; private set; }
        private bool ownsFont;
        private Sprite rounded;
        private Texture2D roundedTexture;
        public AdventureTheme(Font supplied = null)
        {
            Font = supplied != null ? supplied : Resources.Load<Font>("MazeMathKorean");
#if !UNITY_WEBGL || UNITY_EDITOR
            if (Font == null)
            {
                Font = UnityEngine.Font.CreateDynamicFontFromOSFont(new[]{"Malgun Gothic","Apple SD Gothic Neo","Noto Sans CJK KR","Noto Sans KR","sans-serif"}, 20);
                ownsFont = Font != null;
            }
#endif
            if (Font != null) { Font.RequestCharactersInTexture("모험숫자제작",20); Korean = Font.HasCharacter('모') && Font.HasCharacter('험'); }
            if (Font == null || !Korean)
            {
                if (ownsFont && Font != null) Object.Destroy(Font);
                ownsFont = false; Font = global::MazeMath.UI.RuntimeFontProvider.Get(); Korean = false;
            }
        }
        public string T(string ko,string en) => Korean ? ko : en;
        public string Material(int i) => Korean ? new[]{"철 조각","철판","기어","에너지석","코어"}[i] : new[]{"Scrap","Iron","Gear","Crystal","Core"}[i];
        public string Tool(int i) => Korean ? new[]{"곡괭이 팔","파워 렌치","점프 부스터","에너지 실드","탐험 센서","가방"}[i] : new[]{"Mining arm","Wrench","Jump boots","Shield","Sensor","Bag"}[i];
        public string Effect(int tool,int variant)
        {
            string[] ko={"메아리 · 보물 표식","행운 · 보물 철 조각 +1","빠른 수리 · 한 번에 수리","회로 감지 · 수리 위치 안내","착지 보호 · 낙하 피해 없음","경로 탐색 · 높은 보물 위치","재충전 · 퍼즐로 실드 회복","안정장 · 보스 예고 연장","기억 · 방문한 경로 강조","길잡이 · 다음 목표 방향"};
            string[] en={"Echo · mark hidden loot","Lucky · extra scrap","Quick fix · one-step repair","Circuit · repair marker","Soft landing · no fall damage","Route scan · high loot","Recharge · puzzle shield refill","Stable · longer boss warning","Memory · highlight your trail","Guide · next objective"};
            return Korean ? ko[tool*2+variant] : en[tool*2+variant];
        }
        public RectTransform Rect(Transform parent,string name,float x0,float y0,float x1,float y1)
        {
            var go=new GameObject(name,typeof(RectTransform)); go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>(); r.anchorMin=new Vector2(x0,y0); r.anchorMax=new Vector2(x1,y1); r.offsetMin=r.offsetMax=Vector2.zero; return r;
        }
        public Image Fill(RectTransform parent,Color c,bool raycast=false)
        {
            var image=parent.gameObject.AddComponent<Image>(); image.color=c; image.raycastTarget=raycast; return image;
        }
        public RectTransform Box(Transform parent,string name,float x0,float y0,float x1,float y1)
        {
            var r=Rect(parent,name,x0,y0,x1,y1); var image=Fill(r,Stone); image.sprite=Rounded(); image.type=Image.Type.Sliced;
            var shadow=r.gameObject.AddComponent<Shadow>(); shadow.effectDistance=new Vector2(0,-2); shadow.effectColor=new Color(.12f,.23f,.26f,.10f);
            return r;
        }
        public Text Label(Transform parent,string text,int size,TextAnchor align,float x0,float y0,float x1,float y1)
        {
            var r=Rect(parent,"Label",x0,y0,x1,y1); var label=r.gameObject.AddComponent<Text>();
            label.font=Font; label.text=text; label.fontSize=Mathf.Clamp(size,10,28); label.color=Ink; label.alignment=align;
            label.fontStyle=FontStyle.Normal; label.lineSpacing=1.05f; label.supportRichText=false;
            label.resizeTextForBestFit=true; label.resizeTextMinSize=Mathf.Min(12,label.fontSize); label.resizeTextMaxSize=label.fontSize;
            label.horizontalOverflow=HorizontalWrapMode.Wrap; label.verticalOverflow=VerticalWrapMode.Truncate; label.raycastTarget=false; return label;
        }
        public Button Button(Transform parent,string text,UnityAction action,float x0,float y0,float x1,float y1,bool primary=false)
        {
            var r=Box(parent,string.IsNullOrEmpty(text)?"IconButton":text,x0,y0,x1,y1);
            var image=r.GetComponent<Image>(); image.color=primary?Mint:Sky; image.raycastTarget=true;
            var button=r.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.onClick.AddListener(action);
            var colors=button.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(.94f,.98f,1f);
            colors.pressedColor=new Color(.80f,.90f,.88f); colors.selectedColor=Color.white;
            colors.disabledColor=new Color(.78f,.81f,.81f,.65f); colors.fadeDuration=.10f; button.colors=colors;
            Label(r,text,16,TextAnchor.MiddleCenter,.06f,.08f,.94f,.92f);
            button.navigation=new Navigation{mode=Navigation.Mode.None}; return button;
        }
        private Sprite Rounded()
        {
            if(rounded!=null) return rounded;
            const int size=64; const float radius=13;
            roundedTexture=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,name="Momo soft card"};
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                float dx=Mathf.Max(Mathf.Abs(x+0.5f-size/2f)-(size/2f-radius),0);
                float dy=Mathf.Max(Mathf.Abs(y+0.5f-size/2f)-(size/2f-radius),0);
                pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(radius-Mathf.Sqrt(dx*dx+dy*dy)));
            }
            roundedTexture.SetPixels(pixels); roundedTexture.Apply(false,true);
            rounded=Sprite.Create(roundedTexture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(15,15,15,15)); return rounded;
        }
        public void Dispose()
        {
            if(ownsFont && Font!=null) Object.Destroy(Font);
            if(rounded!=null) Object.Destroy(rounded);
            if(roundedTexture!=null) Object.Destroy(roundedTexture);
        }
    }
}
