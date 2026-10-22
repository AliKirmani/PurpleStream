const fs=require('fs');fs.mkdirSync('work',{recursive:true});
function chunk(tag,data){let h=Buffer.alloc(8);h.write(tag);h.writeUInt32LE(data.length,4);return Buffer.concat([h,data, ...(data.length%2?[Buffer.alloc(1)]:[])]);}
function LIST(type,...parts){return chunk('LIST',Buffer.concat([Buffer.from(type),...parts]));}
let a=Buffer.alloc(56);[100000,432000,0,0,200,0,1,43200,160,90].forEach((v,i)=>a.writeUInt32LE(v,i*4));
let sh=Buffer.alloc(56);sh.write('vids');sh.write('DIB ',4);sh.writeUInt32LE(1,20);sh.writeUInt32LE(10,24);sh.writeUInt32LE(200,32);sh.writeUInt32LE(43200,36);sh.writeUInt32LE(0xffffffff,40);sh.writeInt16LE(160,52);sh.writeInt16LE(90,54);
let sf=Buffer.alloc(40);sf.writeUInt32LE(40);sf.writeInt32LE(160,4);sf.writeInt32LE(90,8);sf.writeUInt16LE(1,12);sf.writeUInt16LE(24,14);sf.writeUInt32LE(43200,20);
let frames=[];for(let f=0;f<200;f++){let pixels=Buffer.alloc(43200);for(let i=0;i<pixels.length;i+=3){pixels[i]=f;pixels[i+1]=90;pixels[i+2]=180;}frames.push(chunk('00db',pixels));}
let data=Buffer.concat([Buffer.from('AVI '),LIST('hdrl',chunk('avih',a),LIST('strl',chunk('strh',sh),chunk('strf',sf))),LIST('movi',...frames)]);let head=Buffer.alloc(8);head.write('RIFF');head.writeUInt32LE(data.length,4);const avi=Buffer.concat([head,data]);fs.writeFileSync('work/test-video.avi',avi);

