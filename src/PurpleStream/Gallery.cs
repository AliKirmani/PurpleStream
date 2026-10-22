namespace PurpleStream;
// Maintainer-only screenshots: isolated sample state; never reads a saved account.
sealed partial class Main {
 internal async Task Gallery(string folder) {
  Directory.CreateDirectory(folder);
  async Task Capture(Form form,string name){await Task.Delay(900);form.Refresh();using var bitmap=new Bitmap(form.Width,form.Height);form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,form.Size));bitmap.Save(Path.Combine(folder,name+".png"));}
  try {
   SeedDemo();Home();await Capture(this,"dashboard");
   await ShowLibrary("Live TV");await Capture(this,"live-tv");
   await ShowLibrary("Movies");await Capture(this,"movies");
   await ShowLibrary("Series");await Capture(this,"series");
   using(var multi=new MultiscreenWindow(catalogs["Live TV"],4,saved)){multi.Show(this);await Capture(multi,"multiscreen");await multi.Shutdown();multi.Close();}
   using(var picker=new ChannelPicker(catalogs["Live TV"],saved)){picker.Show(this);await Capture(picker,"channel-picker");picker.Close();}
  }catch(Exception error){File.WriteAllText(Path.Combine(folder,"gallery-error.txt"),error.ToString());}
  finally {Close();}
 }
}
