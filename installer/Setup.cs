using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
class Setup : Form {
 const string Key="Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\PurpleStream";
 static string Home=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Programs","PurpleStream");
 Label status=new Label(); Button install=new Button(); CheckBox desktop=new CheckBox(); CheckBox reset=new CheckBox(); ProgressBar progress=new ProgressBar(); bool busy;
 [STAThread] static void Main(string[] args) {
  if(args.Length==2 && args[0]=="--test-install") { Extract(Path.GetFullPath(args[1]),delegate(int p){}); return; }
  Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
  if(args.Length>0 && args[0]=="--uninstall") { Uninstall(); return; }
  Application.Run(new Setup());
 }
 Setup() {
  Text="Purple Stream Setup • 0.4.3"; ClientSize=new Size(560,340); StartPosition=FormStartPosition.CenterScreen; FormBorderStyle=FormBorderStyle.FixedDialog; MaximizeBox=false;
  BackColor=Color.FromArgb(23,17,43); ForeColor=Color.White; Font=new Font("Segoe UI",10);
  var title=new Label {Text="Install Purple Stream",Font=new Font("Segoe UI",21,FontStyle.Bold),Location=new Point(24,22),AutoSize=true}; Controls.Add(title);
  status.SetBounds(26,80,510,80); status.Text="Independent Windows IPTV player\nInstall once for your Windows account. No administrator password needed.\nLocation: "+Home; Controls.Add(status);
  desktop.SetBounds(26,168,400,30); desktop.Text="Create a Desktop shortcut"; desktop.Checked=true; Controls.Add(desktop);
  reset.SetBounds(26,200,510,30); reset.Text="Clear previous local account, favorites and history"; reset.Checked=false; Controls.Add(reset); Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath); progress.SetBounds(26,240,508,18); Controls.Add(progress);
  install.SetBounds(364,284,170,36); install.Text="Install"; install.BackColor=Color.FromArgb(108,60,190); install.FlatStyle=FlatStyle.Flat; Controls.Add(install);
  install.Click+=async delegate { if(install.Text=="Finish") { Close(); return; } busy=true; install.Enabled=false; desktop.Enabled=false; bool shortcut=desktop.Checked; bool clear=reset.Checked; reset.Enabled=false;
   try { CheckRunning(); await Task.Run(delegate { Extract(Home,delegate(int p) { BeginInvoke((Action)delegate {progress.Value=p;}); }); });
    if(clear) { string settings=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PurpleStream","settings.dat"); if(File.Exists(settings))File.Delete(settings); } File.Copy(Application.ExecutablePath,Path.Combine(Home,"Uninstall.exe"),true);
    string start=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),"Purple Stream"); Directory.CreateDirectory(start);
    Link(Path.Combine(start,"Purple Stream.lnk"),Path.Combine(Home,"PurpleStream.exe"),"");
    Link(Path.Combine(start,"Uninstall Purple Stream.lnk"),Path.Combine(Home,"Uninstall.exe"),"--uninstall");
    if(shortcut) Link(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"Purple Stream.lnk"),Path.Combine(Home,"PurpleStream.exe"),"");
    using(var k=Registry.CurrentUser.CreateSubKey(Key)) { k.SetValue("DisplayName","Purple Stream"); k.SetValue("DisplayVersion","0.4.3"); k.SetValue("Publisher","Purple Stream — Independent Player"); k.SetValue("InstallLocation",Home); k.SetValue("UninstallString","\""+Path.Combine(Home,"Uninstall.exe")+"\" --uninstall"); k.SetValue("DisplayIcon",Path.Combine(Home,"PurpleStream.exe")); k.SetValue("NoModify",1); k.SetValue("NoRepair",1); }
    status.Text="Installed successfully.\nOpen Purple Stream from Start or your Desktop.\nTo keep it on the Taskbar: open the app, right-click its icon, then select Pin to taskbar."; install.Text="Finish";
   } catch(Exception ex) { status.Text="Installation could not finish.\n"+ex.Message+"\nClose Purple Stream and try again."; }
   finally {busy=false; install.Enabled=true;}
  };
  FormClosing+=delegate(object s,FormClosingEventArgs e) { if(busy)e.Cancel=true; };
 }
 static void CheckRunning() { foreach(var p in Process.GetProcessesByName("PurpleStream")) { p.Dispose(); throw new IOException("Please close all Purple Stream windows first."); } }
 static void Extract(string root,Action<int> report) {
  Directory.CreateDirectory(root); string prefix=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
  var files=new List<string>();
  using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("PurpleStream.Payload.zip"))
  using(var zip=new ZipArchive(stream,ZipArchiveMode.Read)) {
   int n=0; foreach(var entry in zip.Entries) {
    const string top="PurpleStream-v0.4.3-Windows-x64/";
    if(!entry.FullName.StartsWith(top,StringComparison.Ordinal))throw new InvalidDataException("Unexpected package path.");
    string relative=entry.FullName.Substring(top.Length); if(relative.Length==0)continue;
    string path=Path.GetFullPath(Path.Combine(root,relative.Replace('/',Path.DirectorySeparatorChar)));
    if(!path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Unsafe package path.");
    if(entry.Name.Length==0)Directory.CreateDirectory(path); else { Directory.CreateDirectory(Path.GetDirectoryName(path)); entry.ExtractToFile(path,true); files.Add(relative); }
    report(Math.Min(100,++n*100/zip.Entries.Count));
   }
  }
  if(!File.Exists(Path.Combine(root,"PurpleStream.exe")))throw new InvalidDataException("Application missing.");
  File.WriteAllLines(Path.Combine(root,"installed-files.txt"),files); report(100);
 }
 static void Link(string link,string target,string arguments) {
  Type type=Type.GetTypeFromProgID("WScript.Shell"); object shell=Activator.CreateInstance(type); object shortcut=type.InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{link});
  Type t=shortcut.GetType(); t.InvokeMember("TargetPath",BindingFlags.SetProperty,null,shortcut,new object[]{target}); t.InvokeMember("Arguments",BindingFlags.SetProperty,null,shortcut,new object[]{arguments}); t.InvokeMember("WorkingDirectory",BindingFlags.SetProperty,null,shortcut,new object[]{Home}); t.InvokeMember("Save",BindingFlags.InvokeMethod,null,shortcut,null);
  System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut); System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
 }
 static void Uninstall() {
  Home=Path.GetDirectoryName(Application.ExecutablePath);
  if(MessageBox.Show("Remove Purple Stream? Your saved account, favorites and downloaded videos will be kept.","Uninstall Purple Stream",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
  try { CheckRunning(); string prefix=Path.GetFullPath(Home).TrimEnd('\\')+"\\";
   foreach(string relative in File.ReadAllLines(Path.Combine(Home,"installed-files.txt"))) { string path=Path.GetFullPath(Path.Combine(Home,relative)); if(!path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException(); if(File.Exists(path))File.Delete(path); }
   string start=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),"Purple Stream");
   foreach(string path in new[]{Path.Combine(start,"Purple Stream.lnk"),Path.Combine(start,"Uninstall Purple Stream.lnk"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"Purple Stream.lnk")})if(File.Exists(path))File.Delete(path);
   Registry.CurrentUser.DeleteSubKeyTree(Key,false); File.Delete(Path.Combine(Home,"installed-files.txt"));
   // Windows releases this executable on exit; schedule its removal at next sign-in.
   using(var k=Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce"))k.SetValue("PurpleStreamCleanup","cmd.exe /c del /q \""+Application.ExecutablePath+"\"");
   MessageBox.Show("Purple Stream has been removed. Setup cleanup will finish at your next Windows sign-in. Your personal data was kept.");
  } catch(Exception ex) {MessageBox.Show("Unable to finish removal. Close Purple Stream and retry.\n"+ex.Message);}
 }
}






