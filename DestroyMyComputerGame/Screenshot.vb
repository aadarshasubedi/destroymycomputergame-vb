Option Strict On
Imports System.Drawing
Imports System.Windows.Forms

Public Class Screenshot
    'Takes a screenshot
    'Source: http://www.reflectionforbrain.com/21201014292.php
    Public Shared Image As Image
    Public Shared Bounds As Rectangle
    Shared Function GetDesktopImage(Optional ByVal Width As Integer = 0, Optional ByVal Height As Integer = 0, Optional ByVal ShowCursor As Boolean = True) As Image
        Dim W As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim H As Integer = Screen.PrimaryScreen.Bounds.Height
        Dim DesktopBitmap As New Bitmap(W, H)
        Dim g As Graphics = Graphics.FromImage(DesktopBitmap)
        g.CopyFromScreen(0, 0, 0, 0, New Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), CopyPixelOperation.SourceCopy)
        If ShowCursor Then Cursors.Default.Draw(g, New Rectangle(Cursor.Position, New Size(32, 32)))
        g.Dispose()
        If Width = 0 And Height = 0 Then
            Image = DesktopBitmap
            Return DesktopBitmap
        Else
            Dim ScaledBitmap As Image = DesktopBitmap.GetThumbnailImage(Width, Height, Nothing, IntPtr.Zero)
            DesktopBitmap.Dispose()
            Image = ScaledBitmap
            Return ScaledBitmap
        End If
    End Function
End Class
