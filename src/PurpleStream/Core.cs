using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace PurpleStream;
record Channel(string Name,string Url,string Group,string Kind="Live TV",string Id="",string CountryHint="",string EpgId="",bool HasArchive=false,int ArchiveDays=0,string CatchupSource="",string GenreHint="",string Season="",string SeriesTitle="");
record RecentMedia(Channel Media,long Position,DateTimeOffset Played);
class Saved {
 public string Mode{get;set;}="M3U";public string Server{get;set;}="";public string User{get;set;}="";public string Secret{get;set;}="";
 public List<string> Favorites{get;set;}=new();public int Volume{get;set;}=80;public Dictionary<string,ChannelOverride> Organization{get;set;}=new();
 public List<Channel> FavoriteItems{get;set;}=new();public List<RecentMedia> History{get;set;}=new();public string EpgUrl{get;set;}="";
 public string DownloadFolder{get;set;}=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),"Purple Stream");
}
static class SettingsStore {
 public static string Folder=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PurpleStream");
 public static Saved Load(){try{return JsonSerializer.Deserialize<Saved>(ProtectedData.Unprotect(File.ReadAllBytes(Path.Combine(Folder,"settings.dat")),null,DataProtectionScope.CurrentUser))??new();}catch{return new();}}
 public static void Save(Saved saved){Directory.CreateDirectory(Folder);var bytes=ProtectedData.Protect(JsonSerializer.SerializeToUtf8Bytes(saved),null,DataProtectionScope.CurrentUser);var temp=Path.Combine(Folder,"settings.tmp");File.WriteAllBytes(temp,bytes);File.Move(temp,Path.Combine(Folder,"settings.dat"),true);}
 public static string Protect(string value)=>Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(value),null,DataProtectionScope.CurrentUser));
 public static string Secret(Saved saved){try{return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(saved.Secret),null,DataProtectionScope.CurrentUser));}catch{return "";}}
}
static class Playlist {
 public static string Attribute(string line,string key){var match=Regex.Match(line,"(?:^|\\s)"+Regex.Escape(key)+"\\s*=\\s*(?:\"([^\"]*)\"|'([^']*)')",RegexOptions.IgnoreCase);return match.Success?(match.Groups[1].Success?match.Groups[1].Value:match.Groups[2].Value):"";}
 static int Separator(string line){char quote='\0';for(int i=0;i<line.Length;i++){char c=line[i];if(quote!='\0'){if(c==quote)quote='\0';}else if(c is '\'' or '"')quote=c;else if(c==',')return i;}return -1;}
 public static string GuideUrl(string text)=>Attribute(text.Split('\n').FirstOrDefault()??"","url-tvg") is {Length:>0} url?url:Attribute(text.Split('\n').FirstOrDefault()??"","x-tvg-url");
 public static List<Channel> Parse(string text,Uri? origin=null){var result=new List<Channel>();string meta="",name="";foreach(var raw in text.Split('\n')){var line=raw.Trim().TrimStart('\uFEFF');if(line.StartsWith("#EXTINF",StringComparison.OrdinalIgnoreCase)){meta=line;int comma=Separator(line);name=comma>=0?line[(comma+1)..].Trim():"Untitled";}else if(line.Length>0&&!line.StartsWith('#')){if((Uri.TryCreate(line,UriKind.Absolute,out var url)||(origin!=null&&Uri.TryCreate(origin,line,out url)))&&url!.Scheme is "http" or "https" or "rtsp" or "rtmp" or "udp"){
 string group=Attribute(meta,"group-title");string type=Attribute(meta,"type").ToLowerInvariant();string kind=type is "movie" or "vod"||Regex.IsMatch(group,@"\b(movies|cinema|films)\b",RegexOptions.IgnoreCase)?"Movies":"Live TV";
 var serial=Regex.Match(name,@"^(.*?)[ ._-]+S(\d{1,2})E(\d{1,3})",RegexOptions.IgnoreCase);bool episode=type is "series" or "episode"||Regex.IsMatch(group,@"\b(series|serials)\b",RegexOptions.IgnoreCase)||url.AbsolutePath.Contains("/series/",StringComparison.OrdinalIgnoreCase);if(episode)kind="Episode";string source=Attribute(meta,"catchup-source");int.TryParse(Attribute(meta,"catchup-days"),out int days);result.Add(new(name.Length>0?name:Path.GetFileName(url.AbsolutePath),url.AbsoluteUri,group.Length>0?group:"Other",kind,CountryHint:Attribute(meta,"tvg-country"),EpgId:Attribute(meta,"tvg-id"),HasArchive:source.Length>0,ArchiveDays:days,CatchupSource:source,Season:episode?(serial.Success?serial.Groups[2].Value:"1"):"",SeriesTitle:episode?(serial.Success?serial.Groups[1].Value:group):""));}meta="";name="";}}return result;}
}
static class CatalogDirectory {
 public static string English(string value)=>value.Split(" / ")[0];
 public static readonly string[] VideoGenres={"Action","Adventure","Animation","Comedy","Crime","Drama","Family","Fantasy","Horror","Romance","Science Fiction","Thriller","War","Western","Documentary","History","Music","Other"};
 public static ChannelLocation Locate(Channel c,IReadOnlyDictionary<string,ChannelOverride> custom){var basis=ChannelDirectory.Locate(c,custom);if(c.Kind=="Live TV")return basis;string genre="Other";string text=c.GenreHint+" "+c.Group;
 foreach(var label in VideoGenres.Where(g=>g!="Other")){var pattern=label=="Science Fiction"?@"\b(sci[ -]?fi|science fiction)\b":@"\b"+Regex.Escape(label)+@"\b";if(Regex.IsMatch(text,pattern,RegexOptions.IgnoreCase)){genre=label;break;}}
 if(custom.TryGetValue(c.Url,out var over)&&!string.IsNullOrWhiteSpace(over.Genre))genre=English(over.Genre);
 return new(basis.Country,genre);}
}
static class Program {
 [STAThread]static void Main(string[] args){ApplicationConfiguration.Initialize();if(args.Contains("--screenshots")){var gallery=new Main(true,false);gallery.Shown+=async(_,_)=>await gallery.Gallery(Path.Combine(Environment.CurrentDirectory,"gallery"));Application.Run(gallery);return;}if(args.Contains("--ui-tests")){Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);Application.ThreadException+=(_,e)=>{File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"ui-exception.txt"),e.Exception.ToString());Application.Exit();};var test=new Main(true,false);test.Shown+=async(_,_)=>await test.UiChecks();Application.Run(test);return;}if(args.Contains("--tests")){AppTests.Run().GetAwaiter().GetResult();return;}if(args.Contains("--directory-tests")){DirectoryTests.Run();return;}Application.Run(new Main(args.Contains("--demo")));}
}




