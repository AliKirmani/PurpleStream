namespace PurpleStream;
sealed class MultiscreenWindow:Form {
 readonly List<AsyncPlayback> engines=new();
 readonly Panel[] cells=new Panel[4],videos=new Panel[4];
 readonly Label[] titles=new Label[4]; readonly Button[] audioButtons=new Button[4];
 readonly bool[] audio=new bool[4]; readonly Channel?[] media=new Channel?[4];
 readonly TableLayoutPanel grid=new(){Dock=DockStyle.Fill,Padding=new(6)};
 readonly ComboBox count=new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=65};
 readonly ComboBox layout=new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=165};
 readonly ComboBox main=new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=110};
 bool stopped,stopping,updating; int visibleCount=4,mainIndex;
 public async Task Shutdown(){if(stopped)return;foreach(var engine in engines){engine.Stop();engine.Dispose();}try{await Task.WhenAll(engines.Select(e=>e.Completion)).WaitAsync(TimeSpan.FromSeconds(8));}catch{}stopped=true;}
 public MultiscreenWindow(List<Channel> channels,int limit,Saved? settings=null){
  Ui.Dark(this);Text="Multiscreen";Size=new(1300,850);MinimumSize=new(1000,650);
  var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=48,Padding=new(8),WrapContents=false};
  bar.Controls.Add(new Label{Text="Screens",AutoSize=true,Padding=new(0,7,0,0)});bar.Controls.Add(count);
  count.Items.AddRange(new object[]{"2","3","4"});count.SelectedIndex=2;
  bar.Controls.Add(new Label{Text="Layout",AutoSize=true,Padding=new(8,7,0,0)});bar.Controls.Add(layout);
  layout.Items.AddRange(new object[]{"Equal screens","Main + sub screens"});layout.SelectedIndex=0;
  bar.Controls.Add(new Label{Text="Main",AutoSize=true,Padding=new(8,7,0,0)});bar.Controls.Add(main);
  bar.Controls.Add(Ui.Button("Mute all",()=>{for(int i=0;i<4;i++)SetAudio(i,false);}));
  var note=Ui.Label("Each Audio button controls only its own screen. Hidden screens stop. Provider connection limit: "+(limit>0?limit.ToString():"not supplied"),32);
  note.Dock=DockStyle.Bottom;Controls.Add(grid);Controls.Add(note);Controls.Add(bar);
  for(int i=0;i<4;i++){
   int slot=i;var cell=cells[i]=new Panel{Dock=DockStyle.Fill,Margin=new(5),BackColor=Ui.Surface};
   var video=videos[i]=new Panel{Dock=DockStyle.Fill,BackColor=Color.Black};
   var title=titles[i]=new Label{Text="Screen "+(i+1),Dock=DockStyle.Top,Height=28,ForeColor=Ui.Text,Padding=new(6),AutoEllipsis=true};
   var controls=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=76,AutoScroll=true};
   cell.Controls.Add(video);cell.Controls.Add(title);cell.Controls.Add(controls);
   var engine=new AsyncPlayback(IntPtr.Zero);engines.Add(engine);
   video.HandleCreated+=(_,_)=>engine.Handle(video.Handle);
   controls.Controls.Add(Ui.Button("Choose / Play",()=>{using var picker=new ChannelPicker(channels,settings);if(picker.ShowDialog(this)==DialogResult.OK&&picker.Selected is{} c)PlaySlot(slot,c);}));
   controls.Controls.Add(Ui.Button("Stop",()=>{engine.Stop();SetAudio(slot,false);}));
   audioButtons[i]=Ui.Button("Audio: Off",()=>SetAudio(slot,!audio[slot]));controls.Controls.Add(audioButtons[i]);
   controls.Controls.Add(Ui.Button("Make main",()=>{mainIndex=slot;layout.SelectedIndex=1;main.SelectedIndex=slot;Arrange();}));
  }
  count.SelectedIndexChanged+=(_,_)=>{visibleCount=count.SelectedIndex+2;if(mainIndex>=visibleCount)mainIndex=0;Arrange();};
  layout.SelectedIndexChanged+=(_,_)=>Arrange();
  main.SelectedIndexChanged+=(_,_)=>{if(updating)return;mainIndex=Math.Max(0,main.SelectedIndex);Arrange();};
  Arrange();
  FormClosing+=async(_,e)=>{if(stopped)return;e.Cancel=true;if(stopping)return;stopping=true;await Shutdown();Close();};
  FormClosed+=(_,_)=>{foreach(var engine in engines)engine.Dispose();};
 }
 void SetAudio(int slot,bool on){audio[slot]=on;engines[slot].Volume(on?80:0);engines[slot].Mute(!on);audioButtons[slot].Text=on?"Audio: On":"Audio: Off";}
 internal void PlaySlot(int slot,Channel channel){media[slot]=channel;titles[slot].Text="Screen "+(slot+1)+" • "+channel.Name;engines[slot].Handle(videos[slot].Handle);engines[slot].Play(channel.Url,audio[slot]?80:0,muted:!audio[slot]);}
 void Arrange(){
  updating=true;main.BeginUpdate();main.Items.Clear();for(int i=0;i<visibleCount;i++)main.Items.Add("Screen "+(i+1));main.SelectedIndex=mainIndex;main.EndUpdate();updating=false;main.Enabled=layout.SelectedIndex==1;
  grid.SuspendLayout();grid.Controls.Clear();grid.ColumnStyles.Clear();grid.RowStyles.Clear();
  bool featured=layout.SelectedIndex==1;
  grid.ColumnCount=featured?2:visibleCount==4?2:visibleCount;grid.RowCount=featured?visibleCount-1:visibleCount==4?2:1;
  for(int col=0;col<grid.ColumnCount;col++)grid.ColumnStyles.Add(new(SizeType.Percent,featured?(col==0?70:30):100f/grid.ColumnCount));
  for(int row=0;row<grid.RowCount;row++)grid.RowStyles.Add(new(SizeType.Percent,100f/grid.RowCount));
  int sub=0;for(int i=0;i<4;i++){
   grid.SetRowSpan(cells[i],1);grid.SetColumnSpan(cells[i],1);
   if(i>=visibleCount){if(cells[i].Visible){engines[i].Stop();SetAudio(i,false);}cells[i].Visible=false;continue;}
   cells[i].Visible=true;
   if(featured){if(i==mainIndex){grid.Controls.Add(cells[i],0,0);grid.SetRowSpan(cells[i],grid.RowCount);}else grid.Controls.Add(cells[i],1,sub++);}
   else grid.Controls.Add(cells[i],i%grid.ColumnCount,i/grid.ColumnCount);
  }
  grid.ResumeLayout(true);
 }
 internal async Task CheckControls(string url){
  void Check(bool ok,string message){if(!ok)throw new Exception(message);}
  for(int i=0;i<4;i++)PlaySlot(i,new("Local screen "+i,url,"Test"));
  var until=DateTime.UtcNow.AddSeconds(30);while(engines.Any(e=>e.State.State!=3)&&DateTime.UtcNow<until)await Task.Delay(150);Check(engines.All(e=>e.State.State==3),"Native players not ready: "+string.Join(", ",engines.Select(e=>e.State.Message)));
  Check((await Task.WhenAll(engines.Select(e=>e.AudioLevel()))).All(v=>v==0),"First playback was not muted");await Task.Delay(2000);Check((await Task.WhenAll(engines.Select(e=>e.AudioLevel()))).All(v=>v==0),"Startup mute was reset after audio output initialization");audioButtons[0].PerformClick();await Task.Delay(250);Check((await engines[0].AudioLevel())>0,"First click did not enable audio");audioButtons[0].PerformClick();await Task.Delay(250);Check((await engines[0].AudioLevel())==0,"Second click did not disable audio");PlaySlot(0,new("Replacement",url,"Test"));await Task.Delay(1000);Check((await engines[0].AudioLevel())==0,"Changing channel lost mute");
  SetAudio(0,true);SetAudio(1,true);await Task.Delay(300);
  Check((await engines[0].AudioLevel())>0&&(await engines[1].AudioLevel())>0,"Independent audio enable failed");
  audioButtons[0].PerformClick();await Task.Delay(300);
  Check((await engines[0].AudioLevel())==0&&(await engines[1].AudioLevel())>0,"Muting screen 1 changed screen 2");
  audioButtons[2].PerformClick();await Task.Delay(300);
  Check((await engines[0].AudioLevel())==0&&(await engines[1].AudioLevel())>0&&(await engines[2].AudioLevel())>0&&(await engines[3].AudioLevel())==0,"Audio leaked to unrelated screens: "+string.Join(",",await Task.WhenAll(engines.Select(e=>e.AudioLevel())))+"; button="+audioButtons[2].Text+" visible="+audioButtons[2].Visible);
  count.SelectedIndex=0;await Task.Delay(300);Check(grid.Controls.Count==2,"Two-screen layout");Check(!audio[2]&&!audio[3],"Hidden screens not muted");
  count.SelectedIndex=1;Check(grid.Controls.Count==3,"Three-screen layout");
  layout.SelectedIndex=1;main.SelectedIndex=1;Check(grid.GetColumn(cells[1])==0&&grid.GetRowSpan(cells[1])==2,"Selected main screen not enlarged");
  count.SelectedIndex=2;Check(grid.Controls.Count==4&&grid.GetRowSpan(cells[1])==3,"Four-screen featured layout");
  layout.SelectedIndex=0;Check(grid.GetRowSpan(cells[1])==1&&grid.RowCount==2,"Equal layout restore");
  Check(engines[0].State.State!=7&&engines[1].State.State!=7,"Layout change broke playback");
 }
}




