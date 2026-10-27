using System.Linq;
namespace Captovix;
public partial class MainWindow {
 private bool _skipSavedStyle;
 private void ApplyLatestSavedStyle(EditorTool tool){
  if(_skipSavedStyle)return;var kind=tool switch{EditorTool.Callout=>AnnotationKind.Callout,EditorTool.Text=>AnnotationKind.Text,EditorTool.Step=>AnnotationKind.Step,EditorTool.Shape=>Surface.ShapeKind,EditorTool.Arrow=>AnnotationKind.Arrow,_=>System.Enum.TryParse<AnnotationKind>(tool.ToString(),out var parsed)?parsed:(AnnotationKind?)null};
  if(kind==null)return;var saved=_favorites.LastOrDefault(f=>f.Style.Kind==kind||tool==EditorTool.Shape&&ShapeStudio.IsClosed(f.Style.Kind));if(saved==null)return;
  var a=saved.Style;var target=tool==EditorTool.Callout?Surface.CalloutDefaults:tool==EditorTool.Text||tool==EditorTool.Step?Surface.TextDefaultsFor(kind.Value):FocusStudio.Supports(kind.Value)?Surface.FocusDefaultsFor(kind.Value):Surface.ShapeDefaults;
  ShapeStudio.CopyStyle(a,target);TextStudio.CopyStyle(a,target);Surface.InkColor=a.Color;Surface.StrokeSize=a.Stroke;Surface.TextSize=a.FontSize;Surface.EffectSize=a.EffectSize;Surface.ArrowRoute=a.Route;if(tool==EditorTool.Shape)Surface.ShapeKind=a.Kind;
 }
}
