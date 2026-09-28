/* Original Canvas block art and tool animations. No third-party game art is used. */
(function(root){'use strict';
const palettes={mine:['#b4d2bc','#547a61','#8d7960','#8db159'],lava:['#76533f','#483e38','#837362','#df923f'],sky:['#b8deed','#83afc2','#b9ad88','#f2d990'],crystal:['#6b7fa1','#394b70','#8d8aab','#87d9d3'],star:['#606b8e','#303c60','#928ca2','#f0d47b'],forest:['#b4d198','#597954','#897f62','#a6c960'],ice:['#c1e4ef','#7dacc7','#bed0d7','#e3f5f1'],sun:['#f0d3a0','#ae8d63','#d3b889','#f6df8e'],clock:['#bdd7dc','#6c919a','#b3a58c','#dfb865'],aurora:['#6d93ab','#344b6b','#9394ae','#b7e8cf']};
class Renderer{
 constructor(canvas){this.canvas=canvas;this.ctx=canvas.getContext('2d');this.cx=8;this.cy=3;this.last=0;this.particles=[];}
 resize(){const r=this.canvas.getBoundingClientRect(),dpr=Math.min(devicePixelRatio||1,2);this.w=Math.max(1,r.width);this.h=Math.max(1,r.height);this.u=Math.max(25,Math.min(48,this.w/14));if(this.canvas.width!==Math.round(this.w*dpr)||this.canvas.height!==Math.round(this.h*dpr)){this.canvas.width=Math.round(this.w*dpr);this.canvas.height=Math.round(this.h*dpr);}this.ctx.setTransform(dpr,0,0,dpr,0,0);this.ctx.imageSmoothingEnabled=false;}
 X(x){return(this.w/2+(x-this.cx)*this.u);}Y(y){return(this.h/2-(y-this.cy)*this.u);}
 rect(x,y,w,h,color){const c=this.ctx;c.fillStyle=color;c.fillRect(Math.round(this.X(x)),Math.round(this.Y(y+h)),Math.ceil(w*this.u),Math.ceil(h*this.u));}
 line(points,color,width=.07){const c=this.ctx;c.strokeStyle=color;c.lineWidth=Math.max(1,width*this.u);c.beginPath();points.forEach(([x,y],i)=>i?c.lineTo(this.X(x),this.Y(y)):c.moveTo(this.X(x),this.Y(y)));c.stroke();}
 text(value,x,y,size=15,color='#fff6d8'){const c=this.ctx;c.font=`800 ${size}px -apple-system, BlinkMacSystemFont, "Malgun Gothic", sans-serif`;c.textAlign='center';c.textBaseline='middle';c.lineWidth=3;c.strokeStyle='#243529';c.strokeText(String(value),this.X(x),this.Y(y));c.fillStyle=color;c.fillText(String(value),this.X(x),this.Y(y));}
 block(x,y,color,top){this.rect(x,y,1,.62,'#394638');this.rect(x+.04,y+.04,.92,.54,color);this.rect(x+.04,y+.49,.92,.09,top);this.rect(x+.18,y+.2,.18,.09,'#ffffff16');this.rect(x+.6,y+.12,.2,.08,'#17252225');}
 tool(i,x,y,size=28,angle=-.5){const c=this.ctx;c.save();c.translate(x,y);c.rotate(angle);c.scale(size/30,size/30);c.lineCap='square';if(i===0){c.fillStyle='#966b3f';c.fillRect(-3,-2,6,28);c.fillStyle='#d8e6d2';c.fillRect(-16,-9,32,7);c.fillRect(-16,-4,7,9);c.fillRect(9,-4,7,9);c.fillStyle='#6a8174';c.fillRect(-10,-2,20,3);}else if(i===1){c.fillStyle='#d1dcd7';c.fillRect(-4,-5,8,30);c.fillRect(-12,-17,7,19);c.fillRect(5,-17,7,19);c.fillRect(-10,-3,20,6);c.fillStyle='#718578';c.fillRect(-2,2,3,16);c.fillStyle='#d8b871';c.fillRect(-5,18,10,5);}else if(i===2){c.fillStyle='#edd5a4';c.fillRect(-9,0,18,18);c.fillStyle='#6b8b91';c.fillRect(-9,-8,18,10);c.fillStyle='#ffc767';c.fillRect(-7,18,5,10);c.fillRect(2,18,5,10);}else if(i===3){c.fillStyle='#93d5cb';c.beginPath();c.moveTo(-14,-15);c.lineTo(14,-15);c.lineTo(13,5);c.lineTo(0,20);c.lineTo(-13,5);c.closePath();c.fill();c.strokeStyle='#eef5d3';c.lineWidth=3;c.stroke();c.fillStyle='#f0d178';c.fillRect(-3,-9,6,20);}else{c.fillStyle='#e5dcad';c.fillRect(-10,-12,20,30);c.fillStyle='#417d81';c.fillRect(-7,-8,14,14);c.fillStyle='#b9edc0';c.fillRect(-4,-4,8,7);c.fillStyle='#e1c473';c.fillRect(-2,-24,4,13);c.fillRect(-7,-26,14,4);}c.restore();}
 generator(t,s,time){const C=root.MM,st=C.session(s),on=C.has(s,t.id),x=t.x,y=t.floor*8,c=this.ctx,working=st.action?.id===t.id&&st.clock<st.action.until;
 this.rect(x-1.18,y,2.36,.3,'#304439');this.rect(x-1.02,y+.3,2.04,1.56,'#506864');this.rect(x-.94,y+1.65,1.88,.2,'#afbcaa');this.rect(x-.89,y+.38,1.12,1.14,'#243d38');
 c.save();c.translate(this.X(x-.32),this.Y(y+.96));c.rotate(on?time*3.5:working?Math.sin(time*17)*.16:0);c.fillStyle='#b0bfaa';for(let k=0;k<8;k++){c.rotate(Math.PI/4);c.fillRect(-3,-this.u*.46,6,this.u*.34);}c.beginPath();c.arc(0,0,this.u*.3,0,Math.PI*2);c.strokeStyle='#b9c7ad';c.lineWidth=5;c.stroke();c.fillStyle=on?'#f2d17b':'#758778';c.fillRect(-5,-5,10,10);c.restore();
 for(let i=0;i<5;i++)this.rect(x+.4,y+.54+i*.17,.36,.1,on?'#e6ac5e':'#9f7d57');
 this.rect(x+.75,y+1.81,.19,.74,'#3f544a');this.rect(x+.63,y+2.49,.43,.13,'#8ba395');this.rect(x-.87,y+1.88,.8,.36,'#24392c');
 for(let i=0;i<3;i++)this.rect(x-.8+i*.23,y+1.96,.16,.13,on?'#c6e87e':'#865e52');
 this.line([[x+.97,y+.68],[x+1.4,y+.68],[x+1.4,y+.18],[x+2,y+.18]],on?'#efc768':'#6b7665',.1);
 if(on){for(let i=0;i<3;i++){const p=(time*.65+i*.33)%1;c.globalAlpha=(1-p)*.55;c.fillStyle='#e9dcc0';c.beginPath();c.arc(this.X(x+.84+Math.sin(i+time)*p*.15),this.Y(y+2.7+p*.8),this.u*(.08+p*.11),0,Math.PI*2);c.fill();}c.globalAlpha=1;}
 if(on)this.text('가동 중',x,y+2.75,13,'#d6f49a');
 if(working)this.sparks(x+.45,y+1,time);
 }
 sparks(x,y,time){const c=this.ctx;c.fillStyle='#ffe7a4';for(let i=0;i<7;i++){const a=i*2.39,r=((time*4+i*.13)%1)*.65;c.fillRect(this.X(x+Math.cos(a)*r),this.Y(y+Math.sin(a)*r),3,3);}}
 golem(t,s,time,mode){const C=root.MM,b=C.bossInfo(s),x=t.x,y=t.floor*8,c=this.ctx,speaking=mode==='boss'||mode==='boss-question',nod=speaking?Math.sin(time*5)*.025:0;
 this.rect(x-.93,y,.66,.72,'#667e6b');this.rect(x+.27,y,.66,.72,'#667e6b');this.rect(x-1,y+.63,2,1.78,'#7e9679');this.rect(x-.89,y+2.05,1.78,.18,'#acba96');
 this.rect(x-1.38,y+.9,.45,1.24,'#acb593');this.rect(x+.98,y+1.0,.48,1.24,'#acb593');this.rect(x-.98,y+2.4+nod,1.96,1.03,'#33493c');this.rect(x-.86,y+2.48+nod,1.72,.83,'#d2d5a8');
 this.rect(x-.58,y+2.87+nod,.2,.24,'#354b40');this.rect(x+.38,y+2.87+nod,.2,.24,'#354b40');this.rect(x-.14,y+2.66+nod,.28,speaking?.08+(Math.sin(time*12)+1)*.02:.07,'#637858');
 this.rect(x-.9,y+3.39,1.8,.18,'#6e985b');this.rect(x-.19,y+1.32,.38,.49,b.cleared?'#b1e4b5':'#f2cb76');
 // The golem holds an open book, not an enemy weapon.
 this.rect(x+.94,y+1.58,.82,.6,'#876440');this.rect(x+.98,y+1.66,.74,.49,'#f9edc4');this.line([[x+1.34,y+1.65],[x+1.34,y+2.12]],'#c6aa77',.035);this.text('?',x+1.52,y+1.9,15,'#536d54');
 for(let i=0;i<b.total;i++){const bx=x+(i-(b.total-1)/2)*.4;this.rect(bx-.12,y+3.82,.24,.25,i<b.done?'#9ddd94':'#efd487');if(i<b.done)this.text('✓',bx,y+3.95,12,'#264c35');}
 this.text(b.cleared?'고마워, 탐험가!':`문제 ${Math.min(b.done+1,b.total)} / ${b.total}`,x,y+4.42,16,'#fff2b9');
 }
 drawObject(t,s,time,near,mode){const C=root.MM,x=t.x,y=t.floor*8,on=C.has(s,t.id),st=C.session(s);if(t.kind==='generator'){this.generator(t,s,time);}
 else if(t.kind==='golem'){this.golem(t,s,time,mode);}
 else if(t.kind==='bench'){this.rect(x-.9,y+.6,1.8,.24,'#c89b61');this.rect(x-.76,y,.18,.65,'#79573b');this.rect(x+.57,y,.18,.65,'#79573b');this.rect(x-.57,y+.88,1.14,.37,'#725637');for(let i=0;i<3;i++)for(let j=0;j<3;j++)this.rect(x-.44+i*.3,y+.94+j*.09,.23,.055,'#dec99b');this.text('3×3',x,y+1.66,16);}
 else if(t.kind==='ore'){if(!on){this.rect(x-.75,y,1.5,1.9,'#58645e');this.rect(x-.63,y+.14,1.22,1.6,'#9aa493');for(let i=0;i<5;i++)this.rect(x-.4+(i%2)*.52,y+.4+Math.floor(i/2)*.4,.25,.15,'#dbc383');const hits=st.toolHits[t.id]||0;if(hits>0)this.line([[x,y+1.75],[x-.18,y+1.27],[x+.12,y+.98]],'#2d4036',.07);if(hits>1)this.line([[x+.12,y+.98],[x-.2,y+.58],[x+.17,y+.12]],'#2d4036',.09);this.text(`캐기 ${hits}/${t.hits}`,x,y+2.2,14);}else{this.rect(x-.6,y,1.2,.13,'#a1ab91');this.text('채굴 완료',x,y+.8,13,'#d5ecb2');}}
 else if(t.kind==='chest'||t.type==='cache'){const opened=on||C.has(s,'reward/'+t.id);this.rect(x-.64,y,1.28,.84,'#815f3b');this.rect(x-.57,y+.08,1.14,.7,'#bf955a');this.rect(x-.68,y+.67+(opened?.32:0),1.36,.22,'#795738');this.rect(x-.1,y+.31,.2,.29,'#f5d278');}
 else if(t.kind==='weightCrate'){this.rect(x-.37,y,.74,.68,'#a87f4f');this.rect(x-.31,y+.07,.62,.54,'#ceb481');this.text(t.weight,x,y+.36,15);}
 else if(t.kind==='lift'){this.rect(x-.9,y,1.8,.22,'#77958f');this.rect(x-.76,y+.22,1.52,.15,on?'#bbdd91':'#e2cb92');for(let i=0;i<3;i++)this.line([[x-.3,y+.52+i*.38],[x,y+.76+i*.38],[x+.3,y+.52+i*.38]],'#e9f4d0',.07);this.text('부스터 점프',x,y+2.1,14);}
 else if(t.kind==='hazard'){this.rect(x-.95,y,1.9,.15,'#536d75');if(!on)for(let i=0;i<4;i++){let h=.55+(i%2)*.3;this.line([[x-.8+i*.5,y+.25],[x-.6+i*.5,y+h],[x-.85+i*.5,y+h+.25]],'#edd876',.06);}this.text(on?'안전해요':'실드 사용',x,y+1.7,14);}
 else if(t.kind==='scale'){this.rect(x-.85,y,1.7,.22,'#526d56');this.rect(x-.09,y+.22,.18,.9,'#c6aa72');this.rect(x-.95,y+1.09,1.9,.17,'#ddc9a2');const p=C.puzzle(s,t.id),w=p.placed.reduce((n,i)=>n+p.numbers[i],0);this.text(`${w} / ${p.target} kg`,x,y+1.77,16,on?'#d3efae':'#fff0b7');for(const i of p.placed){const bx=x-.58+p.placed.indexOf(i)*.52;this.rect(bx-.2,y+1.25,.4,.37,'#ceb481');this.text(p.numbers[i],bx,y+1.43,12);}}
 else if(t.kind==='pipe'){this.rect(x-.8,y,1.6,.25,'#586859');this.rect(x-.7,y+.25,1.4,1.45,'#8c7a5c');this.line([[x-.65,y+.66],[x-.24,y+.66],[x-.24,y+1.33],[x+.34,y+1.33],[x+.34,y+.7],[x+.8,y+.7]],on?'#ffd578':'#c5b38a',.19);this.rect(x+.73,y+.43,.5,.64,'#445a4a');this.rect(x+.82,y+.53,.31,.31,on?'#ffd578':'#826745');this.text('배관 연결',x,y+2.1,14);}
 else if(t.kind==='laser'){this.rect(x-.8,y,1.6,.2,'#5e7c79');this.rect(x-.55,y+.2,.15,1.36,'#a6b0ac');this.rect(x-.8,y+1.34,.8,.64,'#accfcd');this.line([[x-.75,y+1.42],[x-.07,y+1.87]],'#f7edc4',.09);this.rect(x+.58,y+.3,.34,.74,on?'#dcecb2':'#638e95');if(on)this.line([[x-.4,y+1.65],[x+.75,y+.84]],'#fae391',.05);this.text('거울 장치',x,y+2.26,14);}
 else if(t.kind==='plates'){}
 else {const col=t.kind==='scanner'?'#87bebe':t.kind==='memory'?'#b59bc7':'#a3c6ba';this.rect(x-.48,y,.96,.2,'#495f4c');this.rect(x-.58,y+.22,1.16,1.23,'#354f46');this.rect(x-.44,y+.46,.88,.75,on?'#b7da97':col);this.text(on?'✓':t.kind==='cratePuzzle'?'▣':t.kind==='toolGate'?'⚒':t.kind==='memory'?'◆':'?',x,y+.85,22);}
 if(near?.id===t.id){const c=this.ctx;c.strokeStyle='#f5df98';c.lineWidth=2;c.setLineDash([4,3]);c.strokeRect(this.X(x-1.08),this.Y(y+2.15),2.16*this.u,2.23*this.u);c.setLineDash([]);if(t.kind!=='golem')this.text(t.label,x,y+(t.kind==='generator'?3.23:2.58),14);}
 }
 robot(s,time,moving){const C=root.MM,st=C.session(s),x=st.x,y=st.y,f=st.face,action=st.action&&st.clock<st.action.until?st.action:null,bob=st.grounded&&moving?Math.sin(time*15)*.04:0;
 this.rect(x-.45,y+.42, .9,.73,'#657d64');this.rect(x-.36,y+.52,.72,.56,'#b7cb91');this.rect(x-.48,y+1.12+bob,.96,.58,'#344d43');this.rect(x-.38,y+1.2+bob,.76,.41,'#ebebc6');this.rect(x-.3,y+1.27+bob,.6,.23,'#85b8bc');this.rect(x+f*.13,y+1.31+bob,.09,.15,'#294c49');
 const step=st.grounded&&moving?Math.sin(time*15)*.12:0;this.rect(x-.36,y,.26,.43+step,'#526858');this.rect(x+.1,y,.26,.43-step,'#526858');this.rect(x-.5,y+.7,.17,.41,'#dcc897');this.rect(x+.33,y+.7,.17,.41,'#dcc897');
 const tool=action?.tool??st.selectedHotbar;if(C.tool(s,tool)){
  let swing=action?Math.sin((st.clock-action.started)*19)*.9:0;this.tool(tool,this.X(x+f*.58),this.Y(y+.95),this.u*.6,f*(.4+swing));
 }
 if(action?.kind==='mine'||action?.kind==='repair')this.sparks(action.x,action.y+1,time);
 if(C.tool(s,2)&&!st.grounded&&st.vy>0){this.rect(x-.29,y-.32,.18,.32,'#ffe1a1');this.rect(x+.11,y-.34,.18,.34,'#ffb565');}
 if(C.tool(s,3)&&(st.selectedHotbar===3||action?.kind==='shield')){const c=this.ctx;c.strokeStyle='#a2e4dacc';c.lineWidth=3;c.beginPath();c.ellipse(this.X(x),this.Y(y+.95),this.u*.83,this.u*1.12,0,0,Math.PI*2);c.stroke();}
 if(action?.kind==='scan'){const c=this.ctx;c.strokeStyle='#a7ecebbb';c.lineWidth=2;c.beginPath();c.arc(this.X(x),this.Y(y+1),(1+(st.clock-action.started)*4)*this.u,0,Math.PI*2);c.stroke();}
 const mx=x-f*1.0,my=y+2.0+Math.sin(time*3)*.08;this.rect(mx-.27,my,.54,.42,'#435e48');this.rect(mx-.21,my+.06,.42,.3,'#dce8b8');this.rect(mx-.13,my+.16,.08,.1,'#3a6356');this.rect(mx+.05,my+.16,.08,.1,'#3a6356');this.rect(mx-.03,my+.42,.06,.16,'#e4be71');
 }
 render(s,near,ui={}){this.resize();const C=root.MM,P=root.MMPuzzles,st=C.session(s),d=C.definition(s),c=this.ctx,time=performance.now()/1000,dt=Math.min(.08,time-this.last||.016);this.last=time;const target=st.x+st.face*.9;this.cx+=(target-this.cx)*(1-Math.exp(-dt*9));this.cy+=(st.y+2.8-this.cy)*(1-Math.exp(-dt*9));
 const pal=palettes[d.theme]||palettes.mine;const grad=c.createLinearGradient(0,0,0,this.h);grad.addColorStop(0,pal[0]);grad.addColorStop(1,pal[1]);c.fillStyle=grad;c.fillRect(0,0,this.w,this.h);
 const left=Math.max(0,Math.floor(this.cx-this.w/this.u/2)-1),right=Math.min(d.width,Math.ceil(this.cx+this.w/this.u/2)+1);
 for(let f=0;f<3;f++){for(let x=left;x<right;x++){if((x+f*3)%7===0){this.rect(x,f*8+.8,.45,5.1,pal[1]);this.rect(x+.08,f*8+1,.1,4.8,'#ffffff10');}this.block(x,f*8-.62,pal[2],pal[3]);if((x+f)%11===0){this.rect(x+.37,f*8+2.3,.12,.7,'#564731');this.rect(x+.26,f*8+2.82,.34,.3,'#f3d392');}}}
 for(const l of C.ladders(s)){this.rect(l[0]-.28,l[1],.1,l[2]-l[1],'#cfaa70');this.rect(l[0]+.18,l[1],.1,l[2]-l[1],'#cfaa70');for(let y=l[1]+.3;y<l[2];y+=.48)this.rect(l[0]-.2,y,.4,.1,'#e8c48b');}
 for(const f of C.solids(s)){if(f[4]||f[0]<0||f[0]>=60)continue;for(let y=f[1];y<f[3];y+=.65){this.rect(f[0],y,f[2]-f[0],.62,'#526858');this.rect(f[0]+.04,y+.08,f[2]-f[0]-.08,.47,'#afb59a');}this.text('🔒',(f[0]+f[2])/2,f[1]+2,19);}
 for(const t of d.tasks.filter(t=>t.type==='sequence')){const p=C.puzzle(s,t.id),active=C.currentTask(s)?.id===t.id,ev=st.plateEvent,completed=C.has(s,t.id);for(const plate of C.plateLayout(s,t)){const pressed=ev?.id===t.id&&ev.n===plate.n&&st.clock<ev.until,ok=completed||p.input.includes(plate.n),next=active&&p.sequence[p.input.length]===plate.n,down=pressed?Math.max(0,1-(st.clock-ev.started)/.45)*.12:0;this.rect(plate.x-.65,plate.y-.04,1.3,.14,'#465a40');this.rect(plate.x-.63,plate.y+.09-down,1.26,.16,pressed&&!ev.ok?'#d98a6b':ok?'#b5d779':next?'#efcf70':'#b5a080');this.text(ok?'✓ '+plate.n:plate.n,plate.x,plate.y+.59-down,18,ok?'#e3f8c0':'#fff2c4');if(next){c.strokeStyle='#fff0aa';c.lineWidth=2;c.strokeRect(this.X(plate.x-.76),this.Y(plate.y+.34),1.52*this.u,.35*this.u);}}
  if(ev?.id===t.id&&st.clock<ev.until)this.text(ev.ok?`${ev.n} ✓ · ${ev.progress}/${p.sequence.length}`:'다시 첫 숫자부터!',ev.x,ev.y+1.05+(st.clock-ev.started)*.4,16,ev.ok?'#e3ffc0':'#ffdfb4');
 }
 for(const t of C.objects(s))if(Math.abs(t.x-this.cx)<this.w/this.u/2+3&&Math.abs(t.floor*8-this.cy)<this.h/this.u/2+5)this.drawObject(t,s,time,near,ui.mode);
 this.robot(s,time,ui.moving);if(st.carried){const p=C.puzzle(s,st.carried.id);this.rect(st.x-.4,st.y+1.95,.8,.68,'#c2a36b');this.text(p.numbers[st.carried.index]+'kg',st.x,st.y+2.3,14);}
 if(ui.effect&&time<ui.effect.until){const e=ui.effect,p=1-(e.until-time)/1.4;c.save();c.globalAlpha=Math.max(0,1-p);c.strokeStyle='#ffe499';c.lineWidth=3;c.beginPath();c.arc(this.X(e.x),this.Y(e.y+1),(p*1.4+.5)*this.u,0,Math.PI*2);c.stroke();c.restore();}
 if(!ui.modal){const o=C.waypoint(s),dx=o.x-st.x;if(Math.abs(dx)>2.5){c.font='900 22px sans-serif';c.textAlign='center';c.fillStyle='#fff5c9';c.fillText(dx<0?'◀':'▶',dx<0?20:this.w-20,this.h/2);}}
 }
}
root.MMRenderer=Renderer;
})(typeof globalThis!=='undefined'?globalThis:this);
