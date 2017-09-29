Imports System.Data
Imports System.Data.SqlClient
Imports System.IO

Class MainWindow
    Dim conString As String = "Data Source=.;Initial Catalog=TokoRoti;Integrated Security=True"
    Dim addMode As Boolean = False
    Dim idBarang, idStaff, level As String
    Dim transactionTable, dataTable, itemSuggestionTable, categoryTable, statusTable As New DataTable
    Dim dlg As Microsoft.Win32.OpenFileDialog
    Dim tableName, columnName, columnIdName, insertCMD, saveCMD As String
    Dim categoryValue As String
    Dim discountValue, minReq As Integer
    Dim menuIndex As Integer
    Dim navButton() As Button

    Public Sub New(staff As String, nameStaff As String, lvl As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        idStaff = staff
        Welcome.Text = "Welcome, " + nameStaff + " !!"
        level = lvl
    End Sub

    Private Sub fillPreview()
        Using con As New SqlConnection(conString)
            Dim data1 As New SqlDataAdapter("Select * FROM " + tableName + " WHERE " + columnName + " LIKE '" + searchBox.Text + "%'", con)
            dataTable = New DataTable
            data1.Fill(dataTable)

            masterGrid.ItemsSource = dataTable.DefaultView
        End Using
    End Sub


    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Dim s As New Style
        s.Setters.Add(New Setter(UIElement.VisibilityProperty, Visibility.Collapsed))
        mainTab.ItemContainerStyle = s

        navButton = New Button() {dashboardButton, transactionButton, breadButton, staffButton, reportButton}
        FillComboBox()

        dateSelect.DisplayDate = Now

        dlg = New Microsoft.Win32.OpenFileDialog

        nowDate.Content = Date.Today.ToShortDateString
    End Sub

    Private Sub FillComboBox()
        Using con As New SqlConnection(conString)
            Dim data As New SqlDataAdapter("SELECT * FROM JenisBarang", con)
            categoryTable = New DataTable
            data.Fill(categoryTable)
            itemCategory.Items.Clear()
            itemCategoryData.Items.Clear()

            For i = 0 To categoryTable.Rows.Count - 1
                itemCategory.Items.Add(categoryTable.Rows(i)(1))
                itemCategoryData.Items.Add(categoryTable.Rows(i)(1))
            Next

            If menuIndex = 3 Then
                itemCategoryData.Items.Clear()
                itemCategoryData.Items.Add("0")
                itemCategoryData.Items.Add("1")

                Return
            End If

            itemCategory.SelectedIndex = 0
            itemCategoryData.SelectedIndex = 0
        End Using
    End Sub

    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        itemSuggestion.Visibility = Windows.Visibility.Collapsed
    End Sub

    Private Sub ChangeNavButton(index As Integer)
        For i = 0 To navButton.Length - 1
            If i = index Then
                navButton(i).Style = CType(Application.Current.Resources("ActiveNav"), Style)
                menuIndex = index
            Else
                navButton(i).Style = CType(Application.Current.Resources("PrimaryButton"), Style)
            End If
        Next
    End Sub

    Private Sub SumTotal()
        Dim n As Integer = 0
        For i = 0 To transactionTable.Rows.Count - 1
            n += transactionTable.Rows(i)(2) * transactionTable.Rows(i)(3)
            n -= (transactionTable.Rows(i)(4) / 100) * n
        Next
        totalPrice.Text = n
        Dim moneyValue As Integer
        If Not Integer.TryParse(moneyAmount.Text, moneyValue) Then
            moneyValue = 0
        End If
        returnMoney.Text = moneyValue - n
    End Sub

    Private Sub TransactionTableFunction(mode As Integer)
        '0 = fill, 1 = insert, 2 = update, 3 delete
        If mode = 0 Then
            Using con As New SqlConnection(conString)
                Dim data As New SqlDataAdapter("select * from transactionTemp", con)
                transactionTable = New DataTable
                data.Fill(transactionTable)

                transactionGrid.ItemsSource = transactionTable.DefaultView
            End Using
        ElseIf mode = 1 Then
            Using con As New SqlConnection(conString)
                Dim data As New SqlDataAdapter("Select jumlah from transactiontemp where idbarang='" + idBarang + "'", con)
                Dim table As New DataTable
                data.Fill(table)

                Dim n As New Integer
                If Not Integer.TryParse(itemQty.Text, n) Or itemQty.Text = "0" Then
                    n = 1
                End If

                con.Open()
                If table.Rows.Count = 1 Then
                    n += table.Rows(0)(0)

                    Dim cmd1 As New SqlCommand("DELETE FROM TransactionTemp WHERE idbarang=" + idBarang, con)
                    cmd1.ExecuteNonQuery()

                End If

                Dim tempDis As String = 0

                If minReq <= n Then
                    tempDis = discountValue
                End If

                Dim cmd As New SqlCommand("insert into transactionTemp (idBarang, NamaBarang, Harga, Jumlah, Diskon, idJenis) VALUES (@id, @name, @price, @qty, @discount, @idJenis)", con)
                cmd.Parameters.AddWithValue("@id", idBarang)
                cmd.Parameters.AddWithValue("@name", itemName.Text)
                cmd.Parameters.AddWithValue("@price", itemPrice.Text)
                cmd.Parameters.AddWithValue("@qty", n)
                cmd.Parameters.AddWithValue("@discount", tempDis)
                cmd.Parameters.AddWithValue("@idJenis", itemSuggestionTable.Rows(itemCategory.SelectedIndex)(5))

                cmd.ExecuteNonQuery()

                TransactionTableFunction(0)
            End Using
        ElseIf mode = 2 Then
            Using con As New SqlConnection(conString)
                Dim cmd As New SqlCommand("update transactiontemp set namabarang='" + itemName.Text + "', harga='" + itemPrice.Text + "', jumlah='" + itemQty.Text + "', diskon=" + discountValue + " WHERE idBarang='" + idBarang + "'", con)

                con.Open()
                cmd.ExecuteNonQuery()
            End Using
            TransactionTableFunction(0)
        ElseIf mode = 3 Then
            Using con As New SqlConnection(conString)
                Dim cmd As New SqlCommand("DELETE FROM transactiontemp WHERE idbarang=" + idBarang, con)

                con.Open()
                cmd.ExecuteNonQuery()
            End Using
            TransactionTableFunction(0)
        ElseIf mode = 4 Then
            Using con As New SqlConnection(conString)
                Dim cmd As New SqlCommand("DELETE FROM transactiontemp", con)
                con.Open()
                cmd.ExecuteNonQuery()
            End Using
            TransactionTableFunction(0)
        End If
    End Sub

    Private Sub ItemSuggestoinFill()
        Using con As New SqlConnection(conString)
            Dim data As New SqlDataAdapter("select idBarang, nama, harga, diskon, minPembelian, idJenis from barang where nama LIKE '" + itemName.Text.Trim + "%' AND idJenis='" + categoryTable.Rows(itemCategory.SelectedIndex)(0).ToString + "'", con)
            itemSuggestionTable = New DataTable
            data.Fill(itemSuggestionTable)

            If itemSuggestionTable.Rows.Count <= 0 Then

                Return
            End If

            itemSuggestion.Visibility = Windows.Visibility.Visible
            For i = 0 To itemSuggestionTable.Rows.Count - 1
                itemSuggestion.Items.Add(itemSuggestionTable.Rows(i)(1))
            Next
        End Using
    End Sub

    Private Sub dashboardButton_Click(sender As Object, e As RoutedEventArgs) Handles dashboardButton.Click
        ChangeNavButton(0)
        mainTab.SelectedIndex = 0
    End Sub

    Private Sub transactionButton_Click(sender As Object, e As RoutedEventArgs) Handles transactionButton.Click
        ChangeNavButton(1)
        TransactionTableFunction(0)
        mainTab.SelectedIndex = 1
    End Sub

    Private Sub breadButton_Click(sender As Object, e As RoutedEventArgs) Handles breadButton.Click
        ChangeNavButton(2)
        mainTab.SelectedIndex = 2
        FillMasterData()
    End Sub

    Private Sub staffButton_Click(sender As Object, e As RoutedEventArgs) Handles staffButton.Click
        ChangeNavButton(3)
        mainTab.SelectedIndex = 2
        FillMasterData()
    End Sub

    Private Sub reportButton_Click(sender As Object, e As RoutedEventArgs) Handles reportButton.Click
        ChangeNavButton(4)
        mainTab.SelectedIndex = 2
        FillMasterData()
    End Sub

    Private Sub addToTable_Click(sender As Object, e As RoutedEventArgs) Handles addToTable.Click
        TransactionTableFunction(1)
        SumTotal()
    End Sub

    Private Sub updateTable_Click(sender As Object, e As RoutedEventArgs) Handles updateTable.Click
        TransactionTableFunction(2)
    End Sub

    Private Sub deleteTable_Click(sender As Object, e As RoutedEventArgs) Handles deleteTable.Click
        TransactionTableFunction(3)
    End Sub

    Private Sub reset_Click(sender As Object, e As RoutedEventArgs) Handles reset.Click
        TransactionTableFunction(4)
    End Sub

    Private Sub itemName_TextChanged(sender As Object, e As TextChangedEventArgs)
        itemSuggestion.Items.Clear()
        ItemSuggestoinFill()
    End Sub

    Private Sub itemCategory_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)

    End Sub

    Private Sub itemSuggestion_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If itemSuggestion.SelectedItem = itemName.Text Or itemSuggestion.SelectedIndex < 0 Then
            Return
        End If
        idBarang = itemSuggestionTable.Rows(itemSuggestion.SelectedIndex)(0)
        itemPrice.Text = itemSuggestionTable.Rows(itemSuggestion.SelectedIndex)(2)
        discountValue = itemSuggestionTable.Rows(itemSuggestion.SelectedIndex)(3)
        minReq = itemSuggestionTable.Rows(itemSuggestion.SelectedIndex)(4)
        itemName.Text = itemSuggestion.SelectedItem

        itemSuggestion.Visibility = Windows.Visibility.Collapsed
    End Sub

    Private Sub filterButton_Click(sender As Object, e As RoutedEventArgs) Handles filterButton.Click

        If filter.Visibility = Windows.Visibility.Visible Then
            filter.Visibility = Visibility.Collapsed
        Else
            filter.Visibility = Windows.Visibility.Visible
        End If
    End Sub

    Private Sub FillMasterData()
        FillComboBox()
        If menuIndex = 2 Then
            tableName = "Barang"
            label1.Content = "Harga"
            label2.Content = "Stock"
            columnName = "Nama"
            columnIdName = "idBarang"
            insertCMD = "(nama, harga, stok, diskon, minPembelian, idJenis) VALUES ('" + breadName.Text + "', '" + breadPrice.Text + "', '" + breadStock.Text + "', '" + discount.Text + "', '" + minBuy.Text + "', " + Convert.ToString(itemCategoryData.SelectedIndex + 1) + ")"
            saveCMD = "nama='" + breadName.Text + "',harga= '" + breadPrice.Text + "', stok ='" + breadStock.Text + "',diskon= '" + discount.Text + "', minpembelian='" + minBuy.Text + "', idJenis='" + Convert.ToString(itemCategoryData.SelectedIndex + 1) + "'"
            If Not IsDBNull(dlg.FileName) Then
                saveCMD = "nama='" + breadName.Text + "',harga= '" + breadPrice.Text + "', stok ='" + breadStock.Text + "',diskon= '" + discount.Text + "', minpembelian='" + minBuy.Text + "', idJenis='" + Convert.ToString(itemCategoryData.SelectedIndex + 1) + "', gambar=@image"
                insertCMD = "(nama, harga, stok, diskon, minPembelian, idJenis, gambar) VALUES ('" + breadName.Text + "', '" + breadPrice.Text + "', '" + breadStock.Text + "', '" + discount.Text + "', '" + minBuy.Text + "', " + Convert.ToString(itemCategoryData.SelectedIndex + 1) + ", @image)"
            End If
            minBuy.Visibility = Windows.Visibility.Visible
            discount.Visibility = Windows.Visibility.Visible
            minBuyLabel.Visibility = Windows.Visibility.Visible
            discountLabel.Visibility = Windows.Visibility.Visible


        ElseIf menuIndex = 3 Then
            tableName = "Login"
            label1.Content = "Username"
            label2.Content = "Password"
            columnName = "Nama"
            columnIdName = "idPenjaga"
            insertCMD = "(nama, username, password, level) VALUES ('" + breadName.Text + "', '" + breadPrice.Text + "','" + breadStock.Text + "', '" + itemCategoryData.SelectedIndex.ToString + "') "
            saveCMD = "nama='" + breadName.Text + "', username='" + breadPrice.Text + "', password='" + password.Password + "', level='" + itemCategoryData.SelectedIndex.ToString + "'"
            minBuy.Visibility = Windows.Visibility.Collapsed
            discount.Visibility = Windows.Visibility.Collapsed
            minBuyLabel.Visibility = Windows.Visibility.Collapsed
            discountLabel.Visibility = Windows.Visibility.Collapsed

        ElseIf menuIndex = 4 Then
            tableName = "Penjualan"
            label1.Content = "Username"
            label2.Content = "Password"
            columnName = "NamaBarang"
            columnIdName = "idPenjualan"
        End If

        Using con As New SqlConnection(conString)
            If (menuIndex = 4) Then
                Dim data As New SqlDataAdapter("Select * FROM " + tableName + " WHERE " + columnName + " LIKE '" + searchBox.Text + "%' AND tanggal LIKE '" + dateSelect.DisplayDate + "%'", con)
                dataTable = New DataTable
                data.Fill(dataTable)

                masterGrid.ItemsSource = dataTable.DefaultView
            End If
            Dim data1 As New SqlDataAdapter("Select * FROM " + tableName + " WHERE " + columnName + " LIKE '" + searchBox.Text + "%'", con)
            dataTable = New DataTable
            data1.Fill(dataTable)

            masterGrid.ItemsSource = dataTable.DefaultView
        End Using

    End Sub

    Private Sub searchBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        FillMasterData()
    End Sub

    Private Sub FillDataBase()
        If String.IsNullOrWhiteSpace(imageURL.Text) Then
            insertCMD = "(nama, harga, stok, diskon, minPembelian, idJenis) VALUES ('" + breadName.Text + "', '" + breadPrice.Text + "', '" + breadStock.Text + "', '" + discount.Text + "', '" + minBuy.Text + "', " + Convert.ToString(itemCategoryData.SelectedIndex + 1) + ")"
        End If
        Using con As New SqlConnection(conString)
            Dim cmd As New SqlCommand("INSERT INTO " + tableName + " " + insertCMD, con)
            If Not IsDBNull(dlg.FileName) And menuIndex = 2 And dlg.FileName <> "" Then
                Dim stream As FileStream = File.OpenRead(dlg.FileName)
                Dim content(stream.Length) As Byte
                stream.Read(content, 0, stream.Length)
                stream.Close()

                cmd.Parameters.AddWithValue("@image", content)
            End If
            con.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub SaveDataBase()
        If String.IsNullOrWhiteSpace(imageURL.Text) Then
            saveCMD = "nama='" + breadName.Text + "',harga= '" + breadPrice.Text + "', stok ='" + breadStock.Text + "',diskon= '" + discount.Text + "', minpembelian='" + minBuy.Text + "', idJenis='" + Convert.ToString(itemCategoryData.SelectedIndex + 1) + "'"
        End If
        Using con As New SqlConnection(conString)
            Dim cmd As New SqlCommand("UPDATE " + tableName + " SET " + saveCMD + " WHERE " + columnIdName + " = " + idBarang, con)
            If Not IsDBNull(dlg.FileName) And menuIndex = 2 And dlg.FileName <> "" And String.IsNullOrWhiteSpace(imageURL.Text) Then
                Dim stream As FileStream = File.OpenRead(dlg.FileName)
                Dim content(stream.Length) As Byte
                stream.Read(content, 0, stream.Length)
                stream.Close()

                cmd.Parameters.AddWithValue("@image", content)
            End If
            con.Open()
            cmd.ExecuteNonQuery()

        End Using
        dlg.FileName = vbEmpty
    End Sub

    Private Sub save_Click(sender As Object, e As RoutedEventArgs) Handles save.Click
        FillMasterData()

        If addMode Then
            FillDataBase()
        Else
            SaveDataBase()
        End If

        FillMasterData()
        mainTab.SelectedIndex = 2
    End Sub

    Private Sub delete_Click(sender As Object, e As RoutedEventArgs) Handles delete.Click
        Using con As New SqlConnection(conString)
            Dim cmd As New SqlCommand("DELETE FROM " + tableName + " WHERE " + columnIdName + " = " + idBarang, con)
            con.Open()
            cmd.ExecuteNonQuery()
        End Using

        FillMasterData()
        mainTab.SelectedIndex = 2
    End Sub

    Private Sub itemCategoryData_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)

    End Sub

    Private Sub masterGrid_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        addMode = False
        idBarang = dataTable.Rows(masterGrid.SelectedIndex)(0)
        breadName.Text = dataTable.Rows(masterGrid.SelectedIndex)(1)
        breadPrice.Text = dataTable.Rows(masterGrid.SelectedIndex)(2)
        FillComboBox()
        mainTab.SelectedIndex = 3

        If menuIndex = 2 Then
            breadStock.Text = dataTable.Rows(masterGrid.SelectedIndex)(6)
            minBuy.Text = dataTable.Rows(masterGrid.SelectedIndex)(4)
            discount.Text = dataTable.Rows(masterGrid.SelectedIndex)(3)
            password.Visibility = Windows.Visibility.Collapsed

            Dim img As New BitmapImage
            If IsDBNull(dataTable.Rows(masterGrid.SelectedIndex)(7)) Then
                Return
            End If
            Dim content() As Byte = dataTable.Rows(masterGrid.SelectedIndex)(7)
            Using Stream As New MemoryStream(content)
                img.BeginInit()
                img.CacheOption = BitmapCacheOption.OnLoad
                img.StreamSource = Stream
                img.EndInit()

                imgPreview.Source = img
            End Using

        Else
            password.Password = dataTable.Rows(masterGrid.SelectedIndex)(3)
            itemCategoryData.SelectedIndex = dataTable.Rows(masterGrid.SelectedIndex)(4)
            password.Visibility = Windows.Visibility.Visible

            FillMasterData()
        End If
    End Sub

    Private Sub checkOut_Click(sender As Object, e As RoutedEventArgs) Handles checkOut.Click
        Using con As New SqlConnection(conString)
            Dim cmd As New SqlCommand
            con.Open()
            For i = 0 To transactionTable.Rows.Count - 1
                cmd = New SqlCommand("INSERT INTO PENJUALAN (idPenjualan, NamaBarang, harga,Jumlah, Diskon, idJenis, tanggal) VALUES (@id, @name, @price, @qty, @discount, @idjenis, @date)", con)
                cmd.Parameters.AddWithValue("@id", transactionTable.Rows(i)(0))
                cmd.Parameters.AddWithValue("@name", transactionTable.Rows(i)(1))
                cmd.Parameters.AddWithValue("@price", transactionTable.Rows(i)(2))
                cmd.Parameters.AddWithValue("@qty", transactionTable.Rows(i)(3))
                cmd.Parameters.AddWithValue("@discount", transactionTable.Rows(i)(4))
                cmd.Parameters.AddWithValue("@idJenis", transactionTable.Rows(i)(5))
                cmd.Parameters.AddWithValue("@date", Now.ToString)

                cmd.ExecuteNonQuery()
            Next
            TransactionTableFunction(4)
        End Using
    End Sub

    Private Sub moneyAmount_TextChanged(sender As Object, e As TextChangedEventArgs)
        SumTotal()
    End Sub

    Private Sub transactionGrid_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If transactionGrid.SelectedIndex < 0 Or transactionGrid.SelectedIndex > transactionTable.Rows.Count Then
            Return
        End If

        SumTotal()
        idBarang = transactionTable.Rows(transactionGrid.SelectedIndex)(0)
        itemName.Text = transactionTable.Rows(transactionGrid.SelectedIndex)(1)
        itemPrice.Text = transactionTable.Rows(transactionGrid.SelectedIndex)(2)
        itemQty.Text = transactionTable.Rows(transactionGrid.SelectedIndex)(3)
        itemCategory.SelectedIndex = transactionTable.Rows(transactionGrid.SelectedIndex)(5) - 1
    End Sub

    Private Sub addData_Click(sender As Object, e As RoutedEventArgs)
        addMode = True

        If menuIndex = 2 Then
            password.Visibility = Windows.Visibility.Collapsed
            FillMasterData()
        Else
            password.Visibility = Windows.Visibility.Visible
            FillMasterData()
        End If
        mainTab.SelectedIndex = 3
    End Sub

    Private Sub browse_Click(sender As Object, e As RoutedEventArgs)
        dlg.ShowDialog()

        imageURL.Text = dlg.FileName
        imgPreview.Source = New BitmapImage(New Uri(dlg.FileName, UriKind.RelativeOrAbsolute))
    End Sub

    Private Sub reportButton_Copy_Click(sender As Object, e As RoutedEventArgs) Handles reportButton_Copy.Click
        Dim login As New Login
        login.Show()

        Me.Close()
    End Sub

    Private Sub closeButton_Click(sender As Object, e As RoutedEventArgs) Handles closeButton.Click
        mainTab.SelectedIndex = 2
    End Sub
End Class
