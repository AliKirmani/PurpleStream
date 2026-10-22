namespace PurpleStream;
sealed class ChannelPicker:Form {
 readonly TreeView tree=new(){Dock=DockStyle.Fill,HideSelection=false,FullRowSelect=true,ItemHeight=30};
 readonly ListView list=new(){Dock=DockStyle.Fill,View=View.Details,VirtualMode=true,FullRowSelect=true,HideSelection=false};
 readonly TextBox search=new(){Dock=DockStyle.Top,PlaceholderText="Search name, country or category — e.g. Pakistan sports"};
 readonly CheckBox favorites=new(){Text="Favorites only",AutoSize=true};
 readonly CheckBox recent=new(){Text="Recently watched only",AutoSize=true};
 readonly Label status=Ui.Label("Organizing channels…",38);
 readonly ImageList icons=new(){ImageSize=new(32,22),ColorDepth=ColorDepth.Depth32Bit};readonly List<Image> images=new();
 readonly System.Windows.Forms.Timer debounce=new(){Interval=180};
 readonly Saved saved; readonly Channel[] channels;
 BrowserEntry[] entries=Array.Empty<BrowserEntry>(),matches=Array.Empty<BrowserEntry>();string country="",genre="";int version;bool closed;
 public Channel? Selected{get;private set;}
 public ChannelPicker(IEnumerable<Channel> source,Saved? settings=null){
  channels=source.ToArray();saved=settings??new Saved();Ui.Dark(this);Text="Choose a channel • Country / Category";Size=new(1040,720);MinimumSize=new(800,550);
  var split=new SplitContainer{Dock=DockStyle.Fill,Width=1000,SplitterDistance=285};Controls.Add(split);split.Panel1.Controls.Add(tree);split.Panel1.Controls.Add(Ui.Label("Countries → Categories",32));
  tree.BackColor=list.BackColor=Ui.Surface;tree.ForeColor=list.ForeColor=Ui.Text;tree.ImageList=icons;
  list.Columns.Add("Channel",300);list.Columns.Add("Category",130);list.Columns.Add("Country",150);
  split.Panel2.Controls.Add(list);split.Panel2.Controls.Add(search);split.Panel2.Controls.Add(status);
  var tools=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=46};tools.Controls.Add(favorites);tools.Controls.Add(recent);tools.Controls.Add(Ui.Button("Search all",()=>{favorites.Checked=recent.Checked=false;search.Clear();if(tree.Nodes.Count>0)tree.SelectedNode=tree.Nodes[0];}));tools.Controls.Add(Ui.Button("Choose / Play",Choose));Controls.Add(tools);
  list.RetrieveVirtualItem+=(_,e)=>{if(e.ItemIndex>=matches.Length){e.Item=new ListViewItem("");return;}var entry=matches[e.ItemIndex];e.Item=new ListViewItem(new[]{(saved.Favorites.Contains(entry.Media.Url)?"★ ":"")+entry.Media.Name,CatalogDirectory.English(entry.Location.Genre),entry.Location.Country.Name});};
  list.DoubleClick+=(_,_)=>Choose();list.KeyDown+=(_,e)=>{if(e.KeyCode==Keys.Enter){Choose();e.Handled=true;}};
  tree.AfterSelect+=(_,e)=>{if(e.Node?.Tag is DirectorySelection s){country=s.Country;genre=s.Genre;if(genre.Length==0)e.Node.Expand();_ = Filter();}};
  favorites.CheckedChanged+=(_,_)=>_ = Filter();recent.CheckedChanged+=(_,_)=>_ = Filter();
  search.TextChanged+=(_,_)=>{debounce.Stop();debounce.Start();};debounce.Tick+=(_,_)=>{debounce.Stop();_ = Filter();};
  Shown+=async(_,_)=>await Prepare();FormClosed+=(_,_)=>{closed=true;version++;debounce.Dispose();icons.Dispose();foreach(var image in images)image.Dispose();};
 }
 async Task Prepare(){
  var custom=new Dictionary<string,ChannelOverride>(saved.Organization);entries=await Task.Run(()=>channels.Select(c=>new BrowserEntry(c,CatalogDirectory.Locate(c,custom))).ToArray());if(closed)return;
  var generic=FlagIcons.Draw("GENRE");images.Add(generic);icons.Images.Add("genre",generic);
  tree.BeginUpdate();tree.Nodes.Add(new TreeNode("All countries ("+entries.Length+")"){Tag=new DirectorySelection(),ImageKey="genre",SelectedImageKey="genre"});
  foreach(var group in entries.GroupBy(e=>e.Location.Country.Code).OrderBy(g=>g.First().Location.Country.Name)){
   var c=group.First().Location.Country;var image=FlagIcons.Draw(c.Code);images.Add(image);icons.Images.Add(c.Code,image);
   var node=new TreeNode(c.Name+" ("+group.Count()+")"){Tag=new DirectorySelection(c.Code),ImageKey=c.Code,SelectedImageKey=c.Code};
   foreach(var category in group.GroupBy(e=>e.Location.Genre).OrderBy(g=>CatalogDirectory.English(g.Key)))node.Nodes.Add(new TreeNode(CatalogDirectory.English(category.Key)+" ("+category.Count()+")"){Tag=new DirectorySelection(c.Code,category.Key),ImageKey="genre",SelectedImageKey="genre"});tree.Nodes.Add(node);
  }tree.EndUpdate();tree.SelectedNode=tree.Nodes[0];await Filter();
 }
 async Task Filter(){
  int request=++version;string selectedCountry=country,selectedGenre=genre;string[] terms=search.Text.Split(' ',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);bool star=favorites.Checked,history=recent.Checked;var stars=saved.Favorites.ToHashSet();var watched=saved.History.Select(h=>h.Media.Url).ToHashSet();var source=entries;
  var result=await Task.Run(()=>source.Where(e=>(selectedCountry.Length==0||e.Location.Country.Code==selectedCountry)&&(selectedGenre.Length==0||e.Location.Genre==selectedGenre)&&(!star||stars.Contains(e.Media.Url))&&(!history||watched.Contains(e.Media.Url))&&terms.All(t=>(e.Media.Name+" "+e.Media.Group+" "+e.Location.Country.Name+" "+CatalogDirectory.English(e.Location.Genre)).Contains(t,StringComparison.OrdinalIgnoreCase))).OrderBy(e=>e.Media.Name,StringComparer.OrdinalIgnoreCase).ToArray());
  if(closed||request!=version)return;list.SelectedIndices.Clear();list.VirtualListSize=0;matches=result;list.VirtualListSize=matches.Length;list.Invalidate();status.Text=matches.Length.ToString("N0")+" channels • "+(country.Length==0?"All countries":ChannelDirectory.Countries[country].Name)+(genre.Length==0?"":" › "+CatalogDirectory.English(genre))+(matches.Length==0?" • Clear filters or use Search all":"");
 }
 void Choose(){if(list.SelectedIndices.Count>0&&list.SelectedIndices[0]<matches.Length){Selected=matches[list.SelectedIndices[0]].Media;DialogResult=DialogResult.OK;Close();}}
 internal async Task CheckFilters(){
  async Task Settle(){await Task.Delay(400);}
  void Check(bool ok,string name){if(!ok)throw new Exception("Channel picker: "+name);}
  while(entries.Length==0&&!closed)await Task.Delay(50);await Settle();
  var pakistan=tree.Nodes.Cast<TreeNode>().Single(n=>n.Tag is DirectorySelection d&&d.Country=="PK");tree.SelectedNode=pakistan.Nodes.Cast<TreeNode>().Single(n=>n.Text.StartsWith("Sports"));await Settle();Check(matches.Length==1&&matches[0].Media.Name=="PTV Sports HD","country and sports filter");
  tree.SelectedNode=tree.Nodes[0];search.Text="Pakistan news";await Settle();Check(matches.Length==2,"multiple search terms across country and category");search.Clear();favorites.Checked=true;await Settle();Check(matches.Length==1&&matches[0].Media.Name=="PTV Sports HD","favorites filter");favorites.Checked=false;recent.Checked=true;await Settle();Check(matches.Length==1&&matches[0].Media.Name=="HUM TV","recent filter");recent.Checked=false;search.Text="nonexistent-channel-123";await Settle();Check(matches.Length==0,"empty result");search.Text="ARY Musik";await Settle();list.Items[0].Selected=true;Choose();Check(Selected?.Name=="ARY Musik","selected result playback mapping");
 }
}

