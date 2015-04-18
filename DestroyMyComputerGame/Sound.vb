#Const useThreads = False
Option Strict On
Option Explicit On
Imports OpenTK.Audio
Public Class Sound
    Private Const audioCleanupDelay As Integer = 30
    Private Audio_Context As OpenTK.Audio.AudioContext = Nothing
    Private Sources As New List(Of Integer)
    Private initialized As Boolean = False
    Private audioCleanupThread As System.Threading.Thread = New System.Threading.Thread(AddressOf AudioCleanup)
#If useThreads Then
    Private Delegate Sub BeginPlay(ByVal Path As String, ByVal Gain As Single, ByVal DoesLoop As Boolean)
    Private Delegate Sub BeginPlayRAM(ByVal data As Byte(), ByVal Gain As Single, ByVal DoesLoop As Boolean)
#End If
    Private Function LoadWave(ByVal Path As String, ByVal fileStream As IO.Stream, ByRef channels As Integer, ByRef bits As Integer, ByRef rate As Integer) As Byte()
        If fileStream Is Nothing Then
            Throw New ArgumentNullException("stream")
        End If
        Using reader As New IO.BinaryReader(fileStream)
            ' RIFF header
            Dim signature As New String(reader.ReadChars(4))
            If signature <> "RIFF" Then
                Throw New NotSupportedException("Specified stream is not a wave file.")
            End If
            Dim riff_chunck_size As Integer = reader.ReadInt32()
            Dim format As New String(reader.ReadChars(4))
            If format <> "WAVE" Then
                Throw New NotSupportedException("Specified stream is not a wave file.")
            End If
            ' WAVE header
            Dim format_signature As New String(reader.ReadChars(4))
            If format_signature <> "fmt " Then
                Throw New NotSupportedException("Specified wave file is not supported.")
            End If
            Dim format_chunk_size As Integer = reader.ReadInt32()
            Dim audio_format As Integer = reader.ReadInt16()
            Dim num_channels As Integer = reader.ReadInt16()
            Dim sample_rate As Integer = reader.ReadInt32()
            Dim byte_rate As Integer = reader.ReadInt32()
            Dim block_align As Integer = reader.ReadInt16()
            Dim bits_per_sample As Integer = reader.ReadInt16()
            Dim data_signature As New String(reader.ReadChars(4))
            If data_signature <> "data" Then
                Throw New NotSupportedException("Specified wave file is not supported.")
            End If
            Dim data_chunk_size As Integer = reader.ReadInt32()
            channels = num_channels
            bits = bits_per_sample
            rate = sample_rate
            Return reader.ReadBytes(CInt(reader.BaseStream.Length))
        End Using
    End Function
    'This loadwav function works with data in memory.
    Private Function LoadWave(ByVal data As Byte(), ByRef channels As Integer, ByRef bits As Integer, ByRef rate As Integer) As Byte()
        Dim ms As System.IO.Stream = New System.IO.MemoryStream(data)
        Using reader As New IO.BinaryReader(ms)
            ms.Seek(0, IO.SeekOrigin.Begin)
            ' RIFF header
            Dim signature As New String(reader.ReadChars(4))
            If signature <> "RIFF" Then
                Throw New NotSupportedException("Specified stream is not a wave file.")
            End If
            Dim riff_chunck_size As Integer = reader.ReadInt32()
            Dim format As New String(reader.ReadChars(4))
            If format <> "WAVE" Then
                Throw New NotSupportedException("Specified stream is not a wave file.")
            End If
            ' WAVE header
            Dim format_signature As New String(reader.ReadChars(4))
            If format_signature <> "fmt " Then
                Throw New NotSupportedException("Specified wave file is not supported.")
            End If
            Dim format_chunk_size As Integer = reader.ReadInt32()
            Dim audio_format As Integer = reader.ReadInt16()
            Dim num_channels As Integer = reader.ReadInt16()
            Dim sample_rate As Integer = reader.ReadInt32()
            Dim byte_rate As Integer = reader.ReadInt32()
            Dim block_align As Integer = reader.ReadInt16()
            Dim bits_per_sample As Integer = reader.ReadInt16()
            Dim data_signature As New String(reader.ReadChars(4))
            If data_signature <> "data" Then
                Throw New NotSupportedException("Specified wave file is not supported.")
            End If
            Dim data_chunk_size As Integer = reader.ReadInt32()
            channels = num_channels
            bits = bits_per_sample
            rate = sample_rate
            Return reader.ReadBytes(CInt(reader.BaseStream.Length))
        End Using
    End Function
    Private Function GetSoundFormat(ByVal channels As Integer, ByVal bits As Integer) As OpenAL.ALFormat
        Select Case channels
            Case 1
                Return If(bits = 8, OpenAL.ALFormat.Mono8, OpenAL.ALFormat.Mono16)
            Case 2
                Return If(bits = 8, OpenAL.ALFormat.Stereo8, OpenAL.ALFormat.Stereo16)
            Case Else
                Throw New NotSupportedException("The specified sound format is not supported.")
        End Select
    End Function
#If useThreads Then
    'The following method loads a file, and plays the data from it
    Sub Play(ByVal Path As String, ByVal Gain As Single, ByVal DoesLoop As Boolean)
        On Error Resume Next
        If (Audio_Context Is Nothing) Then Audio_Context = New AudioContext
        If Err.Number <> 0 Then Game.SayLine("Error initializing audio context: " & Err.Description)
        On Error GoTo 0
        Dim T As New BeginPlay(AddressOf PlayAudio)
        T.BeginInvoke(Path, Gain, DoesLoop, Nothing, Nothing)
    End Sub
    'The following method loads data from a memory stream, playing the audio in it.
    Sub Play(ByVal data As Byte(), ByVal Gain As Single, ByVal DoesLoop As Boolean)
        Try
            If (Audio_Context Is Nothing) Then Audio_Context = New AudioContext
        Catch ex As Exception
            Game.SayLine("Error initializing audio context: " & ex.Message)
        End Try
        Dim T As New BeginPlayRAM(AddressOf PlayAudioFromRAM)
        T.BeginInvoke(data, Gain, DoesLoop, Nothing, Nothing)
    End Sub
#End If

    Friend Sub PlaySound(ByVal sound As Object, ByVal Gain As Single, ByVal DoesLoop As Boolean, Optional ByVal VectorX As Integer = 0, Optional ByVal VectorY As Integer = 0)
        If (Not initialized) Then
            Try
                If (Audio_Context Is Nothing) Then Audio_Context = New AudioContext
                audioCleanupThread.Start()
                initialized = True
                Game.SayLine("Sound system initialized succesfully! :)")
            Catch ex As Exception
                Game.SayLine("Error initializing sound system: " & ex.Message)
                Exit Sub
            End Try
        End If
        Try
            If (sound.GetType().ToString = "System.String") Then
                Play(CStr(sound), Gain, DoesLoop, New OpenTK.Vector3(0, 0, 0))
            ElseIf (sound.GetType().ToString = "System.Byte[]") Then
                Play(CType(sound, Byte()), Gain, DoesLoop, New OpenTK.Vector3(0, 0, 0))
            Else
                MsgBox("Invalid Sound Type: " & sound.GetType().ToString)
                End
            End If
        Catch ex As Threading.ThreadAbortException
            Exit Sub 'Its OK, we don't WANT to catch/handle this
        Catch ex As Exception
            MsgBox("Exception occured when playing sound: " & ex.Message)
        End Try
    End Sub


    Private Sub Play(ByVal data As Byte(), ByVal Gain As Single, ByVal DoesLoop As Boolean, ByVal sourceLocation As OpenTK.Vector3)
        Dim Buffer As Integer = OpenAL.AL.GenBuffer
        Dim Source As Integer = OpenAL.AL.GenSource
        Dim Channels As Integer = Nothing
        Dim Bits As Integer = Nothing
        Dim Rate As Integer = Nothing
        Dim soundData As Byte() = Nothing
        Try
            soundData = LoadWave(data, Channels, Bits, Rate)
            OpenAL.AL.BufferData(Buffer, GetSoundFormat(Channels, Bits), soundData, soundData.Length, Rate)
            'soundData = Nothing
            Dim xf As Integer = OpenAL.AL.GetError
            If xf <> 0 Then Game.SayLine("OpenAL error while loading: " & xf)
        Catch ex As Threading.ThreadAbortException
            Exit Sub 'Its OK, we don't WANT to catch/handle this
        Catch ex As Exception
            MsgBox("Error loading sound file: " & ex.Message)
        End Try
        Try
            OpenAL.AL.Source(Source, OpenAL.ALSourcei.Buffer, Buffer)
            OpenAL.AL.Source(Source, OpenAL.ALSource3f.Position, sourceLocation)
            OpenAL.AL.Source(Source, OpenAL.ALSourcef.Gain, Gain)
            Dim xs As Integer = OpenAL.AL.GetError
            If xs <> 0 Then Game.SayLine("OpenAL error while setting sources: " & xs)
            If DoesLoop Then OpenAL.AL.Source(Source, OpenAL.ALSourceb.Looping, True)
            SyncLock Sources
                Sources.Add(Source)
            End SyncLock
            OpenAL.AL.SourcePlay(Source)
            Dim xp As Integer = OpenAL.AL.GetError
            If xp <> 0 Then Game.SayLine("OpenAL error while playing sources: " & xp)
#If useThreads Then
        While OpenAL.AL.GetSourceState(Source) = OpenAL.ALSourceState.Playing
            Threading.Thread.Sleep(250)
        End While
        OpenAL.AL.SourceStop(source)
        OpenAL.AL.DeleteSource(source)
        OpenAL.AL.DeleteBuffer(Convert.ToUInt32(Buffer))
#End If
            Dim X = OpenAL.AL.GetError
            If X <> 0 Then Game.SayLine("OpenAL error: " & X)
#If useThreads Then
        SyncLock Sources
            Sources.Remove(Source)
        End SyncLock
#End If
        Catch ex As Exception
            MsgBox("Exception when playing sound: " & ex.Message)
        End Try
    End Sub

    Private Sub Play(ByVal Path As String, ByVal Gain As Single, ByVal DoesLoop As Boolean, ByVal sourceLocation As OpenTK.Vector3)
        Dim Buffer As Integer = OpenAL.AL.GenBuffer
        Dim Source As Integer = OpenAL.AL.GenSource
        Dim SoundData As Byte()
        Dim Channels As Integer = Nothing
        Dim Bits As Integer = Nothing
        Dim Rate As Integer = Nothing
        Try
            SoundData = LoadWave(Path, IO.File.OpenRead(Path), Channels, Bits, Rate)
            OpenAL.AL.BufferData(Buffer, GetSoundFormat(Channels, Bits), SoundData, SoundData.Length, Rate)
        Catch ex As Threading.ThreadAbortException
            Exit Sub 'Its OK, we don't WANT to catch/handle this
        Catch ex As Exception
            MsgBox("Error loading sound file: " & ex.Message)
        End Try
        OpenAL.AL.Source(Source, OpenAL.ALSourcei.Buffer, Buffer)
        OpenAL.AL.Source(Source, OpenAL.ALSource3f.Position, sourceLocation)
        OpenAL.AL.Source(Source, OpenAL.ALSourcef.Gain, Gain)
        If DoesLoop Then OpenAL.AL.Source(Source, OpenAL.ALSourceb.Looping, True)
        SyncLock Sources
            Sources.Add(Source)
            If (Sources(Sources.Count - 1) = Nothing) Then
                Game.SayLine("Failed to load source.")
                Exit Sub
            End If
            OpenAL.AL.SourcePlay(Source)
        End SyncLock
        Dim xp As Integer = OpenAL.AL.GetError
        If xp <> 0 Then
            Game.SayLine("OpenAL error while playing sources: " & xp)
        End If
#If useThreads Then
            While OpenAL.AL.GetSourceState(Source) = OpenAL.ALSourceState.Playing
                Threading.Thread.Sleep(250)
            End While
            OpenAL.AL.SourceStop(Source)
            OpenAL.AL.DeleteSource(Source)
            OpenAL.AL.DeleteBuffer(Convert.ToUInt32(Buffer))
#End If
        Dim X = OpenAL.AL.GetError
        If X <> 0 Then MsgBox("OpenAL error: " & X)
#If useThreads Then
        SyncLock Sources
            Sources.Remove(Source)
        End SyncLock
#End If
    End Sub


    Protected Function GetSourceCount() As Integer
        SyncLock Sources
            Return Sources.Count
        End SyncLock
    End Function

    Private Sub AudioCleanup()
        Do
            Threading.Thread.Sleep(audioCleanupDelay)
            'Game.SayLine("Audio Cleanup is running.")
            SyncLock Sources
                'For Each Source In Sources
                '    If (Not (OpenAL.AL.GetSourceState(Source) = OpenAL.ALSourceState.Playing) _
                '    OrElse (OpenAL.AL.GetSourceState(Source) = OpenAL.ALSourceState.Initial)) Then
                '        OpenAL.AL.SourceStop(Source)
                '        OpenAL.AL.DeleteSource(Source)
                '        Game.SayLine("Removing Source")
                '        Sources.Remove(Source)
                '    End If
                'Next
                For I = Sources.Count - 1 To 0 Step -1
                    If (Not (OpenAL.AL.GetSourceState(Sources(I)) = OpenAL.ALSourceState.Playing) _
                        OrElse (OpenAL.AL.GetSourceState(Sources(I)) = OpenAL.ALSourceState.Initial)) Then
                        OpenAL.AL.SourceStop(Sources(I))
                        OpenAL.AL.DeleteSource(Sources(I))
                        'Game.SayLine("Removing Source")
                        Sources.RemoveAt(I)
                    End If
                Next
            End SyncLock
        Loop
    End Sub

    Overloads Sub Dispose()
        For Each S As Integer In Sources
            OpenAL.AL.SourceStop(S)
        Next
        Audio_Context.Dispose()
    End Sub
End Class
