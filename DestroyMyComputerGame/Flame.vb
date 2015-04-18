Option Strict On
Option Explicit On
Option Infer Off
Public Class Flame
    Inherits Damage
    Public Sub New(ByVal location As System.Drawing.Point, ByVal S As System.Drawing.Size, ByVal DeltaX As Integer, ByVal DeltaY As Integer)
        MyBase.New(location, S)
        deltaMatrix = New Integer() { _
            DeltaX, DeltaY, _
            DeltaX, DeltaY, _
            DeltaX, DeltaY, _
            DeltaX, DeltaY}
    End Sub
    NotOverridable Overrides ReadOnly Property IsFlame() As Boolean
        Get
            Return True
        End Get
    End Property
End Class
