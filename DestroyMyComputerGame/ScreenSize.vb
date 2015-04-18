Option Strict On
Imports System.Windows.Forms

Module ScreenSize
    'Gets the size (width/height) of the screen
    'Written by Dylan Taylor
    Public Function ScreenWidth() As Integer
        Return Screen.PrimaryScreen.Bounds.Width
    End Function
    Public Function ScreenHeight() As Integer
        Return Screen.PrimaryScreen.Bounds.Height
    End Function
    Public Function Size() As Drawing.Size
        Return Screen.PrimaryScreen.Bounds.Size
    End Function
End Module
