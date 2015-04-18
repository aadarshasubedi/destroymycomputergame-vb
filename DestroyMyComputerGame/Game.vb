'Destroy My Computer!
'Written by Dylan Taylor and Scott Ketelaar
'We are using the OpenTK library to provide low-level bindings for OpenGL and OpenAL. http://www.opentk.com/
'All graphics are rendered using the OpenGL API. http://www.opengl.org/
'This project uses OpenAL Soft, a software implementation of OpenAL, since OpenAL isn't installed at school.
'OpenAL Soft Site: http://kcat.strangesoft.net/openal.html
'The rasterized FPS counter numbers are based off of the font "Chinese Rocks Regular", by Typodermic [http://typodermicfonts.com/]
'Distortion effects are a work in progress still.
'With renderGrid set to true, the screen texture is divided into a grid for distortion effects. 
'Reference: http://www.soulstorm-creations.com/index.php?option=com_content&view=article&id=111%3Aopengl-making-a-2d-grid-image&catid=18%3Aprogramming-articles&Itemid=39
Option Strict On
Option Explicit On
Option Infer Off

#Region "Compilation Constants - used to optimize and set settings before compiling"
#Const Debug = True 'Enables certain debugging functionality that is unnecessary in a final release
#Const useRenderTest = False 'Uses a code-generated rendering test pattern instead of a screenshot. Written by Dylan Taylor.
#Const forcedPowerOfTwoTextureScaling = False 'No longer necessary, due to new checking code written by Dylan Taylor.
#Const drawDamage = True 'Whether or not to actually draw the dbility with older graphics cards that only support texture sizes in powers of two.
#Const ultraVerbose = False 'Displays HUGE amounts of additional debugging information on the console
#Const soundEnabled = True 'Enables or disables sound. If this is set to false, sound won't even be _SUPPORTED_.
#Const enableBackgroundMusic = True 'Whether or not to load and play background music. Depends on soundEnabled.
#Const enableAnisotropicFiltering = True 'Detects and enables anisotropic filtering to increase image quality if supported by the GPU
'Information on anisotropic filtering: http://en.wikipedia.org/wiki/Anisotropic_filtering; http://www.youtube.com/watch?v=YM3ieQHRYOc 
#Const useFrameBufferObjects = True 'HIGHLY recommended. Reduces the difference between windowed and full screen mode, and increases texture quality
'Note: Using ARB_framebuffer_object instead of GL_EXT_framebuffer_object would be a better choice. Unfortunately, that requires OpenGL 3.0+
'OpenTK frame buffer object documentation: http://www.opentk.com/doc/graphics/frame-buffer-objects
'Information on the GL_EXT_framebuffer_object extension: http://www.opengl.org/wiki/GL_EXT_framebuffer_object
'Information on ARB_framebuffer_object: http://www.opengl.org/registry/specs/ARB/framebuffer_object.txt
#Const hideConsoleWindow = False 'Attempts to hide the console window before taking a screenshot
'There is no reason to disable vertex arrays in favor of immediate mode rendering unless you're using OpenGL < 1.1.
#Const useVertexArrays = True 'Uses vertex arrays instead of immediate mode rendering for a significant performance increase.
#Const startFullscreen = False 'whether or not to start in fullscreen mode
#Const useFlamethrowerBurstPoints = True 'MUCH better effect, more accurate circles, but is also MUCH more CPU intensive, and slower
#Const renderErrorChecking = True 'Checks for errors when rendering, but causes a slight slowdown.
#Const oneFlameAnimationTexture = True 'Instead of using multiple flame textures, use a single texure, with different coordinates for each flame
#Const rasterizeDecals = True 'Modifies the screen texture on the graphics card for optimization
#Const rapidRasterization = True 'Rasterizes image every frame drawn. It is not recommended to turn this setting on for normal play.
#Const saveRasterizations = False 'Saves a copy of each rasterized image. Requires rasterization to be enabled (obviously).
#Const checkForOpenALDLL = True 'Checks if the OpenAL32 dll is installed on the system. If this fails, OpenAL Soft will be used.
#Const showSupportedOpenGLExtensions = False 'Displays a list of all supported OpenGL extensions. Lots of output, should probably leave disabled.
#Const showFramesPerSecond = True 'Displays the number of frames rendered each second on the screen. Coded by Dylan Taylor.
#Const drawWireframe = False 'Instead of drawing the textures normally, only a wireframe of each texture is drawn.
#Const mouseWheelWeaponChangeScrollLoop = False 'When scrolling through weapons with the mouse wheel, and going past the last weapon, go back to the beginning.
#Const copyAudioToRAM = False 'Instead of streaming the audio files from the disk, copy them into memory for quicker access.
#Const renderGrid = True 'Used for basic fluid ripple distortion physic effects, WORK IN PROGRESS. _HUGE_ FPS DROP.
#Const runBenchmark = True 'Automatically creates damage at random locations to test performance
#End Region

#Region "Imports Section - Imports Necessary Libraries"
Imports OpenTK.Graphics.OpenGL
Imports OpenTK.Input 'for mouse and keyboard input
#End Region

Public Class Game
    Inherits OpenTK.GameWindow

#Region "Constant and ReadOnly Preferences - Relatively safe to change"
    Const BaseTitle As String = "Destroy My Computer!  " 'The base title of the window.
    Const screenBitmapName As String = "screen.bmp" 'What to store the screenshot bitmap as
    ReadOnly defaultBrush As Drawing.Brush = Drawing.Brushes.Crimson 'Default text color for TextToBitmap
#If rasterizeDecals Then
    Const rasterizationThreshold As Integer = 2000 'Maximum textures to draw in real-time before rasterizing.
#End If
#If useRenderTest Then
    Const testPatternBitmapName As String = "expectedResult.bmp" 'What the screen SHOULD look like, pixel for pixel
    Const currentRenderingBitmapName As String = "currentRendering.bmp" 'Automatically generated screenshot of current rendering
#End If
    Const shotgunShots As Integer = 50 'How many shots the shotgun fires at once
    Const FlameUpdateDelay As Integer = 100
    Private Const machineGunFiringDelay As Integer = 77 'Actual machine gun firing rate
    Const deltaMultiplier As UShort = 1 'How much to multiply the flame delta by. Probably should be left at 1.
    Private Shared Fbitmap As New Drawing.Size(64, 128) ' Should be set to the size of the flame bitmap (Width, Height)
    Const bgmGain As Single = 1.0
    Const machineGunGain As Single = 0.65
    Const shotgunGain As Single = 0.35
    Const rifleGain As Single = 1.0
    Const fireGain As Single = 0.25
    Const cwOffsetX As Integer = -6
    Const cwOffsetY As Integer = -4
    Const flameThrowerMinRadius As Integer = 24
    Const flameThrowerMaxRadius As Integer = 64
    Const flameThrowerCirclePoints As Integer = 32
#If useFlamethrowerBurstPoints Then
    Const flameThrowerBurstPoints As Integer = 8
    Const flameThrowerBurstRadius As Integer = 32
#End If
#If runBenchmark Then
    Const minBenchmarkDelay As Integer = 75
    Const maxBenchmarkDelay As Integer = 850
#End If
#End Region

#Region "Windows DLL Library Code - Do not edit, uses compiler constants"
#If hideConsoleWindow Then
    <DllImport("kernel32.dll")> Friend Shared Function GetConsoleWindow() As Long 'Necessary to hide the Console Window
    End Function
    <DllImport("user32.dll")> Friend Shared Function ShowWindow(ByVal HWND As Integer, ByVal WindowState As Integer) As Integer
    End Function
    Const SW_HIDE = 0 : Const SW_RESTORE = 9
#End If
#If checkForOpenALDLL And soundEnabled Then
    Private Declare Function LoadLibrary Lib "kernel32" Alias "LoadLibraryA" (ByVal lpLibFileName As String) As Long
    Private Declare Function FreeLibrary Lib "kernel32" (ByVal hLibModule As Long) As Long
    Private openaldll As Long
#End If
#End Region

#Region "Experimental Grid Rendering Code Related Variables"
#If renderGrid Then
    Const gridSize As Integer = 63
    Dim Velocity(gridSize, gridSize) As Single
    Dim Position(gridSize, gridSize) As Single
    Const Viscosity As Single = 0.96
    Dim Vertex(gridSize, gridSize) As OpenTK.Vector3
    Dim Normals(gridSize, gridSize) As OpenTK.Vector3
#End If
#End Region

#Region "OpenGL Texture IDs"
    'The texture bitmaps used to be initialized as ReadOnly here, but because of the new texture scaling check,
    'they are now declared and initialized in the onLoad method after running the check. Because of this, we're using
    'sizes instead of bitmaps to keep track of how large the images are.
    Dim screenTexture As UInteger 'Texture identification for the screenshot bitmap
    Dim bulletTexture As UInteger
    Dim shotgunBulletTexture As UInteger
    Dim rifleBulletTexture As UInteger
    Dim numbersTexture As UInteger 'For FPS display
    Dim BurnTexture As UInteger
#If oneFlameAnimationTexture Then
    Dim FireFrames As UInteger
    Dim FlameCoordinates As New List(Of Single())
#Else
    Dim FireFrames As New List(Of UInteger)
#End If
    Protected Shared currentTex As UInteger
#End Region

#Region "Vertices and Coordinates"
    'We aren't using a single square, but rather two triangles, since squares are converted to triangles anyways.
    Dim screenVertices As Short() = New Short() { _
         0, 0, _
         0, 1, _
         1, 0, _
         1, 1} 'Automatically changed later.
    ReadOnly textureCoords As Short() = New Short() {0, 0, _
                                                    0, 1, _
                                                    1, 0, _
                                                    1, 1}
    Dim screenTextureCoords As Short() = textureCoords 'because the screen texture flips when we use GL.CopyTexImage2D
    Dim RasterizedNumberCoordinates As New List(Of Single()) 'For storing the location of the number images used in the FPS counter
    Shared ReadOnly powersOfTwo As Integer() = New Integer() {1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192}
#If renderGrid Then
    Dim Xrotation As Integer = 0
    Dim Yrotation As Integer = 0
    Dim Zrotation As Integer = 0
#End If
#End Region

#Region "Constant Weapon IDs"
    Const NONE As Short = 9001
    Const RIFLE As Short = 9002
    Const MACHINE_GUN As Short = 9003
    Const SHOTGUN As Short = 9004
    Const MACHINE_SHOTGUN As Short = 9005 'It doesn't need to make sense... All you need to know is that it's AWESOME!
    Const FLAMETHROWER As Short = 9006
    ReadOnly weapons As Short() = New Short() {RIFLE, MACHINE_GUN, MACHINE_SHOTGUN, SHOTGUN, FLAMETHROWER}
    Dim currentWeapon As Short = MACHINE_GUN 'This is the weapon that players will start out with.
#End Region

#Region "Size Tracking Variable Declarations"
    Dim rifleBulletHoleSize As Drawing.Size
    Dim adjustedBulletHoleSize As Drawing.Size
    Dim adjustedRifleBulletHoleSize As Drawing.Size
    Dim shotgunBulletHoleSize As Drawing.Size
    Dim adjustedShotgunBulletHoleSize As Drawing.Size
#End Region

#Region "Thread Declarations - Used for background tasks"
    Dim automaticWeaponFiringThread As System.Threading.Thread = New System.Threading.Thread(AddressOf automaticWeaponFire)
#If runBenchmark Then
    Dim benchmarkThread As System.Threading.Thread = New System.Threading.Thread(AddressOf runBenchmarkTest)
#End If
#End Region

#Region "Automatically Calculated Variables - Do NOT modify these. It will likely break the code."
    Dim nonPowerOfTwoTextureSizesSupported As Boolean = False 'Automatically detected. MUST be False.
    Dim clearScreen As Boolean = False 'whether or not to clear the screen of decals on the next render
    Dim clearBuffer As Boolean = False 'clears the buffer on the next render
    Dim screen As Drawing.Bitmap 'A bitmap image used to store a screenshot
    Private Shared scrW As Integer = ScreenSize.ScreenWidth 'screen width in pixels
    Private Shared scrH As Integer = ScreenSize.ScreenHeight 'screen height in pixels
    Dim busy As Boolean = False 'used to prevent race conditions when resizing, etc.
    Dim firstRender As Boolean = True 'used for debugging
    Dim wireframeView As Boolean = False
    Dim weaponNames As String()
    Dim weaponNameLabels As List(Of System.Drawing.Bitmap) = New List(Of System.Drawing.Bitmap)
    Dim weaponNameTextures As New List(Of UInteger)
    Dim weaponNameTrueSizes As New List(Of Drawing.Size)
    Dim weaponNameVertices As New List(Of Short())
    Dim weaponNameTextureCoordinates As New List(Of Single())
    Dim Damage As New List(Of Damage)
    Private CurrentFireFrame As Integer = 0
    Const DeltaD As Short = -1 * deltaMultiplier 'Delta for decreasing position
    Const DeltaI As Short = 1 * deltaMultiplier 'Delta for increasing position
    Dim framesElapsed As Integer = 9999 'Automatically changed later.
    Protected Shared framesPerSecond As String = "9999"
    Dim frameBufferObjectsSupported As Boolean = False
#End Region

#Region "Compliation Constant Conditional Code "
#If showFramesPerSecond Then
    Const FPSSpacing As Integer = 5
    Dim FPSVertices As New List(Of Short())
#End If
#If rasterizeDecals Then
    Dim rasterizationCount As Integer = 0 'How many times the screen has been rasterized
    Dim rasterizedTextures As Integer = 0 'Mostly because this is an interesting statistic
    Dim originalScreenTexture As UInteger
#End If
#If useRenderTest Then
    ReadOnly testPattern As Bitmap = Gfx.createTestPattern(ScreenSize.ScreenWidth, ScreenSize.ScreenHeight)
#End If
#If soundEnabled Then
    Protected Shared Sound As Sound 'Initialized later.
    Protected Shared soundFails As Boolean = False
#If copyAudioToRAM Then
    Dim rifleSounds As List(Of Byte()) = New List(Of Byte())
    Dim fireSoundBytes As Byte()
    Dim machineGunSoundBytes As Byte()
    Dim shotgunSoundBytes As Byte()
#Else
    Dim rifleSounds As List(Of String) = New List(Of String)
#End If
#End If
    Dim anisotropicFilteringSupported As Boolean = False
    Dim maximumAnisotropy As Single = 1.0
#If useFrameBufferObjects Then
    Dim screenFrameBufferImg As UInteger
    Dim screenRenderBuffer As UInteger
    Dim frameBufferObject As UInteger
    Dim frameBufferImageVertices As Short() = New Short() { _
            0, 1, _
            0, 0, _
            1, 1, _
            1, 0}
    Dim fboVertices As Short()
#End If
#End Region

#Region "Game Class Constructor - Run when a new game instance is created"
    Private Shared Function getWindowHeight() As Integer
        For powerOfTwo As Integer = powersOfTwo.Count - 1 To 0 Step -1
            If (powersOfTwo(powerOfTwo) <= scrH) Then
                Return powersOfTwo(powerOfTwo)
            End If
        Next powerOfTwo
        Return 0
    End Function

    Private Shared Function getWindowWidth() As Integer
        For powerOfTwo As Integer = powersOfTwo.Count - 1 To 0 Step -1
            If (powersOfTwo(powerOfTwo) <= scrW) Then
                Return powersOfTwo(powerOfTwo)
            End If
        Next powerOfTwo
        Return 0
    End Function

    Public Sub New()
        'Because certain cards require power of two viewports, like the ones at school, we need to default to one.
        MyBase.New(getWindowWidth, getWindowHeight) 'creates a new window with size of (width, height)  
#If hideConsoleWindow Then
        Dim Chwnd As Long = GetConsoleWindow()
        ShowWindow(Chwnd, SW_HIDE)
        Threading.Thread.Sleep(500) 'Give the window time to clear
#End If
#If Not forcedPowerOfTwoTextureScaling Then
        If (GL.GetString(StringName.Extensions)).Contains("GL_ARB_texture_non_power_of_two") Then nonPowerOfTwoTextureSizesSupported = True
#End If
#If Not useRenderTest Then
        screen = ScaleImage(New Drawing.Bitmap(Screenshot.GetDesktopImage(scrW, scrH, False))) 'takes a screenshot and stores it as an bitmap in memory
        'To fix a bug with the school's graphics card, we have to scale the texture to the nearest power of two size
        'Obviously, this causes a significant loss in quality
        Console.Title = BaseTitle & "Console Window"
#Else
        'NOTE: If our code is working right, you should see pixel-perfect individual alternating black and white squares
        'Because the image is generated to fit the screen size, there should be no excuse for scaling, etc.
        screen = Gfx.createTestPattern(scrW, scrH)
#End If
#If hideConsoleWindow Then
        ShowWindow(Chwnd, SW_RESTORE)
#End If
        AddHandler Keyboard.KeyDown, AddressOf Keyboard_KeyDown 'used to listen for key events
#If Not runBenchmark Then
        AddHandler Mouse.WheelChanged, AddressOf Mouse_WheelChanged
        AddHandler Keyboard.KeyUp, AddressOf Keyboard_KeyUp 'used to listen for key events
        AddHandler Mouse.ButtonDown, AddressOf Mouse_ButtonDown 'used to listen for mouse clicks
        AddHandler Mouse.ButtonUp, AddressOf Mouse_ButtonUp 'used to listen for mouse clicks
#End If
#If startFullscreen Then
        ToggleFullscreen() 'if set, start in fullscreen mode
#End If
#If drawWireframe Then
        wireframeView = True
#End If
    End Sub
#End Region

#Region """ToggleFullscreen"" Method - Switches between fullscreen and windowed mode"
    ' Author: Dylan Taylor, Scott Ketelaar
    Private Sub ToggleFullscreen()
        ' Ternary operator in Visual Basic... Syntax: If(expression,true_value,false_value)
        Me.WindowState = If(Me.WindowState = OpenTK.WindowState.Fullscreen, OpenTK.WindowState.Normal, OpenTK.WindowState.Fullscreen)
    End Sub
#End Region

#Region "Keyboard Event Handling Code - Run when a key is pressed"
    Private Sub Keyboard_KeyDown(ByVal sender As Object, ByVal e As KeyboardKeyEventArgs)
#If Not runBenchmark Then
        If (automaticWeaponFiringThread.IsAlive) Then Return
#End If
        Select Case (e.Key)
            Case Key.Escape
                Me.Close()
                End
            Case Key.F11
                If (Not busy) Then ToggleFullscreen()
#If Not runBenchmark Then
            Case Key.F1
                currentWeapon = RIFLE
            Case Key.F2
                currentWeapon = MACHINE_GUN
            Case Key.F3
                currentWeapon = SHOTGUN
                Return
            Case Key.F4
                currentWeapon = MACHINE_SHOTGUN
            Case Key.F5
                currentWeapon = FLAMETHROWER
#End If
            Case Key.Delete
#If ultraVerbose Then
                Console.WriteLine("Removing all damage, and resetting game")
#End If
                '#If rasterizeDecals Then
                '                Try
                'SyncLock Damage
                'Damage = New List(Of Damage)
                clearScreen = True
                'Dim originalScreen As Bitmap = screen
                'Dim data As System.Drawing.Imaging.BitmapData = originalScreen.LockBits(ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                'GL.BindTexture(TextureTarget.Texture2D, screenTexture)
                'GL.TexSubImage2D(TextureTarget.Texture2D, 0, screen.Width, screen.Height, screen.Width, screen.Height, PixelFormat.Rgb, PixelType.Bitmap, originalScreenTexture)
                'End SyncLock
                'Catch ex As Exception
                ' Console.WriteLine("Exception occured while resetting game: " & ex.Message)
                'End Try
                '#End If
            Case Key.End
                'Rapidly switches between fullscreen and windowed mode, tries to force a glitch to occur
                'For debugging purposes. If our code works right, the game should continue to run as expected.
                ToggleFullscreen() : ToggleFullscreen()
            Case Key.W
                wireframeView = Not wireframeView
                GL.PolygonMode(MaterialFace.FrontAndBack, If(wireframeView, PolygonMode.Line, PolygonMode.Fill))
#If Not runBenchmark Then
            Case Key.Space 'So we can fire with the keyboard :)
                FireWeapon(New Drawing.Point(Mouse.X, Mouse.Y))
#End If
        End Select
    End Sub

    Private Sub Keyboard_KeyUp(ByVal Sender As Object, ByVal e As KeyboardKeyEventArgs)
        If (e.Key = Key.Space) Then
            If (automaticWeaponFiringThread.IsAlive) Then automaticWeaponFiringThread.Abort()
        End If
    End Sub

#End Region

#Region "Mouse Event Handling Code - Run when a mouse button is clicked"
    Private Sub Mouse_ButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        FireWeapon(e.Position)
    End Sub

    Private Sub FireWeapon(ByVal e As Drawing.Point)
        Select Case (currentWeapon)
            Case NONE : Return
            Case RIFLE
#If soundEnabled Then
                PlaySoundFile(rifleSounds(random(0, rifleSounds.Count - 1)), rifleGain, False, e.X, e.Y)
#End If
                Dim cShot As Damage
                Dim adjLoc As Drawing.Point = New Drawing.Point(CInt((e.X / Me.Width) * scrW), CInt((e.Y / Me.Height) * scrH))
                'If (Me.Size <> ScreenSize.Size) Then
                'Console.WriteLine("Adjusted Location: (" & adjLoc.X & ", " & adjLoc.Y & ")")
                'Console.WriteLine("Adjusted Size: (" & adjustedRifleBulletHoleSize.Width.ToString & ", " & _
                '                 adjustedRifleBulletHoleSize.Height.ToString & ")")
                'cShot = New Damage(rifleBulletTexture, adjLoc, adjustedRifleBulletHoleSize)
                'Else
                cShot = New Damage(rifleBulletTexture, e, rifleBulletHoleSize)
#If renderGrid Then
                CreateImpact(e.X, e.Y)
#End If
                'End If
                SyncLock Damage
                    Damage.Add(cShot)
                End SyncLock
                Return
            Case MACHINE_GUN
                If (Not automaticWeaponFiringThread.IsAlive()) Then
                    automaticWeaponFiringThread = New Threading.Thread(AddressOf automaticWeaponFire)
                    automaticWeaponFiringThread.Start()
                End If
                Return
            Case MACHINE_SHOTGUN
                If (Not automaticWeaponFiringThread.IsAlive()) Then
                    automaticWeaponFiringThread = New Threading.Thread(AddressOf automaticWeaponFire)
                    automaticWeaponFiringThread.Start()
                End If
                Return
            Case SHOTGUN
                fireShotgun(e)
                Return
            Case FLAMETHROWER
                If (Not automaticWeaponFiringThread.IsAlive()) Then
                    automaticWeaponFiringThread = New Threading.Thread(AddressOf automaticWeaponFire)
                    automaticWeaponFiringThread.Start()
                End If
        End Select
    End Sub

    'Author: Dylan Taylor
    Private Sub Mouse_WheelChanged(ByVal sender As Object, ByVal e As MouseWheelEventArgs)
        If (automaticWeaponFiringThread.IsAlive) Then Return
#If ultraVerbose Then
        Console.WriteLine("Mouse Wheel Change Detected")
#End If
        Dim delta As Short = CShort(e.Delta)
        If ((currentWeapon + e.Delta) >= weapons(0)) AndAlso ((currentWeapon + e.Delta) <= weapons(weapons.Length - 1)) Then
            currentWeapon = CShort(currentWeapon + e.Delta)
#If mouseWheelWeaponChangeScrollLoop Then
        Else currentWeapon = If(e.Delta > 0, weapons(weapons.Length - 1), weapons(0))
#End If
        End If
    End Sub
#End Region

#Region """FlameOn"" Method - Run when creating flames using the flamethrower"
    Private Sub FlameOn(ByVal e As Drawing.Point)
        'Try
        'NOTE: both points can only have a delta of zero if the new point is the same as the
        'center of the circle. because we have a radius, this is IMPOSSIBLE, so checking
        'if both of the points are equal to zero is not necessary --Dylan
        Dim clickPt As System.Drawing.Point = New System.Drawing.Point(e.X, e.Y)
        Dim circlePoints As List(Of System.Drawing.Point) = MathOps.GetPointsOnCircle(clickPt, random(flameThrowerMinRadius, flameThrowerMaxRadius), flameThrowerCirclePoints)
        'Dim ptDeltaX As Integer
        'Dim ptDeltaY As Integer
        Dim cFlame As Flame

#If useFlamethrowerBurstPoints Then
        Dim burstPoints As List(Of System.Drawing.Point) 'because this looks SO much cooler, and more circular
#End If
        For Each cPt As System.Drawing.Point In circlePoints
#If useFlamethrowerBurstPoints Then
            burstPoints = MathOps.GetPointsOnCircle(cPt, flameThrowerBurstRadius, flameThrowerBurstPoints)
            For Each bPt As System.Drawing.Point In burstPoints
                'ptDeltaX = if((bPoint.X = cPoint.X), 0, if((bPoint.X < cPoint.X), -1, 1))
                'ptDeltaY = if((bPoint.Y = cPoint.Y), 0, if((bPoint.Y < cPoint.Y), -1, 1))
                cFlame = New Flame(bPt, Fbitmap, _
                                            If((bPt.X = cPt.X), CShort(0), If((bPt.X < cPt.X), DeltaD, DeltaI)), _
                                            If((bPt.Y = cPt.Y), CShort(0), If((bPt.Y < cPt.Y), DeltaD, DeltaI)))

                SyncLock Damage
                    'Damage.Add(New Flame(-1, bPoint, flameWidth, flameHeight, ptDeltaX, ptDeltaY))
                    Damage.Add(cFlame)
                End SyncLock
            Next bPt
#Else
                'ptDeltaX = if((cPoint.X = clickPt.X), 0, if((cPoint.X < clickPt.X), -1, 1))
                'ptDeltaY = if((cPoint.Y = clickPt.Y), 0, if((cPoint.Y < clickPt.Y), -1, 1))
                cFlame = New Flame(cPoint, Fbitmap, _
                                   If((cPoint.X = clickPt.X), 0, If((cPoint.X < clickPt.X), DeltaD, DeltaI)), _
                                   If((cPoint.Y = clickPt.Y), 0, If((cPoint.Y < clickPt.Y), DeltaD, DeltaI)))
                SyncLock Damage
                    'Damage.Add(New Flame(-1, cPoint, flameWidth, flameHeight, ptDeltaX, ptDeltaY))
                    Damage.Add(cFlame)
                End SyncLock
#End If
        Next cPt
#If soundEnabled Then
#If copyAudioToRAM Then
        PlaySoundFile(fireSoundBytes, fireGain, False)
#Else
        PlaySoundFile("Resources\fire.wav", fireGain, False)
#End If
#End If
        'Catch ex As Exception
        '    MsgBox(ex.Message)
        'End Try
    End Sub
#End Region

#If runBenchmark Then
    Private Sub runBenchmarkTest()
        While True
            currentWeapon = weapons(random(0, weapons.Length - 1))
            If ((Me.Width <> 0) AndAlso (Me.Height <> 0)) Then
                FireWeapon(New Drawing.Point(random(0, Me.Width), random(0, Me.Height)))
                If (automaticWeaponFiringThread.IsAlive) Then
                    System.Threading.Thread.Sleep(random(minBenchmarkDelay, maxBenchmarkDelay))
                    automaticWeaponFiringThread.Abort()
                End If
            End If
            If (random(0, 10000) = 5000) Then
                clearScreen = True 'randomly clears the screen
            End If
        End While
    End Sub
#End If


#Region """automaticWeaponFire"" Method - Used for automatically firing weapons"
    Private Sub automaticWeaponFire()
        Do
#If Not runBenchmark Then
            Dim e As System.Drawing.Point = PointToClient(Windows.Forms.Cursor.Position)
#Else
            Dim e As System.Drawing.Point = New Drawing.Point(random(0, Me.Width), random(0, Me.Height))
#End If
            'Play("MG.wav", False, AudioMode.FromMemory)
            Select Case (currentWeapon)
                Case FLAMETHROWER
                    FlameOn(e)
                Case MACHINE_SHOTGUN
                    fireShotgun(e)
                Case MACHINE_GUN
#If soundEnabled Then
#If copyAudioToRAM Then
                    PlaySoundFile(machineGunSoundBytes, machineGunGain, False, e.X, e.Y)
#Else

                    PlaySoundFile("Resources\machinegun.wav", machineGunGain, False, e.X, e.Y)
#End If

#End If
                    SyncLock Damage
                        Damage.Add(New Damage(bulletTexture, e, shotgunBulletHoleSize))
#If renderGrid Then
                        CreateImpact(e.X, e.Y)
#End If
                    End SyncLock
                Case Else
                    Console.WriteLine("Attempted to use automatic weapon fire method on an invalid weapon.") : Return
            End Select
            Threading.Thread.Sleep(machineGunFiringDelay)
        Loop
    End Sub
#End Region

#Region "Shotgun Firing Code - Run once every time the shotgun is fired"
    Private Sub fireShotgun(ByVal click As Drawing.Point)
#If soundEnabled Then
#If copyAudioToRAM Then
        PlaySoundFile(shotgunSoundBytes, shotgunGain, False, click.X, click.Y)
#Else
        PlaySoundFile("Resources\shotgun.wav", shotgunGain, False, click.X, click.Y)
#End If
#End If
        Dim shotRangeX As Integer = CInt(Me.Width / 4) '(Me.Width / 5)
        Dim shotRangeY As Integer = CInt(Me.Height / 4) '(Me.Height / 5)
        With click
            Dim left As Integer = CInt(.X - (shotRangeX / 2))
            left = If((left < 0), 0, left)
            Dim right As Integer = CInt(.X + (shotRangeX / 2))
            right = If((right > Me.Width), Me.Width, right)
            Dim top As Integer = CInt(.Y - (shotRangeY / 2))
            top = If((top < 0), 0, top)
            Dim bottom As Integer = CInt(.Y + (shotRangeY / 2))
            bottom = If((bottom > Me.Height), Me.Height, bottom)
            Dim shotP As Drawing.Point
            Dim cShot As Damage
            For shot As Integer = 0 To shotgunShots
                shotP = New Drawing.Point(random(left, right), random(top, bottom))
#If (ultraVerbose) Then
                Console.WriteLine("Adding shotgun bullet at: (" & shotX & ", " & shotY & ")")
#End If
                cShot = New Damage(shotgunBulletTexture, shotP, shotgunBulletHoleSize)
                SyncLock Damage
                    Damage.Add(cShot)
                End SyncLock
#If renderGrid Then
                CreateImpact(click.X, click.Y)
#End If
            Next shot
        End With
    End Sub
#End Region

#Region """AdvanceFireFrames"" Method - Used to advance the frame of the fire animation in a background thread"
    Private Sub AdvanceFireFrames()
        Do
#If oneFlameAnimationTexture Then
            CurrentFireFrame = If(CurrentFireFrame = 14, 0, CurrentFireFrame + 1)
#Else
            CurrentFireFrame = If((CurrentFireFrame = FireFrames.Count - 1), 0, CurrentFireFrame + 1)
#End If
            System.Threading.Thread.Sleep(FlameUpdateDelay)
        Loop
    End Sub
#End Region

#Region """FPSCounter Method"" - used to keep track of how many frames are rendering every second"
    Private Sub FPSCounter()
        Do
            framesPerSecond = framesElapsed.ToString
            framesElapsed = 0
            System.Threading.Thread.Sleep(1000)
        Loop
    End Sub
#End Region

#Region """random"" Method - used for generating random integers in the specified range"
    Private Function random(ByVal min As Integer, ByVal max As Integer) As Integer
        Randomize() : Return CInt((max - min) * Rnd()) + min
    End Function
#End Region

#Region """Mouse_ButtonUp"" Method - Run when mouse button is released"
    Private Sub Mouse_ButtonUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        If (automaticWeaponFiringThread.IsAlive) Then automaticWeaponFiringThread.Abort()
    End Sub
#End Region

#Region """OnLoad"" Method - Sets up OpenGL, and prepares the game by loading and generating resources"
    ' Setup OpenGL and load resources here.
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        VSync = OpenTK.VSyncMode.Off 'Disables VSync (Vertical Synchronization) in order to get a much higher frame rate
        Console.WriteLine("Destroy My Computer!")
        Console.WriteLine("Written by Dylan Taylor, Scott Ketelaar, and Manny Castillo")
        Console.WriteLine("OpenGL vendor: " & GL.GetString(StringName.Vendor))
        'in C++, we could just use 'atof' to convert the version to a Single. In VB, it's a bit more complicated.
        'Therefore, I rewrote 'atof' in Visual Basic. atof is not a standard function provided by the .NET APIs.
        Dim glVersion As Double = atof(GL.GetString(StringName.Version))
        Console.WriteLine("OpenGL version: " & glVersion & " (" & GL.GetString(StringName.Version) & ")")
        Dim glExtensions As String = GL.GetString(StringName.Extensions)
#If showSupportedOpenGLExtensions Then
        Console.WriteLine("Supported OpenGL Extensions: " & glExtensions.Replace(" ", System.Environment.NewLine))
#End If
#If Not forcedPowerOfTwoTextureScaling Then
        nonPowerOfTwoTextureSizesSupported = (glExtensions).Contains("GL_ARB_texture_non_power_of_two")
        Console.WriteLine("Non-Power of Two Texture Sizes: " & If(nonPowerOfTwoTextureSizesSupported, "", "NOT ") & "Supported")
#Else
        Console.WriteLine("NOTE: Non-Power of Two Texture Sizes are DISABLED.")
#End If

        anisotropicFilteringSupported = glExtensions.Contains("texture_filter_anisotropic")
        Console.Write("Anisotropic Filtering: " & If(anisotropicFilteringSupported, "", "NOT ") & "Supported")
        If anisotropicFilteringSupported Then
            GL.GetFloat(CType(ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, GetPName), maximumAnisotropy)
            Console.WriteLine(" (" & CInt(maximumAnisotropy) & "X)")
#If enableAnisotropicFiltering Then
            GL.TexParameter(TextureTarget.Texture2D, CType(ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, TextureParameterName), maximumAnisotropy)
#End If
        Else
            Console.WriteLine()
        End If
        frameBufferObjectsSupported = glExtensions.Contains("GL_EXT_framebuffer")
        Console.WriteLine("Frame Buffer Objects (GL_EXT_framebuffer): " & If(anisotropicFilteringSupported, "", "NOT ") & "Supported")

        Console.WriteLine("OpenGL shading language version: " & GL.GetString(StringName.ShadingLanguageVersion))
        Console.WriteLine("Setting up OpenGL Texturing...")
        TexLib.TexUtil.InitTexturing() 'Necessary to use the texture utility library
        Console.WriteLine("Creating textures from bitmaps...")
        screenTexture = CreateFilterlessTextureFromBitmap(screen, False) 'loads texture from bitmap
#If rasterizeDecals Then
        originalScreenTexture = CreateFilterlessTextureFromBitmap(screen, False)
#End If
        Dim shotgunBulletHole As Drawing.Bitmap = ScaleImage(LoadBitmap("Resources\sgBulletHole.png"))
        Dim rifleBulletHole As Drawing.Bitmap = ScaleImage(LoadBitmap("Resources\rifleBulletHole.png"))
        Dim bulletHole As Drawing.Bitmap = ScaleImage(LoadBitmap("Resources\bulletHole.png"))
        Dim rasterizedNums As Drawing.Bitmap = ScaleImage(LoadBitmap("Resources\rasterizedNumbersCombinedScaled.png"))
        bulletTexture = CreateFilterlessTextureFromBitmap(bulletHole, True)
        rifleBulletTexture = CreateFilterlessTextureFromBitmap(rifleBulletHole, True)
        shotgunBulletTexture = CreateFilterlessTextureFromBitmap(shotgunBulletHole, True)
        numbersTexture = CreateFilterlessTextureFromBitmap(rasterizedNums, True)
        rifleBulletHoleSize = rifleBulletHole.Size
        shotgunBulletHoleSize = shotgunBulletHole.Size
        'bulletHoleSize = bulletHole.Size
        BurnTexture = CreateFilterlessTextureFromBitmap(LoadBitmap("Resources\burn.png"), True)
        Console.WriteLine("Generating current weapon name bitmaps...")
        Dim cwString As String = "Current Weapon: "
        Dim cwFont As Drawing.Font = New Drawing.Font(Drawing.FontFamily.GenericSansSerif, 17, Drawing.FontStyle.Bold)
        Dim currentWeaponName As String
        weaponNames = New String() {"RIFLE", "MACHINE GUN", "SHOTGUN", "MACHINE SHOTGUN", "FLAMETHROWER"}
        For Each weapon As String In weaponNames
            currentWeaponName = cwString & weapon
            'weaponNameLabels.Add(TextToBitmap(currentWeaponName, cwFont, _
            '    MathOps.NearestPowerOfTwo(GetTextWidthInPixels(currentWeaponName, cwFont)), _
            '    GetTextHeightInPixels(currentWeaponName, cwFont)))
            weaponNameLabels.Add(TextToBitmap(currentWeaponName, cwFont, 0, 0))
            'Now we need to generate the texture coordinates so that only the part of the texture with text on it is used.
            'This removes the padding which is added for compatibility with cards that only support NPOT texture sizes.
            weaponNameTrueSizes.Add(New Drawing.Size(GetTextWidthInPixels(currentWeaponName, cwFont) + 1, GetTextHeightInPixels(currentWeaponName, cwFont) + 1))
            weaponNameTextureCoordinates.Add(New Single() { _
                0, 0, _
                0, CSng((GetTextHeightInPixels(currentWeaponName, cwFont) / MathOps.NearestPowerOfTwo(weaponNameLabels.Last.Height))), _
                CSng((GetTextWidthInPixels(currentWeaponName, cwFont) / MathOps.NearestPowerOfTwo(weaponNameLabels.Last.Width))), 0, _
                CSng((GetTextWidthInPixels(currentWeaponName, cwFont) / MathOps.NearestPowerOfTwo(weaponNameLabels.Last.Width))), _
                CSng((GetTextHeightInPixels(currentWeaponName, cwFont) / MathOps.NearestPowerOfTwo(weaponNameLabels.Last.Height)))})
            weaponNameVertices.Add(New Short() { _
              CShort(Me.Width - weaponNameTrueSizes.Last.Width + cwOffsetX), CShort(Me.Height - weaponNameTrueSizes.Last.Height + cwOffsetY), _
              CShort(Me.Width - weaponNameTrueSizes.Last.Width + cwOffsetX), CShort(Me.Height + cwOffsetY), _
              CShort(Me.Width + cwOffsetX), CShort(Me.Height - weaponNameTrueSizes.Last.Height + cwOffsetY), _
              CShort(Me.Width + cwOffsetX), CShort(Me.Height + cwOffsetY)})
            weaponNameLabels.Last.Save(weapon & ".png")
            weaponNameTextures.Add(CreateFilterlessTextureFromBitmap(weaponNameLabels(weaponNameLabels.Count - 1), True))
        Next weapon
#If oneFlameAnimationTexture Then
        Console.WriteLine("Creating flame animation texture...")
        Dim FlameBitmap As Drawing.Bitmap = ScaleImage(LoadBitmap("Resources\Flames\single_texture\flameanimation.png"))
        FireFrames = CreateFilterlessTextureFromBitmap(FlameBitmap, True)
        Console.WriteLine("Mapping flame animation texture coordinates...")
        For flame As Integer = 0 To 14
            'upper left, bottom left, upper right, bottom right. (x, y) for each point.
            'size of one flame: 64, 128, coords are from 0.0 to 1.0
            With FlameBitmap
                FlameCoordinates.Add(New Single() {CSng(((flame * (.Width / 15)) / .Width)), 0, _
                                        CSng(((flame * (.Width / 15)) / .Width)), 1, _
                                        CSng((((flame + 1) * (.Width / 15)) / .Width)), 0, _
                                        CSng((((flame + 1) * (.Width / 15)) / .Width)), 1})
            End With
        Next flame

#Else
        Console.WriteLine("Creating textures from flame images...")
        For Each F As String In IO.Directory.GetFiles("Resources\Flames")
            FireFrames.Add(CreateFilterlessTextureFromBitmap(ScaleImage(LoadBitmap(F)), True))
        Next F
#End If
        Console.WriteLine("Mapping rasterized number coordinates...")
        Dim rev As Integer
        For n As Integer = 0 To 9
            With rasterizedNums
                rev = 9 - n 'used to get the numbers in ascending order, since they go from 9 to 0 in the image
                RasterizedNumberCoordinates.Add(New Single() {CSng(((rev * (.Width / 10)) / .Width)), 0, _
                                                    CSng(((rev * (.Width / 10)) / .Width)), 1, _
                                                    CSng((((rev + 1) * (.Width / 10)) / .Width)), 0, _
                                                    CSng((((rev + 1) * (.Width / 10)) / .Width)), 1})
            End With
        Next n
#If showFramesPerSecond Then
        Console.WriteLine("Pre-calculating FPS counter vertices...")
        Dim vleft As Short
        Dim vright As Short
        Dim vtop As Short
        Dim vbottom As Short
        For vert As Integer = 1 To 7 'Pre-calculates vertices for numbers up to 7 digits, just in case 
            vleft = CShort((2 * FPSSpacing) + ((FPSSpacing + (rasterizedNums.Width / 10)) * (vert - 1)))
            vright = CShort((vleft + (rasterizedNums.Width / 10)))
            vtop = CShort(1.5 * FPSSpacing)

            vbottom = CShort((1.5 * FPSSpacing) + rasterizedNums.Height)
            'Console.WriteLine("Creating new FPS vertex:")
            'Console.Write("Left: " & vleft)
            'Console.Write(", Right: " & vright)
            'Console.Write(", Top: " & vtop)
            'Console.WriteLine(", Bottom: " & vbottom)
            FPSVertices.Add(New Short() { _
                vleft, vtop, _
                vleft, vbottom, _
                vright, vtop, _
                vright, vbottom})
        Next vert
#End If
#If soundEnabled Then
        Console.WriteLine("Attempting to initialize sound system...")
        Try
            Sound = New Sound
        Catch ex As Exception
            Console.WriteLine("Loading sound failed: " & ex.Message)
            soundFails = True
        End Try
        Console.WriteLine("Indexing and loading rifle sound files...")
        For Each rifleSoundFile As String In IO.Directory.GetFiles("Resources\RifleSounds")
#If copyAudioToRAM Then
            rifleSounds.Add(LoadFile(rifleSoundFile))
            fireSoundBytes = LoadFile("Resources\fire.wav")
            machineGunSoundBytes = LoadFile("Resources\machinegun.wav")
            shotgunSoundBytes = LoadFile("Resources\machinegun.wav")
#Else
            rifleSounds.Add(rifleSoundFile)
#End If
        Next rifleSoundFile

#End If
        Console.WriteLine("Starting flame animation thread...")
        Dim T As Threading.Thread = New Threading.Thread(AddressOf AdvanceFireFrames)
        T.Priority = Threading.ThreadPriority.BelowNormal : T.Start()
        Console.WriteLine("Initializing FPS counter...")
        Dim FPS As Threading.Thread = New Threading.Thread(AddressOf FPSCounter)
        FPS.Priority = Threading.ThreadPriority.BelowNormal : FPS.Start()
        Console.WriteLine("Configuring OpenGL states and settings...")
        GL.Enable(EnableCap.Blend) 'Enable blending
#If Not renderGrid Then
        GL.Disable(EnableCap.DepthTest) ' Disables Pixel Checking
        GL.ShadeModel(ShadingModel.Flat) 'should have better performance than smooth.
#Else
        'GL.ClearDepth(1.0)
        'GL.Enable(EnableCap.DepthTest)
        GL.ShadeModel(ShadingModel.Smooth)
        'GL.BlendFunc(BlendingFactorSrc.ConstantColor, BlendingFactorDest.One)
        GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest)
        GL.Enable(EnableCap.Texture2D)
        'GL.Enable(EnableCap.Blend)
        'GL.TexGen(TextureCoordName.S, TextureGenParameter.TextureGenMode, TextureGenMode.SphereMap)
        'GL.TexGen(TextureCoordName.T, TextureGenParameter.TextureGenMode, TextureGenMode.SphereMap)
        'GL.Enable(EnableCap.TextureGenS)
        'GL.Enable(EnableCap.TextureGenT)
#End If
#If soundEnabled And enableBackgroundMusic Then
        Console.WriteLine("Loading background music...")
        'Dim bgFile As Byte() = 
        'ReDim bgMusic(bgFile.Length)
        'bgMusic = LoadFile("Resources\backgroundMusic.wav")
        Console.WriteLine("Playing background music using OpenAL...")
        'PlaySoundFile(bgMusic, 0.85, True)
#If copyAudioToRAM Then
        PlaySoundFile(LoadFile("Resources\backgroundMusic.wav"), bgmGain, True)
#Else
        PlaySoundFile("Resources\backgroundMusic.wav", bgmGain, True)
#End If
#End If
        Array.Sort(weapons) 'sort the list of weapons
#If useVertexArrays Then
        GL.EnableClientState(ArrayCap.VertexArray) 'Turns vertex arrays on
        GL.EnableClientState(ArrayCap.TextureCoordArray) 'Turns texture coordinate arrays on
#End If
#If rasterizeDecals Then
        GL.PixelStore(PixelStoreParameter.PackSwapBytes, 0)
        GL.PixelStore(PixelStoreParameter.PackAlignment, 4)
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1)
        GL.PixelStore(PixelStoreParameter.PackSkipRows, 0)
        GL.PixelStore(PixelStoreParameter.PackSkipPixels, 0)
        GL.PixelStore(PixelStoreParameter.PackRowLength, 0)
        GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0)
        GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0)
#End If
        GL.ClearColor(0, 0, 0, 0)
#If drawWireframe Then
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line)
#Else
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill)
#End If
#If useFrameBufferObjects Then
        Console.WriteLine("Creating Screen Buffer Object...")
        If frameBufferObjectsSupported Then
            GL.Ext.GenFramebuffers(1, frameBufferObject)
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, frameBufferObject)
            'Create and bind the depth buffer
            'GL.Ext.GenRenderbuffers(1, screenRenderBuffer)
            'GL.Ext.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, screenRenderBuffer)
            'GL.Ext.RenderbufferStorage(RenderbufferTarget.RenderbufferExt, RenderbufferStorage.DepthComponent24, scrW, scrH)
            'GL.Ext.FramebufferRenderbuffer(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, RenderbufferTarget.RenderbufferExt, screenRenderBuffer)
            'Now to create and bind the texture to render to
            GL.GenTextures(1, screenFrameBufferImg)
            GL.BindTexture(TextureTarget.Texture2D, screenFrameBufferImg)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, CInt(TextureMinFilter.Nearest))
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, CInt(TextureMagFilter.Nearest))
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, scrW, scrH, 0, PixelFormat.Rgba, PixelType.UnsignedByte, Nothing)
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, screenFrameBufferImg, 0)
            Dim FBstatus As Integer = GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt)
            If (FBstatus <> FramebufferErrorCode.FramebufferComplete) Then
                MsgBox("ERROR: Frame buffer status is " & FBstatus)
                End
            End If
        End If
#End If
#If runBenchmark Then
        Console.WriteLine("Benchmarking mode is enabled. Starting benchmark thread.")
        benchmarkThread.Start()
#End If
        Console.WriteLine("Loading game complete.")
    End Sub
#End Region

#Region """OnClosed""  Method - Run when the application is closed."
    Protected Overrides Sub OnClosed(ByVal e As System.EventArgs)
        MyBase.OnClosed(e) : End
    End Sub
#End Region

#Region """atof"" Method - This is a port of the function 'atof' in C++, written by Dylan Taylor. It is used to parse some strings."
    'This function does the same thing as the built-in 'atof' function in C++, or at least close enough that it doesn't matter
    'Author: Dylan Taylor
    Protected Function atof(ByVal s As String) As Double
        Dim ca As Char() = s.Trim().ToCharArray()
        If (Not Char.IsDigit(ca(0))) Then Return 0 'the string can't be parsed as a double
        Dim dec As Boolean = False
        For ch As Integer = 0 To ca.Length
            If (Char.IsDigit(ca(ch))) Then
                If (ch < s.Length) Then : Continue For
                Else : Return CDbl(s)
                End If
            ElseIf ca(ch) = "." Then
                If (dec) Then : Return Convert.ToDouble(s.Substring(0, (ch)))
                Else : dec = True : Continue For
                End If
            Else : Return CDbl(s.Substring(0, (ch)))
            End If
        Next
    End Function
#End Region

#Region """ScaleImage"" Method - Scales a bitmap image to the nearest power of two"
    'Authors: Dylan Taylor and Scott Ketelaar
    Public Function ScaleImage(ByVal original As Drawing.Bitmap) As Drawing.Bitmap
#If Not forcedPowerOfTwoTextureScaling Then
        'Unless we are forcing the the textures to be powers of two, there is no need to scale them if the GPU doesn't require it
        If (nonPowerOfTwoTextureSizesSupported) Then Return original
#End If
        'Console.WriteLine("Scaling bitmap...")
        If (original.Width = MathOps.NearestPowerOfTwo(original.Width) _
            AndAlso original.Height = MathOps.NearestPowerOfTwo(original.Height)) Then
            Return original 'No scaling necessary, decreases loading time
        End If
        Dim scaledImage As New Drawing.Bitmap(MathOps.NearestPowerOfTwo(original.Width), _
                           MathOps.NearestPowerOfTwo(original.Height))
        Dim g1 As Drawing.Graphics = Drawing.Graphics.FromImage(scaledImage)
        g1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
        'g1.Clear(Color.FromArgb(0, 0, 0, 0))
        g1.DrawImage(original, 0, 0, scaledImage.Width, scaledImage.Height)
        'g1.DrawImage(original, 0, 0, original.Width, original.Height)
        Return scaledImage
    End Function
#End Region

#Region """OnResize"" Method - Responds to resize events"
    Protected Overrides Sub OnResize(ByVal e As EventArgs)
        Console.WriteLine("Window Resized to: (" & Me.Width & ", " & Me.Height & ")")
        adjustedRifleBulletHoleSize = New Drawing.Size(CInt((rifleBulletHoleSize.Width / Me.Width) * scrW), CInt((rifleBulletHoleSize.Height / Me.Height) * scrH))
        adjustedShotgunBulletHoleSize = New Drawing.Size(CInt(shotgunBulletHoleSize.Width * (Me.Width / scrW)), CInt(shotgunBulletHoleSize.Height * (Me.Height / scrH)))
        If (busy) Then Return : busy = True
        'Coordinates are in the form of (x,y). This only allows for 2D drawing, but that's fine in this case.
        SyncLock screenVertices
            screenVertices = New Short() { _
              0, 0, _
              0, CShort(Me.Height), _
              CShort(Me.Width), 0, _
              CShort(Me.Width), CShort(Me.Height)}
#If useFrameBufferObjects Then
            fboVertices = New Short() {screenVertices(2), screenVertices(3), _
                           screenVertices(0), screenVertices(1), _
                           screenVertices(6), screenVertices(7), _
                           screenVertices(4), screenVertices(5)}
            DrawEntire2DObject(fboVertices, screenFrameBufferImg)
#End If
        End SyncLock
        SyncLock weaponNameVertices
            Console.WriteLine("Recalculating weapon name vertices for new size...")
            Dim wnl As Short
            Dim wnr As Short
            Dim wnt As Short
            Dim wnb As Short
            For weapon As Integer = 0 To weaponNameLabels.Count - 1
                Console.WriteLine("Weapon: " & weaponNames(weapon))
                Console.WriteLine("True Size: (" & weaponNameTrueSizes(weapon).Width & "," & weaponNameTrueSizes(weapon).Height & ")")
                wnl = CShort(Me.Width - weaponNameTrueSizes(weapon).Width + cwOffsetX)
                wnt = CShort(Me.Height - weaponNameTrueSizes(weapon).Height + cwOffsetY)
                wnb = CShort(Me.Height + cwOffsetY)
                wnr = CShort(Me.Width + cwOffsetX)
                Console.WriteLine("Left: " & wnl & " Right: " & wnr & " Top: " & wnt & " Bottom: " & wnb)
                weaponNameVertices(weapon) = New Short() { _
                  wnl, wnt, _
                  wnl, wnb, _
                  wnr, wnt, _
                  wnr, wnb}
            Next weapon
        End SyncLock
        GL.Viewport(0, 0, Me.Width, Me.Height)
        GL.MatrixMode(MatrixMode.Projection)
#If Debug And useRenderTest Then
            'NOTE: This code runs VERY slow... I'd advise against resizing the window when using the test pattern
            'Because of older graphics cards, we NEED to use texture sizes that are powers of two.
            screen = Gfx.createTestPattern(MathOps.NearestPowerOfTwo(1024), MathOps.NearestPowerOfTwo(768))
            screenTexture = CreateFilterlessTextureFromBitmap(screen, False) 'loads texture from bitmap
#End If
        Console.WriteLine("Resizing window complete")
    End Sub
#End Region

#Region """OnUpdateFrame"" method - used for game logic, physics"
#If renderGrid Then
    Protected Overrides Sub OnUpdateFrame(ByVal e As OpenTK.FrameEventArgs)
        'Ripple calculations are based off of Delphi source code by Jan Horn [http://home.global.co.za/~jhorn].
        DoGridCalculations()
    End Sub
#End If
#End Region

#Region """OnRenderFrame"" Method and related methods - actual rendering code goes here"
    Protected Overrides Sub OnRenderFrame(ByVal e As OpenTK.FrameEventArgs)
        framesElapsed += 1
#If rasterizeDecals Then
        Dim dCount As Integer = Damage.Count - 1
#If rapidRasterization Then
        Dim rasterizeAfterThisFrame As Boolean = True
#Else
        Dim rasterizeAfterThisFrame As Boolean = False
        Dim nonFlames As Integer = 0
        For decal As Integer = 0 To dCount
            SyncLock Damage
                If (Not Damage(decal).IsFlame) Then nonFlames += 1
            End SyncLock
        Next decal
        If (nonFlames > rasterizationThreshold) Then 'We have a large number of decals, we should rasterize them.
            Console.WriteLine("Rasterization threshold exceeded. Rasterizing decals this frame.")
            rasterizeAfterThisFrame = True
        End If
#End If
        If rasterizeAfterThisFrame Then GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill)
#End If
        If (clearScreen) Then 'Note: clearScreen will be set to false after rendering if rasterization is ON.
#If rasterizeDecals Then
            rasterizeAfterThisFrame = True
            'We need to draw this frame using fill mode
            If (wireframeView) Then GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill)
#Else
            Damage = New List(Of Damage) 'When rasterizing, damage is cleared. No need to clear it twice.
            clearScreen = False 'Because we're not rasterizing, we need to manually toggle this
#End If
        End If
        If (wireframeView OrElse clearBuffer) Then GL.Clear(ClearBufferMask.ColorBufferBit) 'shouldn't be necessary otherwise since we're drawing the screen texture over everything
#If renderErrorChecking Then
        Dim glerror As Integer = GL.GetError()
#End If
        GL.MatrixMode(MatrixMode.Modelview)
        GL.LoadIdentity()
        'GL.Ortho(0, Me.Width, Me.Height, 0, -1, 1)
        GL.Ortho(0, Me.Width, Me.Height, 0, -1, 100)
        'Yrotation += 1
        'Xrotation += 1
        'Zrotation += 1
        'GL.Rotate(Xrotation, 1.0F, 0.0F, 0.0F) 'x
        'GL.Rotate(Yrotation, 0.0F, 1.0F, 0.0F) 'y
        'GL.Rotate(Zrotation, 0.0F, 0.0F, 1.0F) 'z
        'GL.Color4(1.0, 1.0, 1.0, 1.0)
        'DrawEntire2DObject(screenVertices, screenTexture) 'Draw the screen texture on a triangle strip, using OpenGL hardware acceleration
#If useFrameBufferObjects Then 'We need to bind the frame buffer for rendering, if the graphics card supports it, that is
        If (frameBufferObjectsSupported) Then
            'Console.WriteLine("Binding frame buffer")
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, frameBufferObject)
            GL.PushAttrib(AttribMask.ViewportBit) 'This stores the current GL.Viewport() paramenters
            GL.Viewport(0, 0, scrW, scrH) 'Sets the viewport size to the screen width and height
        End If
#End If 'Now that we have the frame buffer ready, we can simply draw what we want like normal!
        DrawShortCoordinateMapped2DObject(screenVertices, screenTexture, screenTextureCoords) 'because we need to flip the texture on rasterization
#If rasterizeDecals Then
        DrawDamageObjects(rasterizeAfterThisFrame)
#Else
        DrawDamageObjects(False)
#End If
#If useFrameBufferObjects Then 'Once we're done drawing, we need to switch back to the visible framebuffer and restore our viewport
        If (frameBufferObjectsSupported) Then
            'rasterize using the viewport of our frame buffer, if we have rasterization supported.
#If rasterizeDecals Then
            If rasterizeAfterThisFrame Then
                Console.WriteLine("Rasterizing frame buffer to screen texture")
                If clearScreen Then
                    DrawEntire2DObject(screenVertices, originalScreenTexture)
                    clearScreen = False
                End If
                SyncLock Damage
                    DoRasterization(True)
                    RemoveNonFlameDecals()
                End SyncLock
                If (wireframeView) Then GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line)
            End If
#End If
            'Console.WriteLine("Returning to visible framebuffer")
            GL.PopAttrib() 'restores the viewport parameters
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0) 'returns to the visible frame buffer
            'GL.BindTexture(TextureTarget.Texture2D, screenFrameBufferImg)
            'Console.WriteLine("Drawing frame buffer image...")
            'In order to flip the framebuffer object's rendering so that it's not upside down, we need to "flip" the screen vertices...
            'draw the screen FBO regularly without physics effects
            DrawEntire2DObject(fboVertices, screenFrameBufferImg)
#If renderGrid Then
            GL.BindTexture(TextureTarget.Texture2D, screenFrameBufferImg)
            For J As Integer = 0 To gridSize - 1
                'Note: This is using immediate mode rendering
                GL.Begin(BeginMode.QuadStrip)
                For I As Integer = 0 To gridSize
                    GL.Normal3(Normals(I, J + 1))
                    GL.Vertex3(Vertex(I, J + 1))
                    GL.Normal3(Normals(I, J))
                    GL.Vertex3(Vertex(I, J))
                Next
                GL.End()
            Next
#End If
            Dim FBstatus As Integer = GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt)
            If (FBstatus <> FramebufferErrorCode.FramebufferComplete) Then
                MsgBox("ERROR: Frame buffer status is " & FBstatus)
                End
            End If
        End If
#End If 'Now that we have the frame buffer ready, we can simply draw what we want!
#If rasterizeDecals Then
#If useFrameBufferObjects Then
        If Not (frameBufferObjectsSupported) Then 'Frame buffer objects take care of rasterizations for us, since we're rendering to a texture.
#End If
            If (clearScreen) Then 'Draw the original screen texture over everything
                DrawEntire2DObject(screenVertices, originalScreenTexture)
                clearScreen = False
            End If
            If rasterizeAfterThisFrame Then
                SyncLock Damage
                    DoRasterization(False)
                    RemoveNonFlameDecals()
                End SyncLock
                If (wireframeView) Then GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line)
            End If
#If useFrameBufferObjects Then
        End If
#End If
#End If
#If rapidRasterization And rasterizeDecals Then
        'if rapid rasterization is on, we need to ensure flames still work despite the performance hit
        DrawDamageObjects(False)
#End If
        'Displays current weapon on the screen
        'Console.WriteLine("Drawing current weapon label")
        'DrawEntire2DObject(screenVertices, GetCurrentWeaponLabelTexture())
        'because we're adapting the image for NPOT screen sizes and not rendering to the whole screen, we need to get the vertices of the texture
        DrawSingleCoordinateMapped2DObject(GetCurrentWeaponLabelVertices(), GetCurrentWeaponLabelTexture(), GetCurrentWeaponLabelTextureCoordinates())
        'Displays current frames per second
#If showFramesPerSecond Then
        Dim currentNum As Integer = 0
        GL.BindTexture(TextureTarget.Texture2D, numbersTexture)
        For num As Integer = 0 To (framesPerSecond.Length - 1)
            currentNum = Val(framesPerSecond(num))
            DrawSingleCoordinateMapped2DObject(FPSVertices(num), numbersTexture, RasterizedNumberCoordinates(currentNum))
        Next num
#End If
#If renderErrorChecking Then
        Dim glerror1 As Integer = GL.GetError()
        If (glerror1 <> 0) Then 'Handles errors with OpenGL
            Console.WriteLine("OpenGL Error Occured: " & glerror1)
        End If
#End If
#If Not Debug Then
            MsgBox("An error occured when texturing the objects in the game. The game will now close.")
            Me.Close()
#End If
        Me.SwapBuffers() 'DO NOT EVER COMMENT THIS OUT. EVER. DON'T DO IT. I WARNED YOU.
#If renderErrorChecking Then
        If (glerror <> 0) Then 'Handles errors with OpenGL
            Console.WriteLine("OpenGL Error Occured: " & glerror)
            'Console.WriteLine("Setting texturingFails to true")
        End If
#End If
#If Debug And useRenderTest Then
            Dim renderingResult As Bitmap = New Bitmap(Screenshot.GetDesktopImage(scrW, scrH, False))
            renderingResult.Save(currentRenderingBitmapName)
            If (firstRender) Then
                'now, we check that everything rendered EXACTLY how it was supposed to, pixel for pixel
                If (checkIfIdenticalBitmap(testPattern, renderingResult)) Then
                    MsgBox("Rendering works PERFECTLY on your System!")
                Else
                    MsgBox("Rendering test FAILED! The rendering is not accurate, pixel-for-pixel.")
                End If
            End If
#End If
        firstRender = False
        busy = False
    End Sub

    Private Sub DrawEntire2DObject(ByRef vertices As Short(), ByVal texture As UInteger)
        DrawShortCoordinateMapped2DObject(vertices, texture, textureCoords)
    End Sub


    Private Sub RemoveNonFlameDecals()
        Dim dCount As Integer = Damage.Count - 1
        For decal As Integer = dCount To 0 Step -1
            Try
                If (Not Damage(decal).IsFlame) Then Damage.RemoveAt(decal)
            Catch ex As Exception
                Exit For
            End Try
        Next decal
    End Sub

    Private Sub DrawDamageObjects(ByRef rasterizeAfterThisFrame As Boolean)
        Dim Mcount As Long = 0 'can't be null
        Dim dmg As Damage
        SyncLock Damage
            Mcount = Damage.Count - 1
        End SyncLock
        For I As Long = 0 To Mcount 'For Each bullet As Damage In Damage
            If (I > Mcount) Then Exit For
            Try
                SyncLock Damage
                    dmg = Damage(CInt(I)) 'this uses more memory, but it _feels_ faster because the synclock isn't as necessary
                End SyncLock
            Catch ex As Exception
                Exit For
            End Try
            Try
                If Not (dmg.IsFlame()) Then 'If the damage being drawn is NOT a flame
#If drawDamage Then
                    DrawEntire2DObject(dmg.getVertices, dmg.TextureID)
#End If
                Else 'The damage is a flame, we need to handle this special case.
#If rasterizeDecals Then 'If we're rasterizing after this frame, then skip drawing the flames.
                    If rasterizeAfterThisFrame Then Continue For
#End If
                    If Not ((dmg.Bottom > 0) AndAlso (dmg.Top < Me.Height) AndAlso (dmg.Right > 0) AndAlso (dmg.Left < Me.Width)) Then
                        'If the flame is outside of the drawing area, remove it.
#If (ultraVerbose) Then
                        Console.WriteLine("Removing flame with damage index '" & I & "' of '" & Mcount & "'")
#End If
                        Damage.RemoveAt(CInt(I))
                        Mcount -= 1
                        I -= 1
                        Continue For
                    Else 'Otherwise, we need to draw the flame.
#If oneFlameAnimationTexture And drawDamage Then
                        DrawSingleCoordinateMapped2DObject(dmg.getVertices, FireFrames, FlameCoordinates(CurrentFireFrame))
#Else
                        DrawEntire2DObject(dmg.getVertices, FireFrames(CurrentFireFrame))
#End If
                    End If
                End If
            Catch ex As Exception
                Continue For
            End Try
        Next I
    End Sub

#Region "DoRasterizaton Method"
    Private Sub DoRasterization(ByVal FrameBuffer As Boolean)

        '                Dim bmp As Bitmap = New Bitmap(Width, Height)
        '                Dim data As System.Drawing.Imaging.BitmapData = bmp.LockBits(ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb)
        '                GL.ReadPixels(0, 0, Width, Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0)
        '                bmp.UnlockBits(data)
        '                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY)
        '                bmp = ScaleImage(bmp)
        '#If saveRasterizations Then
        '                Console.WriteLine("Saving rasterization " & rasterizationCount)
        '                bmp.Save("rasterization" & rasterizationCount & ".bmp")
        '                rasterizationCount += 1
        '#End If              
        'GL.DeleteTexture(screenTexture) 'Deletes old screen texture
        'screenTexture = CUInt(GL.GenTexture()) 'Creates a new texture
        GL.BindTexture(TextureTarget.Texture2D, screenTexture) 'Binds the new screen texture
        'Copy our viewport to the new screen texture -- IMPORTANT: This returns the rasterized texture vertically flipped
        If (Not FrameBuffer) Then 'because the framebuffer is always rendered using the size of the whole screen
            GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 0, 0, Me.Width, Me.Height, 0)
        Else
            GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 0, 0, scrW, scrH, 0)
        End If

        'As a workaround, we're going to reverse the array of vertices so that it draws upside down
        'original verts are specified as ul, bl, ur, br
        Dim originalCoords As Short() = screenTextureCoords
        screenTextureCoords = New Short() { _
            0, 1, _
            0, 0, _
            1, 1, _
            1, 0}
        'screenTextureCoords = New Short()
        'Dim Left As Integer = 0
        'Dim Right As Integer = (screenVertices.Count - 1)
        'Dim Temp As Short
        'While (Left < Right)
        '    Temp = screenVertices(Left)
        '    screenVertices(Left) = screenVertices(Right)
        '    screenVertices(Right) = Temp
        '    Left += 1 : Right -= 1
        'End While
        'screenTexture = CreateFilterlessTextureFromBitmap(bmp, False)
    End Sub
#End Region

#Region "Coordinate Mapped Object Drawing Code"
    Private Sub DrawShortCoordinateMapped2DObject(ByRef vertices As Short(), ByVal texture As UInteger, ByRef coords As Short())
        If (Not currentTex = texture) Then
            currentTex = texture
            GL.BindTexture(TextureTarget.Texture2D, currentTex)
            'GL.BindTexture(TextureTarget.Texture2D, texture)
#If renderErrorChecking Then
            If (GL.GetError() <> 0) Then Console.WriteLine("Binding texture failed")
#End If
        End If
#If useVertexArrays Then
        'Try to render object image using vertex arrays
        SyncLock vertices 'just in case, really...
            GL.VertexPointer(2, VertexPointerType.Short, 0, vertices)
            GL.TexCoordPointer(2, TexCoordPointerType.Short, 0, coords)
            GL.DrawArrays(BeginMode.TriangleStrip, 0, vertices.Length \ 2) ' /2 is necissary cause we're using PAIRS
        End SyncLock
#Else
        'The deprecated immediate mode rendering code is below.
        'This will almost certainly not be needed, as vertex arrays should work just fine.
        'We switched from using quads to using triangle strips for more efficiency.
        GL.Begin(BeginMode.TriangleStrip)
        GL.TexCoord2(coords(0), coords(1)) : GL.Vertex2(vertices(0), vertices(1)) 'upper left
        GL.TexCoord2(coords(2), coords(3)) : GL.Vertex2(vertices(2), vertices(3)) 'bottom left
        GL.TexCoord2(coords(4), coords(5)) : GL.Vertex2(vertices(4), vertices(5)) 'upper right
        GL.TexCoord2(coords(6), coords(7)) : GL.Vertex2(vertices(6), vertices(7)) 'bottom right
        GL.End()
#End If
#If renderErrorChecking Then
        Dim er As Integer = GL.GetError
        If (er <> 0) Then Console.WriteLine("Drawing texture failed: " & er)
#End If
    End Sub

    Private Sub DrawSingleCoordinateMapped2DObject(ByRef vertices As Short(), ByVal texture As UInteger, ByRef coords As Single())
        If (Not currentTex = texture) Then
            currentTex = texture
            GL.BindTexture(TextureTarget.Texture2D, currentTex)
#If renderErrorChecking Then
            If (GL.GetError() <> 0) Then Console.WriteLine("Binding texture failed")
#End If
        End If
#If useVertexArrays Then 'Try to render object image using vertex arrays
        SyncLock vertices 'just in case, really...
            GL.VertexPointer(2, VertexPointerType.Short, 0, vertices)
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, coords)
            GL.DrawArrays(BeginMode.TriangleStrip, 0, vertices.Length \ 2) ' /2 is necissary cause we're using PAIRS
        End SyncLock
#Else
        'The deprecated immediate mode rendering code is below.
        'This will almost certainly not be needed, as vertex arrays should work just fine.
        'We switched from using quads to using triangle strips for more efficiency.
        GL.Begin(BeginMode.TriangleStrip)
        GL.TexCoord2(coords(0), coords(1)) : GL.Vertex2(vertices(0), vertices(1)) 'upper left
        GL.TexCoord2(coords(2), coords(3)) : GL.Vertex2(vertices(2), vertices(3)) 'bottom left
        GL.TexCoord2(coords(4), coords(5)) : GL.Vertex2(vertices(4), vertices(5)) 'upper right
        GL.TexCoord2(coords(6), coords(7)) : GL.Vertex2(vertices(6), vertices(7)) 'bottom right
        GL.End()
#End If
#If renderErrorChecking Then
        Dim er As Integer = GL.GetError
        If (er <> 0) Then Console.WriteLine("Drawing texture failed: " & er)
#End If
    End Sub
#End Region

#If renderGrid Then
    Private Sub DoGridCalculations()
        Try
            'First we have to calculate the new velocity.
            Dim VectLength As Single
            For I As Integer = 2 To gridSize - 2
                For J As Integer = 2 To gridSize - 2
                    Velocity(I, J) += (Position(I, J) - _
                        (4 * (Position(I - 1, J) + Position(I + 1, J) + Position(I, J + 1) + Position(I, J - 1)) + _
                         Position(I - 1, J - 1) + Position(I + 1, J - 1) + Position(I - 1, J + 1) + Position(I + 1, J + 1) / 25) / 7)
                Next
            Next
            'Next, we have to calculate the new ripple positions.
            For I As Integer = 2 To gridSize - 2
                For J As Integer = 2 To gridSize - 2
                    Position(I, J) -= Velocity(I, J)
                    Velocity(I, J) *= Viscosity
                Next
            Next
            'The last thing we have to do before rendering the result is calculate the new vertex normals.
            For I As Integer = 0 To gridSize
                For J As Integer = 0 To gridSize
                    If (I > 0) And (J > 0) And (I < gridSize) And (J < gridSize) Then
                        With Normals(I, J)
                            .X = Position(I + 1, J) - Position(I - 1, J)
                            .Y = -2048
                            .Z = Position(I, J + 1) - Position(I, J - 1)
                            VectLength = CSng(Math.Sqrt(.X * .X + .Y * .Y + .Z * .Z))
                            If VectLength <> 0 Then
                                .X /= VectLength
                                .Y /= VectLength
                                .Z /= VectLength
                            End If
                        End With
                    Else
                        Normals(I, J) = New OpenTK.Vector3(0, 1, 0)
                    End If
                Next
            Next
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub CreateImpact(ByVal X As Integer, ByVal Y As Integer)
        Try
            If (Y <= Me.Height) AndAlso (X <= Me.Width) Then
                Dim gX As Integer = Math.Abs(CInt((X / Me.Width) * gridSize))
                Dim gY As Integer = Math.Abs(CInt((Y / Me.Height) * gridSize))
                Console.WriteLine("Creating Fluid Impact at: (" & gX & ", " & gY & ")")
                Velocity(gX, gY) = 1060
            End If
        Catch ex As Exception
            'MsgBox("Error creating fluid impact: " & ex.Message)
            Console.WriteLine("Error creating fluid impact: " & ex.Message)
        End Try
    End Sub
#End If
#End Region

#Region """GetCurrentWeaponLabel"" Methods - Gets the texture id,etc. for the current weapon's label image"
    'Author: Dylan Taylor
    Private Function GetCurrentWeaponLabelTexture() As UInteger
        Return weaponNameTextures(currentWeapon - 9002)
    End Function

    Private Function GetCurrentWeaponLabelTextureCoordinates() As Single()
        Return weaponNameTextureCoordinates(currentWeapon - 9002)
    End Function

    Private Function GetCurrentWeaponLabelVertices() As Short()
        Return weaponNameVertices(currentWeapon - 9002)
    End Function
#End Region

#Region """TextToBitmap"" Method - Generates a bitmap containing the specified text"
    'Author: Dylan Taylor
    Private Function TextToBitmap(ByVal text As String, ByVal font As Drawing.Font, ByVal x As Integer, ByVal y As Integer) As Drawing.Bitmap
        Return TextToBitmap(text, font, x, y, defaultBrush)
    End Function

    Private Function TextToBitmap(ByVal text As String, ByVal font As Drawing.Font, ByVal x As Integer, ByVal y As Integer, ByVal brush As Drawing.Brush) As Drawing.Bitmap
        Dim bmap As Drawing.Bitmap
        'If (nonPowerOfTwoTextureSizesSupported) Then
        'bmap = New Drawing.Bitmap(GetTextWidthInPixels(text, font), GetTextHeightInPixels(text, font))
        'Else
        bmap = New Drawing.Bitmap(MathOps.NearestPowerOfTwo(GetTextWidthInPixels(text, font)), MathOps.NearestPowerOfTwo(GetTextHeightInPixels(text, font)))
        'End If
        Dim gfx As Drawing.Graphics = Drawing.Graphics.FromImage(bmap)
        gfx.DrawString(text, font, brush, x, y)
        Return bmap
    End Function
#End Region

#Region "Methods for getting the specified rasterized text's width and height in the specified font"
    Private Function GetTextWidthInPixels(ByVal text As String, ByVal font As Drawing.Font) As Integer
        Dim bmap As Drawing.Bitmap = New Drawing.Bitmap(Me.Width, Me.Height)
        Dim gfx As Drawing.Graphics = Drawing.Graphics.FromImage(bmap)
        Return CInt(gfx.MeasureString(text, font).Width)
    End Function

    Private Function GetTextHeightInPixels(ByVal text As String, ByVal font As Drawing.Font) As Integer
        Dim bmap As Drawing.Bitmap = New Drawing.Bitmap(Me.Width, Me.Height)
        Dim gfx As Drawing.Graphics = Drawing.Graphics.FromImage(bmap)
        Return CInt(gfx.MeasureString(text, font).Height)
    End Function
#End Region

#Region """CreateFilterlessTextureFromBitmap Method - used to create more accurate textures than TexLib's ""CreateTextureFromBitmap"" method."
    'Based off of TexLib's "CreateTextureFromBitmap" method. Modified by Dylan Taylor
    'Uses nearest filtering instead of linear, and compressedrgba instead of rgba
    'OpenGL automatically falls back on rgba if compression isn't supported by the GPU.
    Private Function CreateFilterlessTextureFromBitmap(ByVal bitmap As Drawing.Bitmap, ByVal alphaTransparency As Boolean) As UInteger
        Dim tex As UInteger = TexLib.TexUtil.GiveMeATexture()
        'The following line forces OpenGL to use the BEST compression algorithm available, regardless of the speed trade-off
        GL.Hint(HintTarget.TextureCompressionHint, HintMode.Nicest)
        GL.BindTexture(TextureTarget.Texture2D, tex)
        Dim data As Drawing.Imaging.BitmapData = bitmap.LockBits(New Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), _
        Drawing.Imaging.ImageLockMode.[ReadOnly], Drawing.Imaging.PixelFormat.Format32bppArgb)
        'GL_COMPRESSED_RGB_S3TC_DXT1 is the best choice when the alpha channel is not necessary
        'GL.TexImage2D(TextureTarget.Texture2D, 0, If(alphaTransparency, PixelInternalFormat.CompressedRgbaS3tcDxt5Ext, PixelInternalFormat.CompressedRgbS3tcDxt1Ext), bitmap.Width, bitmap.Height, 0, _
        GL.TexImage2D(TextureTarget.Texture2D, 0, If(alphaTransparency, PixelInternalFormat.Rgba, PixelInternalFormat.Rgb), bitmap.Width, bitmap.Height, 0, _
          PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0)
        bitmap.UnlockBits(data)
        'Instead of using TexLib's setParameters, we want to override those with our own parameters.
        'By default, TexLib uses "Linear" filtering. We want to use "Nearest" filtering instead.
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, CInt(TextureMinFilter.Nearest))
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, CInt(TextureMagFilter.Nearest))
        Return CUInt(tex)
    End Function
#End Region

#Region """PlaySoundFile"" Method - Plays the specified sound file"
#If soundEnabled Then
    'Authors: Dylan Taylor, Scott Ketelaar
    '    Public Shared Sub PlaySoundFile(ByVal fileName As String, ByVal gain As Single, ByVal loopSound As Boolean)
    '        If soundFails Then Return
    '#If ultraVerbose Then
    '        Console.WriteLine("Playing sound file '" & fileName & "' with gain of " & gain.ToString & ".")
    '#End If

    '        Sound.PlayAudio(fileName, CSng(gain), loopSound)
    '        SyncLock Sound.Sources
    '#If ultraVerbose Then
    '            Console.WriteLine("This sound file is set to " & If(loopSound, "", "not ") & "repeat. " & _
    '                              Sound.Sources.Count.ToString & " total audio sources stored")
    '#End If
    '        End SyncLock
    '    End Sub
    '#End If
    Public Shared Sub PlaySoundFile(ByVal fileName As String, ByVal gain As Single, ByVal loopSound As Boolean, Optional ByVal VX As Integer = 0, Optional ByVal VY As Integer = 0)
        If soundFails Then Return
#If ultraVerbose Then
            Console.WriteLine("Playing sound file '" & fileName & "' with gain of " & gain.ToString & ".")
#End If
        Sound.PlaySound(fileName, CSng(gain), loopSound, VX, VY)
#If ultraVerbose Then
                Console.WriteLine("This sound file is set to " & If(loopSound, "", "not ") & "repeat. " & _
                                  Sound.GetSourcesCount().ToString & " total audio sources stored")
#End If
    End Sub

    'This method is used to play sound data stored in memory, rather than a file.
    Public Shared Sub PlaySoundFile(ByRef soundData As Byte(), ByVal gain As Single, ByVal loopSound As Boolean, Optional ByVal VX As Integer = 0, Optional ByVal VY As Integer = 0)
        If soundFails Then Return
#If ultraVerbose Then
            Console.WriteLine("Playing sound data with gain of " & gain.ToString & ".")
#End If
        Sound.PlaySound(soundData, CSng(gain), loopSound, VX, VY)
#If ultraVerbose Then
                Console.WriteLine("This sound file is set to " & If(loopSound, "", "not ") & "repeat. " & _
                                  Sound.GetSourcesCount().ToString & " total audio sources stored")
#End If
    End Sub
#End If

#End Region

#Region "File and Bitmap Loading Methods"
    Private Function LoadBitmap(ByVal filename As String) As Drawing.Bitmap
        Dim f As System.IO.FileStream = Nothing
        Try
            f = New System.IO.FileStream(filename, IO.FileMode.Open)
            Return New Drawing.Bitmap(Drawing.Bitmap.FromStream(f))
        Catch ex As Exception
            Return Nothing
        Finally
            f.Close()
        End Try
    End Function

    'Private Function LoadFile(ByVal filename As String) As System.IO.MemoryStream
    'Dim f As System.IO.FileStream = Nothing
    'Dim bytesRead As Byte()
    'Try
    ' f = New System.IO.FileStream(filename, IO.FileMode.Open)
    '  ReDim bytesRead(CInt(f.Length))
    '   f.Read(bytesRead, 0, CInt(f.Length))
    '    Return New System.IO.MemoryStream(bytesRead)
    ' Catch ex As Exception
    '      Return Nothing
    '   Finally
    '        f.Close()
    '    End Try
    'End Function

    Private Function LoadFile(ByVal filename As String) As Byte()
        Dim f As System.IO.FileStream = Nothing
        Dim bytesRead As Byte()
        Try
            f = New System.IO.FileStream(filename, IO.FileMode.Open)
            If (CLng(f.Length) > CLng(CInt(f.Length))) Then
                Console.WriteLine("File too large")
            End If
            ReDim bytesRead(CInt(f.Length))
            f.Read(bytesRead, 0, CInt(f.Length))
            Console.WriteLine("File """ & filename & """ loaded successfully. Length: " & bytesRead.Length)
            f.Close()
            Return bytesRead
        Catch ex As Exception
            Console.WriteLine("Loading the file failed. Miserably. Filename: " & filename & ex.ToString)
            Return Nothing
        End Try
    End Function
#End Region

#Region "CheckForOpenALDLL Method - Used to automatically detect OpenAL and fallback to OpenAL Soft if necessary"
#If checkForOpenALDLL And soundEnabled Then
    Function IsDLLAvailable(ByVal DllFilename As String) As Boolean
        Dim hModule As Long ' attempt to load the module
        hModule = LoadLibrary(DllFilename)
        If hModule > 32 Then
            FreeLibrary(hModule) ' decrement the DLL usage counter   
            Return True
        Else : Return False
        End If
    End Function
#End If
#End Region

#Region "Cross-Class Communication Methods - Used to allow other classes to write to this class's console"
    'This method is used to allow other classes to write to this class's console.
    'Supported data types: String, Boolean, Char, Decimal, Single, Double, Long, Integer, Short UInteger, ULong, UShort 
    'Author: Dylan Taylor
    Public Shared Sub SayLine(ByVal o As Object)
        Try
            Console.WriteLine(o)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Shared Sub Say(ByVal o As Object)
        Try
            Console.Write(o)
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
#End Region

#Region "Class Entry Point - Run prior to the game window being created"
    <STAThread()> _
    Public Shared Sub Main()
        'Splash = New SplashScreen
        'SplashThread = New Threading.Thread(AddressOf Splash.ShowDialog)
        'SplashThread.Start()
        Try 'If it exists, delete the OpenAL Soft dll
            Kill("OpenAL32.dll")
        Catch ex As Exception
            'Do nothing.
        End Try
#If checkForOpenALDLL And soundEnabled Then
        Dim openalDLLAvailable As Boolean = False
        Dim hModule As Long
        hModule = LoadLibrary("OpenAL32.dll")
        If hModule > 32 Then
            FreeLibrary(hModule)
            openalDLLAvailable = True
        End If
        If (Not openalDLLAvailable) Then 'we don't have OpenAL installed on the system
            Console.WriteLine("OpenAL is NOT installed. Attempting to use OpenAL Soft instead...")
            Try
                System.IO.File.Copy("Resources\OpenALSoft\OpenAL32.dll", "OpenAL32.dll", True) 'installs the openal soft dll
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
        End If
#End If
        Try
            Using gameInstance As New Game()
                gameInstance.Title = BaseTitle 'Sets the title
                gameInstance.Run(30.0, 0.0)
            End Using
        Catch ex As Exception
            MsgBox("Failed to load game. Exception:" & ex.Message)
        End Try
    End Sub
#End Region

End Class
