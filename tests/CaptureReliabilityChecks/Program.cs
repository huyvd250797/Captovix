using Captovix;
int checks=0;
void Check(bool value,string message){if(!value)throw new Exception(message);checks++;Console.WriteLine("PASS: "+message);}
foreach(int wait in new[]{350,700,1500,5000})foreach(int steps in new[]{1,2,3}){
    var timing=new CaptureTiming(wait,steps);timing.Validate();Check(timing.IntervalMilliseconds>wait,"Scroll interval includes settling budget");
}
foreach(var timing in new[]{new CaptureTiming(349),new CaptureTiming(5001),new CaptureTiming(700,0),new CaptureTiming(700,4)}){
    bool rejected=false;try{timing.Validate();}catch(ArgumentOutOfRangeException){rejected=true;}Check(rejected,"Reject timing outside capture bounds");
}
for(int bits=0;bits<16;bits++){
    bool alive=(bits&1)!=0,same=(bits&2)!=0,front=(bits&4)!=0,visible=(bits&8)!=0;
    var result=CaptureReliability.Evaluate(alive,same,front,visible);
    Check((result==CaptureReadiness.Ready)==(bits==15),"Only stable, foreground, visible source can auto-capture: "+bits);
    Check(!string.IsNullOrWhiteSpace(CaptureReliability.Explain(result)),"Readiness includes recovery instruction: "+bits);
}
Check(CaptureReliability.Evaluate(false,false,false,false)==CaptureReadiness.Closed,"Closed source takes priority");
Check(CaptureReliability.Evaluate(true,false,true,true)==CaptureReadiness.Moved,"Resize/move blocks capture");
Check(CaptureReliability.Evaluate(true,true,false,true)==CaptureReadiness.Background,"Background source pauses capture");
Check(CaptureReliability.Evaluate(true,true,true,false)==CaptureReadiness.Covered,"Occlusion pauses capture");
Console.WriteLine($"{checks} capture reliability checks passed.");
