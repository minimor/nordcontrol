namespace NordControl.Core.Models;

public sealed record WindowOperationResult(
    bool Success,
    string Message,
    int? NativeErrorCode = null,
    nint? Handle = null)
{
    public static WindowOperationResult Succeeded(string message, nint? handle = null)
    {
        return new WindowOperationResult(true, message, null, handle);
    }

    public static WindowOperationResult Failed(string message, int? nativeErrorCode = null, nint? handle = null)
    {
        return new WindowOperationResult(false, message, nativeErrorCode, handle);
    }
}
