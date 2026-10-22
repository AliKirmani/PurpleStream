using System.Collections.Concurrent;
using System.Runtime.InteropServices;
namespace PurpleStream;
record PlaybackState(int State=0,long Time=0,long Length=0,bool Seekable=false,string Message="Ready") { public bool Playing=>State==3; }
record Track(int Id,string Name);
sealed class Vlc:IDisposable {
 const string D="libvlc";IntPtr instance,player;readonly Callback error;bool failed;
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)]delegate void Callback(IntPtr ev,IntPtr data);
 [StructLayout(LayoutKind.Sequential)]struct Description{public int Id;public IntPtr Name;public IntPtr Next;}
 [DllImport("kernel32",CharSet=CharSet.Unicode)]static extern bool SetDllDirectory(string path);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern IntPtr libvlc_new(int n,IntPtr args);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern IntPtr libvlc_media_player_new(IntPtr instance);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern IntPtr libvlc_media_new_location(IntPtr instance,[MarshalAs(UnmanagedType.LPUTF8Str)]string url);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_media_add_option(IntPtr media,[MarshalAs(UnmanagedType.LPUTF8Str)]string option);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_media_player_set_media(IntPtr p,IntPtr m);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_media_release(IntPtr m);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_media_player_set_hwnd(IntPtr p,IntPtr hwnd);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_media_player_play(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_media_player_pause(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_media_player_stop(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_audio_output_set(IntPtr p,[MarshalAs(UnmanagedType.LPUTF8Str)]string name);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_audio_get_volume(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_audio_get_mute(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_audio_set_volume(IntPtr p,int value);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_audio_set_mute(IntPtr p,int value);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_media_player_get_state(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern long libvlc_media_player_get_time(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern long libvlc_media_player_get_length(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_media_player_is_seekable(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_media_player_set_time(IntPtr p,long value);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_media_player_set_rate(IntPtr p,float value);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern IntPtr libvlc_audio_get_track_description(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern IntPtr libvlc_video_get_spu_description(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_track_description_list_release(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_audio_set_track(IntPtr p,int id);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_video_set_spu(IntPtr p,int id);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_video_take_snapshot(IntPtr p,uint num,[MarshalAs(UnmanagedType.LPUTF8Str)]string file,uint w,uint h);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_media_player_release(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_release(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern IntPtr libvlc_media_player_event_manager(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern int libvlc_event_attach(IntPtr manager,int type,Callback cb,IntPtr data);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]static extern void libvlc_event_detach(IntPtr manager,int type,Callback cb,IntPtr data);
 public Vlc(IntPtr hwnd,Action onError){SetDllDirectory(Path.Combine(AppContext.BaseDirectory,"libvlc","win-x64"));var options=new[]{Marshal.StringToCoTaskMemUTF8("--aout=directsound"),Marshal.StringToCoTaskMemUTF8("--no-volume-save"),Marshal.StringToCoTaskMemUTF8("--directx-volume=0")};var argv=Marshal.AllocHGlobal(IntPtr.Size*options.Length);try{Marshal.Copy(options,0,argv,options.Length);instance=libvlc_new(options.Length,argv);}finally{Marshal.FreeHGlobal(argv);foreach(var option in options)Marshal.FreeCoTaskMem(option);}if(instance==IntPtr.Zero)throw new Exception("Playback engine could not initialize.");player=libvlc_media_player_new(instance);if(player==IntPtr.Zero)throw new Exception("Playback engine could not create a player.");SetHandle(hwnd);error=(_,_)=>{failed=true;onError();};libvlc_event_attach(libvlc_media_player_event_manager(player),0x10a,error,IntPtr.Zero);}
 public void SetHandle(IntPtr h)=>libvlc_media_player_set_hwnd(player,h);
 public void Play(string url,string? output=null,bool transcode=false){failed=false;var m=libvlc_media_new_location(instance,url);if(m==IntPtr.Zero)throw new Exception("Stream could not be opened.");try{libvlc_media_add_option(m,":network-caching=1200");libvlc_media_add_option(m,":http-reconnect");libvlc_media_add_option(m,":ipv4-timeout=10000");libvlc_media_add_option(m,":rtsp-tcp");if(output!=null){var path=output.Replace('\\','/').Replace("'","");var pipeline=(transcode?"transcode{vcodec=mp2v,vb=900,acodec=mpga,ab=128}:":"")+"standard{access=file,mux=ts,dst='"+path+"'}";libvlc_media_add_option(m,":sout=#"+pipeline);libvlc_media_add_option(m,":sout-all");}libvlc_media_player_set_media(player,m);}finally{libvlc_media_release(m);}if(libvlc_media_player_play(player)<0)throw new Exception("Playback could not start.");}
 public PlaybackState Snapshot(){int state=failed?7:libvlc_media_player_get_state(player);return new(state,Math.Max(0,libvlc_media_player_get_time(player)),Math.Max(0,libvlc_media_player_get_length(player)),libvlc_media_player_is_seekable(player)!=0,state switch{1=>"Connecting…",2=>"Buffering…",3=>"Playing",4=>"Paused",5=>"Stopped",6=>"Ended",7=>"Stream failed. Check provider availability or connection limit.",_=>"Ready"});}
 public int AudioLevel()=>libvlc_audio_get_mute(player)==1?0:libvlc_audio_get_volume(player);
 public void Pause()=>libvlc_media_player_pause(player);public void Stop()=>libvlc_media_player_stop(player);public void Volume(int value)=>libvlc_audio_set_volume(player,value);public void Mute(bool value)=>libvlc_audio_set_mute(player,value?1:0);
 public void Seek(long ms){if(libvlc_media_player_is_seekable(player)!=0)libvlc_media_player_set_time(player,Math.Max(0,ms));}
 public void Rate(float value){if(libvlc_media_player_set_rate(player,value)!=0)throw new Exception("Playback speed is unavailable for this stream.");}
 public void SelectTrack(bool subtitle,int id){if(subtitle)libvlc_video_set_spu(player,id);else libvlc_audio_set_track(player,id);}
 public List<Track> Tracks(bool subtitle){var first=subtitle?libvlc_video_get_spu_description(player):libvlc_audio_get_track_description(player);var list=new List<Track>();try{for(var p=first;p!=IntPtr.Zero;){var item=Marshal.PtrToStructure<Description>(p);list.Add(new(item.Id,Marshal.PtrToStringUTF8(item.Name)??"Track"));p=item.Next;}}finally{if(first!=IntPtr.Zero)libvlc_track_description_list_release(first);}return list;}
 public void Snapshot(string path){if(libvlc_video_take_snapshot(player,0,path,0,0)!=0)throw new Exception("No video frame is available.");}
 public void Dispose(){if(player!=IntPtr.Zero){libvlc_event_detach(libvlc_media_player_event_manager(player),0x10a,error,IntPtr.Zero);Stop();libvlc_media_player_release(player);player=IntPtr.Zero;}if(instance!=IntPtr.Zero){libvlc_release(instance);instance=IntPtr.Zero;}GC.KeepAlive(error);}
}
// Every native operation, including initialization, getters, stop and disposal,
// runs on this dedicated background worker. UI controls only read cached state.
sealed class AsyncPlayback:IDisposable {
 readonly BlockingCollection<Action<Vlc>> commands=new();readonly Thread worker;readonly TaskCompletionSource finished=new(TaskCreationOptions.RunContinuationsAsynchronously);Vlc? native;IntPtr hwnd;int generation;int desiredVolume=80;bool desiredMute,audioPending;volatile bool disposed;PlaybackState state=new();
 public PlaybackState State=>Volatile.Read(ref state);public Task Completion=>finished.Task;
 public AsyncPlayback(IntPtr handle){hwnd=handle;worker=new Thread(Run){IsBackground=true,Name="Purple Stream playback"};worker.Start();}
 void Run(){try{while(!commands.IsCompleted){if(commands.TryTake(out var command,150)){try{native??=new Vlc(hwnd,()=>{});command(native);}catch(Exception ex){Volatile.Write(ref state,new(7,Message:"Playback operation failed. "+ex.GetType().Name));}}if(native!=null)try{var snapshot=native.Snapshot();if(snapshot.State is 3 or 4){int expected=desiredMute?0:desiredVolume;if(audioPending||native.AudioLevel()!=expected){native.Volume(desiredVolume);native.Mute(desiredMute);audioPending=false;}}Volatile.Write(ref state,snapshot);}catch{}}}finally{try{native?.Dispose();}catch{}finished.TrySetResult();}}
 void Post(Action<Vlc> action){if(disposed)return;try{commands.Add(action);}catch(InvalidOperationException){}}
 public void Play(string url,int volume=80,long resume=0,string? output=null,bool transcode=false,bool? muted=null){int version=Interlocked.Increment(ref generation);Volatile.Write(ref state,new(1,Message:"Connecting…"));Post(v=>{if(version!=generation)return;v.Stop();if(version!=generation)return;v.SetHandle(hwnd);desiredVolume=volume;desiredMute=muted??desiredMute;audioPending=true;v.Volume(volume);v.Mute(desiredMute);v.Play(url,output,transcode);v.Volume(volume);v.Mute(desiredMute);if(resume>0){for(int i=0;i<50&&version==generation;i++){var snap=v.Snapshot();if(snap.Seekable){v.Seek(resume);break;}if(snap.State is 6 or 7)break;Thread.Sleep(100);}}});}
 public void Handle(IntPtr handle){hwnd=handle;Post(v=>v.SetHandle(handle));}public void Pause()=>Post(v=>v.Pause());public void Stop(){Interlocked.Increment(ref generation);Volatile.Write(ref state,new(5,Message:"Stopping…"));Post(v=>v.Stop());}
 public void Seek(long ms)=>Post(v=>v.Seek(ms));public void Skip(long delta)=>Post(v=>{var s=v.Snapshot();v.Seek(s.Time+delta);});public void Volume(int volume)=>Post(v=>{desiredVolume=volume;audioPending=true;v.Volume(volume);});public void Mute(bool value)=>Post(v=>{desiredMute=value;audioPending=true;v.Mute(value);});public void Rate(float value)=>Post(v=>v.Rate(value));public void Snapshot(string path)=>Post(v=>v.Snapshot(path));
 public Task<int> AudioLevel(){var tcs=new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);Post(v=>tcs.TrySetResult(v.AudioLevel()));return tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));}
 public Task StopAsync(){Interlocked.Increment(ref generation);var done=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);Post(v=>{v.Stop();done.TrySetResult();});return done.Task;}
 public Task<List<Track>> Tracks(bool subtitle){var tcs=new TaskCompletionSource<List<Track>>(TaskCreationOptions.RunContinuationsAsynchronously);Post(v=>{try{tcs.TrySetResult(v.Tracks(subtitle));}catch{tcs.TrySetResult(new());}});return tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));}public void SelectTrack(bool sub,int id)=>Post(v=>v.SelectTrack(sub,id));
 public void Dispose(){if(disposed)return;disposed=true;Interlocked.Increment(ref generation);commands.CompleteAdding();}
}







