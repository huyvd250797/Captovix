using System.Windows.Media.Imaging;
namespace Captovix;
public static partial class EditorRenderer
{
    public static void ReleaseImageCaches(BitmapSource? source=null,bool clearSharedRaster=true)
    {
        _recoveredSampling=null;_recoveryDrawings=null;_layouts=null;_scanInk=null;_calloutDrawings=null;_calloutGeometries=null;
        _rasterSampling=null;_rasterSamplingOriginal=null;_rasterSamplingKey=null;_rasterData=null;
        if(source!=null)RedactionRenderer.Release(source);
        if(clearSharedRaster)RasterPixels.ClearCache();
    }
}
