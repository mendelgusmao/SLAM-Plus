Imports System.Net
Imports System.Threading

Public Class APIServer
    Dim ServerThread As Thread
    Dim Games As List(Of SourceGame)

    Public Sub Start(ByRef games As List(Of SourceGame))
        Me.Games = games

        ServerThread = New Thread(AddressOf Me.Listen)
        ServerThread.Start()
    End Sub

    Private Sub Listen()
        Dim listener As New HttpListener
        listener.Prefixes.Add("http://127.0.0.1:7526/")

        Try
            listener.Start()
        Catch ex As Exception
            Console.WriteLine($"Error trying to start server : {ex.Message}")
            Return
        End Try

        Threading.ThreadPool.QueueUserWorkItem(Sub()
                                                   Do
                                                       Dim context = listener.GetContext
                                                       Dim response As HttpListenerResponse = context.Response
                                                       Dim request = context.Request

                                                       Select Case request.RawUrl.Split("?")(0)
                                                           Case "/tracks"
                                                               Dim Serializer = New System.Runtime.Serialization.Json.DataContractJsonSerializer(GetType(List(Of SourceGame)))
                                                               Dim Game As String() = request.QueryString.GetValues("game")

                                                               response.AddHeader("Content-Type", "application/json")

                                                               If Game Is Nothing Then
                                                                   Serializer.WriteObject(response.OutputStream, Me.Games)
                                                               Else
                                                                   Dim SelectedGame As String = Game(0)
                                                                   Dim FilteredGames As List(Of SourceGame) = Games.Where(Function(g) g.name = SelectedGame).ToList()
                                                                   Serializer.WriteObject(response.OutputStream, FilteredGames)
                                                               End If
                                                       End Select

                                                       response.OutputStream.Close()
                                                   Loop
                                               End Sub)
    End Sub
End Class
