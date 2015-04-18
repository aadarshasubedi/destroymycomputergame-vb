Option Strict On
Option Explicit On
Option Infer Off
#Const showCreationInformation = False
Public Class Damage
    Private Texid As UInteger
    Private ReadOnly creationTime As Long = DateTime.Now.Ticks
    Private _S As System.Drawing.Size
    Private vertices As Short()
    Protected deltaMatrix As Integer()
    Public Sub New(ByVal location As System.Drawing.Point, ByVal S As System.Drawing.Size)
        MyBase.New()
        Dim left As Short = 0
        Dim right As Short = 0
        Dim top As Short = 0
        Dim bottom As Short = 0
        If ((S.Width <> 0) AndAlso (S.Height <> 0)) Then
            left = CShort(location.X - (S.Width / 2))
            right = CShort(location.X + (S.Width / 2))
            top = CShort(location.Y - (S.Height / 2))
            bottom = CShort(location.Y + (S.Height / 2))
        End If
#If showCreationInformation Then
        Game.SayLine("Creating new damage texture:")
        Game.Say("Left: " & left)
        Game.Say(", Right: " & right)
        Game.Say(", Top: " & top)
        Game.Say(", Bottom: " & bottom)
        Game.Say(", Width: " & S.Width)
        Game.SayLine(", Height: " & S.Height)
#End If
        vertices = New Short() { _
            left, top, _
            left, bottom, _
            right, top, _
            right, bottom}
    End Sub
    Public Sub New(ByVal Tex_id_number As UInteger, ByVal location As System.Drawing.Point, ByVal S As System.Drawing.Size)
        Me.New(location, S)
        Texid = Tex_id_number
    End Sub
    ReadOnly Property TextureID() As UInteger
        Get
            Return Texid
        End Get
    End Property
    ReadOnly Property Top() As Integer
        Get
            Return vertices(1)
        End Get
    End Property
    ReadOnly Property Bottom() As Integer
        Get
            Return vertices(3)
        End Get
    End Property
    ReadOnly Property Left() As Integer
        Get
            Return vertices(0)
        End Get
    End Property
    ReadOnly Property Right() As Integer
        Get
            Return vertices(4)
        End Get
    End Property
    ReadOnly Property getVertices() As Short()
        Get
            SyncLock vertices
                If (IsFlame) Then
                    Dim cDiff As Long = CLng((creationTime - DateTime.Now.Ticks) \ System.TimeSpan.TicksPerMillisecond)
                    For m As Integer = 0 To 7
                        'SyncLock vertices
                        vertices(m) = CShort(vertices(m) + (deltaMatrix(m) * (cDiff / 66)))
                        'vertices(m) = CShort(vertices(m) + (deltaMatrix(m) * (cDiff / 33)))
                        'End SyncLock
                    Next
                End If
                Return vertices
            End SyncLock
        End Get
    End Property
    ReadOnly Property Width() As Integer
        Get
            Return _S.Width
        End Get
    End Property
    ReadOnly Property Height() As Integer
        Get
            Return _S.Height
        End Get
    End Property
    Overridable ReadOnly Property IsFlame() As Boolean
        Get
            Return False
        End Get
    End Property
End Class

