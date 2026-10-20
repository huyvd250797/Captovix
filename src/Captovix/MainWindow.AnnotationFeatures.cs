using System.Windows;
using System.Windows.Controls;

namespace Captovix;
public partial class MainWindow
{
    private List<AnnotationPreset> _customPresets=[];
    private bool _presetsUnavailable;
    private void InitializeAnnotationFeatures()
    {
        _customPresets=AnnotationPresetStore.Load(App.DataRoot,out var warning);_presetsUnavailable=warning!=null;
        RefreshPresets();SavePresetButton.IsEnabled=DeletePresetButton.IsEnabled=!_presetsUnavailable;
        if(warning!=null)SetStatus(warning);
    }
    private void RefreshPresets(string? selected=null)
    {
        PresetPicker.ItemsSource=AnnotationPresetStore.BuiltIn.Concat(_customPresets).ToArray();
        PresetPicker.SelectedItem=PresetPicker.Items.Cast<AnnotationPreset>().FirstOrDefault(p=>p.Name==selected)??PresetPicker.Items[0];
    }
    private void SyncAnnotationFeatures()
    {
        if(EffectPicker==null)return;SelectValue(EffectPicker,Surface.EffectSize);
        var filters=Surface.Tool is EditorTool.Blur or EditorTool.Pixelate or EditorTool.Cover||_session?.Document.Annotations.Any(a=>Surface.SelectedIds.Contains(a.Id)&&a.IsRedaction)==true;
        EffectProperties.Visibility=filters?Visibility.Visible:Visibility.Collapsed;
        EffectPicker.IsEnabled=Surface.Tool is EditorTool.Blur or EditorTool.Pixelate||_session?.Document.Annotations.Any(a=>Surface.SelectedIds.Contains(a.Id)&&a.Kind is AnnotationKind.Blur or AnnotationKind.Pixelate)==true;
        AlignmentPanel.IsEnabled=Surface.SelectedIds.Count>0;
        MultiSelectionHint.Visibility=Surface.SelectedIds.Count>1?Visibility.Visible:Visibility.Collapsed;
    }
    private void ChangeSelected(Action<Annotation> change)
    {
        if(_session==null||Surface.SelectedIds.Count==0)return;var ids=Surface.SelectedIds.ToHashSet();
        _session.Commit();_session.Change(d=>{foreach(var a in d.Annotations.Where(a=>ids.Contains(a.Id)))change(a);});
    }
    private void Effect_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(_syncProperties||Surface==null||EffectPicker==null)return;
        Surface.EffectSize=(int)PickedValue(EffectPicker,24);int strength=Surface.EffectSize;
        ChangeSelected(a=>{if(a.Kind is AnnotationKind.Blur or AnnotationKind.Pixelate)a.EffectSize=strength;});Surface.Refresh();
    }
    private void ApplyPreset_Click(object sender,RoutedEventArgs e)
    {
        if(PresetPicker.SelectedItem is not AnnotationPreset preset)return;
        Surface.InkColor=preset.Color;Surface.StrokeSize=preset.Stroke;Surface.TextSize=preset.FontSize;Surface.EffectSize=preset.EffectSize;
        ChangeSelected(preset.Apply);Surface.Refresh();SyncProperties();SetStatus($"Đã áp dụng preset {preset.Name}.");
    }
    private void SavePreset_Click(object sender,RoutedEventArgs e)
    {
        if(_presetsUnavailable)return;
        try{
            var preset=new AnnotationPreset(PresetName.Text.Trim(),Surface.InkColor,Surface.StrokeSize,Surface.TextSize,Surface.EffectSize);
            var candidate=_customPresets.Append(preset).ToList();AnnotationPresetStore.Save(App.DataRoot,candidate);
            _customPresets=candidate;RefreshPresets(preset.Name);PresetName.Clear();SetStatus("Đã lưu preset riêng.");
        }catch(Exception ex){SetStatus("Chưa lưu preset: "+ex.Message);}
    }
    private void DeletePreset_Click(object sender,RoutedEventArgs e)
    {
        if(_presetsUnavailable||PresetPicker.SelectedItem is not AnnotationPreset preset)return;
        if(!_customPresets.Contains(preset)){SetStatus("Preset có sẵn được giữ. Chọn một preset riêng để xóa.");return;}
        try{
            var candidate=_customPresets.Where(p=>p!=preset).ToList();AnnotationPresetStore.Save(App.DataRoot,candidate);
            _customPresets=candidate;RefreshPresets();SetStatus("Đã xóa preset riêng; đối tượng trên ảnh được giữ nguyên.");
        }catch(Exception ex){SetStatus("Chưa xóa preset: "+ex.Message);}
    }
    private void Align_Click(object sender,RoutedEventArgs e)
    {
        if(_session==null||Surface.SelectedIds.Count==0||sender is not Button {Tag:string tag})return;
        var ids=Surface.SelectedIds.ToHashSet();_session.Commit();
        _session.Change(d=>AnnotationEditing.Align(d,ids,Enum.Parse<AnnotationAlignment>(tag),EditorRenderer.Box));
        Surface.Refresh();SyncProperties();Surface.Focus();
    }
}
