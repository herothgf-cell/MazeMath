/* Original chapter content. Shared by the browser and Node verification. */
(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;else root.MMChapters=api;})(typeof globalThis!=='undefined'?globalThis:this,function(){
'use strict';
const tools=['곡괭이 팔','파워 렌치','점프 부스터','에너지 실드','탐험 센서'];
const task=(id,type,label,x,floor,extra={})=>({id,type,label,x,floor,kind:({repair:'generator',mine:'ore',jump:'lift',shield:'hazard',scan:'scanner',sequence:'plates',laser:'laser',pipe:'pipe',weight:'scale',sokoban:'cratePuzzle',memory:'memory',supply:'chest',number:'terminal',rule:'scanner',equipment:'toolGate'})[type]||type,...extra});
const q=(id,type,prompt,extra={})=>({id,type,prompt,...extra});
const chapter=(n,title,theme,bossName,requiredTools,tasks,questions)=>({id:'chapter-'+String(n).padStart(2,'0'),order:n,title,theme,width:60,floors:3,start:[8,0],bench:{id:'workshop',kind:'bench',label:'3×3 작업대',x:18,floor:0},requiredTools,tasks,boss:{id:'golem',kind:'golem',label:bossName,x:53,floor:2,questions},ladders:[[54,0,8],[12,8,16]],gateX:[51,16],optional:[task('cache.lower','cache','숨은 재료 상자',5,0),task('cache.upper','cache','탐험 보물',6,2)]});
const intro=(id)=>task(id,'supply','탐험 준비 상자',14,0,{text:'상자를 열어 필요한 재료와 다음 목표를 확인해요.'});
const chapters=[
chapter(1,'광산 입구','mine','광산 수호 골렘',[0],[
 task('math','number','숫자 잠금 장치',30,0,{problem:'add',text:'오른쪽 숫자 장치에서 문제를 풀어요.'}),
 task('mined','mine','균열 광석벽',34,0,{tool:0,hits:3,text:'곡괭이 팔로 균열벽을 세 번 두드려요.'}),
 task('bridge','weight','8kg 저울 다리',44,0,{numbers:[3,5,2],target:8,text:'상자를 들고 저울에 올려 8kg을 만들어요.'}),
 task('laser','laser','광산 거울 장치',44,1,{text:'거울을 돌려 빛을 오른쪽 수정에 보내요.'}),
 task('sequence','sequence','숫자 발판',24,1,{sequence:[2,4,6],guided:true,text:'2 → 4 → 6 순서로 발판을 밟아요.'})
],[q('boss.math','number','광산 문을 지킬 수 있겠니? 이 덧셈을 풀어 줘.',{problem:'add'}),q('boss.sequence','order','이번에는 2, 4, 6 순서대로 눌러 줄래?',{sequence:[2,4,6]}),q('boss.laser','laser','마지막 문제야. 빛이 수정에 닿도록 거울을 돌려 줘.')]),
chapter(2,'용암 제련소','lava','용광로 골렘',[0,1],[intro('c2.intro'),
 task('c2.genA','repair','발전기 A',30,0,{tool:1,hits:3,text:'파워 렌치로 발전기 A를 수리해요.'}),
 task('c2.genB','repair','발전기 B',44,1,{tool:1,hits:3,text:'2층 발전기 B를 렌치로 수리해요.'}),
 task('c2.bridge','pipe','용암 배관',24,1,{text:'불꽃에서 용광로까지 배관을 연결해요.'})
],[q('boss.math','number','발전기에 전기를 모으자. 같은 묶음은 모두 몇 개일까?',{problem:'mul'}),q('boss.power','pipe','내 용광로로 불꽃이 흐르도록 배관을 이어 줘.'),q('boss.plates','order','2 → 3 → 5 순서로 신호를 보내 줘.',{sequence:[2,3,5]})]),
chapter(3,'하늘 창고','sky','창고 관리자 골렘',[0,1,2],[intro('c3.intro'),
 task('c3.sokoban','sokoban','화물 정리대',34,0,{text:'상자를 밀어 별 표시 칸에 놓아요.'}),
 task('c3.lift','jump','부스터 리프트',44,0,{tool:2,text:'점프 부스터를 장착하고 리프트 옆에서 점프해요.'}),
 task('c3.memory','memory','창고 기억 패널',28,1,{length:3,text:'빛난 순서를 기억해서 같은 순서로 눌러요.'})
],[q('boss.sokoban','sokoban','상자가 제자리에 있어야 출발할 수 있어. 별 칸으로 밀어 줄래?'),q('boss.memory','memory','내가 보여 주는 세 신호를 기억해 줘.',{length:3}),q('boss.missing','number','빈칸에 어떤 수가 들어갈까?',{problem:'missing'})]),
chapter(4,'수정 회로 연구소','crystal','수정 코어 골렘',[0,1,2,3,4],[intro('c4.intro'),
 task('c4.rule','rule','숨은 규칙 스캐너',34,0,{tool:4,text:'탐험 센서로 단서를 읽고 규칙 문제를 풀어요.'}),
 task('c4.shield','shield','전기 보호 통로',44,0,{tool:3,text:'실드를 선택해 전기장을 안전하게 끊어요.'}),
 task('c4.laser','laser','수정 거울 회로',28,1,{level:4,text:'거울을 돌려 실제 빛의 경로를 연결해요.'})
],[q('boss.rule','rule','내 수정들의 공통 규칙을 찾아 줄래?'),q('boss.laser','laser','빛을 코어로 보내면 다음 문제를 줄게.',{level:4}),q('boss.switch','rule','두 조건을 모두 만족하는 신호를 골라 줘.',{level:4})]),
chapter(5,'별빛 코어 타워','star','별빛 코어 골렘',[0,1,2,3,4],[
 task('c5.number','number','별빛 숫자 엔진',14,0,{problem:'twoStep',text:'별빛 엔진의 두 단계 계산을 풀어요.'}),
 task('c5.equipment','equipment','도구 시험문',30,0,{text:'상황에 맞는 다섯 도구를 선택해요.'}),
 task('c5.spatial','sokoban','코어 운반대',44,0,{text:'상자를 목표 칸으로 옮겨요.'}),
 task('c5.memory','memory','별빛 기억 회로',28,1,{length:4,text:'네 개의 별빛 순서를 기억해요.'})
],[q('boss.number','number','지금까지 배운 계산을 써 볼까?',{problem:'div'}),q('boss.equipment','equipment','내가 말하는 상황에 알맞은 도구를 골라 줘.'),q('boss.spatial','sokoban','코어 상자를 제자리로 밀어 줘.'),q('boss.memory','memory','네 빛의 순서를 기억할 수 있겠니?',{length:4}),q('boss.core','number','마지막 빈칸을 채우면 더 먼 세계로 보내 줄게.',{problem:'missing'})]),
chapter(6,'초록 물레방아 숲','forest','숲의 물레 골렘',[0,1,2,3,4],[intro('c6.intro'),
 task('c6.water','pipe','물레방아 수로',32,0,{text:'물 입구에서 물레방아까지 관을 이어 주세요.'}),
 task('c6.wheel','repair','물레 발전기',44,0,{tool:1,hits:3,text:'렌치로 물레 발전기의 축을 고쳐요.'}),
 task('c6.balance','weight','숲의 저울',40,1,{numbers:[4,6,3],target:10,text:'4kg와 6kg를 골라 10kg을 맞춰요.'}),
 task('c6.pattern','number','잎사귀 규칙문',24,1,{problem:'pattern',text:'잎사귀 숫자의 규칙을 찾아요.'})
],[q('boss.divide','number','열매를 같은 수로 나누어 줄래?',{problem:'div'}),q('boss.water','pipe','내 나무까지 물이 흐르게 해 줘.'),q('boss.leaves','memory','네 잎사귀의 순서를 기억해 줘.',{length:4})]),
chapter(7,'얼음 기계 동굴','ice','빙하 수호 골렘',[0,1,2,3,4],[intro('c7.intro'),
 task('c7.ore','mine','얼어붙은 광석',32,0,{tool:0,hits:3,text:'곡괭이 팔로 얼음 광석을 세 번 캐요.'}),
 task('c7.lift','jump','빙하 리프트',44,0,{tool:2,text:'부스터로 뛰어 얼음 리프트를 켜요.'}),
 task('c7.order','sequence','얼음 숫자 발판',40,1,{sequence:[3,6,9],text:'3 → 6 → 9 순서로 직접 밟아요.'}),
 task('c7.beam','laser','얼음 거울',24,1,{level:5,text:'세 거울로 빛을 수정에 연결해요.'})
],[q('boss.ice.rule','rule','얼음 결정 중 조건에 맞는 것을 골라 줘.'),q('boss.ice.missing','number','사라진 숫자를 찾아 줄래?',{problem:'missing'}),q('boss.ice.light','laser','마지막 빛을 코어에 보내 줘.',{level:5})]),
chapter(8,'사막 태양 발전소','sun','태양 엔진 골렘',[0,1,2,3,4],[intro('c8.intro'),
 task('c8.solar','repair','태양 발전기',32,0,{tool:1,hits:3,text:'렌치로 태양 발전기를 수리해요.'}),
 task('c8.scan','rule','태양 신호 스캐너',44,0,{tool:4,level:5,text:'센서로 두 조건을 읽고 알맞은 신호를 골라요.'}),
 task('c8.shield','shield','열기 보호막',42,1,{tool:3,text:'실드로 뜨거운 전기장을 끊어요.'}),
 task('c8.laser','laser','태양 반사판',24,1,{level:6,text:'반사판으로 빛을 축전기에 모아요.'})
],[q('boss.sun.mul','number','태양 전지의 묶음 수를 세어 줘.',{problem:'mul'}),q('boss.sun.pipe','pipe','엔진 냉각관을 이어 줘.'),q('boss.sun.rule','rule','두 조건을 모두 지키는 숫자는 무엇일까?',{level:6})]),
chapter(9,'구름 시계 공방','clock','시계 장인 골렘',[0,1,2,3,4],[intro('c9.intro'),
 task('c9.clock','repair','시계 발전기',32,0,{tool:1,hits:3,text:'렌치로 시계의 동력축을 고쳐요.'}),
 task('c9.crates','sokoban','기어 정리실',44,0,{level:7,text:'두 상자를 각각 별 표시 칸으로 밀어요.'}),
 task('c9.memory','memory','시계 신호기',40,1,{length:4,text:'다시 보기로 확인하며 네 신호를 기억해요.'}),
 task('c9.number','number','시간 계산문',24,1,{problem:'twoStep',text:'두 단계 계산으로 시계문을 열어요.'})
],[q('boss.clock.sum','number','먼저 묶음을 나누고 남은 것을 더해 볼까?',{problem:'divideAdd'}),q('boss.clock.memory','memory','다섯 신호를 기억해 줘. 다시 보아도 괜찮아.',{length:5}),q('boss.clock.crates','sokoban','마지막 기어 상자들을 옮겨 줘.',{level:7})]),
chapter(10,'별빛 세계수 성채','aurora','세계수 코어 골렘',[0,1,2,3,4],[intro('c10.intro'),
 task('c10.core','repair','세계수 발전기',32,0,{tool:1,hits:3,text:'렌치로 세계수 발전기를 다시 켜요.'}),
 task('c10.tools','equipment','다섯 도구 문',44,0,{text:'다섯 상황에 맞는 도구를 모두 사용해요.'}),
 task('c10.pipe','pipe','별빛 에너지 관',42,1,{text:'입구와 코어를 실제 배관으로 연결해요.'}),
 task('c10.memory','memory','세계수 기억 회로',24,1,{length:5,text:'다섯 빛을 기억해 같은 순서로 눌러요.'})
],[q('boss.world.number','number','첫 번째 문제! 묶음과 덧셈을 함께 생각해 줘.',{problem:'divideAdd'}),q('boss.world.pattern','number','두 번째 문제! 숫자의 규칙을 찾아 줘.',{problem:'pattern'}),q('boss.world.rule','rule','세 번째 문제! 두 조건을 모두 만족하는 것을 찾아 줘.',{level:8}),q('boss.world.light','laser','네 번째 문제! 빛을 세계수 코어까지 보내 줘.',{level:8}),q('boss.world.memory','memory','마지막 문제야! 다섯 빛을 기억해서 보내 줘.',{length:5})])
];
function freeze(o){Object.freeze(o);Object.values(o).forEach(v=>{if(v&&typeof v==='object'&&!Object.isFrozen(v))freeze(v);});return o;}
chapters.forEach(freeze);const byId=Object.fromEntries(chapters.map(d=>[d.id,d]));
return{tools,get:id=>byId[id]||null,list:()=>chapters.slice(),firstId:'chapter-01',lastId:'chapter-10'};
});
