Imports System.Data
Imports System.Data.SqlClient
Public Class Login

    Private Sub filterButton_Click(sender As Object, e As RoutedEventArgs) Handles filterButton.Click
        Using con As New SqlConnection("Data Source=.;Initial Catalog=TokoRoti;Integrated Security=True")
            Dim data As New SqlDataAdapter("SELECT * FROM Login WHERE nama='" + username.Text.Trim + "' AND password ='" + password.Password + "'", con)
            Dim table As New DataTable
            data.Fill(table)

            If table.Rows.Count = 1 Then
                Dim main As New MainWindow(table.Rows(0)(0), table.Rows(0)(1), table.Rows(0)(4))
                main.Show()
                Me.Close()
            Else
                MessageBox.Show("Username & Password Salah")
            End If
        End Using
    End Sub
End Class
