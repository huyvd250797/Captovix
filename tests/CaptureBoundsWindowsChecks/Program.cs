using Captovix;
using System.Reflection;
internal static class Program
{
 [STAThread]static int Main(){try{
 var type=typeof(RegionPicker).GetNestedType("Session",BindingFlags.NonPublic)!;
 var displays=new[]{new DisplayInfo("test",new(-300,-120,640,480),true,144,144)};
 var session=Activator.CreateInstance(type,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,[displays,true,null],null)!;
 try{
 var current=type.GetProperty("Current")!;var locked=type.GetField("Locked")!;
 PixelRect Get()=>(PixelRect)current.GetValue(session)!;void Set(PixelRect r)=>current.SetValue(session,r);
 Set(new(-308,-128,656,496));if(Get()!=displays[0].Bounds)throw new Exception("Initial raw window not bounded");
 var suggestion=(ElementSuggestion)type.GetField("Suggestion")!.GetValue(session)!;suggestion.Offer([new(Get(),"Cửa sổ",true)],0,0,16);
 type.GetMethod("Begin")!.Invoke(session,[new Native.POINT{X=0,Y=0},8]);type.GetMethod("Move")!.Invoke(session,[new Native.POINT{X=0,Y=0},false]);type.GetMethod("End")!.Invoke(session,null);if(Get()!=displays[0].Bounds)throw new Exception("Click changes clipped region");
 Set(new(-180,-20,200,100));suggestion.Clear();locked.SetValue(session,true);type.GetMethod("Begin")!.Invoke(session,[new Native.POINT{X=-100,Y=0},8]);type.GetMethod("Move")!.Invoke(session,[new Native.POINT{X=900,Y=800},false]);type.GetMethod("End")!.Invoke(session,null);if(Get()!=new PixelRect(140,260,200,100))throw new Exception("Session move shrinks/escapes at edge");
 var finish=type.GetMethod("Finish")!;finish.Invoke(session,[finish.GetParameters()[0].DefaultValue]);var done=(TaskCompletionSource<PixelRect?>)type.GetField("Done")!.GetValue(session)!;if(done.Task.Result!=Get())throw new Exception("Confirmation returns different rectangle than displayed");
 Console.WriteLine("Actual selector session clamps suggestions/movement and confirms identical bounded rectangle. Physical Windows/DPI checks remain separate.");
 }finally{((IDisposable)session).Dispose();}return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
