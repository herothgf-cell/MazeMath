/* Original browser renderer + touch UI. Unity source/assets are not modified. */
(()=>{'use strict';
const C=window.MM,$=id=>document.getElementById(id),canvas=$('world'),ctx=canvas.getContext('2d'),stage=$('stage'),veil=$('veil'),sheet=$('sheet');
const KEY='mazemath.instant.v1',names=['철 조각','철판','기어','에너지석','코어'],icons=['⛏','⚒','⇧','◈','⌖','▣'],tools=['곡괭이 팔','파워 렌치','점프 부스터','에너지 실드','탐험 센서'];
const desc=['균열벽을 부수어 새 길을 열어요','고장 난 보물 통로를 수리해요','더 높이 점프할 수 있어요','낙하할 때 한 번 보호해요','보물과 방문한 길을 찾아요'];
let campaign=C.freshCampaign(1),s=campaign.chapterState,running=false,modal='title',seq=0,closeTimer=0,toastTimer=0,lastSave=0,storageOK=true,held=new Map(),keys=new Set(),jumpQueued=false,near=null,carry=-1,weights=[false,false,false],mirrors=[false,false,false],steps=[0,0],lastPlate='',repair=0,winNotified=false;
let W=1,H=1,U=40,DPR=1,cx=14,cy=3.2,time=0,lastTime=0,acc=0,walk=0,lastUi='';
const seed=()=>Date.now()^(Math.random()*0xffffffff),esc=v=>String(v).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const has=k=>C.has(s,k),done=()=>{save();refresh();};
function load(){try{return C.restore(localStorage.getItem(KEY));}catch{storageOK=false;return null;}}
function save(){if(!running)return;try{if(campaign&&campaign.v===2){campaign.chapterState=s;campaign.inventory={items:s.items,owned:s.owned,equipped:s.equipped,ench:s.ench,unlocked:s.unlocked};campaign.campaignXp=s.xp;C.syncCampaign(campaign);}localStorage.setItem(KEY,JSON.stringify(campaign));storageOK=true;}catch{storageOK=false;}lastSave=time;}
function clearInput(){held.clear();keys.clear();jumpQueued=false;}
function install(state){campaign=state&&state.v===2?state:(state?C.restore(JSON.stringify(state)):C.freshCampaign(seed()));if(!campaign)campaign=C.freshCampaign(seed());s=campaign.chapterState||campaign;s.chapterId=campaign.chapter;s.items=campaign.inventory.items;s.owned=campaign.inventory.owned;s.equipped=campaign.inventory.equipped;s.ench=campaign.inventory.ench;s.unlocked=campaign.inventory.unlocked;s.xp=campaign.campaignXp??s.xp;carry=-1;weights=[false,false,false];mirrors=[has('laser'),has('laser'),has('boss.laser')];steps=[0,0];lastPlate='';repair=0;cx=s.x;cy=s.y+3.2;winNotified=has('clear');lastUi='';refresh();}
function toast(text){$('toast').textContent=text;$('toast').style.display='block';clearTimeout(toastTimer);toastTimer=setTimeout(()=>$('toast').style.display='none',3400);}
function open(kind,title,html){clearInput();clearTimeout(closeTimer);seq++;modal=kind;veil.classList.add('open');sheet.innerHTML=(title?`<button class="btn x" id="close" aria-label="닫기">×</button><h2>${esc(title)}</h2>`:'')+html;if($('close'))$('close').onclick=hide;sheet.scrollTop=0;}
function hide(){clearTimeout(closeTimer);seq++;if(!running){title();return;}modal='';veil.classList.remove('open');clearInput();save();}
function bind(id,fn){let e=$(id);if(e)e.onclick=fn;}
function title(){let old=load();open('title','',`<div class="center"><span class="tag">MAZEMATH · MOBILE PLAY</span><div class="hero"><div class="face"></div><div class="glass"></div></div><h1>모모의<br>블록 탐험</h1><p>길을 찾고, 숫자 장치를 풀고,<br>직접 만든 도구로 모모를 도와주세요.</p><span class="tag">3층 미로 · 수학 · 제작 · 골렘</span></div><div class="stack">${old?'<button class="btn primary wide" id="continue">이어하기</button>':''}<button class="btn ${old?'':'primary'} wide" id="new">새 탐험 시작</button></div><p class="sub center">별도 설치 없이 터치로 플레이합니다.<br>가로 화면을 권장하지만 세로 화면도 지원합니다.<br><b>모바일 즉시 플레이판</b> · Unity 본편과 별도 버전/저장</p>`);bind('new',()=>{if(old){open('confirm','새 탐험','<p>모바일 웹판의 이어하기 기록을 새로 시작할까요?</p><button class="btn primary wide" id="yes">새로 시작</button>');bind('yes',newRun);}else newRun();});bind('continue',()=>{install(old);running=true;hide();toast('모모: 돌아오셨네요! 탐험을 이어가요.');});}
function newRun(){install(C.freshCampaign(seed()));running=true;hide();save();toast('오른쪽 숫자 장치를 찾아요! 이동은 ← →, 조사는 초록 버튼이에요.');}
function pause(){if(!running){title();return;}open('pause','잠깐 쉬어 가요',`<p>게임이 멈췄습니다. 다시 시작해도 장비와 해결한 장치는 유지돼요.</p><div class="stack"><button class="btn primary wide" id="resume">계속 탐험하기</button><button class="btn wide" id="checkpoint">체크포인트로 돌아가기</button><button class="btn wide" id="help">조작 방법</button><button class="btn wide" id="title">처음 화면</button></div><p class="sub">${storageOK?'이 브라우저에 자동 저장 중':'저장이 제한된 브라우저입니다. 창을 닫으면 기록이 사라질 수 있어요.'}<br>Web Play 1.0 · Unity 빌드가 아닌 경량 웹판</p>`);bind('resume',hide);bind('checkpoint',()=>{s.x=s.checkpoint[0];s.y=s.checkpoint[1];s.vy=0;s.climb=null;hide();});bind('help',help);bind('title',()=>{save();running=false;title();});}
function help(){open('help','모모의 탐험 안내','<p><b>← →</b>를 누르고 있으면 걷습니다.<br><b>↑ ↓</b>는 사다리 앞에서 사용합니다.<br><b>점프</b>로 발판을 건너뛰고, <b>조사</b>로 가까운 장치를 사용하세요.</p><div class="help">숫자 장치 → 작업대로 돌아가기 → 곡괭이 팔 제작 → 균열벽 열기 → 저울 → 위층 거울과 발판 → 골렘</div><p class="sub">상자는 가까이에서 조사해 들고, 저울에서 다시 조사해 내려놓습니다. 너무 무거우면 빈손으로 저울을 조사해 하나를 꺼내세요.<br>정답을 맞히면 창이 자동으로 닫힙니다. 오답에는 벌점이 없어요.<br>닫기 ×로 탐험에 돌아갈 수 있습니다.</p>');}
function resources(){return `<div class="resources">${names.map((n,i)=>`${esc(n)} <b>${s.items[i]}</b>`).join(' · ')}<br>지식 경험치 <b>${s.xp} XP</b></div>`;}
function bag(){if(!running)return;open('bag','탐험 가방',resources()+`<div class="grid5">${tools.map((n,i)=>`<div class="item"><div class="itemicon">${icons[i]}</div><div class="itembody"><b>${n}</b><small>${desc[i]}${s.ench[i]>=0?' · 인챈트 적용':''}</small></div><button class="btn small" id="equip${i}" ${s.owned[i]?'':'disabled'}>${s.owned[i]?(s.equipped[i]?'장착 중':'장착'):'미제작'}</button></div>`).join('')}</div><p class="sub">새 장비는 1층의 작업대에서 만듭니다.</p>`);tools.forEach((n,i)=>bind('equip'+i,()=>{s.equipped[i]=!s.equipped[i];done();bag();}));}
function atBench(){return Math.abs(s.x-18)<2&&Math.abs(s.y)<1;}
function workshop(){if(!atBench()){toast('제작은 1층 작업대에서 할 수 있어요.');return;}open('workshop','모모의 작업대',resources()+'<div class="tabs"><button class="btn primary" id="normal">만들기</button><button class="btn" id="pattern">3×3 조합</button><button class="btn" id="enchant">인챈트</button></div><div class="grid5">'+tools.map((n,i)=>`<div class="item"><div class="itemicon">${icons[i]}</div><div class="itembody"><b>${n}</b><small>${C.costs[i].map((v,j)=>v?names[j]+' '+v:'').filter(Boolean).join(' · ')}</small></div><button class="btn small ${i===0&&!s.owned[0]?'primary':''}" id="craft${i}" ${s.owned[i]?'disabled':''}>${s.owned[i]?'보유 중':i===0&&!s.owned[0]?'지금 만들기':'제작'}</button></div>`).join('')+'</div><p class="sub">첫 도구는 곡괭이 팔입니다. 재료가 모자라면 숫자 장치를 먼저 풀어보세요.</p>');tools.forEach((n,i)=>bind('craft'+i,()=>{if(C.craft(s,i)){done();workshop();toast(n+' 제작과 장착 완료!');}else toast(i&&!s.owned[0]?'곡괭이 팔을 먼저 만들어 주세요.':'재료가 더 필요해요. 보물과 퍼즐을 찾아보세요.');}));bind('pattern',patternUI);bind('enchant',enchantUI);bind('normal',workshop);}
function patternUI(){let grid=Array(9).fill(-1),pick=2;open('pattern','3×3 센서 조합',`<p class="sub">재료를 선택한 뒤 칸을 눌러 놓으세요.<br>같은 재료를 다시 누르면 그 칸이 비워집니다.</p><div class="help center">기어 · 빈칸 · 기어<br>빈칸 · 에너지석 · 빈칸<br>철 조각 · 빈칸 · 철 조각</div><div class="pickers">${[0,2,3].map(i=>`<button class="btn small" id="pick${i}">${names[i]}</button>`).join('')}</div><div class="recipe">${grid.map((v,i)=>`<button class="btn" id="cell${i}" aria-label="조합 칸 ${i+1}">·</button>`).join('')}</div><button class="btn primary wide" id="makePattern">센서 조립하기</button><p class="sub center">실패해도 재료는 사라지지 않아요.</p>`);[0,2,3].forEach(i=>bind('pick'+i,()=>{pick=i;[0,2,3].forEach(j=>$('pick'+j).classList.toggle('chosen',j===i));}));$('pick2').classList.add('chosen');grid.forEach((v,i)=>bind('cell'+i,()=>{grid[i]=grid[i]===pick?-1:pick;$('cell'+i).textContent=grid[i]<0?'·':['▰','▥','⚙','◆'][grid[i]];}));bind('makePattern',()=>{if(!atBench())return;if(C.craft(s,4,grid)){done();hide();toast('탐험 센서를 조립했어요!');}else toast('도안, 필요한 재료, 센서 보유 여부를 확인해 주세요.');});}
const enchantNames=[['메아리','행운'],['빠른 수리','회로 감지'],['착지 보호','경로 탐색'],['재충전','튼튼한 실드'],['기억','길잡이']];
function enchantUI(){open('enchant','도구에 특별한 능력',resources()+'<p class="sub">최초 해금 10 XP · 해금한 인챈트로 바꾸기는 무료</p>'+tools.map((n,i)=>`<div class="item"><div class="itembody"><b>${icons[i]} ${n}</b><div class="tabs">${enchantNames[i].map((v,j)=>`<button class="btn small ${s.ench[i]===j?'primary':''}" id="ench${i}${j}" ${s.owned[i]?'':'disabled'}>${v}${s.unlocked.includes(i*2+j)?' ✓':''}</button>`).join('')}</div></div></div>`).join(''));tools.forEach((n,i)=>[0,1].forEach(j=>bind('ench'+i+j,()=>{if(!atBench())return;if(C.enchant(s,i,j)){if(i===3)s.shield=j===1?2:1;done();enchantUI();toast(enchantNames[i][j]+' 적용!');}else toast('문제와 퍼즐을 풀어 10 XP를 모아주세요.');})));}
function mapUI(){open('map','지나온 길과 다음 목표',`<p class="sub">${esc(C.goal(s)[2])}<br>지도를 눌러도 순간이동하지 않습니다. 사다리를 찾아 이동해요.</p>`+[2,1,0].map(f=>`<b>${f+1}층 ${['작업대와 저울','거울과 발판','수호 골렘'][f]}</b><div class="map">${Array.from({length:5},(_,i)=>{let id=f*5+i,here=Math.floor((s.y+.25)/8)===f&&Math.floor(s.x/12)===i;return`<div class="room ${here?'here':s.visited.includes(id)?'':'unknown'}">${here?'모모':s.visited.includes(id)?['보물','작업대','장치','갈림길','사다리'][i]:'?'}</div>`;}).join('')}</div>`).join('')+`<p class="help">${esc(C.waypoint(s)[2])}<br>다음 방향: ${C.waypoint(s)[0]<s.x?'← 왼쪽':'오른쪽 →'}</p>`);}
function questionUI(id){if(!s.question||s.question.id!==id||s.question.solved)s.question=campaign.chapter==='chapter-01'?C.question(s.seed,id,has(id)?1+(s.flags.length%5):id==='math'?0:2):C.chapterQuestion(s.seed,campaign.chapter,id,2);save();let q=s.question,answer='',locked=false,stamp;open('question','숫자 장치',`<div class="quizrow"><div><span class="tag">${id.startsWith('boss')?'골렘 보호막':'모모의 숫자 탐험'}</span><div class="equation">${esc(q.prompt)}</div><div class="answer" id="answer" aria-live="polite">?</div><div class="feedback" id="feedback">숫자를 입력해 주세요</div><button class="linklike" id="hint">모모의 힌트</button></div><div class="keys">${['1','2','3','4','5','6','7','8','9','⌫','0','확인'].map((v,i)=>`<button class="btn ${i===11?'primary':''}" id="key${i}">${v}</button>`).join('')}</div></div>`);stamp=seq;function render(){$('answer').textContent=answer||'?';}function submit(){if(locked||!answer)return;if(C.check(q,answer)){locked=true;q.solved=true;if(campaign.chapter==='chapter-01')C.complete(s,id);else MMRuntime.completeStep(campaign,id);done();$('feedback').textContent='정답이에요! 잘했어요.';sheet.querySelectorAll('.keys button').forEach(b=>b.disabled=true);closeTimer=setTimeout(()=>{if(seq===stamp){if(id==='math'&&!s.owned[0]){open('craft-guide','첫 제작 미션',`<div class="help"><b>지금 만들 것: ⛏ 곡괭이 팔</b><br><br>필요한 재료: 철 조각 2 + 철판 1<br>방금 문제를 풀어서 재료가 준비됐어요.<br><br><b>← 왼쪽 작업대</b>로 돌아가서 <b>지금 만들기</b>를 눌러보세요.</div><div class="stack"><button class="btn primary wide" id="craftGuideClose">작업대로 가기 ←</button></div>`);bind('craftGuideClose',hide);}else{hide();toast('정답! 탐험을 계속해요.');}}},650);}else{$('feedback').textContent='괜찮아요. 다시 생각해 볼까요?';answer='';render();}}
for(let i=0;i<12;i++)bind('key'+i,()=>{if(locked)return;if(i<9&&answer.length<3)answer+=i+1;else if(i===9)answer=answer.slice(0,-1);else if(i===10&&answer.length<3)answer+='0';else if(i===11)submit();if(seq===stamp)render();});let hints=0;bind('hint',()=>{if(locked)return;hints++;$('feedback').textContent=hints>1?'함께 확인해요: '+q.answer:q.op===0?'10을 먼저 만들고 더해 보세요.':q.op===3?'같은 크기의 묶음으로 나눠 보세요.':'숫자를 작은 묶음으로 나눠 생각해요.';});}
function campaignThings(){
 const ch=campaign.chapter;
 if(ch==='chapter-02')return[
 ['workshop',18,0,'작업대','bench'],['c2.intro',14,0,'멈춘 발전 제어판','terminal'],['c2.genA',30,0,'발전기 A','terminal'],
 ['ladder',54,0,'2층 사다리','sign'],['c2.genB',10,8,'발전기 B','terminal'],['c2.bridge',46,8,'용암 다리 제어기','terminal'],
 ['boss.math',38,16,'곱셈 보호막','terminal'],['boss.power',48,16,'배관 보호막','mirror'],['boss.plates',46,16,'2 → 3 → 5','sign'],
 ['chapterBoss',57,16,'용광로 골렘','golem'],['checkpoint',52,8,'체크포인트','flag'],['checkpointTop',34,16,'체크포인트','flag']
 ];
 if(ch==='chapter-03')return[
 ['workshop',18,0,'작업대','bench'],['c3.intro',14,0,'높은 창고 입구','terminal'],['c3.sokoban',34,0,'상자 정리 퍼즐','sign'],
 ['ladder',54,0,'2층 사다리','sign'],['c3.memory',22,8,'기억 경로','sign'],['c3.lift',50,8,'높은 리프트','terminal'],
 ['boss.sokoban',36,16,'상자 보호막','sign'],['boss.memory',45,16,'기억 보호막','sign'],['boss.missing',54,16,'빈칸 보호막','terminal'],
 ['chapterBoss',58,16,'창고 관리자 로봇','golem'],['checkpoint',52,8,'체크포인트','flag'],['checkpointTop',31,16,'체크포인트','flag']
 ];
 if(ch==='chapter-04')return[
 ['workshop',18,0,'작업대','bench'],['c4.intro',14,0,'잠긴 회로문','terminal'],['c4.rule',34,0,'규칙 기계','terminal'],
 ['ladder',54,0,'2층 사다리','sign'],['c4.shield',18,8,'전기 통로','terminal'],['c4.laser',44,8,'수정 다중 거울','mirror'],
 ['boss.rule',36,16,'규칙 보호막','terminal'],['boss.laser',46,16,'다중 거울 보호막','mirror'],['boss.switch',54,16,'조건 스위치','terminal'],
 ['chapterBoss',58,16,'수정 코어 수호자','golem'],['checkpoint',52,8,'체크포인트','flag'],['checkpointTop',31,16,'체크포인트','flag']
 ];
 if(ch==='chapter-05')return[
 ['workshop',18,0,'작업대','bench'],['c5.number',14,0,'별빛 숫자 엔진','terminal'],['c5.equipment',30,0,'장비 선택 게이트','terminal'],['c5.spatial',46,0,'공간 상자 퍼즐','sign'],
 ['ladder',54,0,'2층 사다리','sign'],['c5.memory',20,8,'별빛 기억 경로','sign'],
 ['boss.number',34,16,'숫자 엔진 보호막','terminal'],['boss.equipment',40,16,'장비 보호막','terminal'],['boss.spatial',46,16,'공간 보호막','sign'],['boss.memory',52,16,'기억 보호막','sign'],['boss.core',56,16,'별빛 코어','terminal'],
 ['chapterBoss',58,16,'별빛 코어 골렘','golem'],['checkpoint',50,8,'체크포인트','flag'],['checkpointTop',30,16,'체크포인트','flag']
 ];
 return[];
}
function bossRemaining(){let d=MMChapters.get(campaign.chapter);return d?d.boss.phases.filter(k=>!has(k)).length:0;}
function showTutorial(eventId){
 let cue=MMRuntime.tutorialCueFor(campaign,eventId);if(!cue)return false;
 open('tutorial',cue.title,`<div class="help">${esc(cue.text).replace(/\n/g,'<br>')}</div><div class="stack"><button class="btn primary wide" id="tutorialOk">알겠어요</button></div>`);
 bind('tutorialOk',()=>{MMRuntime.ackTutorial(campaign,eventId);hide();});return true;
}
function pipeUI(id){
 let rot=[0,0,0],target=[1,2,1];
 open('pipe','배관 연결',`<p>파이프를 눌러 돌려서 <b>용광로까지 한 줄로 연결</b>해 주세요.</p><div class="recipe">${rot.map((v,i)=>`<button class="btn" id="pipe${i}">┐</button>`).join('')}</div><button class="btn primary wide" id="pipeCheck">연결 확인</button><div class="feedback" id="pipeFeedback"></div>`);
 const glyph=['┐','┘','└','┌'];
 rot.forEach((_,i)=>bind('pipe'+i,()=>{rot[i]=(rot[i]+1)%4;$('pipe'+i).textContent=glyph[rot[i]];}));
 bind('pipeCheck',()=>{if(rot.every((v,i)=>v===target[i])){MMRuntime.completeStep(campaign,id);done();hide();toast('배관 연결 성공! 남은 보호막 '+bossRemaining()+'개');}else $('pipeFeedback').textContent='아직 연결이 끊긴 곳이 있어요. 다시 돌려 보세요.';});
}
function sokobanUI(id){
 let px=0,py=2,bx=1,by=1,gx=2,gy=1;
 const cells=()=>{let out='';for(let y=2;y>=0;y--)for(let x=0;x<3;x++){let v=x===gx&&y===gy?'◎':x===bx&&y===by?'▣':x===px&&y===py?'●':'·';out+='<div class="room" id="soko-'+x+'-'+y+'">'+v+'</div>';}return out;};
 open('sokoban','상자 밀기',`<p>● 탐험가가 ▣ 상자를 밀어서 ◎ 칸에 놓아요.</p><div class="map" id="sokoGrid" style="grid-template-columns:repeat(3,1fr)">${cells()}</div><div class="keys"><button class="btn" id="sUp">↑</button><button class="btn" id="sLeft">←</button><button class="btn" id="sRight">→</button><button class="btn" id="sDown">↓</button></div><button class="linklike" id="sReset">퍼즐만 다시 시작</button>`);
 function render(){let g=$('sokoGrid');if(g)g.innerHTML=cells();}
 function move(dx,dy){let nx=px+dx,ny=py+dy;if(nx<0||nx>2||ny<0||ny>2)return;if(nx===bx&&ny===by){let nbx=bx+dx,nby=by+dy;if(nbx<0||nbx>2||nby<0||nby>2)return;bx=nbx;by=nby;}px=nx;py=ny;render();if(bx===gx&&by===gy){MMRuntime.completeStep(campaign,id);done();hide();toast('상자 퍼즐 성공!');}}
 bind('sUp',()=>move(0,1));bind('sDown',()=>move(0,-1));bind('sLeft',()=>move(-1,0));bind('sRight',()=>move(1,0));bind('sReset',()=>sokobanUI(id));
}
function memoryUI(id){
 const sequence=MMRuntime.memorySequence(s.seed,id),symbols=['▲','■','●','◆'];let input=[],locked=true;
 open('memory','기억 경로',`<p>잠깐 보이는 3개의 순서를 기억해요.</p><div class="equation" id="memoryShow">${sequence.map(i=>symbols[i]).join('  ')}</div><div class="keys">${symbols.map((v,i)=>'<button class="btn" id="mem'+i+'" disabled>'+v+'</button>').join('')}</div><div class="feedback" id="memoryFeedback">순서를 기억해 주세요...</div>`);
 setTimeout(()=>{if(modal!=='memory')return;locked=false;$('memoryShow').textContent='?  ?  ?';symbols.forEach((_,i)=>$('mem'+i).disabled=false);$('memoryFeedback').textContent='같은 순서로 눌러 보세요.';},900);
 symbols.forEach((_,i)=>bind('mem'+i,()=>{if(locked)return;input.push(i);if(input[input.length-1]!==sequence[input.length-1]){input=[];$('memoryFeedback').textContent='괜찮아요. 처음부터 다시 해요.';return;}if(input.length===sequence.length){MMRuntime.completeStep(campaign,id);done();hide();toast('기억 경로 성공!');}}));
}
function ruleUI(id){
 const p=MMRuntime.rulePuzzle(s.seed,id);
 open('rule','규칙 기계',`<p class="help">${esc(p.clue)}</p><div class="stack">${p.options.map((o,i)=>'<button class="btn wide" id="rule'+i+'">'+esc(o.label)+'</button>').join('')}</div><div class="feedback" id="ruleFeedback">조건을 잘 읽고 골라보세요.</div>`);
 p.options.forEach((o,i)=>bind('rule'+i,()=>{if(o.correct){MMRuntime.completeStep(campaign,id);done();hide();toast('규칙을 찾았어요!');}else $('ruleFeedback').textContent='그 답은 조건과 달라요. 벌점 없이 다시 골라요.';}));
}
function multiLaserUI(id){
 let rot=[0,0,0,0],target=[1,3,2,1],glyph=['╱','—','╲','│'];
 open('laser-grid','수정 거울 연결',`<p>네 거울을 돌려 빛이 코어까지 이어지게 해요.</p><div class="recipe">${rot.map((v,i)=>'<button class="btn" id="laser'+i+'">'+glyph[v]+'</button>').join('')}</div><button class="btn primary wide" id="laserCheck">빛 연결 확인</button><div class="feedback" id="laserFeedback"></div>`);
 rot.forEach((_,i)=>bind('laser'+i,()=>{rot[i]=(rot[i]+1)%4;$('laser'+i).textContent=glyph[rot[i]];}));
 bind('laserCheck',()=>{if(rot.every((v,i)=>v===target[i])){MMRuntime.completeStep(campaign,id);done();hide();toast('수정 빛 연결 성공!');}else $('laserFeedback').textContent='빛이 끊긴 거울이 있어요. 다시 돌려 보세요.';});
}
function switchUI(id){
 const correct=((s.seed+id.length)%3+3)%3,labels=['파란 스위치','노란 스위치','초록 스위치'];
 open('switch','조건 스위치',`<p class="help">힌트: <b>${labels[correct]}</b>는 짝수 신호와 연결되어 있어요. 조건에 맞는 스위치를 선택하세요.</p><div class="stack">${labels.map((x,i)=>'<button class="btn wide" id="switch'+i+'">'+x+'</button>').join('')}</div><div class="feedback" id="switchFeedback"></div>`);
 labels.forEach((_,i)=>bind('switch'+i,()=>{if(i===correct){MMRuntime.completeStep(campaign,id);done();hide();toast('조건 스위치 성공! 남은 보호막 '+bossRemaining()+'개');}else $('switchFeedback').textContent='조건과 맞지 않아요. 다시 생각해 봐요.';}));
}
function equipmentTrialUI(id){
 const order=id==='boss.equipment'?[4,3,2,1,0]:[0,1,2,3,4],labels=['곡괭이 팔','파워 렌치','점프 부스터','에너지 실드','탐험 센서'];let step=0;
 open('equipment-trial','장비 선택 게이트',`<p>표시된 상황에 맞는 장비를 순서대로 선택해요.</p><div class="help center" id="equipClue"></div><div class="grid5">${labels.map((x,i)=>'<button class="btn wide" id="trialTool'+i+'">'+x+'</button>').join('')}</div><div class="feedback" id="equipFeedback"></div>`);
 const clues=['균열벽을 열려면?','고장 난 기계를 고치려면?','높은 발판에 오르려면?','위험을 한 번 막으려면?','숨은 단서를 찾으려면?'];
 function render(){let need=order[step];$('equipClue').textContent=(step+1)+' / 5 · '+clues[need];}
 labels.forEach((_,i)=>bind('trialTool'+i,()=>{let need=order[step];if(i!==need){$('equipFeedback').textContent='이 상황에 더 잘 맞는 장비를 다시 골라봐요.';return;}if(!s.owned[i]){$('equipFeedback').textContent='아직 만들지 않은 장비예요. 작업대에서 먼저 제작해요.';return;}s.equipped[i]=true;step++;if(step===order.length){MMRuntime.completeStep(campaign,id);done();hide();toast('장비 선택 성공! 모든 도구를 잘 기억했어요.');}else{render();$('equipFeedback').textContent='좋아요! 다음 상황이에요.';}}));render();
}
function coreInspectUI(id){
 open('core','별빛 코어',`<div class="hero"><div class="face"></div><div class="glass"></div></div><p class="center">마지막 코어가 안정되려면 직접 조사해야 해요.</p><button class="btn primary wide" id="coreInspect">별빛 코어 조사하기</button>`);
 bind('coreInspect',()=>{MMRuntime.completeStep(campaign,id);done();hide();toast('마지막 보호막 해제! 골렘에게 다가가 조사해요.');});
}
function finishCampaignChapter(){
 if(bossRemaining()>0){toast('아직 보호막이 '+bossRemaining()+'개 남았어요. 상단 목표를 따라가요.');return;}
 if(!has('clear'))s.flags.push('clear');
 C.markChapterComplete(campaign,campaign.chapter);save();
 const d=MMChapters.get(campaign.chapter),next=campaign.unlockedChapters.find(x=>!campaign.completedChapters.includes(x)),allDone=campaign.completedChapters.length>=5;
 open('clear','챕터 완료!',`<div class="hero"><div class="face"></div><div class="glass"></div></div><h1 class="center">${d.title} 탐험 성공!</h1><p class="center">${allDone?'모모의 별빛 코어를 모두 되찾았어요!':'모모와 함께 모든 장치를 해결했어요.'}</p><div class="help center">지식 경험치 ${s.xp} XP · 완료 챕터 ${campaign.completedChapters.length}/5</div><div class="stack">${next?'<button class="btn primary wide" id="nextChapter">다음 챕터로</button>':''}<button class="btn wide" id="chapters">챕터 선택</button></div>`);
 if(next)bind('nextChapter',()=>switchChapter(next));bind('chapters',chapterSelect);
}
function interactCampaign(){
 if(!near)return;let id=near[0];
 if(id==='workshop'){workshop();return;}
 if(id==='ladder'){toast('사다리 앞에서 ↑ 또는 ↓를 누르고 있어요.');return;}
 if(id.startsWith('checkpoint')){s.checkpoint=[near[1],near[2]];s.health=5;done();toast('체크포인트 저장!');return;}
 if(id==='c2.intro'){
   if(!has(id)){MMRuntime.completeStep(campaign,id);s.items[0]+=2;s.items[2]+=1;s.items[3]+=1;done();if(!showTutorial('first-wrench'))toast('작업대에서 파워 렌치를 만들어요.');}
   else toast('발전기가 멈췄어요. 파워 렌치로 수리해요.');return;
 }
 if(id==='c2.genA'||id==='c2.genB'){
   if(!C.tool(s,1)){toast('파워 렌치를 장착해야 발전기를 고칠 수 있어요.');showTutorial('first-wrench');return;}
   if(!has(id)){MMRuntime.completeStep(campaign,id);s.xp+=10;done();toast((id.endsWith('A')?'발전기 A':'발전기 B')+' 수리 완료!');}else toast('이미 수리한 발전기예요.');return;
 }
 if(id==='c2.bridge'){
   if(!has('c2.genA')||!has('c2.genB')){toast('발전기 A와 B를 모두 수리해야 해요.');return;}
   if(!has(id)){MMRuntime.completeStep(campaign,id);done();toast('용암 다리 작동! 3층 보스로 가요.');}return;
 }
 if(id==='boss.math'){questionUI(id);return;}
 if(id==='boss.power'){if(has(id))toast('배관 보호막은 이미 풀렸어요.');else pipeUI(id);return;}
 if(id==='boss.plates'){toast('바닥의 2 → 3 → 5 발판을 순서대로 직접 밟아요.');return;}
 if(id==='c3.intro'){
   if(!has(id)){MMRuntime.completeStep(campaign,id);s.items[0]+=2;s.items[2]+=2;s.items[3]+=1;done();if(!showTutorial('first-booster'))toast('작업대에서 점프 부스터를 만들어요.');}
   else toast('높은 창고를 가려면 점프 부스터가 필요해요.');return;
 }
 if(id==='c3.sokoban'||id==='boss.sokoban'){if(!has(id)){showTutorial('first-sokoban');sokobanUI(id);}else toast('이미 해결한 상자 퍼즐이에요.');return;}
 if(id==='c3.memory'||id==='boss.memory'){if(!has(id)){showTutorial('first-memory');memoryUI(id);}else toast('이미 해결한 기억 경로예요.');return;}
 if(id==='c3.lift'){if(!C.tool(s,2)){toast('점프 부스터를 장착해 주세요.');showTutorial('first-booster');return;}if(!has(id)){MMRuntime.completeStep(campaign,id);done();toast('높은 리프트에 도착했어요! 3층으로 올라가요.');}return;}
 if(id==='boss.missing'){questionUI(id);return;}
 if(id==='c4.intro'){
   if(!has(id)){MMRuntime.completeStep(campaign,id);s.items[0]+=2;s.items[1]+=2;s.items[2]+=2;s.items[3]+=3;done();if(!showTutorial('first-sensor'))toast('작업대에서 탐험 센서를 만들어요.');}
   else toast('센서로 숨은 규칙 단서를 찾을 수 있어요.');return;
 }
 if(id==='c4.rule'||id==='boss.rule'){if(!has(id)){showTutorial('first-rule');ruleUI(id);}else toast('이미 해결한 규칙 기계예요.');return;}
 if(id==='c4.shield'){
   if(!C.tool(s,3)){toast('전기 통로는 에너지 실드를 장착하면 안전해요. 작업대에서 제작해 보세요.');showTutorial('first-shield');return;}
   if(!has(id)){MMRuntime.completeStep(campaign,id);done();toast('실드로 전기 구간을 안전하게 통과했어요!');}return;
 }
 if(id==='c4.laser'||id==='boss.laser'){if(!has(id))multiLaserUI(id);else toast('수정 빛이 이미 연결되어 있어요.');return;}
 if(id==='boss.switch'){if(!has(id))switchUI(id);else toast('조건 스위치가 이미 맞춰졌어요.');return;}
 if(id==='c5.number'||id==='boss.number'){questionUI(id);return;}
 if(id==='c5.equipment'||id==='boss.equipment'){if(!has(id))equipmentTrialUI(id);else toast('장비 게이트는 이미 해결했어요.');return;}
 if(id==='c5.spatial'||id==='boss.spatial'){if(!has(id))sokobanUI(id);else toast('공간 퍼즐은 이미 해결했어요.');return;}
 if(id==='c5.memory'||id==='boss.memory'){if(!has(id))memoryUI(id);else toast('기억 퍼즐은 이미 해결했어요.');return;}
 if(id==='boss.core'){if(!has(id))coreInspectUI(id);else toast('별빛 코어는 안정됐어요.');return;}
 if(id==='chapterBoss'){finishCampaignChapter();return;}
}

function things(){if(campaign.chapter!=='chapter-01')return campaignThings();let a=[['workshop',18,0,'작업대','bench'],['math',30,0,'숫자 장치','terminal'],['mine',34,0,'균열벽','rock'],['scale',44,0,'8kg 저울','scale'],['ladder',54,0,'2층 사다리','sign'],['mirrorA',44,8,'거울 A','mirror'],['mirrorB',48,8,'거울 B','mirror'],['checkpoint',52,8,'체크포인트','flag'],['sequence',18,8,'2 → 4 → 6','sign'],['repair',13.8,8,'수리 통로','bench'],['cacheA',5,8,'부품 보물','chest'],['cacheB',24,16,'부품 보물','chest'],['boss.math',38,16,'숫자 보호막','terminal'],['boss.sequence',44,16,'2 → 4 → 6','sign'],['boss.mirror',54,16,'거울 보호막','mirror'],['boss',57,16,'수호 골렘','golem'],['checkpointTop',32,16,'체크포인트','flag'],['highCache',6,19,'높은 보물','chest']];if(!has('bridge'))[38,40,42].forEach((x,i)=>{if(carry!==i&&!weights[i])a.push(['box'+i,x,0,[3,5,2][i]+'kg 상자','crate',i]);});return a;}
function interact(){if(!running||modal||!near)return;if(campaign.chapter!=='chapter-01')return interactCampaign();let id=near[0];if(id==='workshop'){workshop();return;}if(id==='math'||id==='boss.math'){questionUI(id);return;}if(id==='mine'){if(has('mined'))toast('열린 통로예요.');else if(C.tool(s,0)){s.flags.push('mined');done();toast('균열벽이 열렸어요! 오른쪽 저울로 가요.');}else toast('왼쪽 작업대에서 곡괭이 팔을 만들고 장착해 주세요.');return;}
if(id.startsWith('box')){if(carry>=0){toast('들고 있는 상자를 저울에 먼저 내려놓아요.');return;}carry=Number(id.slice(3));toast([3,5,2][carry]+'kg 상자를 들었어요. 저울에서 조사하세요.');return;}
if(id==='scale'){if(has('bridge')){toast('다리가 이미 열렸어요.');return;}if(carry>=0){weights[carry]=true;carry=-1;}else{let i=weights.lastIndexOf(true);if(i>=0){weights[i]=false;carry=i;}}let kg=weights.reduce((n,b,i)=>n+(b?[3,5,2][i]:0),0);if(kg===8){C.complete(s,'bridge');done();toast('8kg! 다리를 건너 사다리로 올라가요.');}else toast('현재 '+kg+' / 8kg'+(carry>=0?' · 상자를 하나 꺼냈어요.':''));return;}
if(id==='mirrorA'||id==='mirrorB'){if(has('laser')){toast('전원이 연결되어 있어요.');return;}let i=id==='mirrorA'?0:1;mirrors[i]=!mirrors[i];if(mirrors[0]&&mirrors[1]){C.complete(s,'laser');done();toast('전원 연결! 이제 왼쪽 문을 지날 수 있어요.');}else toast('거울을 돌렸어요. 빛이 어디로 가는지 살펴보세요.');return;}
if(id==='boss.mirror'){if(!has('boss.laser')){mirrors[2]=!mirrors[2];if(mirrors[2]){C.complete(s,'boss.laser');done();toast(C.shields(s)===0?'보호막 0개! 오른쪽 수호 골렘에게 가서 조사 버튼을 눌러요.':'골렘의 거울 보호막 해제! 남은 보호막 '+C.shields(s)+'개');}}return;}
if(id==='sequence'||id==='boss.sequence'){toast('2 → 4 → 6 순서로 직접 밟아요. 다른 숫자는 점프로 넘어요.');return;}
if(id.startsWith('checkpoint')){s.checkpoint=[near[1],near[2]];s.health=5;s.shield=C.hasEnchant(s,3,1)?2:1;done();toast('체크포인트 저장! 체력도 회복했어요.');return;}
if(id==='repair'){if(has('repaired'))toast('수리된 통로예요.');else if(!C.tool(s,1))toast('작업대에서 파워 렌치를 만들어 보세요.');else if(++repair>=2||C.hasEnchant(s,1,0)){s.flags.push('repaired');done();toast('보물 통로를 수리했어요!');}else toast('회로 하나 연결! 한 번 더 조사해요.');return;}
if(id.includes('Cache')||id.startsWith('cache')){if(id==='highCache'&&!C.tool(s,2)){toast('점프 부스터를 장착해 주세요.');return;}if(C.claim(s,id,12,[5+(C.hasEnchant(s,0,1)?1:0),3,3,3,0])){done();toast('보물 발견! 제작 재료와 경험치 획득!');}else toast('이미 연 보물상자예요.');return;}
if(id==='boss'){if(C.finish(s)){C.markChapterComplete(campaign,campaign.chapter);save();done();winNotified=true;let next=campaign.unlockedChapters.find(x=>!campaign.completedChapters.includes(x));open('clear','탐험 성공!',`<div class="hero"><div class="face"></div><div class="glass"></div></div><h1 class="center">잘했어요, 탐험가!</h1><p class="center">세 개의 보호막을 모두 풀었어요.<br>골렘: “모모를 도와주셔서 감사합니다!”</p><div class="help center">챕터 1 완료 · 코어 +1<br>지식 경험치 ${s.xp} XP</div><div class="stack">${next?'<button class="btn primary wide" id="nextChapter">다음 챕터로</button>':''}<button class="btn wide" id="keep">남은 보물 찾아보기</button><button class="btn wide" id="home">처음 화면</button></div>`);if(next)bind('nextChapter',()=>switchChapter(next));bind('keep',hide);bind('home',()=>{running=false;title();});}else toast('골렘: 숫자·발판·거울 장치를 풀어 주시겠습니까? 남은 보호막 '+C.shields(s)+'개');return;}
if(id==='ladder')toast('여기서 ↑ 버튼을 누르고 있으면 올라가요.');}
function switchChapter(id){save();if(!C.startChapter(campaign,id)){toast('아직 잠긴 챕터예요.');return false;}install(campaign);running=true;hide();save();let d=window.MMChapters&&MMChapters.get(id);toast((d?d.title:id)+' 탐험을 시작해요!');return true;}
function chapterSelect(){let list=MMChapters.list();open('chapters','챕터 선택',`<div class="grid5">${list.map(d=>{let unlocked=campaign.unlockedChapters.includes(d.id),done=campaign.completedChapters.includes(d.id);return '<div class="item" id="'+d.id.replace('-','')+'"><div class="itembody"><b>'+d.order+'장 · '+d.title+'</b><small>'+(done?'완료':unlocked?'도전 가능':'잠김')+'</small></div><button class="btn small" id="go-'+d.id+'" '+(unlocked?'':'disabled')+'>'+(done?'다시 하기':unlocked?'시작':'잠김')+'</button></div>';}).join('')}</div>`);list.forEach(d=>bind('go-'+d.id,()=>switchChapter(d.id)));}
function plates(f){let nums=campaign.chapter==='chapter-02'&&f===2?[2,3,5]:[2,4,6];let r=C.rng(s.seed+f*777);for(let i=2;i>0;i--){let j=r(i+1);[nums[i],nums[j]]=[nums[j],nums[i]];}return nums.map((n,i)=>({x:(f===1?16:42)+i*4,y:f*8,n}));}
function tick(dt){if(!running||modal||document.hidden)return;let directions=[...held.values(),...keys],h=+(directions.includes('right'))-+(directions.includes('left')),v=+(directions.includes('up'))-+(directions.includes('down'));let prevY=s.y;C.move(s,h,v,jumpQueued,dt);jumpQueued=false;walk+=Math.abs(h)*dt;
if(prevY< -2&&s.y>=0){if(C.tool(s,3)&&s.shield>0)s.shield--;else if(!C.hasEnchant(s,2,0))s.health=s.health<=1?5:s.health-1;toast('체크포인트로 돌아왔어요. 장비는 그대로예요.');}
near=null;let d=1.9;for(let t of things()){if(Math.abs(t[2]-s.y)>1.8)continue;let q=Math.abs(t[1]-s.x);if(q<d){near=t;d=q;}}
let hit='';if(s.grounded)for(let f of [1,2]){if(campaign.chapter==='chapter-02'&&f===1)continue;for(let p of plates(f)){if(Math.abs(s.y-p.y)<.12&&Math.abs(s.x-p.x)<.65){hit=f+':'+p.x;let id=campaign.chapter==='chapter-02'?'boss.plates':(f===1?'sequence':'boss.sequence'),expected=campaign.chapter==='chapter-02'?[2,3,5]:[2,4,6];if(hit!==lastPlate&&!has(id)){if(p.n===expected[steps[f-1]]){steps[f-1]++;toast('발판 '+p.n+' · '+steps[f-1]+'/3');if(steps[f-1]===3){if(campaign.chapter==='chapter-01')C.complete(s,id);else MMRuntime.completeStep(campaign,id);done();toast(campaign.chapter==='chapter-02'?'발판 보호막 해제! 오른쪽 골렘에게 가요.':f===1?'사다리와 작업대 지름길이 열렸어요!':'골렘의 발판 보호막 해제!');}}else{steps[f-1]=0;toast('다시 '+expected[0]+'부터 시작해요. 다른 발판은 점프로 넘어요.');}}}}}
lastPlate=hit;if(time-lastSave>6)save();refresh();}
function refresh(){let f=Math.max(0,Math.min(3,Math.floor((s.y+.25)/8))),g=campaign&&campaign.v===2&&window.MMRuntime?MMRuntime.waypoint(campaign):C.waypoint(s),text=(g.x!==undefined?(g.x<s.x?'← ':'→ ')+g.text:(g[0]<s.x?'← ':'→ ')+g[2]);$('goal').textContent=running?text:'모모와 블록 미로로 떠나요';$('status').textContent=(f+1)+'층 · '+(MMChapters.get(campaign.chapter)?.title||'탐험')+' · '+'♥'.repeat(s.health||5)+' · '+s.xp+' XP';$('near').textContent=running&&near?near[3]+' · 조사':'';$('near').style.display=running&&near?'block':'none';$('use').textContent=carry>=0?'내려놓기':'조사 E';let sig=s.owned.join()+s.equipped.join();if(sig!==lastUi){lastUi=sig;$('hotbar').innerHTML=icons.map((v,i)=>`<button class="slot ${i===5?'':s.equipped[i]?'on':'dim'}" aria-label="${i===5?'가방':tools[i]}">${v}<span class="num">${i+1}</span></button>`).join('');$('hotbar').querySelectorAll('button').forEach(b=>b.onclick=bag);}}
function resize(){let r=stage.getBoundingClientRect();W=Math.max(1,r.width);H=Math.max(1,r.height);DPR=Math.min(devicePixelRatio||1,2);canvas.width=Math.round(W*DPR);canvas.height=Math.round(H*DPR);U=Math.max(28,Math.min(48,H/10.5));}
const X=x=>(x-cx)*U+W/2,Y=y=>H/2-(y-cy)*U;
function rect(x,y,w,h,col){ctx.fillStyle=col;ctx.fillRect(Math.round(X(x)),Math.round(Y(y+h)),Math.ceil(w*U),Math.ceil(h*U));}
function line(points,col,width=.08){ctx.strokeStyle=col;ctx.lineWidth=Math.max(2,width*U);ctx.beginPath();points.forEach((p,i)=>i?ctx.lineTo(X(p[0]),Y(p[1])):ctx.moveTo(X(p[0]),Y(p[1])));ctx.stroke();}
function label(text,x,y,size=14,col='#fff3d0'){ctx.font=`750 ${size}px -apple-system,BlinkMacSystemFont,"Apple SD Gothic Neo",sans-serif`;ctx.textAlign='center';ctx.lineWidth=3;ctx.strokeStyle='#244032';ctx.strokeText(text,X(x),Y(y));ctx.fillStyle=col;ctx.fillText(text,X(x),Y(y));}
function crate(x,y,kg){rect(x-.43,y,.86,.84,'#435039');rect(x-.35,y+.08,.7,.68,'#ad7c4f');rect(x-.31,y+.57,.62,.12,'#e4b674');rect(x-.3,y+.24,.6,.07,'#684c33');if(kg)label(kg+'',x,y+.25,16);}
function robot(x,y,big=false){let b=big?1:0,wave=running&&!modal?Math.sin(walk*16)*.06:0;rect(x-.28,y,.22,.32+wave,'#4d694c');rect(x+.06,y,.22,.32-wave,'#4d694c');rect(x-.33,y+.31,.66,.68,'#465a3d');rect(x-.26,y+.36,.52,.59,'#94b871');rect(x-.48,y+.43,.16,.5,'#e4d9b1');rect(x+.32,y+.43,.16,.5,'#e4d9b1');rect(x-.53,y+.97,1.06,.66,'#344b3a');rect(x-.47,y+1.03,.94,.53,'#f8edc9');rect(x-.37,y+1.14,.74,.28,'#8fbfc2');rect(x-.23,y+1.19,.10,.13,'#2c4640');rect(x+.14,y+1.19,.10,.13,'#2c4640');rect(x-.32,y+.91,.65,.14,'#dba460');rect(x-.07,y+.61,.14,.19,'#e9ce70');}
function drawThing(t){let[id,x,y,n,k,i]=t,col=has(id)?'#8dab65':'#d0ad59';if(k==='bench'){rect(x-.8,y,.16,.55,'#674f37');rect(x+.63,y,.16,.55,'#674f37');rect(x-.9,y+.55,1.8,.24,'#e0b071');rect(x-.55,y+.8,1.1,.22,'#58724f');label('⚒',x,y+1,21,'#f2d99b');}
else if(k==='terminal'){rect(x-.36,y,.72,.45,'#63745b');rect(x-.56,y+.45,1.12,1.15,'#344f3c');rect(x-.44,y+.66,.88,.76,has(id)?'#aed580':'#a4d5d5');label(has(id)?'✓':'?',x,y+.87,23,has(id)?'#f0f7dd':'#fff8d3');}
else if(k==='rock'){if(!has('mined')){rect(x-.4,y,.8,1.4,'#6d7b67');line([[x+.1,y+1.2],[x-.1,y+.8],[x+.2,y+.6],[x,y+.1]],'#3c4e3a');}}
else if(k==='crate')crate(x,y,[3,5,2][i]);
else if(k==='scale'){rect(x-.65,y,1.3,.25,'#435b41');rect(x-.1,y+.25,.2,.7,'#d8c486');rect(x-.78,y+.94,1.56,.15,'#e7d7aa');let kg=weights.reduce((n,b,i)=>n+(b?[3,5,2][i]:0),0);label(has('bridge')?'8/8':' '+kg+'/8',x,y+1.42,16);weights.forEach((b,i)=>{if(b)crate(x-.42+i*.42,y+1.1,0);});}
else if(k==='mirror'){rect(x-.25,y,.5,.3,'#59764e');rect(x-.075,y+.3,.15,2.7,'#918166');let on=id==='mirrorA'?mirrors[0]:id==='mirrorB'?mirrors[1]:mirrors[2],my=id==='boss.mirror'?y+1:y+3;rect(x-.38,my-.38,.76,.76,'#324f4d');rect(x-.3,my-.3,.6,.6,'#a5d9db');line(on?[[x-.3,my-.3],[x+.3,my+.3]]:[[x-.3,my+.3],[x+.3,my-.3]],'#fef9d6',.09);}
else if(k==='flag'){rect(x-.04,y,.09,1.8,'#58674b');rect(x+.05,y+1.1,.65,.65,'#a9c96f');rect(x+.05,y+1.66,.65,.1,'#d3e4a0');}
else if(k==='chest'){crate(x,y,0);rect(x-.08,y+.23,.16,.36,'#ffe09d');if(has('reward/'+id))rect(x-.43,y+.86,.86,.16,'#bd945a');}
else if(k==='golem'){let sh=C.shields(s);rect(x-.8,y,.55,.66,'#6e8670');rect(x+.23,y,.55,.66,'#6e8670');rect(x-.9,y+.62,1.8,1.6,'#7e967c');rect(x-1.2,y+.87,.38,1.35,'#97ad8b');rect(x+.82,y+.87,.38,1.35,'#97ad8b');rect(x-1,y+2.18,2,1.1,'#394f3a');rect(x-.9,y+2.28,1.8,.86,'#c3cfaa');rect(x-.6,y+2.65,.18,.24,'#354e39');rect(x+.42,y+2.65,.18,.24,'#354e39');rect(x-.16,y+2.44,.32,.1,'#69784c');rect(x-.24,y+1.16,.48,.55,'#ecc769');rect(x-.85,y+3.13,1.7,.2,'#668e49');label('◈'.repeat(sh)||'감사합니다!',x,y+3.85,18,'#ffe698');}
else{rect(x-.055,y,.11,1,'#8e7250');rect(x-.5,y+1,1,.57,'#d9b879');label(id.includes('sequence')?'2·4·6':'↑',x,y+1.15,14);}
if(near&&near[0]===id){ctx.strokeStyle='#fff4b6';ctx.lineWidth=2;ctx.strokeRect(X(x-.95),Y(y+2.1),1.9*U,2.2*U);}}
function draw(){ctx.setTransform(DPR,0,0,DPR,0,0);ctx.clearRect(0,0,W,H);let sky=ctx.createLinearGradient(0,0,0,H);sky.addColorStop(0,'#96b6b1');sky.addColorStop(.6,'#729789');sky.addColorStop(1,'#486b55');ctx.fillStyle=sky;ctx.fillRect(0,0,W,H);
let left=Math.max(0,Math.floor(cx-W/U/2)-1),right=Math.min(60,Math.ceil(cx+W/U/2)+1);for(let f=0;f<3;f++){let y=f*8;for(let x=left;x<right;x++){if((x+f)%6===0){rect(x,y+.6,.6,5.8,'#547763');rect(x+.1,y+.6,.14,5.8,'#648775');}if(f===0&&x>=47&&x<50&&!has('bridge'))continue;rect(x,y-.75,1,.75,f===0?'#8a7152':'#718369');rect(x+.04,y-.38,.91,.30,f===0?'#a58a61':'#93a183');rect(x,y-.06,1,.13,'#82a354');rect(x+.06,y-.01,.45,.1,'#afd080');if((x+f)%8===0){rect(x+.3,y+2,.14,.75,'#48583d');rect(x+.15,y+2.65,.46,.15,'#364935');rect(x+.21,y+2.2,.35,.5,'#dfba6b');}}}
for(let f of C.solids(s)){if(f[4]||f[0]<0||f[0]>=60)continue;for(let y=f[1];y<f[3];y+=.75){rect(f[0],y,f[2]-f[0],.71,'#596b56');rect(f[0]+.05,y+.08,Math.max(.1,f[2]-f[0]-.1),.52,'#a0a58b');}label('🔒',(f[0]+f[2])/2,f[1]+2.8,19);}
for(let l of C.ladders(s)){rect(l[0]-.28,l[1],.10,l[2]-l[1],'#d1aa73');rect(l[0]+.18,l[1],.10,l[2]-l[1],'#d1aa73');for(let y=l[1]+.3;y<l[2];y+=.46)rect(l[0]-.2,y,.4,.1,'#ebc897');}
for(let f of [1,2]){let id=f===1?'sequence':'boss.sequence';for(let p of plates(f)){rect(p.x-.65,p.y,1.3,.12,has(id)?'#b1d474':'#c7a261');label(p.n,p.x,p.y+.39,17,'#fff0b2');}}
line([[44,8.5],[44,11]],'#ffe894');if(mirrors[0]){line([[44,11],[48,11]],'#ffe894');line(mirrors[1]?[[48,11],[48,8.5]]:[[48,11],[48,14]],'#ffe894');}else line([[44,11],[40,11]],'#ffe894');rect(47.7,8.1,.6,.4,has('laser')?'#c3e977':'#b17959');
line([[51.5,17],[54,17]],'#ffe894');line(mirrors[2]?[[54,17],[54,20]]:[[54,17],[54,16.2]],'#ffe894');rect(53.7,20,.6,.3,has('boss.laser')?'#cced78':'#ad8163');
for(let t of things())if(Math.abs(t[1]-cx)<W/U/2+3&&Math.abs(t[2]-cy)<H/U/2+5)drawThing(t);
robot(s.x,s.y);let mx=s.x-1.1,my=s.y+2+Math.sin(time*3)*.09;rect(mx-.29,my,.58,.52,'#344f3c');rect(mx-.23,my+.06,.46,.40,'#c5d99e');rect(mx-.16,my+.18,.09,.13,'#374f40');rect(mx+.07,my+.18,.09,.13,'#374f40');rect(mx-.03,my+.52,.06,.2,'#d8ad67');if(carry>=0)crate(s.x,s.y+1.8,[3,5,2][carry]);
if(running&&!modal){let g=C.waypoint(s),dx=g[0]-s.x;if(Math.abs(dx)>2.3){let px=dx<0?26:W-26;ctx.fillStyle='#f8efc1';ctx.font='bold 22px sans-serif';ctx.textAlign='center';ctx.fillText(dx<0?'◀':'▶',px,H/2);ctx.font='600 12px sans-serif';ctx.fillText(Math.round(Math.abs(dx))+'m',px,H/2+19);}}
}
function loop(t){let dt=Math.min(.06,(t-lastTime)/1000||0);lastTime=t;time+=dt;acc+=dt;for(let n=0;acc>=1/60&&n<5;n++){tick(1/60);acc-=1/60;}let target=s.x+s.face*1.2;cx+=(target-cx)*(1-Math.exp(-dt*8));cy+=(s.y+3.2-cy)*(1-Math.exp(-dt*8));draw();requestAnimationFrame(loop);}
// Pointer capture prevents stuck movement, and distinct pointer ids allow move+jump together.
document.querySelectorAll('[data-hold]').forEach(b=>{b.addEventListener('pointerdown',e=>{e.preventDefault();if(modal||!running)return;held.set(e.pointerId,b.dataset.hold);try{b.setPointerCapture(e.pointerId);}catch{}});for(let ev of ['pointerup','pointercancel','lostpointercapture'])b.addEventListener(ev,e=>{held.delete(e.pointerId);});});
$('jump').addEventListener('pointerdown',e=>{e.preventDefault();if(running&&!modal)jumpQueued=true;});$('use').addEventListener('pointerdown',e=>{e.preventDefault();interact();});
bind('pausebtn',pause);bind('mapbtn',()=>{if(running)mapUI();else title();});
let km={ArrowLeft:'left',a:'left',ArrowRight:'right',d:'right',ArrowUp:'up',w:'up',ArrowDown:'down',s:'down'};
window.addEventListener('keydown',e=>{if(modal){if(e.key==='Escape')hide();return;}if(km[e.key]){e.preventDefault();keys.add(km[e.key]);}if(e.code==='Space'){e.preventDefault();if(!e.repeat)jumpQueued=true;}if(e.key.toLowerCase()==='e'&&!e.repeat)interact();if(e.key.toLowerCase()==='b')bag();if(e.key==='Escape')pause();});window.addEventListener('keyup',e=>{if(km[e.key])keys.delete(km[e.key]);});
window.addEventListener('blur',()=>{clearInput();save();});document.addEventListener('visibilitychange',()=>{if(document.hidden){clearInput();save();if(running&&!modal)pause();}});window.addEventListener('pagehide',save);window.addEventListener('resize',resize);if(window.visualViewport)visualViewport.addEventListener('resize',resize);
window.MMApp={state:()=>s,campaign:()=>campaign,get modal(){return modal;},save,interact,refresh,switchChapter,chapterSelect};resize();refresh();title();requestAnimationFrame(loop);
})();
