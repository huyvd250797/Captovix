namespace Captovix;

/// <summary>Bounded, user-selected timing. A quiet frame is not proof of page end.</summary>
public sealed record CaptureTiming(int SettleMilliseconds=700,int WheelSteps=1)
{
    public void Validate()
    {
        if(SettleMilliseconds<350||SettleMilliseconds>5000||WheelSteps<1||WheelSteps>3)
            throw new ArgumentOutOfRangeException(nameof(SettleMilliseconds),"Thời gian chờ 350–5000 ms; mỗi nhịp cuộn 1–3 nấc.");
    }
    public int IntervalMilliseconds=>checked(SettleMilliseconds+700);
}

public enum CaptureReadiness { Ready, Closed, Moved, Covered, Background }

public static class CaptureReliability
{
    public static CaptureReadiness Evaluate(bool alive,bool sameBounds,bool foreground,bool visible)
        =>!alive?CaptureReadiness.Closed:!sameBounds?CaptureReadiness.Moved:!foreground?CaptureReadiness.Background:!visible?CaptureReadiness.Covered:CaptureReadiness.Ready;
    public static string Explain(CaptureReadiness value)=>value switch
    {
        CaptureReadiness.Closed=>"Cửa sổ nguồn đã đóng. Kết thúc phiên và chọn lại vùng.",
        CaptureReadiness.Moved=>"Cửa sổ nguồn đã di chuyển hoặc đổi kích thước. Kết thúc phiên và chọn lại vùng.",
        CaptureReadiness.Background=>"Đã tạm dừng: đưa cửa sổ nguồn ra trước rồi bấm Tiếp tục.",
        CaptureReadiness.Covered=>"Đã tạm dừng: vùng chụp bị cửa sổ khác che. Bỏ phần che rồi bấm Tiếp tục.",
        _=>"Sẵn sàng"
    };
}
