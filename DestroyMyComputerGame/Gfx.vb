Imports System.Drawing
Imports System.Drawing.Imaging

Module Gfx

    'Creates the test pattern for debugging purposes
    'Author: Dylan Taylor
    Friend Function createTestPattern(ByVal screenWidth As Integer, ByVal screenHeight As Integer) As Bitmap
        Dim pattern As Bitmap = New Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb)
        Dim regColor As System.Drawing.Color = Color.White
        Dim altColor As System.Drawing.Color = Color.Black
        Dim alternate As Boolean = False
        For Y As Integer = 0 To (screenHeight - 1)
            For X As Integer = 0 To (screenWidth - 1)
                pattern.SetPixel(X, Y, If(alternate, altColor, regColor))
                alternate = Not alternate
            Next X
            If (screenWidth Mod 2 = 0) Then alternate = Not alternate
        Next Y
        Return pattern
    End Function

    'Checks if two bitmap images are identical
    'Author: Dylan Taylor
    Friend Function checkIfIdenticalBitmap(ByVal originalBitmap As Bitmap, ByVal comparison As Bitmap) As Boolean
        If (originalBitmap.Width <> comparison.Width) OrElse (originalBitmap.Height <> comparison.Height) Then Return False
        For Y As Integer = 0 To (ScreenHeight() - 1)
            For X As Integer = 0 To (ScreenWidth() - 1)
                If (originalBitmap.GetPixel(X, Y) <> comparison.GetPixel(X, Y)) Then Return False
            Next X
        Next Y
        Return True
    End Function

End Module
