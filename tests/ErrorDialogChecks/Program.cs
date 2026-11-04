using Captovix;
var gate=new ExceptionDialogGate();int count=0,reports=0;
void Check(bool ok,string text){if(!ok)throw new Exception(text);count++;Console.WriteLine("PASS: "+text);}
Check(gate.Report(()=>{reports++;Check(!gate.Report(()=>reports++),"Nested dispatcher error does not open a second report");}),"First report runs");
Check(reports==1,"Reentrant report neither logs nor opens repeated dialogs");
Check(gate.Report(()=>reports++)&&reports==2,"A later independent error can still be reported");
try{gate.Report(()=>throw new InvalidOperationException("display failed"));}catch(InvalidOperationException){}
Check(gate.Report(()=>reports++)&&reports==3,"Gate releases after report throws");
using var entered=new ManualResetEventSlim();using var release=new ManualResetEventSlim();
var worker=Task.Run(()=>gate.Report(()=>{entered.Set();release.Wait();}));entered.Wait();Check(!gate.Report(()=>reports++),"Concurrent errors cannot bypass active report");release.Set();await worker;
Check(gate.Report(()=>reports++)&&reports==4,"Report available after concurrent owner completes");
Console.WriteLine(count+" error dialog checks passed.");
