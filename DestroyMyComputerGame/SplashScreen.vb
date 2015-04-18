Option Strict On
Public NotInheritable Class SplashScreen
    Private Sub SplashScreen_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        CheckForIllegalCrossThreadCalls = False ' Messy, but if not used, we block for a while
        'waiting for it to close
        Version.Text = System.String.Format(Version.Text, My.Application.Info.Version.Major, My.Application.Info.Version.Minor)
    End Sub
End Class
