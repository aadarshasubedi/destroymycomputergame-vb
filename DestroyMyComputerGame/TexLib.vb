'Based off of 'Texture Utility Library'
'Source: http://www.opentk.com/project/TexLib
Imports OpenTK.Graphics.OpenGL
Imports System.Diagnostics
Imports System.Drawing
Imports Img = System.Drawing.Imaging


' Example code:
'
'      // Setup GL state for ordinary texturing.
'      TexUtil.InitTexturing();
'
'      // Load a bitmap from disc, and put it in a GL texture.
'      int tex = TexUtil.CreateTextureFromFile("mybitmapfont.png");
'
'      // Create a TextureFont object from the loaded texture.
'      TextureFont texFont = new TextureFont(tex);
'
'      // Write something centered in the viewport.
'      texFont.WriteStringAt("Center", 10, 50, 50, 0);
'

Namespace TexLib
    ''' <summary>
    ''' The TexUtil class is released under the MIT-license.
    ''' /Olof Bjarnason
    ''' </summary>
    Public NotInheritable Class TexUtil
        Private Sub New()
        End Sub
#Region "Public"

        ''' <summary>
        ''' Initialize OpenGL state to enable alpha-blended texturing.
        ''' Disable again with GL.Disable(EnableCap.Texture2D).
        ''' Call this before drawing any texture, when you boot your
        ''' application, eg. in OnLoad() of GameWindow or Form_Load()
        ''' if you're building a WinForm app.
        ''' </summary>
        Public Shared Sub InitTexturing()
            GL.Disable(EnableCap.CullFace)
            GL.Enable(EnableCap.Texture2D)
            GL.Enable(EnableCap.Blend)
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha)
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1)
        End Sub

        ''' <summary>
        ''' Create an opaque OpenGL texture object from a given byte-array of r,g,b-triplets.
        ''' Make sure width and height is 1, 2, .., 32, 64, 128, 256 and so on in size since all
        ''' 3d graphics cards support those dimensions. Not necessarily square. Don't forget
        ''' to call GL.DeleteTexture(int) when you don't need the texture anymore (eg. when switching
        ''' levels in your game).
        ''' </summary>
        Public Shared Function CreateRGBTexture(ByVal width As Integer, ByVal height As Integer, ByVal rgb As Byte()) As Integer
            Return CreateTexture(width, height, False, rgb)
        End Function

        ''' <summary>
        ''' Create a translucent OpenGL texture object from given byte-array of r,g,b,a-triplets.
        ''' See CreateRGBTexture for more info.
        ''' </summary>
        Public Shared Function CreateRGBATexture(ByVal width As Integer, ByVal height As Integer, ByVal rgba As Byte()) As Integer
            Return CreateTexture(width, height, True, rgba)
        End Function

        ''' <summary>
        ''' Create an OpenGL texture (translucent or opaque) from a given Bitmap.
        ''' 24- and 32-bit bitmaps supported.
        ''' </summary>
        Public Shared Function CreateTextureFromBitmap(ByVal bitmap As Bitmap) As Integer
            Dim data As Img.BitmapData = bitmap.LockBits(New Rectangle(0, 0, bitmap.Width, bitmap.Height), Img.ImageLockMode.[ReadOnly], Img.PixelFormat.Format32bppArgb)
            Dim tex As Integer = GiveMeATexture()
            GL.BindTexture(TextureTarget.Texture2D, tex)
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, _
             PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0)
            bitmap.UnlockBits(data)
            SetParameters()
            Return tex
        End Function

        ''' <summary>
        ''' Create an OpenGL texture (translucent or opaque) by loading a bitmap
        ''' from file. 24- and 32-bit bitmaps supported.
        ''' </summary>
        Public Shared Function CreateTextureFromFile(ByVal path As String) As Integer
            Return CreateTextureFromBitmap(New System.Drawing.Bitmap(System.Drawing.Bitmap.FromFile(path)))
        End Function

#End Region

        Private Shared Function CreateTexture(ByVal width As Integer, ByVal height As Integer, ByVal alpha As Boolean, ByVal bytes As Byte()) As Integer
            Dim expectedBytes As Integer = width * height * (If(alpha, 4, 3))
            Debug.Assert(expectedBytes = bytes.Length)
            Dim tex As Integer = GiveMeATexture()
            Upload(width, height, alpha, bytes)
            SetParameters()
            Return tex
        End Function

        Public Shared Function GiveMeATexture() As UInteger
            Dim tex As UInteger = GL.GenTexture()
            GL.BindTexture(TextureTarget.Texture2D, tex)
            Return tex
        End Function

        Private Shared Sub SetParameters()
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, CInt(TextureMinFilter.Linear))
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, CInt(TextureMagFilter.Linear))
        End Sub

        Private Shared Sub Upload(ByVal width As Integer, ByVal height As Integer, ByVal alpha As Boolean, ByVal bytes As Byte())
            GL.TexImage2D(Of Byte)(TextureTarget.Texture2D, 0, If(alpha, PixelInternalFormat.Rgba, PixelInternalFormat.Rgb), width, height, 0, _
             If(alpha, PixelFormat.Rgba, PixelFormat.Rgb), PixelType.UnsignedByte, bytes)
        End Sub
    End Class

    Public Class TextureFont
        ''' <summary>
        ''' Create a TextureFont object. The sent-in textureId should refer to a
        ''' texture bitmap containing a 16x16 grid of fixed-width characters,
        ''' representing the ASCII table. A 32 bit texture is assumed, aswell as
        ''' all GL state necessary to turn on texturing. The dimension of the
        ''' texture bitmap may be anything from 128x128 to 512x256 or any other
        ''' order-by-two-squared-dimensions.
        ''' </summary>
        Public Sub New(ByVal textureId As Integer)
            Me.textureId = textureId
        End Sub

        ''' <summary>
        ''' Draw an ASCII string around coordinate (0,0,0) in the XY-plane of the
        ''' model space coordinate system. The height of the text is 1.0.
        ''' The width may be computed by calling ComputeWidth(string).
        ''' This call modifies the currently bound
        ''' 2D-texture, but no other GL state.
        ''' </summary>
        Public Sub WriteString(ByVal text As String)
            GL.BindTexture(TextureTarget.Texture2D, textureId)
            GL.PushMatrix()
            Dim width As Double = ComputeWidth(text)
            GL.Translate(-width / 2.0, -0.5, 0)
            GL.Begin(BeginMode.Quads)
            Dim xpos As Double = 0
            For Each ch As Char In text
                WriteCharacter(ch, xpos)
                xpos += AdvanceWidth
            Next
            GL.[End]()
            GL.PopMatrix()
        End Sub

        ''' <summary>
        ''' Determines the distance from character center to adjacent character center, horizontally, in
        ''' one written text string. Model space coordinates.
        ''' </summary>
        Public AdvanceWidth As Double = 0.75

        ''' <summary>
        ''' Determines the width of the cut-out to do for each character when rendering. This is necessary
        ''' to avoid artefacts stemming from filtering (zooming/rotating). Make sure your font contains some
        ''' "white space" around each character so they won't be clipped due to this!
        ''' </summary>
        Public CharacterBoundingBoxWidth As Double = 0.8

        ''' <summary>
        ''' Determines the height of the cut-out to do for each character when rendering. This is necessary
        ''' to avoid artefacts stemming from filtering (zooming/rotating). Make sure your font contains some
        ''' "white space" around each character so they won't be clipped due to this!
        ''' </summary>
        Public CharacterBoundingBoxHeight As Double = 0.8
        '{ get { return 1.0 - borderY * 2; } set { borderY = (1.0 - value) / 2.0; } }
        ''' <summary>
        ''' Computes the expected width of text string given. The height is always 1.0.
        ''' Model space coordinates.
        ''' </summary>
        Public Function ComputeWidth(ByVal text As String) As Double
            Return text.Length * AdvanceWidth
        End Function

        ''' <summary>
        ''' This is a convenience function to write a text string using a simple coordinate system defined to be 0..100 in x and 0..100 in y.
        ''' For example, writing the text at 50,50 means it will be centered onscreen. The height is given in percent of the height of the viewport.
        ''' No GL state except the currently bound texture is modified. This method is not as flexible nor as fast
        ''' as the WriteString() method, but it is easier to use.
        ''' </summary>
        Public Sub WriteStringAt(ByVal text As String, ByVal heightPercent As Double, ByVal xPercent As Double, ByVal yPercent As Double, ByVal degreesCounterClockwise As Double)
            GL.MatrixMode(MatrixMode.Projection)
            GL.PushMatrix()
            GL.LoadIdentity()
            GL.Ortho(0, 100, 0, 100, -1, 1)
            GL.MatrixMode(MatrixMode.Modelview)
            GL.PushMatrix()
            GL.LoadIdentity()
            GL.Translate(xPercent, yPercent, 0)
            Dim aspectRatio As Double = ComputeAspectRatio()
            GL.Scale(aspectRatio * heightPercent, heightPercent, heightPercent)
            GL.Rotate(degreesCounterClockwise, 0, 0, 1)
            WriteString(text)
            GL.PopMatrix()
            GL.MatrixMode(MatrixMode.Projection)
            GL.PopMatrix()
            GL.MatrixMode(MatrixMode.Modelview)
        End Sub

        Private Shared Function ComputeAspectRatio() As Double
            Dim viewport As Integer() = New Integer(3) {}
            GL.GetInteger(GetPName.Viewport, viewport)
            Dim w As Integer = viewport(2)
            Dim h As Integer = viewport(3)
            Dim aspectRatio As Double = CSng(h) / CSng(w)
            Return aspectRatio
        End Function

        Private Sub WriteCharacter(ByVal ch As Char, ByVal xpos As Double)
            Dim ascii As Byte = CByte(AscW(ch))

            Dim row As Integer = ascii >> 4
            Dim col As Integer = ascii And &HF

            Dim centerx As Double = (col + 0.5) * Sixteenth
            Dim centery As Double = (row + 0.5) * Sixteenth
            Dim halfHeight As Double = CharacterBoundingBoxHeight * Sixteenth / 2.0
            Dim halfWidth As Double = CharacterBoundingBoxWidth * Sixteenth / 2.0
            Dim left As Double = centerx - halfWidth
            Dim right As Double = centerx + halfWidth
            Dim top As Double = centery - halfHeight
            Dim bottom As Double = centery + halfHeight

            GL.TexCoord2(left, top)
            GL.Vertex2(xpos, 1)
            GL.TexCoord2(right, top)
            GL.Vertex2(xpos + 1, 1)
            GL.TexCoord2(right, bottom)
            GL.Vertex2(xpos + 1, 0)
            GL.TexCoord2(left, bottom)
            GL.Vertex2(xpos, 0)
        End Sub

        Private textureId As Integer
        Private Const Sixteenth As Double = 1.0 / 16.0
    End Class

End Namespace