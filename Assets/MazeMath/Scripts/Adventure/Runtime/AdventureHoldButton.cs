using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MazeMath.Adventure
{
    public sealed class AdventureHoldButton : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
    {
        private AdventureControls controls;
        private int direction;
        private readonly HashSet<int> tokens=new HashSet<int>();
        public void Bind(AdventureControls controls,int direction) { this.controls=controls; this.direction=direction; }
        public void OnPointerDown(PointerEventData e) { if(controls==null)return; tokens.Add(e.pointerId); controls.Hold(e.pointerId,direction); }
        public void OnPointerUp(PointerEventData e) { tokens.Remove(e.pointerId); controls?.Release(e.pointerId); }
        public void OnPointerExit(PointerEventData e) { OnPointerUp(e); }
        private void OnDisable() { foreach(int token in tokens)controls?.Release(token); tokens.Clear(); }
    }
}
