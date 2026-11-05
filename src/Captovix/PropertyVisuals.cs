using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Captovix;
internal static class PropertyVisuals
{
    internal static CheckBox Typography(string content,string tooltip){var c=new CheckBox{Content=content,ToolTip=tooltip,FontWeight=content=="B"?FontWeights.Bold:FontWeights.Normal,FontStyle=content=="I"?FontStyles.Italic:FontStyles.Normal};if(content=="U")c.Content=new TextBlock{Text="U",TextDecorations=TextDecorations.Underline};c.SetResourceReference(FrameworkElement.StyleProperty,"TypographyToggle");return c;}
    internal static FrameworkElement ColorField(string title,TextBox field,Func<string?> pick,bool transparent=false){
        var panel=new StackPanel{Margin=new(0,6,0,4)};panel.Children.Add(new TextBlock{Text=title,Margin=new(3,0,0,3)});
        var chip=new Border{Width=34,Height=22,CornerRadius=new(4),BorderThickness=new(1),Margin=new(0,0,9,0)};chip.SetResourceReference(Border.BorderBrushProperty,"PopupLine");var caption=new TextBlock{Text=title,VerticalAlignment=VerticalAlignment.Center};var row=new StackPanel{Orientation=Orientation.Horizontal};row.Children.Add(chip);row.Children.Add(caption);var button=new Button{Content=row,HorizontalContentAlignment=HorizontalAlignment.Left,Padding=new(10,7,10,7)};button.Click+=(_,_)=>{var c=pick();if(c!=null)field.Text=c;};panel.Children.Add(button);
        var palette=new WrapPanel{Margin=new(2,3,0,0)};foreach(var color in new[]{"#FFFFFF","#20242C","#2563EB","#7C3AED","#EF5268","#00979E"}){var b=new Button{Width=23,Height=23,MinWidth=0,MinHeight=0,Margin=new(2),Padding=new(0),Background=EditorRenderer.Ink(color),ToolTip=color};b.Click+=(_,_)=>field.Text=color;palette.Children.Add(b);}if(transparent){var clear=new Button{Content="∅",Width=25,Height=23,MinWidth=0,MinHeight=0,Padding=new(0),ToolTip="Trong suốt"};clear.Click+=(_,_)=>field.Text="#00000000";palette.Children.Add(clear);}panel.Children.Add(palette);
        field.MaxLength=9;panel.Children.Add(new Expander{Header="Mã màu tùy chỉnh",Content=field,IsExpanded=false});
        void Refresh(){try{chip.Background=EditorRenderer.Ink(field.Text);button.ToolTip=field.Text;caption.Text=title;}catch{caption.Text="Chọn màu hợp lệ";}}field.TextChanged+=(_,_)=>Refresh();Refresh();return panel;
    }
}
