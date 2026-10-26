using Captovix;
int checks=0;void Check(bool ok,string text){if(!ok)throw new Exception(text);checks++;Console.WriteLine("PASS: "+text);}
foreach(var language in OcrContract.Languages)foreach(int mode in new[]{3,6,11}){OcrContract.Validate(4000,5000,language,mode);Check(true,"Accept bounded supported language/layout: "+language+"/"+mode);}
foreach(var input in new[]{(0,40,"eng",3),(40,1,"eng",3),(4001,5000,"eng",3),(32769,2,"eng",3),(40,40,"chi",3),(40,40,"eng",99)}){
 bool rejected=false;try{OcrContract.Validate(input.Item1,input.Item2,input.Item3,input.Item4);}catch(ArgumentException){rejected=true;}Check(rejected,"Reject invalid OCR dimensions/language/layout");
}
Check(OcrContract.Normalize("Tiếng Việt\r\nHà Nội\r\n\f")=="Tiếng Việt\nHà Nội","Keep Vietnamese Unicode and line structure");
Check(OcrContract.Normalize("a\r\nb\rc\n")=="a\nb\nc","Normalize mixed line endings");
Check(OcrContract.Normalize("Vie\u0323\u0302t").IsNormalized(),"OCR output uses Unicode NFC");
Check(OcrContract.Selection(90.2,70.1,10.8,20.9,100,80)==new PixelRect(10,20,81,51),"Reverse drag covers exact touched pixels");
Check(OcrContract.Selection(-20,-30,500,600,100,80)==new PixelRect(0,0,100,80),"Selection clips to image bounds");
var random=new Random(116);bool valid=true;
for(int i=0;i<1000;i++){double x1=random.NextDouble()*400-100,y1=random.NextDouble()*300-100,x2=random.NextDouble()*400-100,y2=random.NextDouble()*300-100;var rect=OcrContract.Selection(x1,y1,x2,y2,200,100);valid&=rect.X>=0&&rect.Y>=0&&rect.Right<=200&&rect.Bottom<=100&&rect.Width>=0&&rect.Height>=0;}
Check(valid,"1000 randomized preview selections remain within source pixels");
bool nonfinite=false;try{OcrContract.Selection(double.NaN,0,1,2,100,100);}catch(ArgumentException){nonfinite=true;}Check(nonfinite,"Reject nonfinite pointer coordinates");
Console.WriteLine($"{checks} OCR contract checks passed. Native Windows OCR requires separate acceptance.");
