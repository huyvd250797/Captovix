using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
namespace Captovix;
public partial class MainWindow
{
    MockupInspector? _mockupInspector;EditorSession? _inspectorSession;
    private void OpenMockupInspector(Annotation a){InspectorPane.Visibility=Visibility.Visible;InspectorColumn.MinWidth=210;InspectorColumn.Width=new(Math.Max(250,_inspectorWidth));SyncProperties();MockupProperties.BringIntoView();}
    private void SyncMockupInspector(Annotation a){
        if(_mockupInspector==null){
            var panel=_mockupInspector=new MockupInspector();MockupInspectorHost.Children.Add(panel);
            panel.Changed=(id,change)=>{if(_session==null||!ReferenceEquals(_session,_inspectorSession))return;EndInlineText(true);_session.Change(d=>MockupEditing.Apply(d,id,change));Surface.Refresh();SetStatus("Đã sửa Mockup · Ctrl+Z hoàn tác.");};
            panel.EditText=BeginInlineText;panel.PickColor=()=>NativeColorPicker.Pick(this);
            panel.PickScreen=async target=>{var session=_session;string? id=Surface.SelectedId;long revision=session?.Revision??-1;var color=await ScreenColorPicker.PickAsync(this);if(color!=null&&ReferenceEquals(_session,session)&&session?.Revision==revision&&Surface.SelectedId==id)panel.SetPickedColor(target,color);};
            panel.Recognize=async (language,token)=>{if(_session is not {} session||Surface.Selected is not {Mockup:not null} selected||Surface.Original is not {} source)return;long revision=session.Revision;string id=selected.Id;var region=new PixelRect((int)selected.Left,(int)selected.Top,(int)selected.Width,(int)selected.Height).Intersect(session.Document.Viewport);BitmapSource? snapshot=null;var doc=session.Document.Snapshot();
                try{await StaImageWorker.Run(()=>snapshot=EditorRenderer.MockupSnapshot(source,doc,region));var result=await OcrService.RecognizeAsync(snapshot!,new(0,0,region.Width,region.Height),OcrContract.Languages[language],6,token);if(ReferenceEquals(_session,session)&&session.Revision==revision&&Surface.SelectedId==id){if(result.Error!=null)throw new InvalidOperationException(result.Error);panel.SetText(OcrContract.Normalize(result.Text));}else SetStatus("Ảnh/nội dung đã đổi; chưa áp dụng OCR.");}catch(Exception ex){SetStatus("OCR Mockup: "+ex.Message);}
            };
            panel.RenderPreview=async()=>{if(_session is not {} session||Surface.Original is not {} source)throw new InvalidOperationException("Chưa có ảnh.");var doc=session.Document.Snapshot();var original=doc.Snapshot();original.Annotations.Clear();original.Recoveries.Clear();BitmapSource? before=null,after=null;await StaImageWorker.Run(()=>{try{before=EditorRenderer.Render(source,original,480);after=EditorRenderer.Render(source,doc,480);}finally{EditorRenderer.ReleaseImageCaches(source);}});return(before!,after!);};
        }
        _mockupInspector.Load(a);_inspectorSession=_session;
    }
}
