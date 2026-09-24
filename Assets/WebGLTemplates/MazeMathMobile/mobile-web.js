(function (root, factory) {
  'use strict';
  const api = factory();
  if (typeof module === 'object' && module.exports) module.exports = api;
  else root.MazeMathMobile = api;
})(typeof globalThis !== 'undefined' ? globalThis : this, function () {
  'use strict';
  function positive(value, fallback) { return Number.isFinite(value) && value > 0 ? value : fallback; }
  function isTouchBrowser(nav) {
    return !!nav && (Number(nav.maxTouchPoints) > 0 || /Android|iPhone|iPad|iPod/i.test(nav.userAgent || ''));
  }
  function renderRatio(width, height, dpr, quality) {
    const w=positive(width,844), h=positive(height,390), density=positive(dpr,1);
    return Math.min(density, quality === 'light' ? 1 : 2, Math.sqrt(2400000/(w*h)));
  }
  async function tryFullscreen(element, screen) {
    if (!element || typeof element.requestFullscreen !== 'function') return false;
    try { await element.requestFullscreen(); } catch (_) { return false; }
    try { if (screen && screen.orientation && screen.orientation.lock) await screen.orientation.lock('landscape'); } catch (_) { /* Rotation can always be manual. */ }
    return true;
  }
  function mount(win) {
    const doc=win.document, get=id=>doc.getElementById(id), canvas=get('unity-canvas');
    const start=get('start-button'), layer=get('start-layer'), loading=get('loading');
    const progress=get('progress'), status=get('status'), error=get('error');
    const rotate=get('rotate-layer'), full=get('fullscreen'), quality=get('quality');
    const mobile=isTouchBrowser(win.navigator);
    let instance=null, started=false, failed=false, hidden=false;
    function fail(message) {
      failed=true; error.textContent=String(message); error.hidden=false;
      layer.hidden=false; loading.hidden=true; rotate.hidden=true;
      status.textContent='불러오기 실패'; start.disabled=true;
    }
    function send(method, value) {
      if (instance) instance.SendMessage('MazeMathMobileWebBridge',method,value);
    }
    function orientation() {
      const rect=canvas.getBoundingClientRect();
      const portrait=mobile && rect.height>rect.width;
      rotate.hidden=!(instance && !failed && portrait);
      const next=!!doc.hidden || portrait;
      if(next && !hidden) send('SuspendBrowser','');
      hidden=next;
    }
    function lostFocus() { send('SuspendBrowser',''); }
    doc.addEventListener('visibilitychange',()=>{ if(doc.hidden) lostFocus(); orientation(); });
    win.addEventListener('pagehide',lostFocus);
    win.addEventListener('blur',lostFocus);
    win.addEventListener('resize',orientation);
    win.addEventListener('orientationchange',orientation);
    if(win.visualViewport) win.visualViewport.addEventListener('resize',orientation);
    canvas.addEventListener('contextmenu',e=>e.preventDefault());
    canvas.addEventListener('webglcontextlost',e=>{
      e.preventDefault(); lostFocus(); fail('그래픽 연결이 중단되었습니다. 페이지를 새로고침하고 가볍게 모드로 다시 실행해 주세요.');
    });
    full.addEventListener('click', async ()=>{ await tryFullscreen(doc.documentElement,win.screen); full.hidden=true; });
    start.addEventListener('click', async ()=>{
      if(started) return;
      started=true; start.disabled=true; quality.disabled=true; error.hidden=true;
      if(win.location.protocol==='file:') { fail('파일을 직접 열 수 없습니다. HTTP 또는 HTTPS 웹 주소로 접속해 주세요.'); return; }
      if(!win.WebAssembly) { fail('이 브라우저에는 WebAssembly가 없습니다. 최신 Safari 또는 Chrome에서 열어 주세요.'); return; }
      const probe=doc.createElement('canvas'), gl=probe.getContext('webgl2');
      if(!gl) { fail('WebGL 2를 사용할 수 없습니다. Safari 또는 Chrome에서 직접 열어 주세요.'); return; }
      try { const lose=gl.getExtension && gl.getExtension('WEBGL_lose_context'); if(lose) lose.loseContext(); } catch (_) {}
      try { const key='mazemath-web-probe'; win.localStorage.setItem(key,'1'); win.localStorage.removeItem(key); }
      catch (_) { get('storage-warning').hidden=false; }
      const build=win.MAZEMATH_BUILD;
      if(!build || !build.loaderUrl || build.loaderUrl.includes('{{{')) { fail('Unity 빌드가 아직 생성되지 않은 템플릿입니다. Mobile Web 빌드를 먼저 실행해 주세요.'); return; }
      loading.hidden=false; status.textContent='게임을 불러오고 있어요…';
      const size=canvas.getBoundingClientRect();
      const config=Object.assign({},build.config,{
        devicePixelRatio:renderRatio(size.width,size.height,win.devicePixelRatio,quality.value),
        matchWebGLToCanvasSize:true,
        showBanner:function(message,type){ if(type==='error') { lostFocus(); fail('게임 오류: '+message); } else console.warn(message); }
      });
      try {
        await new Promise((resolve,reject)=>{
          const script=doc.createElement('script'); script.src=build.loaderUrl;
          script.onload=resolve; script.onerror=()=>reject(new Error('게임 로더를 불러오지 못했습니다. 서버와 Build 폴더를 확인해 주세요.'));
          doc.head.appendChild(script);
        });
        instance=await win.createUnityInstance(canvas,config,value=>{
          progress.value=value; status.textContent='불러오는 중 '+Math.round(value*100)+'%';
        });
        // Never present the scene as ready following a loader/runtime error.
        if(failed) { if(instance.Quit) await instance.Quit(); instance=null; return; }
        send('ConfigureBrowser',mobile?'touch':'desktop');
        layer.hidden=true; loading.hidden=true;
        full.hidden=typeof (doc.documentElement || {}).requestFullscreen!=='function';
        // The fullscreen request is optional and temporarily shown only before interaction.
        canvas.addEventListener('pointerdown',()=>{ full.hidden=true; },{once:true});
        canvas.focus(); hidden=false; orientation();
      } catch(ex) {
        fail('불러오기 실패: '+String(ex && ex.message || ex)+' 새로고침하거나 가볍게 모드를 사용해 주세요.');
      }
    });
  }
  return {isTouchBrowser,renderRatio,tryFullscreen,mount};
});
