Imports System.Environment
Imports System.IO
Imports System.Net

Public Class Form1

    Public Shared Function CheckForInternetConnection() As Boolean
        Try
            Using client = New WebClient()
                Using stream = client.OpenRead("http://www.google.com")
                    Return True
                End Using
            End Using
        Catch
            Return False
        End Try
    End Function

    Sub Delay(ByVal dblSecs As Double)
        'iH8Sn0w Delay
        Const OneSec As Double = 1.0# / (1440.0# * 60.0#)
        Dim dblWaitTil As Date
        Now.AddSeconds(OneSec)
        dblWaitTil = Now.AddSeconds(OneSec).AddSeconds(dblSecs)
        Do Until Now > dblWaitTil
            Application.DoEvents()
        Loop
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Button1.Enabled = False

        'Killing itunes
        Dim iTunes() As Process = Process.GetProcessesByName("itunes")
        For Each Process As Process In iTunes
            Process.Kill()
        Next

        idle.Text = "Preparing TEMP folder"
        Dim appData As String = GetFolderPath(SpecialFolder.ApplicationData)
        Dim tempdir As String = appData + "\..\Local\Temp\"

        'deleting all previous restore directories
        Dim no_more_resdirs As Boolean = False
        Do Until no_more_resdirs = True
            Dim restore_dirs As String() = Directory.GetDirectories(tempdir, "Per" + "*" + ".tmp")
            If restore_dirs.Length.ToString = 0 Then
                no_more_resdirs = True
            Else
                IO.Directory.Delete(restore_dirs(0), True)
            End If
        Loop

        'deleting all previous restore directories
        Dim no_more_fwdirs As Boolean = False
        Do Until no_more_fwdirs = True
            Dim fw_dirs As String() = Directory.GetDirectories("C:/ProgramData/Apple Computer/iTunes/", "iP" + "*")
            If fw_dirs.Length.ToString = 0 Then
                no_more_fwdirs = True
            Else
                IO.Directory.Delete(fw_dirs(0), True)
            End If
        Loop

        ' Now that all directories has been deleted, the user can start a new restore
        idle.Text = "Preparing Done, now it's iTunes turn :)"
        Button1.Visible = False
        Button2.Visible = True
        MessageBox.Show("iTunes TEMP directories have been prepared, iTunes will automatically pop up and you'll have to start a normal restore FROM DFU OR RECOVERY MODE with THE LATEST IOS VERSION. When you see the Apple Logo appearing on you device's screen, you have to be quick and click the button ""Continue with boot""")
        Process.Start("itunes")

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Button2.Enabled = False
        idle.Text = "Doing cool checks"
        'Getting dirs
        Dim appData As String = GetFolderPath(SpecialFolder.ApplicationData)
        Dim tempdir As String = appData + "\..\Local\Temp\"

        Dim refreshed_dirs As String() = Directory.GetDirectories(tempdir, "Per" + "*" + ".tmp")
        If refreshed_dirs.Length = 0 Then
            idle.Text = "Directory error!"
            MessageBox.Show("StockBooter wan't able to find the restore directory", "Missing restore directory",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Button1.Visible = True
            Button1.Enabled = True
            Button2.Visible = False
            Exit Sub
        ElseIf refreshed_dirs.Length = 1 Then
            'Ok, there's just one folder
        Else
            idle.Text = "Directory error!"
            MessageBox.Show("There are too many directories! try to relaunch the program", "Too many restore directories!",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Button1.Visible = True
            Button1.Enabled = True
            Button2.Visible = False
            Exit Sub
        End If

        Dim restore_dir As String() = Directory.GetDirectories(tempdir, "Per" + "*" + ".tmp")
        Dim bins_dir As String() = Directory.GetDirectories(restore_dir(0) + "\Firmware\all_flash\", "all_flash." + "*" + ".production")
        Dim apticket As String() = Directory.GetFiles(restore_dir(0) + "\amai\", "ap" + "*" + "ticket.der")
        Dim res_dtree As String() = Directory.GetFiles(bins_dir(0), "RestoreDeviceTree_DeviceTree." + "*")
        Dim res_kc As String() = Directory.GetFiles(restore_dir(0), "RestoreKernelCache_kernelcache.release." + "*")
        Dim apticket_tosend As String = apticket(0)
        Dim kc_tosend As String = res_kc(0)
        Dim dtree_tosend As String = res_dtree(0)
        If Path.GetExtension(res_dtree(0)) = ".ptr" Then
            dtree_tosend = File.ReadAllText(res_dtree(0))
        End If
        If Path.GetExtension(res_kc(0)) = ".ptr" Then
            kc_tosend = File.ReadAllText(res_kc(0))
        End If
        If Path.GetExtension(apticket(0)) = ".ptr" Then
            apticket_tosend = File.ReadAllText(apticket(0))
        End If

        ' Killing iTunes
        idle.Text = "Kiling iTunes"
        Dim iTunes() As Process = Process.GetProcessesByName("itunes")
        For Each Process As Process In iTunes
            Process.Kill()
        Next

        'Extracting iRecovery in the restore directory (moved to Form1 Load)
        'idle.Text = "Extracting resources"
        'Dim irec_container() As Byte = My.Resources.s_irecovery
        'IO.File.WriteAllBytes(restore_dir(0) + "\stockbooter\s-irecovery.exe", irec_container)

        'Creating iRecovery's process
        Dim irecovery As New Process()
        Try
            irecovery.StartInfo.UseShellExecute = False
            irecovery.StartInfo.FileName = appData + "\s-irecovery.exe"
            irecovery.StartInfo.CreateNoWindow = True
        Catch ex As Exception
        End Try

        ' Checking if the device is in recovery mode
        irecovery.StartInfo.RedirectStandardOutput = True
        irecovery.StartInfo.Arguments = "-mode"
        irecovery.Start()
        Dim mode As String
        Using oStreamReader As System.IO.StreamReader = irecovery.StandardOutput
            mode = oStreamReader.ReadToEnd()
        End Using
        If mode.Contains("iboot") Then
            ' Device is in recovery mode
        Else
            MessageBox.Show("StockBooter didn't find any recovery mode device connected to the computer", "No Recovery mode",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Button1.Visible = True
            Button1.Enabled = True
            Button2.Visible = False
            Exit Sub
        End If
        irecovery.StartInfo.RedirectStandardOutput = False

        ' Setting auto-boot 
        If CheckBox2.Checked = True Then
            idle.Text = "Setting auto-boot command"
            irecovery.StartInfo.Arguments = "-c " + """" + "setenv auto-boot true" + """"
            irecovery.Start()
            Do Until irecovery.HasExited = True
                Delay(1)
            Loop

            idle.Text = "saveenving"
            irecovery.StartInfo.Arguments = "-c saveenv"
            irecovery.Start()
            Do Until irecovery.HasExited = True
                Delay(1)
            Loop
        End If

        'Checking if the USB comunication works well
        Dim USBisworking As Boolean = False
        idle.Text = "Checking if the USB comunication is working fine"
        Do Until USBisworking = True
            irecovery.StartInfo.RedirectStandardOutput = True
            irecovery.StartInfo.Arguments = "-f " + """" + apticket_tosend + """"
            irecovery.Start()
            Do Until irecovery.HasExited = True
                Delay(1)
            Loop
            Dim status As String
            Using oStreamReader As System.IO.StreamReader = irecovery.StandardOutput
                status = oStreamReader.ReadToEnd()
            End Using
            If status.Contains("=") Then
                ' USB comunications works, the file has been sent
                USBisworking = True
            Else
                ' USB comunication doesn't work, the file has not been sent
                MessageBox.Show("There's a problem with USB comunication, don't panic! Just un-plug and re-plug your device again, then click OK to this message", "USB Comunication is broken",
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Loop
        irecovery.StartInfo.RedirectStandardOutput = False

        'Sending and activating apticket
        idle.Text = "Sending APTicket"
        irecovery.StartInfo.Arguments = "-f " + """" + apticket_tosend + """"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop
        irecovery.StartInfo.Arguments = "-c ticket"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop

        'Sending and activating Restore_devicetree
        idle.Text = "Sending DeviceTree"
        irecovery.StartInfo.Arguments = "-f " + """" + dtree_tosend + """"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop
        irecovery.StartInfo.Arguments = "-c devicetree"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop

        'Sending and activating apticket
        idle.Text = "Sending APTicket"
        irecovery.StartInfo.Arguments = "-f " + """" + apticket_tosend + """"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop
        irecovery.StartInfo.Arguments = "-c ticket"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop

        'Sending Restore_kc
        idle.Text = "Sending Kernel"
        irecovery.StartInfo.Arguments = "-f " + """" + kc_tosend + """"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop

        'Loading and booting kernel
        idle.Text = "Booting Kernel"
        irecovery.StartInfo.Arguments = "-c bootx"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop

        'Resetting the tool
        idle.Text = "Done! Your device is now booting into iOS"
        Delay(2)

        'Saving restore directory
        If CheckBox1.Checked = True Then
            idle.Text = "Saving restore directory"
            Dim desktop As String = GetFolderPath(SpecialFolder.Desktop)
            My.Computer.FileSystem.CopyDirectory(restore_dir(0), desktop + "\current_restore_folder_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm"), True)
            MessageBox.Show("Done! Restore directory has been saved to your Desktop")
        End If
        Button1.Visible = True
        Button1.Enabled = True
        Button2.Visible = False
        Button2.Enabled = True
        idle.Text = "To begin the magic, just click on the button below"

        'Dim all_flash_folder As String = "all_flash.n53ap.production"
        'Dim all_flash_folder_semiclean As String = Replace(all_flash_folder, "all_flash.", "")
        'Dim apboard As String = Replace(all_flash_folder_semiclean, ".production", "")
        'idle.Text = "Restore bundle is for " + apboard + " devices"
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("http://www.twitter.com/iH8Sn0w")
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Process.Start("http://www.twitter.com/p0sixninja")
    End Sub

    Private Sub LinkLabel3_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel3.LinkClicked
        Process.Start("http://www.twitter.com/blackgeektuto")
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Extracting irecovery
        Dim appData As String = GetFolderPath(SpecialFolder.ApplicationData)
        If My.Computer.FileSystem.FileExists(appData + "\s-irecovery.exe") Then
            My.Computer.FileSystem.DeleteFile(appData + "\s-irecovery.exe")
        End If
        Dim irec_container() As Byte = My.Resources.s_irecovery
        IO.File.WriteAllBytes(appData + "\s-irecovery.exe", irec_container)


        If CheckForInternetConnection() = True Then
            Dim CurrentVersion As String = 1
            Using client As New WebClient
                Dim value As String = client.DownloadString("https://raw.githubusercontent.com/BlackGeekTutorial/StockBooter/master/latestversion.txt")
                If CurrentVersion < value Then
                    Label1.Text = "A new version of StockBooter is avaible"
                    LinkLabel4.Visible = True
                Else
                    Label1.Text = "Hello, this is the latest version of StockBooter"
                End If
            End Using
        Else
            Label1.Text = "StockBooter wasn't able to look for updates, check your internet connection"
        End If
    End Sub

    Private Sub LinkLabel4_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel4.LinkClicked
        Process.Start("https://raw.githubusercontent.com/BlackGeekTutorial/StockBooter/master/StockBooter_latest.zip")
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim appData As String = GetFolderPath(SpecialFolder.ApplicationData)
        Dim irecovery As New Process()
        Try
            irecovery.StartInfo.UseShellExecute = False
            irecovery.StartInfo.FileName = appData + "\s-irecovery.exe"
            irecovery.StartInfo.CreateNoWindow = True
        Catch ex As Exception
        End Try

        'Executing autoboot command
        irecovery.StartInfo.Arguments = "-c " + """" + "setenv auto-boot true" + """"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop

        'Executing saveenv command
        irecovery.StartInfo.Arguments = "-c " + """" + "saveenv" + """"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop

        'Executing reboot command
        irecovery.StartInfo.Arguments = "-c " + """" + "reboot" + """"
        irecovery.Start()
        Do Until irecovery.HasExited = True
            Delay(1)
        Loop
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If CheckBox2.Checked = False Then
            MessageBox.Show("WARNING: unchecking this option will boot your device in Recovery Mode everytime you will power it on untill you set auto-boot back to TRUE using ""Exit Recovery Mode"" button")
        End If
    End Sub
End Class
