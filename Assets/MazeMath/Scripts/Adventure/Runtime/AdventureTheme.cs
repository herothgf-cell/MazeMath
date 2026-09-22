using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace MazeMath.Adventure
{
    public sealed class AdventureTheme
    {
        public readonly Color Ink = new Color32(236,240,219,255), Muted = new Color32(181,191,172,255), Stone = new Color32(52,60,53,255), Edge = new Color32(18,25,18,255), Green = new Color32(178,225,109,255), Gold = new Color32(244,204,123,255);
        public Font Font { get; private set; }
        public bool Korean { get; private set; }
        private bool ownsFont;
        public AdventureTheme(Font supplied = null)
        {
            Font = supplied != null ? supplied : Resources.Load<Font>("MazeMathKorean");
#if !UNITY_WEBGL || UNITY_EDITOR
            if (Font == null)
            {
                Font = UnityEngine.Font.CreateDynamicFontFromOSFont(new[]{"Malgun Gothic","Apple SD Gothic Neo","Noto Sans CJK KR","Noto Sans KR","sans-serif"}, 24);
                ownsFont = Font != null;
            }
#endif
            if (Font != null) { Font.RequestCharactersInTexture("모험숫자제작", 24); Korean = Font.HasCharacter('모') && Font.HasCharacter('험'); }
            if (Font == null || !Korean)
            {
                if (ownsFont && Font != null) Object.Destroy(Font);
                ownsFont = false; Font = global::MazeMath.UI.RuntimeFontProvider.Get(); Korean = false;
            }
        }
        public string T(string ko, string en) { return Korean ? ko : en; }
        public string Material(int i) { return Korean ? new[]{"철 조각","철판","기어","에너지석","희귀 코어"}[i] : new[]{"Scrap","Iron","Gear","Crystal","Core"}[i]; }
        public string Tool(int i) { return Korean ? new[]{"곡괭이 팔","파워 렌치","점프 부스터","에너지 실드","탐험 센서","가방"}[i] : new[]{"Mining Arm","Power Wrench","Jump Boots","Shield","Sensor","Bag"}[i]; }
        public string Effect(int tool, int variant)
        {
            string[] ko = {"메아리: 보물 표식","행운: 보물 철 조각 +1","빠른 수리: 한 번에 수리","회로 감지: 수리 위치 안내","착지 보호: 낙하 피해 없음","경로 탐색: 높은 보물 위치","재충전: 퍼즐 해결 시 실드 회복","안정장: 보스 예고 시간 증가","기억: 방문한 경로 강조","길잡이: 다음 목표 방향"};
            string[] en = {"Echo: mark hidden loot","Lucky: +1 scrap from loot","Quick fix: repair in one action","Circuit: locate broken machine","Soft landing: no fall damage","Route scan: high loot marker","Recharge: shield after puzzles","Stable: longer boss warning","Memory: highlight visited trail","Guide: next objective direction"};
            return Korean ? ko[tool * 2 + variant] : en[tool * 2 + variant];
        }
        public RectTransform Rect(Transform parent, string name, float x0, float y0, float x1, float y1)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>(); r.anchorMin = new Vector2(x0,y0); r.anchorMax = new Vector2(x1,y1); r.offsetMin = r.offsetMax = Vector2.zero; return r;
        }
        public Image Fill(RectTransform parent, Color c, bool raycast = false)
        {
            var i = parent.gameObject.AddComponent<Image>(); i.color = c; i.raycastTarget = raycast; return i;
        }
        public RectTransform Box(Transform parent, string name, float x0, float y0, float x1, float y1)
        {
            var r = Rect(parent, name, x0,y0,x1,y1); Fill(r, Edge);
            var light = Rect(r, "Bevel", 0,0,1,1); light.offsetMin = new Vector2(3,3); light.offsetMax = new Vector2(-3,-3); Fill(light,new Color32(113,129,100,255));
            var inside = Rect(light,"Stone",0,0,1,1); inside.offsetMin = new Vector2(2,2); inside.offsetMax = new Vector2(-2,-2); Fill(inside,Stone); return r;
        }
        public Text Label(Transform parent, string text, int size, TextAnchor align, float x0, float y0, float x1, float y1)
        {
            var r = Rect(parent,"Label",x0,y0,x1,y1); var t = r.gameObject.AddComponent<Text>(); t.font = Font; t.text = text; t.fontSize = size; t.color = Ink; t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate; t.raycastTarget = false; return t;
        }
        public Button Button(Transform parent, string text, UnityAction action, float x0,float y0,float x1,float y1, bool primary = false)
        {
            var r = Box(parent,text,x0,y0,x1,y1); var b = r.gameObject.AddComponent<Button>(); var i = r.GetComponent<Image>(); i.raycastTarget = true; b.targetGraphic = i;
            var inner = r.GetChild(0).GetChild(0).GetComponent<Image>(); inner.color = primary ? Green : new Color32(74,87,70,255);
            b.onClick.AddListener(action); var t = Label(r,text,22,TextAnchor.MiddleCenter,.04f,.06f,.96f,.94f); if (primary) t.color = Edge;
            b.navigation = new Navigation { mode = Navigation.Mode.None }; return b;
        }
        public void Dispose() { if (ownsFont && Font != null) Object.Destroy(Font); }
    }
}
