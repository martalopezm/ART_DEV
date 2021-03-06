'------------------------------------------------------------------------------
'
'ART General Description
'The ART tool Is a standalone application, independent controlling the execution 
'of other auxiliary standalone applications or even scripts (e.g. conversion of 
'file formats, interpolation and specific downloading procedures), adapting 
'automatically the configuration files from those applications, and launching them. 
'For these reasons ART is the “heart” of operational frameworks and currently is 
'configurated for running MOHID Water, MOHID Land and WAVEWATCH III.
'------------------------------------------------------------------------------


Imports System.IO
Imports Mohid_Base
Imports System.Net.Mail
Imports System.Timers
Imports System.Threading
Imports System.Net
Imports System.Text.RegularExpressions
Imports Microsoft.Win32
Imports IntrinsicFunctions

Module ART

#Region "Global variables"
    ''Mail 
    'Dim MailServer As String = "smtp.gmail.com"
    'Dim MailSender As String = "ART for MOHID Water"
    'Dim MailSenderAddress As String = "mailing.maretec@gmail.com"
    'Dim EmailsList() As String

    'Log
    Dim LogFile As OutData
    Dim LogFileName As String
    Dim MOHID_Run_LogFile As String
    Dim MOHID_Run_LogFile_FullPath As String
    Dim MOHID_Run_LogFile_FullPath_In_Network As String
    Dim MPI_DomainDecompositionConsolidation_LogFile As String

    Dim WW3ModelLogPath As String
    Dim WW3_Run_LogFile As String
    Dim WW3_Run_LogFile_FullPath As String
    Dim WW3_Run_LogFile_FullPath_In_Network As String

    Dim WRFLogPath As String
    Dim WRF_Run_LogFile As String
    Dim WRF_Run_LogFile_FullPath As String
    Dim WRF_Run_LogFile_FullPath_In_Network As String

    'Model domains
    Dim Models As New Collection

    'PreProcessor Tools
    Dim PreProcessors As New Collection
    Dim Run_Preprocessing As Boolean = True

    'PreProcessor Tools
    Dim PostProcessors As New Collection
    Dim Run_PostProcessing As Boolean = True

    'Model OpenMP
    Dim OpenMP As Boolean = False
    Dim OpenMP_Num_Threads As Integer = 1

    'Model MPI
    Dim MPI As Boolean = False
    Dim MPI_Num_Processors As Integer = 1
    Dim MPI_Exe_Path As String
    Dim MPI_KeepDecomposedFiles As Boolean = False
    Dim MPI_GatherHDF5DecomposedFiles As Boolean = False
    Dim MPI_Decomposition As New Collection
    Dim MPI_DDCParser_Num_Processors As Integer = 1
    Dim MPI_DDCWorker_Num_Processors As Integer = 1
    Dim MPI_Joiner_Version As String = "1"

    ' WW3
    Dim WW3FirstRun As Boolean = False
    Dim WW3_Exe As String = "ww3_shel.exe"
    Dim WW3_Exe_HandleGrid As String = "ww3_grid.exe"
    Dim WW3_Exe_FirstTime As String = "ww3_strt.exe"
    Dim WW3_Exe_HandleInputs As String = "ww3_prep.exe"
    Dim WW3_Exe_HandleFieldOutputs As String = "ww3_outf.exe"
    Dim WW3_Exe_HandlePointOutputs As String = "ww3_outp.exe"
    Dim WW3_Exe_HandleHDF5Outputs As String = "ww3_to_hdf5.exe"
    Dim WW3_NbrExes As Integer
    Dim WW3_MaxTime As Integer = 86400
    Dim WW3_ScreenOutputToFile As Boolean = False
    Dim WW3_ScreenOutputPath As String
    Dim WW3_ScreenOutputPathInNetwork As String = ""
    Dim OutputExtensionList() As String

    'WRF
    Dim WRF_MPI As Boolean = True
    Dim WRF_MPI_Num_Processors As Integer = 1
    Dim WRF_MPI_Exe_Path As String = "C:\mpich2\bin\mpiexec"
    Dim WRF_MPI_Exe_Path_Renamed As String
    Dim WRF_UnGrib As Boolean = False
    Dim WRF_MetGrid As Boolean = True
    Dim WRF_RunWRF As Boolean = True
    Dim WRF_HandleHDF5Outputs As Boolean = True
    Dim WRF_Restart As Boolean = False
    Dim WRF_Exe_UnGrib As String = "ungrib.exe"
    Dim WRF_Exe_MetGrid As String = "metgridexe"
    Dim WRF_Exe_Real As String = "real.exe"
    Dim WRF_Exe_WRF As String = "wrf.exe"
    Dim WRF_Exe_HandleHDF5Outputs As String = "ConvertToHDF5.exe"
    Dim WRF_InputIntervalSeconds As Integer
    Dim WRF_MaxTime As Integer = 86400
    Dim WRF_ScreenOutputToFile As Boolean = False
    Dim WRF_ScreenOutputPath As String
    Dim WRF_ScreenOutputPathInNetwork As String = ""
    Dim WRFModelPath As String
    Dim WRF_Ungrib_SourcePath As String
    Dim WPSFolder As String = ""
    Dim WRFFolder As String = ""
    Dim WPSDataFolder As String = ""
    'Dim WRF_NbrProcessors As Integer
    'Dim WRFResultFileName As String = ""
    'Dim WRF_MPIPath As String
    'Paths
    Dim MainPath As String
    Dim FatherModelWorkPath As String
    Dim PlotsDir As String = "Plots\"
    Dim DischargesDir As String = "Discharges\"
    Dim DischargesConfigFilename As String = "discharges.tss"
    Dim WorkingDirectory As String
    Dim LogPath As String
    Dim DriveAvailableToMap As String = "B:"
    Dim BackupDisk As New Collection
    Dim BackupDisk_FreeSpace As New Collection
    Dim BackupDisk_TotalSpace As New Collection
    Dim BackupDisk_FreeSpace_Percentage As New Collection
    Dim StorageDisk As New Collection
    Dim StorageDisk_FreeSpace As New Collection
    Dim StorageDisk_TotalSpace As New Collection
    Dim StorageDisk_FreeSpace_Percentage As New Collection
    Dim ModelDisk_FreeSpace, ModelDisk_TotalSpace, ModelDisk_FreeSpace_Percentage As Double
    Dim ModelDisk As String
    Dim CheckDiskSpace As Boolean = False
    Dim MinFreeDiskSpace As Double = 20
    Dim MinFreeDiskSpace_Percentage As Double = 1

    'Others
    Dim Run_MOHID As Boolean = True
    Dim Run_WW3 As Boolean = False
    Public Run_WRF As Boolean = False
    'Dim TimeToWaitOnCopy As Integer = 0
    'Dim IterationTimeOnCopy As Integer = 60
    Dim SoftwareLabel As String = "ART"
    Dim CreateBatchFile As Boolean = True
    Dim Trigger As Boolean = False
    Dim Trigger_FileNameToWatch As String
    Dim Trigger_Wait_Hours As Integer = 18
    Dim Trigger_Neglect_Running As Boolean = True
    Dim TriggerDir As String
    Dim TriggerFolderToWatch As String
    Dim Trigger_Watch_DaysPerRun As Integer = 1
    Dim Trigger_Delay_Days As Integer = 0
    Dim Trigger_Model_ToCheckBackup As String
    Dim MOHID_FlagForTriggerFile As String
    Dim WW3_FlagForTriggerFile As String
    Dim WRF_FlagForTriggerFile As String
    Dim RandomControl As Integer


    'Dates
    Dim GlobalInitialDate, GlobalFinalDate As Date
    Dim FinalDateStr3Days As String
    Dim DaysPerRun As String = 1
    Dim ClassicManualMode As Boolean = True
    Dim ForecastMode As Boolean = True

    'MOHID Settings
    Dim MOHID_MaxTime As Integer = 86400
    Dim MOHID_ScreenOutputToFile As Boolean = False
    Dim MOHID_ScreenOutputPath As String
    Dim MOHID_ScreenOutputPathInNetwork As String = ""
    Dim MOHID_exe As String


#End Region

#Region "Execution"
    Sub Main()



        Dim i As Integer
        AcceptableFailure = False
        ''Create log filename
        'LogFileName = "TagusROFI_" + Now.ToString("yyyy-MM-dd_HHmmss") + ".log"

        ''Create log file
        'Dim LogPath As String = Path.Combine(My.Application.Info.DirectoryPath.ToString, "\Logs")
        'LogFile = New OutData(Path.Combine(LogPath, LogFileName))

        'Read input data file and populate model domain list
        Call ReadInputFile()

        'Check License Status
        Call CheckLicenseStatus()

        'Log the options defined in the input file
        Call LogOptions()

        If Run_MOHID Or Run_WW3 Or Run_WRF Then
            ' Test if model to check backup is well-defined
            Dim Trigger_Model_ToCheckBackup_Exists As Boolean = False
            Dim LastModel As String = ""
            For Each Model As Model In Models
                LastModel = Model.Name
                If Trigger_Model_ToCheckBackup = Model.Name Then
                    Trigger_Model_ToCheckBackup_Exists = True
                End If
            Next
            If Trigger_Model_ToCheckBackup = "" Then
                Trigger_Model_ToCheckBackup = LastModel
            ElseIf Trigger_Model_ToCheckBackup <> "" Then
                If Trigger_Model_ToCheckBackup_Exists = False Then
                    Call UnSuccessfullEnd("Model to check backup doesn't exist: " + Trigger_Model_ToCheckBackup)
                End If
            End If
        End If

        If CheckDiskSpace Then
            Dim di As New System.IO.DirectoryInfo(DriveAvailableToMap)
            If di.Exists() Then
                Call UnSuccessfullEnd("Drive to map (" + DriveAvailableToMap + ") is not available")
            End If
        End If

        For i = 1 To NumberOfRuns

            If FinalDate > GlobalFinalDate Then
                Call UnSuccessfullEnd("The number of days per run (" + DaysPerRun + ") in conjunction with the initial date of this run (" + InitialDate.ToString("yyyy-MM-dd") + ") would lead to a final date of simulation beyond the user-specified end date (keyword END)")
            End If

            If CheckDiskSpace Then
                Call GetDiskSpaceinformation()
                'to(remove)
                '               Call PublishDiskSpaceInformation()
            End If

            Call StartingForecast(i)

            TriggerDir = Path.Combine(LogPath, "Triggers") + "\"
            If Not System.IO.Directory.Exists(TriggerDir) Then
                Directory.CreateDirectory(TriggerDir)
            End If

            If Trigger = True Then
                Call Trigger_()
            End If

            If Run_Preprocessing Then
                RunPreProcessingTools()
            End If
            If Run_MOHID Then

                ''to remove
                'If MPI Then
                '    'getting father domain exe folder
                '    For Each Model As Model In Models
                '        FatherModelWorkPath = MainPath + Model.Path + "exe"
                '        Exit For
                '    Next

                '    MPI_DomainDecompositionConsolidation_LogFile = "DomainDecompositionConsolidation_" + InitialDateStr + ".log"
                '    Call RunDomainDecompositionConsolidation()
                'End If
                ''--------


                Dim FlagForTriggerFilename As String = InitialDateStr + "_" + FinalDateStr + ".dat"
                MOHID_FlagForTriggerFile = Path.Combine(TriggerDir, FlagForTriggerFilename)

                If IO.File.Exists(MOHID_FlagForTriggerFile) Then
                    IO.File.Delete(MOHID_FlagForTriggerFile)
                End If

                'Write Running message to Trigger File
                Call WriteFlagForTrigger("Running", "MOHID", MOHID_FlagForTriggerFile)

                'Get the atmospheric forcing and ocean open boundary conditions files
                Call GatherBoundaryConditions()

                'Get the  flow computed offline
                Call GetDischarge()

                'Get the  flow computed offline
                Call GetDischarges()

                'Get the  timseries ets for discharges
                Call GetDischarges_ets()

                'Get the  timseries srn for discharges
                Call GetDischarges_srn()

                'Create the new Model.dat files with the correct dates for each model domain
                Call CreateNewModelFiles()

                'Rename the restart files to startup from yesterday forecast
                Call GatherRestartFiles()

                'Create the bacth file to run MOHID
                Call CreateMOHIDBatchFile()

                'Write Running message to Trigger File
                Call WriteFlagForTrigger("Running", "MOHID", MOHID_FlagForTriggerFile)

                'Run MOHID
                Call RunMOHID()

                ''Check if MOHID run was successfull and write success file
                Call CheckIfRunOK()

                If MPI Then
                    If MPI_Joiner_Version = "2" Then
                        Call RunDomainDecompositionConsolidation_MPI_Joiner_V2()
                    Else
                        Call RunDomainConsolidation_MPI_Joiner_V1()
                    End If
                End If

                'Backup and store the forecast results and restart files
                Call BackUpMOHIDSimulation()

                Call WriteFlagForTrigger("Finish", "MOHID", MOHID_FlagForTriggerFile)

            End If

            If Run_WW3 Then
                Dim FlagForTriggerFilename As String = InitialDateStr + "_" + FinalDateStr + ".dat"
                WW3_FlagForTriggerFile = Path.Combine(TriggerDir, FlagForTriggerFilename)

                If IO.File.Exists(WW3_FlagForTriggerFile) Then
                    IO.File.Delete(WW3_FlagForTriggerFile)
                End If

                'Write Running message to Trigger File
                Call WriteFlagForTrigger("Running", "WW3", WW3_FlagForTriggerFile)
                Call ConfigureDatesInWW3()
                Call RunWW3()
                Call WriteFlagForTrigger("Finish", "WW3", WW3_FlagForTriggerFile)
            End If

            If Run_WRF Then
                Dim FlagForTriggerFilename As String = InitialDateStr + "_" + FinalDateStr + ".dat"
                WRF_FlagForTriggerFile = Path.Combine(TriggerDir, FlagForTriggerFilename)

                If IO.File.Exists(WRF_FlagForTriggerFile) Then
                    IO.File.Delete(WRF_FlagForTriggerFile)
                End If

                'Write Running message to Trigger File
                Call WriteFlagForTrigger("Running", "WRF", WRF_FlagForTriggerFile)

                Call InitializeVariables()
                Call PrepareConfigFiles()
                Call RunUnGrib()
                Call RunMetgrid()
                Call RunWRF()

                'Backup and store the forecast results and restart files
                Call BackupWRFSimulation()

                Call WriteFlagForTrigger("Finish", "WRF", WRF_FlagForTriggerFile)


            End If
            'Make plots
            'temporarily commented, to use same procedure as PCOMS-BIO
            'Call MakePlots()

            If Run_PostProcessing Then
                ManagePostProcessingTools()
            End If

            'Sucessfull end of the run and / or the execution
            Call SuccessfullEnd(i)


            If ClassicManualMode = False Then
                InitialDate = InitialDate.AddDays(1)
                FinalDate = FinalDate.AddDays(1)
            End If
        Next


    End Sub
    Function CheckTrigger() As Boolean
        CheckTrigger = False
        Dim dir_ As String = TriggerFolderToWatch
        Dim filename_ As String
        Dim path_ As String
        Dim n As Integer
        For n = 0 To -Trigger_Watch_DaysPerRun + 1 + Trigger_Delay_Days Step -1
            filename_ = InitialDate.AddDays(n).ToString("yyyy-MM-dd") + "_" + InitialDate.AddDays(n + Trigger_Watch_DaysPerRun).ToString("yyyy-MM-dd") + ".dat"
            path_ = Path.Combine(dir_, filename_)
            InitialDate.AddDays(n)
            If File.Exists(path_) Then
                CheckTrigger = True
                Trigger_FileNameToWatch = path_
                Exit For
            End If
        Next

    End Function

    Sub ReadInputFile()
        Dim bolkeyword As Boolean = False
        Dim RefDay_toStart As Integer = 0
        WorkingDirectory = Directory.GetCurrentDirectory

        If Not IO.File.Exists(Path.Combine(WorkingDirectory, SoftwareLabel + ".dat")) Then
            WorkingDirectory = App_Path()
            Directory.SetCurrentDirectory(WorkingDirectory)
        End If

        ' log filename
        LogFileName = SoftwareLabel + "_" + Now.ToString("yyyy-MM-dd_HHmmss") + ".log"

        If IO.File.Exists(Path.Combine(WorkingDirectory, SoftwareLabel + ".dat")) Then

            Dim InputFile As New EnterData(Path.Combine(WorkingDirectory, SoftwareLabel + ".dat"))


            Try
                InputFile.GetDataStr("MAIN_PATH", MainPath)
                If Not Directory.Exists(MainPath) Then
                    MainPath = App_Path()
                End If
            Catch ex As Exception
                MainPath = App_Path()
            End Try

            'Create log file
            LogPath = Path.Combine(MainPath, "Logs")
            If Not Directory.Exists(LogPath) Then
                Try
                    Directory.CreateDirectory(LogPath)
                Catch
                    LogPath = MainPath
                End Try
            End If

            LogFileName = Path.Combine(LogPath, LogFileName)
            LogFile = New OutData(LogFileName)

            LogThis("Found input data file! Reading...", "", LogFile)

            InputFile.GetDataStr("DRIVE_AVAILABLE_TO_MAP", DriveAvailableToMap)
            DriveAvailableToMap.Replace("\", "")

            InputFile.GetDataStr("PLOTS_FOLDER", PlotsDir)
            InputFile.GetDataStr("DISCHARGES_FOLDER", DischargesDir)

            InputFile.GetDataLog("TRIGGER", Trigger)
            If Trigger = True Then
                InputFile.GetDataStr("TRIGGER_FOLDERTOWATCH", TriggerFolderToWatch)
                InputFile.GetDataInteger("TRIGGER_WAIT_HOURS", Trigger_Wait_Hours)
                InputFile.GetDataLog("TRIGGER_NEGLECT_RUNNING", Trigger_Neglect_Running)
                InputFile.GetDataInteger("TRIGGER_WATCH_DAYS_PER_RUN", Trigger_Watch_DaysPerRun)
                InputFile.GetDataInteger("TRIGGER_DELAY_DAYS", Trigger_Delay_Days)
            End If
            InputFile.GetDataStr("TRIGGER_MODELTOCHECKBACKUP", Trigger_Model_ToCheckBackup)

            InputFile.GetDataLog("CHECK_DISK_SPACE", CheckDiskSpace)
            If CheckDiskSpace Then
                InputFile.GetDataReal("MIN_FREE_DISK_SPACE", MinFreeDiskSpace)
                InputFile.GetDataReal("MIN_FREE_DISK_SPACE_PERCENTAGE", MinFreeDiskSpace_Percentage)
            End If


            InputFile.GetDataLog("RUN_MOHID", Run_MOHID)
            InputFile.GetDataLog("RUN_WW3", Run_WW3)
            InputFile.GetDataLog("RUN_WRF", Run_WRF)
            InputFile.GetDataLog("RUN_PREPROCESSING", Run_Preprocessing)
            InputFile.GetDataLog("RUN_POSTPROCESSING", Run_PostProcessing)

            Call Read_Instrinsic_Variables(InputFile, LogFile, LogFileName)

            InputFile.GetDataLog("CLASSIC_MANUAL_MODE", ClassicManualMode)
            Dim NowDate As Date
            If Not ClassicManualMode Then
                InputFile.GetDataInteger("DAYS_PER_RUN", DaysPerRun)
                InputFile.GetDataInteger("FORECAST_MODE", ForecastMode)
                If ForecastMode = True Then ' This mode is to run multiple runs in forecast mode (based on actual date)

                    NowDate = New Date(Now.Date.Year, Now.Date.Month, Now.Date.Day, 0, 0, 0)

                    InputFile.GetDataInteger("NUMBER_OF_RUNS", NumberOfRuns)
                    InputFile.GetDataInteger("REFDAY_TO_START", RefDay_toStart)

                    GlobalInitialDate = NowDate.AddDays(RefDay_toStart)
                    GlobalFinalDate = GlobalInitialDate.AddDays(NumberOfRuns).AddDays(DaysPerRun - 1)

                    InitialDate = GlobalInitialDate
                    FinalDate = InitialDate.AddDays(DaysPerRun)
                ElseIf ForecastMode = False Then ' This mode is used to run multiple runs in hindcast mode
                    Try
                        Dim Difference As TimeSpan
                        InputFile.GetDataTime("START", GlobalInitialDate)
                        InputFile.GetDataTime("END", GlobalFinalDate)
                        Difference = GlobalFinalDate.Subtract(GlobalInitialDate)
                        NumberOfRuns = Difference.Days
                    Catch ex As Exception
                        'if the dates are not given in the file, it assumes today as the date and a forecast of 3 days
                        GlobalInitialDate = NowDate
                        GlobalFinalDate = GlobalInitialDate.AddDays(DaysPerRun)
                        NumberOfRuns = 1
                    End Try
                End If

                InitialDate = GlobalInitialDate
                FinalDate = InitialDate.AddDays(DaysPerRun)

            ElseIf ClassicManualMode = True Then ' this mode is used to make a single run, in the traditional way
                NumberOfRuns = 1
                Try
                    InputFile.GetDataTime("START", GlobalInitialDate)
                    InputFile.GetDataTime("END", GlobalFinalDate)
                Catch ex As Exception
                    'if the dates are not given in the file, it assumes today as the date and a forecast of 3 days
                    GlobalInitialDate = New Date(Now.Date.Year, Now.Date.Month, Now.Date.Day, 0, 0, 0)
                    GlobalFinalDate = InitialDate.AddDays(DaysPerRun)
                End Try
                InitialDate = GlobalInitialDate
                FinalDate = GlobalFinalDate
            End If

            'TO REMOVE IN THE FUTURE; NOW IT IS DEFINED INSIDE MOHID SETTINGS BLOCK
            InputFile.GetDataLog("OPENMP", OpenMP)

            If OpenMP Then
                InputFile.GetDataInteger("OPENMP_NUM_THREADS", OpenMP_Num_Threads)
            End If

            InputFile.GetDataLog("MPI", MPI)

            If MPI Then
                InputFile.GetDataStr("MPI_EXE_PATH", MPI_Exe_Path)
                InputFile.GetDataLog("MPI_KEEP_DECOMPOSED_FILES", MPI_KeepDecomposedFiles)
                InputFile.GetDataLog("MPI_GATHER_HDF5_DECOMPOSED_FILES", MPI_GatherHDF5DecomposedFiles)
                InputFile.GetDataInteger("MPI_DDC_PARSER_NUM_PROCESSORS", MPI_DDCParser_Num_Processors)
                InputFile.GetDataInteger("MPI_DDC_WORKER_NUM_PROCESSORS", MPI_DDCWorker_Num_Processors)
                InputFile.GetDataStr("MPI_JOINER_VERSION", MPI_Joiner_Version)


            End If

            If OpenMP And MPI Then
                Call UnSuccessfullEnd("OpenMP and MPI cannot be used simultaneously at the moment")
            End If

            'Convert initial and final dates to string
            '            InitialDateStr = InitialDate.ToString("yyyy-MM-dd")
            '           FinalDateStr = FinalDate.ToString("yyyy-MM-dd")
            'FinalDateStr = FinalDate.AddDays(2).ToString("yyyy-MM-dd")

            DaysPerRun = FinalDate.Subtract(InitialDate).TotalDays

            Call Read_Mohid_Settings_List(InputFile)
            Call Read_WW3_Settings_List(InputFile)
            Call Read_WRF_Settings_List(InputFile)
            Call Read_PreProcessing_List(InputFile)
            Call Read_PostProcessing_List(InputFile)
            Call Read_Model_List(InputFile)

        Else
            'Create log file
            LogPath = Path.Combine(WorkingDirectory, "Logs")
            If Not Directory.Exists(LogPath) Then
                Try
                    Directory.CreateDirectory(LogPath)
                Catch
                    LogPath = WorkingDirectory
                End Try
            End If

            LogFileName = Path.Combine(LogPath, LogFileName)
            LogFile = New OutData(LogFileName)

            ReDim EmailsList(0)
            EmailsList(0) = MailSenderAddress

            Call UnSuccessfullEnd("Input data file was not found: " + Path.Combine(WorkingDirectory, SoftwareLabel + ".dat"))
        End If


    End Sub

    Sub Trigger_()
        Dim InitialTriggerTime As DateTime = Now
        Dim TimeToTrigger As TimeSpan
        LogThis("Trigger function - Waiting...")
        Dim Trigger_OkToGo As String = False
        Dim Status As String

        While Trigger_OkToGo = False

            TimeToTrigger = Now - InitialTriggerTime
            If TimeToTrigger.Hours >= Trigger_Wait_Hours Then
                LogThis("Time to Trigger too long (" + TimeToTrigger.Hours.ToString + " hours). ART will now skip trigger function and try to proceed anyway.")
                Exit While
            End If

            If CheckTrigger() = True Then

                If Not Trigger_Neglect_Running Then
                    Try
                        Dim TriggerFile As New EnterData(Trigger_FileNameToWatch)
                        TriggerFile.GetDataStr("STATUS", Status)
                        If Status.ToUpper = "RUNNING" Then
                            Trigger_OkToGo = False
                            TriggerFile = Nothing
                            Thread.Sleep(10000)
                        ElseIf Status = "FINISHED" Then
                            Trigger_OkToGo = True
                            TriggerFile = Nothing
                            LogThis("Trigger function - trigger activated! (after " + TimeToTrigger.Hours.ToString + " hours and " + TimeToTrigger.Minutes.ToString + " minutes).")
                            Exit While
                        End If
                    Catch ex As IOException
                        LogThis("Trigger file is being used by another process....testing in 5 seconds...")
                        Trigger_OkToGo = False
                        Thread.Sleep(5000)
                    End Try

                Else
                    Trigger_OkToGo = True
                    LogThis("Trigger function - trigger activated! (after " + TimeToTrigger.Hours.ToString + " hours and " + TimeToTrigger.Minutes.ToString + " minutes).")
                    Exit While
                End If
            Else
                Thread.Sleep(10000)
            End If
        End While

    End Sub
    Sub GatherBoundaryConditions(Optional ByVal modelinput As Model = Nothing)
        If Run_WRF Then
            LogThis("Gathering atmospheric forcing files...")


        ElseIf Run_WW3 Then
            'prepare Meteo files
            If modelinput.HasMeteo Then
                Dim MeteoFile As String = ""
                Dim Destination_MeteoFile As String = ""
                Dim DestinationFilename As String = ""
                Call GetMeteoFileName(modelinput, MeteoFile, 1, ".inp")

                LogThis("Gathering atmospheric forcing files...")
                If Not File.Exists(MeteoFile) Then
                    ' if meteofile from meteo solution 1 doesn't exist, 
                    ' meteofile from solution 2 is searched
                    LogThis("Meteo File from Solution 1 (" + MeteoFile + ")  not available.")
                    If modelinput.HasMeteo2 Then
                        MeteoFile = ""
                        Destination_MeteoFile = ""
                        Call GetMeteoFileName(modelinput, MeteoFile, 2, ".inp")

                        If File.Exists(MeteoFile) Then
                            LogThis("Gathering Meteo File " + MeteoFile + " from Solution 2")
                        Else

                            LogThis("Meteo File from Solution 2 (" + MeteoFile + ")  not available.")
                            If modelinput.HasMeteo3 Then
                                MeteoFile = ""
                                Destination_MeteoFile = ""
                                Call GetMeteoFileName(modelinput, MeteoFile, 3, ".inp")

                                If File.Exists(MeteoFile) Then
                                    LogThis("Gathering Meteo File " + MeteoFile + " from Solution 3")
                                Else
                                    LogThis("Meteo File from Solution 3 (" + MeteoFile + ")  not available.")
                                End If
                            Else
                                LogThis("Meteo Solution 3 is not configured")
                            End If

                        End If
                    Else
                        LogThis("Meteo Solution 2 is not configured")
                    End If
                Else
                    LogThis("Gathering Meteo File from Solution 1 (" + MeteoFile + ")")
                End If
                Destination_MeteoFile = Path.Combine(MainPath, modelinput.Path)
                '                DestinationFilename = modelinput.MeteoModelName + ".inp"
                DestinationFilename = "Atmosphere" + ".inp"
                Destination_MeteoFile = Path.Combine(Destination_MeteoFile, DestinationFilename)
                FileCopy(MeteoFile, Destination_MeteoFile, "Could not copy the atmospheric forcing file - " + modelinput.Name, True)

            End If

            If modelinput.HasCurrents Then
                Dim CurrentsFile As String = ""
                Dim Destination_CurrentsFile As String = ""
                Dim DestinationFilename As String = ""
                Call GetCurrentsFileName(modelinput, CurrentsFile, 1, ".inp")

                LogThis("Gathering currents forcing files...")
                Destination_CurrentsFile = Path.Combine(MainPath, modelinput.Path)
                DestinationFilename = "currents.inp"
                Destination_CurrentsFile = Path.Combine(Destination_CurrentsFile, DestinationFilename)
                FileCopy(CurrentsFile, Destination_CurrentsFile, "Could not copy the currents forcing file - " + modelinput.Name, True)
            End If

            If modelinput.HasWaterLevel Then
                Dim WaterLevelFile As String = ""
                Dim Destination_WaterLevelFile As String = ""
                Dim DestinationFilename As String = ""
                Call GetWaterLevelFileName(modelinput, WaterLevelFile, 1, ".inp")

                LogThis("Gathering water level forcing files...")
                Destination_WaterLevelFile = Path.Combine(MainPath, modelinput.Path)
                DestinationFilename = "water_level.inp"
                Destination_WaterLevelFile = Path.Combine(Destination_WaterLevelFile, DestinationFilename)
                FileCopy(WaterLevelFile, Destination_WaterLevelFile, "Could not copy the water level forcing file - " + modelinput.Name, True)
            End If

        ElseIf Run_MOHID Then
            LogThis("Gathering boundary conditions files...")
            For Each Model As Model In Models

                'prepare Meteo files
                If Model.HasMeteo Then
                    Dim MeteoFile As String = ""
                    Dim Destination_MeteoFile As String = ""
                    Call GetMeteoFileName(Model, MeteoFile, 1)

                    If Not File.Exists(MeteoFile) Then
                        ' if meteofile from meteo solution 1 doesn't exist, 
                        ' meteofile from solution 2 is searched
                        LogThis("Meteo File from Solution 1 (" + MeteoFile + ")  not available.")
                        If Model.HasMeteo2 Then
                            MeteoFile = ""
                            Destination_MeteoFile = ""
                            Call GetMeteoFileName(Model, MeteoFile, 2)
                            If File.Exists(MeteoFile) Then
                                LogThis("Gathering Meteo File " + MeteoFile + " from Solution 2")
                            End If
                        Else
                            LogThis("Meteo Solution 2 is not configured")
                        End If
                    Else
                        LogThis("Gathering Meteo File from Solution 1 (" + MeteoFile + ")")
                    End If

                    Destination_MeteoFile = Path.Combine(MainPath, "GeneralData\BoundaryConditions\Atmosphere\" + Model.Name + "\" + Model.MeteoModelName + "\" + Model.MeteoModelName + "_" + Model.Name + ".hdf5")
                    FileCopy(MeteoFile, Destination_MeteoFile, "Could not copy the atmospheric forcing file - " + Model.Name, True)


                End If

                'copy OBC files
                If Model.HasOBC Or Model.HasSolutionFromFile Then
                    Dim FolderLabel As String

                    Dim OBCFinalDateStr As String
                    Dim OBCBasePath As String
                    Dim OBCHydroFile As String
                    Dim OBCWaterFile As String
                    Dim InitialDate_ As Date
                    Dim InitialDateStr_ As String
                    Dim OBCSimulationsAvailable As Integer
                    Dim OBCFilesCopied As Boolean = False
                    'Dim OBCBasePath_Prev As String
                    'Dim InitialDate_Prev As Date
                    'Dim InitialDateStr_Prev As String
                    'Dim OBCFinalDateStr_Prev As String
                    Dim OBCMercatorFile As String

                    If Model.OBCFromMercator = True Then


                        OBCSimulationsAvailable = DaysPerRun - Model.OBCSimulatedDays

                        For n = 0 To OBCSimulationsAvailable Step -1
                            InitialDate_ = InitialDate.AddDays(n)
                            InitialDateStr_ = InitialDate_.ToString("yyyy-MM-dd")
                            OBCFinalDateStr = InitialDate_.AddDays(Model.OBCSimulatedDays).ToString("yyyy-MM-dd")

                            If Model.OBCFromMercator Then
                                FolderLabel = "BoundaryConditions\Hydrodynamics\"
                                OBCBasePath = Model.OBCWorkPath + "\MERCATOR_" + Model.Name + "_" + InitialDateStr_ + "_" + OBCFinalDateStr + ".hdf5"
                                If File.Exists(OBCBasePath) Then
                                    OBCMercatorFile = OBCBasePath
                                    FileCopy(OBCMercatorFile, MainPath + "GeneralData\" + FolderLabel + Model.Name + "\MERCATOR_" + Model.Name + ".hdf5", "Could not copy the window file - " + Model.Name, True)
                                    OBCFilesCopied = True
                                    Exit For
                                End If
                            End If
                        Next

                        If OBCFilesCopied = False Then
                            Dim Message1, Messagebody As String
                            Message1 = "Could not copy one or more OBC files - " + Model.Name
                            '                    Messagebody = "Could not copy one or more window files - " + Model.Name + " (could not copy files from " + Model.OBCWorkPath + " that fit the running period: from " + InitialDateStr + " to " + FinalDateStr
                            Messagebody = "Could not copy one or more OBC files - " + Model.Name + " (could not copy files from " + OBCBasePath + " that fit the running period: from " + InitialDateStr + " to " + FinalDateStr
                            UnSuccessfullEnd(Message1, Messagebody)
                        End If


                    ElseIf Model.OBCFromMyOcean = True Then

                        Dim OBCMyOceanFile As String

                        OBCSimulationsAvailable = DaysPerRun - Model.OBCSimulatedDays
                        For n = 0 To OBCSimulationsAvailable Step -1
                            InitialDate_ = InitialDate.AddDays(n)
                            InitialDateStr_ = InitialDate_.ToString("yyyy-MM-dd")
                            OBCFinalDateStr = InitialDate_.AddDays(Model.OBCSimulatedDays).ToString("yyyy-MM-dd")

                            If Model.OBCFromMyOcean Then
                                FolderLabel = "BoundaryConditions\Hydrodynamics\"
                                OBCBasePath = Model.OBCWorkPath + "\MYOCEAN_" + Model.Name + "_" + InitialDateStr_ + "_" + OBCFinalDateStr + ".hdf5"
                                If File.Exists(OBCBasePath) Then
                                    OBCMyOceanFile = OBCBasePath
                                    FileCopy(OBCMyOceanFile, MainPath + "GeneralData\" + FolderLabel + Model.Name + "\MYOCEAN_" + Model.Name + ".hdf5", "Could not copy the window file - " + Model.Name, True)
                                    OBCFilesCopied = True
                                    Exit For
                                End If
                            End If
                        Next

                        If OBCFilesCopied = False Then
                            Dim Message1, Messagebody As String
                            Message1 = "Could not copy one or more OBC files - " + Model.Name
                            '                    Messagebody = "Could not copy one or more window files - " + Model.Name + " (could not copy files from " + Model.OBCWorkPath + " that fit the running period: from " + InitialDateStr + " to " + FinalDateStr
                            Messagebody = "Could not copy one or more OBC files - " + Model.Name + " (could not copy files from " + OBCBasePath + " that fit the running period: from " + InitialDateStr + " to " + FinalDateStr
                            UnSuccessfullEnd(Message1, Messagebody)
                        End If

                    Else ' Imposed solution
                        OBCSimulationsAvailable = DaysPerRun - Model.OBCSimulatedDays
                        For n = 0 To OBCSimulationsAvailable Step -1

                            InitialDate_ = InitialDate.AddDays(n)
                            InitialDateStr_ = InitialDate_.ToString("yyyy-MM-dd")
                            OBCFinalDateStr = InitialDate_.AddDays(Model.OBCSimulatedDays).ToString("yyyy-MM-dd")

                            If Model.HasSolutionFromFile Then
                                FolderLabel = "BoundaryConditions\Hydrodynamics\"

                                '                            FolderLabel = "ImposedSolution\"
                                OBCBasePath = Model.OBCWorkPath + InitialDateStr_ + "_" + OBCFinalDateStr
                                If File.Exists(OBCBasePath + "\Hydrodynamic" + Model.BoundaryFile_Suffix + ".hdf5") Then
                                    OBCFilesCopied = False
                                    OBCHydroFile = OBCBasePath + "\Hydrodynamic" + Model.BoundaryFile_Suffix + ".hdf5"
                                    FileCopy(OBCHydroFile, MainPath + "GeneralData\" + FolderLabel + Model.Name + "\Hydrodynamic" + Model.BoundaryFile_Suffix + ".hdf5", "Could not copy the window file - " + Model.Name, True)
                                    OBCFilesCopied = True
                                    Exit For
                                End If
                            End If

                        Next
                        For n = 0 To OBCSimulationsAvailable Step -1

                            InitialDate_ = InitialDate.AddDays(n)
                            InitialDateStr_ = InitialDate_.ToString("yyyy-MM-dd")
                            OBCFinalDateStr = InitialDate_.AddDays(Model.OBCSimulatedDays).ToString("yyyy-MM-dd")

                            If Model.HasSolutionFromFile Then
                                FolderLabel = "BoundaryConditions\Hydrodynamics\"

                                '                            FolderLabel = "ImposedSolution\"
                                OBCBasePath = Model.OBCWorkPath + InitialDateStr_ + "_" + OBCFinalDateStr
                                If File.Exists(OBCBasePath + "\WaterProperties" + Model.BoundaryFile_Suffix + ".hdf5") Then
                                    OBCFilesCopied = False
                                    OBCWaterFile = OBCBasePath + "\WaterProperties" + Model.BoundaryFile_Suffix + ".hdf5"
                                    FileCopy(OBCWaterFile, MainPath + "GeneralData\" + FolderLabel + Model.Name + "\WaterProperties" + Model.BoundaryFile_Suffix + ".hdf5", "Could not copy the window file - " + Model.Name, True)
                                    OBCFilesCopied = True
                                    Exit For
                                End If
                            End If

                        Next

                    End If


                    If OBCFilesCopied = False Then
                        Dim Message1, Messagebody As String
                        Message1 = "Could not copy one or more OBC files - " + Model.Name
                        '                    Messagebody = "Could not copy one or more window files - " + Model.Name + " (could not copy files from " + Model.OBCWorkPath + " that fit the running period: from " + InitialDateStr + " to " + FinalDateStr
                        Messagebody = "Could not copy one or more OBC files - " + Model.Name + " (could not copy files from " + OBCBasePath + " that fit the running period: from " + InitialDateStr + " to " + FinalDateStr
                        UnSuccessfullEnd(Message1, Messagebody)
                    End If

                    'OBCFinalDateStr = InitialDate.AddDays(Model.OBCSimulatedDays).ToString("yyyy-MM-dd")
                    'OBCHydroFile = Model.OBCWorkPath + InitialDateStr + "_" + OBCFinalDateStr + "\Hydrodynamic_w1.hdf5"
                    'OBCWaterFile = Model.OBCWorkPath + InitialDateStr + "_" + OBCFinalDateStr + "\WaterProperties_w1.hdf5"

                    'FileCopy(OBCHydroFile, MainPath + "GeneralData\BoundaryConditions\Hydrodynamics\" + Model.Name + "\Hydrodynamic_w1.hdf5", "Could not copy the window file - " + Model.Name, True)
                    'FileCopy(OBCWaterFile, MainPath + "GeneralData\BoundaryConditions\Hydrodynamics\" + Model.Name + "\WaterProperties_w1.hdf5", "Could not copy the window file - " + Model.Name, True)

                End If

            Next
        End If

    End Sub
    Sub GetCurrentsFileName(ByVal Model As Model, ByRef CurrentsFile As String, ByVal CurrentsSolutionIndex As Integer, Optional ByVal extension As String = ".inp")
        Dim CurrentsFinalDateStr As String
        Dim CurrentsModelName As String
        Dim CurrentsSimulatedDays As Integer
        Dim CurrentsWorkPath As String

        Select Case CurrentsSolutionIndex
            Case 1
                CurrentsModelName = Model.CurrentsModelName
                CurrentsWorkPath = Model.CurrentsWorkPath
                CurrentsSimulatedDays = Model.CurrentsSimulatedDays
        End Select

        ' if CURRENTS_SIMULATED_DAYS is not defined (assuming the default value of -99, then final date will be FinalDateStr 
        If CurrentsSimulatedDays = -99 Then
            CurrentsFinalDateStr = FinalDateStr
        Else
            CurrentsFinalDateStr = InitialDate.AddDays(CurrentsSimulatedDays).ToString("yyyy-MM-dd")
        End If

        CurrentsFile = CurrentsWorkPath + CurrentsModelName + "_" + "Currents" + "_" + InitialDateStr + "_" + CurrentsFinalDateStr + extension

    End Sub

    Sub GetWaterLevelFileName(ByVal Model As Model, ByRef WaterLevelFile As String, ByVal WaterLevelSolutionIndex As Integer, Optional ByVal extension As String = ".inp")
        Dim WaterLevelFinalDateStr As String
        Dim WaterLevelModelName As String
        Dim WaterLevelSimulatedDays As Integer
        Dim WaterLevelWorkPath As String

        Select Case WaterLevelSolutionIndex
            Case 1
                WaterLevelModelName = Model.WaterLevelModelName
                WaterLevelWorkPath = Model.WaterLevelWorkPath
                WaterLevelSimulatedDays = Model.WaterLevelSimulatedDays
        End Select

        ' if CURRENTS_SIMULATED_DAYS is not defined (assuming the default value of -99, then final date will be FinalDateStr 
        If WaterLevelSimulatedDays = -99 Then
            WaterLevelFinalDateStr = FinalDateStr
        Else
            WaterLevelFinalDateStr = InitialDate.AddDays(WaterLevelSimulatedDays).ToString("yyyy-MM-dd")
        End If

        WaterLevelFile = WaterLevelWorkPath + WaterLevelModelName + "_" + "Water_Level" + "_" + InitialDateStr + "_" + WaterLevelFinalDateStr + extension

    End Sub

    Sub GetMeteoFileName(ByVal Model As Model, ByRef MeteoFile As String, ByVal MeteoSolutionIndex As Integer, Optional ByVal extension As String = ".hdf5")
        Dim MeteoFinalDateStr As String
        Dim MeteoModelName As String
        Dim MeteoFileSuffix As String
        Dim MeteoFileName_fromModel As Boolean
        Dim MeteoSimulatedDays As Integer
        Dim MeteoWorkPath As String
        Dim MeteoGenericFilename As Boolean

        Select Case MeteoSolutionIndex
            Case 1
                MeteoFileName_fromModel = Model.MeteoFileName_fromModel
                MeteoModelName = Model.MeteoModelName
                MeteoWorkPath = Model.MeteoWorkPath
                MeteoSimulatedDays = Model.MeteoSimulatedDays
                MeteoGenericFilename = Model.MeteoGenericFileName
            Case 2
                MeteoFileName_fromModel = Model.Meteo2FileName_fromModel
                MeteoModelName = Model.Meteo2ModelName
                MeteoWorkPath = Model.Meteo2WorkPath
                MeteoSimulatedDays = Model.Meteo2SimulatedDays
                MeteoGenericFilename = Model.Meteo2GenericFileName
            Case 3
                MeteoFileName_fromModel = Model.Meteo3FileName_fromModel
                MeteoModelName = Model.Meteo3ModelName
                MeteoWorkPath = Model.Meteo3WorkPath
                MeteoSimulatedDays = Model.Meteo3SimulatedDays
                MeteoGenericFilename = Model.Meteo3GenericFileName
        End Select
        If MeteoFileName_fromModel = True Then
            MeteoFileSuffix = Model.Name
        Else
            MeteoFileSuffix = "Tagus3D"
        End If
        'If MeteoModelName = "" Then
        '    If InStr(MeteoWorkPath, "WRF") > 0 Then
        '        MeteoModelName = "WRF"
        '    ElseIf InStr(MeteoWorkPath, "MM5") > 0 Then
        '        MeteoModelName = "MM5"
        '    ElseIf InStr(MeteoWorkPath, "GFS") > 0 Then
        '        MeteoModelName = "GFS"
        '    End If
        'End If

        ' if METEO_SIMULATED_DAYS is not defined (assuming the default value of -99, then final date will be FinalDateStr 
        If MeteoSimulatedDays = -99 Then
            MeteoFinalDateStr = FinalDateStr
        Else
            MeteoFinalDateStr = InitialDate.AddDays(MeteoSimulatedDays).ToString("yyyy-MM-dd")
        End If

        If MeteoFileName_fromModel = False And MeteoGenericFilename = True Then
            MeteoFile = Path.Combine(MeteoWorkPath, "meteo" + "_" + InitialDateStr + "_" + MeteoFinalDateStr + extension)
        Else
            MeteoFile = Path.Combine(MeteoWorkPath, MeteoModelName + "_" + MeteoFileSuffix + "_" + InitialDateStr + "_" + MeteoFinalDateStr + extension)
        End If

    End Sub
    Sub InterpolateMeteoForcing(ByVal Model As Model)

        Dim InterpolationInputDataFile As String = Model.MeteoWorkPath + "ConvertToHDF5Action.dat"
        Dim FinalDate3DaysStr As String = InitialDate.AddDays(3).ToString("yyyy-MM-dd")
        Dim DatabaseMeteoFile As String = "..\Data\MM5_Portugal_" + InitialDateStr + "_" + FinalDate3DaysStr + ".hdf5"

        If IO.File.Exists(InterpolationInputDataFile) Then
            IO.File.Delete(InterpolationInputDataFile)
        End If

        LogThis("Writing meteo interpolator input file...")

        Dim InterpolationInputFile As New OutData(InterpolationInputDataFile)

        InterpolationInputFile.WriteDataLine("<begin_file>")
        InterpolationInputFile.WriteDataLine("ACTION", "INTERPOLATE GRIDS")
        InterpolationInputFile.WriteDataLine("TYPE_OF_INTERPOLATION", 3)
        InterpolationInputFile.WriteDataLine("START", InitialDate)
        InterpolationInputFile.WriteDataLine("END", FinalDate)
        InterpolationInputFile.WriteDataLine("INTERPOLATION_WINDOW", "-10.5 37.5 -8.5 39.5")
        InterpolationInputFile.WriteDataLine("FATHER_FILENAME", DatabaseMeteoFile)
        InterpolationInputFile.WriteDataLine("FATHER_GRID_FILENAME", "Portugal.dat")
        InterpolationInputFile.WriteDataLine("NEW_GRID_FILENAME", Model.GridFile)
        InterpolationInputFile.WriteDataLine("OUTPUTFILENAME", "MM5_" + Trim(Model.Name) + ".hdf5")
        InterpolationInputFile.WriteDataLine("INTERPOLATION3D", False)
        InterpolationInputFile.WriteDataLine("<end_file>")
        InterpolationInputFile.Finish()

        LogThis("Done!")

        LogThis("Creating batch to interpolate MM5 to MOHID grid!")
        Dim InterpolationBatch As New OutData(Model.MeteoWorkPath + "Interpolation.bat")
        InterpolationBatch.WriteDataLine("ConvertToHDF5.exe > Interpolation.log")
        InterpolationBatch.Finish()

        LogThis("Running interpolator...")

        Dim Interpolation As Process = New Process
        Interpolation.StartInfo.FileName = Model.MeteoWorkPath + "Interpolation.bat"
        Interpolation.StartInfo.WorkingDirectory = Model.MeteoWorkPath
        Interpolation.StartInfo.WindowStyle = ProcessWindowStyle.Normal
        Interpolation.Start()
        Interpolation.WaitForExit()

        LogThis("Done!")
        LogThis("Checking if interpolation was sucessfull...")

        If Not ProgramWasSuccessfull(Model.MeteoWorkPath + "Interpolation.log", "Program ConvertToHDF5 successfully terminated") Then
            Call UnSuccessfullEnd("Error interpolating MM5 to MOHID grid!")
        Else
            LogThis("MM5 files successfully interpolated!")
        End If

        FileCopy(Model.MeteoWorkPath + "MM5_" + Trim(Model.Name) + ".hdf5", MainPath + "GeneralData\BoundaryConditions\Atmosphere\" + Model.Name + "\MM5_" + Trim(Model.Name) + ".hdf5", "Could not copy interpolated MM5 file to output folder", False)

        Dim BackUpDir As String = Model.BackUpPath + "Meteo\" + InitialDateStr + "_" + FinalDateStr + "\"
        IO.Directory.CreateDirectory(BackUpDir)
        FileCopy(Model.MeteoWorkPath + "MM5_" + Trim(Model.Name) + ".hdf5", BackUpDir + Trim(Model.Name) + ".hdf5", "Could not copy interpolated MM5 file to backup folder", False)


        If Model.StoragePath <> Nothing Then
            Dim StorageDir As String = Model.StoragePath + "Meteo\" + InitialDateStr + "_" + FinalDateStr + "\"
            IO.Directory.CreateDirectory(StorageDir)
            FileCopy(Model.MeteoWorkPath + "MM5_" + Trim(Model.Name) + ".hdf5", StorageDir + Trim(Model.Name) + ".hdf5", "Could not copy interpolated MM5 file to backup folder", False)
        End If

    End Sub

    Sub RunPreProcessingTools()
        For Each PreProcessor As PreProcessor In PreProcessors
            LogThis("PreProcessor Tool: " + PreProcessor.Name)

            If PreProcessor.InputFile = Nothing Or File.Exists(PreProcessor.InputFile) Then
                If PreProcessor.InputFile <> Nothing Then
                    Dim IO1 As New DataIO()
                    IO1.ChangeStream("START", InitialDate, PreProcessor.InputFile)
                    IO1.ChangeStream("END", FinalDate, PreProcessor.InputFile)
                End If


                Dim AuxFolder_1 = Path.Combine(TriggerDir, "Preprocessor\")
                If Not System.IO.Directory.Exists(AuxFolder_1) Then
                    Directory.CreateDirectory(AuxFolder_1)
                End If
                Dim AuxFolder = Path.Combine(AuxFolder_1, PreProcessor.Name)
                If Not System.IO.Directory.Exists(AuxFolder) Then
                    Directory.CreateDirectory(AuxFolder)
                End If
                Dim FlagForTriggerFilename As String = InitialDateStr + "_" + FinalDateStr + ".dat"
                PreProcessor.FlagForTriggerFile = Path.Combine(AuxFolder, FlagForTriggerFilename)

                If IO.File.Exists(PreProcessor.FlagForTriggerFile) Then
                    IO.File.Delete(PreProcessor.FlagForTriggerFile)
                End If

                'Write Running message to Trigger File
                Call WriteFlagForTrigger("Running", "Preprocessor", PreProcessor.FlagForTriggerFile)

                Call LaunchProcess(PreProcessor, SoftwareLabel)

                'Write Running message to Trigger File
                Call WriteFlagForTrigger("Finish", "Preprocessor", PreProcessor.FlagForTriggerFile)

                LogThis("PreProcessor Tool: " + PreProcessor.Name + " successfully executed")

            ElseIf PreProcessor.InputFile <> Nothing And Not File.Exists(PreProcessor.InputFile) Then
                Call UnSuccessfullEnd("Input data file: " + PreProcessor.InputFile + " was not found")

            End If


        Next
    End Sub

    Sub GetDischarge()

        For Each Model As Model In Models

            If Model.HasDischarge Then

                LogThis("Updating discharges file for model : " + Model.Name)

                Dim DischargeDatabase As New ReadMohidTimeSerie(Model.DischargeDatabase)

                Dim InitialIndex As Long = 0
                For InitialIndex = 0 To DischargeDatabase.Size - 1
                    If DischargeDatabase.DateTime(InitialIndex) >= InitialDate Then
                        InitialIndex = InitialIndex - 1
                        Exit For
                    End If
                Next

                Dim FinalIndex As Long = 0
                For FinalIndex = InitialIndex To DischargeDatabase.Size - 1
                    If DischargeDatabase.DateTime(FinalIndex) >= FinalDate Then
                        FinalIndex = FinalIndex + 1
                        Exit For
                    End If
                Next

                Dim NewDischarge As New OutData(Model.DischargeFile)
                NewDischarge.WriteDataLine("SERIE_INITIAL_DATA", DischargeDatabase.DateTime(InitialIndex))
                NewDischarge.WriteDataLine("TIME_UNITS", "SECONDS")
                NewDischarge.WriteBlankLine()
                NewDischarge.WriteDataLine("<BeginTimeSerie>")

                Dim ncolumns = DischargeDatabase.PropName.Length
                Dim FullColumn As String = ""
                Dim SecondsFromInitialDate As Long = 0
                For row As Long = InitialIndex To FinalIndex
                    SecondsFromInitialDate = DateDiff("s", DischargeDatabase.DateTime(InitialIndex), DischargeDatabase.DateTime(row))
                    FullColumn = SecondsFromInitialDate.ToString
                    For column As Integer = 1 To ncolumns - 1
                        FullColumn = FullColumn + " " + DischargeDatabase.ModelData(row, column).ToString
                    Next
                    NewDischarge.WriteDataLine(FullColumn)
                Next
                NewDischarge.WriteDataLine("<EndTimeSerie>")
                NewDischarge.Finish()

                LogThis("Updated discharges file for model : " + Model.Name)

            End If

        Next

    End Sub

    Sub GetDischarges()
        Dim LinesToWrite As New Collection
        Dim DischargesToCompute As Boolean = False
        Dim Discharges_Log_File As New Collection
        Dim i As Integer


        ' Managing Discharges
        For Each Model As Model In Models

            If Model.HasDischarges Then
                Dim DischargesConfigFile As String = System.IO.Path.Combine(Model.DischargesWorkPath, DischargesConfigFilename)
                DischargesToCompute = True
                LogThis("Managing discharges file for model : " + Model.Name)

                Discharges_Log_File.Add("Discharges_" + Now.ToString("yyyy-MM-dd_HHmmss") + _
                                   "_" + Model.Name.ToString + ".log")
                LinesToWrite.Add(Chr(34) + "Mohid TimeSeriesCreator.exe" + Chr(34) + " " + _
                                   "-a0=Import_Dates " + _
                                   "-a1=" + DischargesConfigFile.ToString + " " + _
                                   "-a2=" + InitialDateStr + " " + _
                                   "-a3=" + FinalDateStr + " " + _
                                   "> " + Discharges_Log_File(Discharges_Log_File.Count))


                ' Checking for Discharges to compute

                LogThis("Writing TimeSeriesCreator batch file...")
                Dim TSBatchFileName As String = Path.Combine(Model.DischargesWorkPath, "make_timeseries.bat")
                Try

                    Dim TSBatchFile As New OutData(TSBatchFileName)


                    For i = 1 To LinesToWrite.Count
                        TSBatchFile.WriteDataLine(LinesToWrite(i))
                    Next
                    TSBatchFile.Finish()
                    LogThis("Done!")
                Catch ex As Exception
                    Call UnSuccessfullEnd("Error Writing TimeSeriesCreator batch file in model " + Model.Name.ToString + " (could not write batch file " + TSBatchFileName + ")")
                End Try

                LogThis("Generating discharges from database...")

                Dim TSBatch As Process = New Process
                TSBatch.StartInfo.FileName = TSBatchFileName
                TSBatch.StartInfo.WorkingDirectory = Model.DischargesWorkPath
                TSBatch.StartInfo.WindowStyle = ProcessWindowStyle.Normal

                Try
                    TSBatch.Start()
                    TSBatch.WaitForExit(30000)

                    LogThis("Done.")

                Catch ex As Exception
                    Call UnSuccessfullEnd("Error executing TimeSeriesCreator batch file in model " + Model.Name.ToString + " (could not execute batch file" + TSBatchFileName + ")")
                End Try

            End If
        Next


        ' Checking if discharges generation was successful 
        i = 0
        For Each Model As Model In Models
            If Model.HasDischarges Then
                i += 1
                LogThis("Checking if discharges generation was successful in model " + Model.Name.ToString + "...")
                If File.Exists(Path.Combine(Model.DischargesWorkPath, Discharges_Log_File(i))) Then
                    If ProgramWasSuccessfull(Path.Combine(Model.DischargesWorkPath, Discharges_Log_File(i)), "TimeSeries successfully finished his tasks.") Then
                        LogThis("Discharge files successfully generated in model " + Model.Name.ToString)
                        Call Model.Read_Discharges_List(DischargesConfigFilename)
                    Else
                        Call UnSuccessfullEnd("Error generating discharge files in model " + Model.Name.ToString + "!")
                    End If
                Else
                    Call UnSuccessfullEnd("Error starting discharge files generation in model " + Model.Name.ToString + " (could not find TimeSeriesCreator log file " + Discharges_Log_File(i) + ")")
                End If
            End If
        Next


    End Sub

    Sub GetDischarges_ets()
        ' Managing Discharges
        Dim SimulationsAvailable As Integer
        For Each Model As Model In Models

            If Model.HasDischarges_ets Then
                Dim InitialDate_ As Date
                Dim InitialDateStr_ As String
                Dim FinalDateStr As String
                Dim FolderLabel As String
                Dim SourceBasePath As String
                SimulationsAvailable = DaysPerRun - Model.etsSimulatedDays

                LogThis("Gathering ets discharges files...")

                For n = 0 To SimulationsAvailable Step -1
                    InitialDate_ = InitialDate.AddDays(n)
                    InitialDateStr_ = InitialDate_.ToString("yyyy-MM-dd")
                    FinalDateStr = InitialDate_.AddDays(Model.etsSimulatedDays).ToString("yyyy-MM-dd")
                    Dim filename As String
                    Dim FullTargetBasePath As String
                    SourceBasePath = Model.Discharges_ets_WorkPath + "\" + InitialDateStr_ + "_" + FinalDateStr
                    FolderLabel = "BoundaryConditions\Discharges\" + Model.Discharges_ets_ModelName + "\"
                    FullTargetBasePath = MainPath + "GeneralData\" + FolderLabel + "\"
                    If Not Directory.Exists(FullTargetBasePath) Then
                        Directory.CreateDirectory(FullTargetBasePath)
                    Else
                        For Each FoundFile As String In My.Computer.FileSystem.GetFiles(FullTargetBasePath, FileIO.SearchOption.SearchTopLevelOnly, "*.ets")
                            My.Computer.FileSystem.DeleteFile(FoundFile)
                        Next
                    End If
                    If Directory.Exists(SourceBasePath) Then
                        Dim files() As String
                        files = Directory.GetFiles(SourceBasePath, "*.ets")

                        For Each file_ As String In files
                            filename = Path.GetFileName(file_)
                            FileCopy(file_, FullTargetBasePath + filename, "Could not copy ets discharge file - " + FullTargetBasePath + filename, True)
                        Next
                        Exit For
                    End If
                Next

            End If
        Next

    End Sub
    Sub GetDischarges_srn()
        ' Managing Discharges
        Dim SimulationsAvailable As Integer
        For Each Model As Model In Models

            If Model.HasDischarges_srn Then
                Dim InitialDate_ As Date
                Dim InitialDateStr_ As String
                Dim FinalDateStr As String
                Dim FolderLabel As String
                Dim SourceBasePath As String
                SimulationsAvailable = DaysPerRun - Model.srnSimulatedDays

                LogThis("Gathering srn discharges files...")

                For n = 0 To SimulationsAvailable Step -1
                    InitialDate_ = InitialDate.AddDays(n)
                    InitialDateStr_ = InitialDate_.ToString("yyyy-MM-dd")
                    FinalDateStr = InitialDate_.AddDays(Model.srnSimulatedDays).ToString("yyyy-MM-dd")
                    Dim filename As String
                    Dim FullTargetBasePath As String
                    SourceBasePath = Model.Discharges_srn_WorkPath + "\" + InitialDateStr_ + "_" + FinalDateStr
                    FolderLabel = "BoundaryConditions\Discharges\" + Model.Discharges_srn_ModelName + "\"
                    FullTargetBasePath = MainPath + "GeneralData\" + FolderLabel + "\"
                    If Not Directory.Exists(FullTargetBasePath) Then
                        Directory.CreateDirectory(FullTargetBasePath)
                    End If
                    If Directory.Exists(SourceBasePath) Then
                        Dim files() As String
                        files = Directory.GetFiles(SourceBasePath, "*.srn")

                        For Each file_ As String In files
                            filename = Path.GetFileName(file_)
                            FileCopy(file_, FullTargetBasePath + filename, "Could not copy srn discharge file - " + FullTargetBasePath + filename, True)
                        Next
                        Exit For
                    End If
                Next

            End If
        Next

    End Sub
    Sub CreateNewModelFiles()

        LogThis("Writing new dates on Model.dat input data file for todays forecast...")

        For Each Model As Model In Models

            'change initial and final dates
            Dim NewModelInputFile As New OutData(Model.Path + "data\Model_" + Model.RunID.ToString + ".dat")
            NewModelInputFile.WriteDataLine("START", InitialDate)
            NewModelInputFile.WriteDataLine("END", FinalDate)
            NewModelInputFile.WriteDataLine("DT", Model.TimeStep)
            NewModelInputFile.WriteDataLine("VARIABLEDT", Model.HasVariableDT)
            If Model.HasVariableDT Then
                NewModelInputFile.WriteDataLine("MAXDT", Model.MaxDT)
            End If
            NewModelInputFile.WriteDataLine("GMTREFERENCE", 0)
            NewModelInputFile.WriteDataLine("OPENMP_NUM_THREADS", OpenMP_Num_Threads)
            If Model.HasLagrangian Then
                NewModelInputFile.WriteDataLine("LAGRANGIAN", 1)
            End If
            If Model.HasWaves Then
W            End If
            NewModelInputFile.Finish()

        Next


    End Sub

    Sub GatherRestartFiles(Optional ByVal modelinput As Model = Nothing)
        If Run_WW3 Then
            LogThis("Gathering the restart file ...")
            If modelinput.GatherRestartFiles Then
                Dim IniDateFromPreviousRun As Date = InitialDate.AddDays(-1)
                Dim IniDateFromPreviousRunStr As String = IniDateFromPreviousRun.ToString("yyyy-MM-dd")
                Dim FinDateFromPreviousRunStr = IniDateFromPreviousRun.AddDays(DaysPerRun).ToString("yyyy-MM-dd")
                Dim PathToReadFinFiles As String
                PathToReadFinFiles = Path.Combine(modelinput.BackUpPath, IniDateFromPreviousRunStr + "_" + FinDateFromPreviousRunStr + "\")

                FileCopy(Path.Combine(PathToReadFinFiles, "restart001.ww3"), Path.Combine(modelinput.Path, "restart.ww3"), "Could not copy or rename the restart file - " + modelinput.Name, True)

            End If

        ElseIf Run_MOHID Then
            LogThis("Gathering the restart files for each model domain...")

            Dim CountProcessors As Integer = 0

            For Each Model As Model In Models
                Dim PathToReadFinFiles As String
                Dim IniDateFromPreviousRun As Date = InitialDate.AddDays(-1)
                Dim IniDateFromPreviousRunStr As String = IniDateFromPreviousRun.ToString("yyyy-MM-dd")
                Dim FinDateFromPreviousRunStr = IniDateFromPreviousRun.AddDays(DaysPerRun).ToString("yyyy-MM-dd")
                PathToReadFinFiles = Model.BackUpPath + "Restart\" + IniDateFromPreviousRunStr + "_" + FinDateFromPreviousRunStr + "\"

                If Model.GatherRestartFiles Then

                    If Not Model.HasSolutionFromFile Then
                        If MPI And Model.MPI_Num_Processors > 1 And MPI_GatherHDF5DecomposedFiles = True Then
                            For i = (CountProcessors + 1) To (CountProcessors + Model.MPI_Num_Processors)
                                If Model.HasHydrodynamics Then
                                    FileCopy(PathToReadFinFiles + "MPI_" + i.ToString + "_Hydrodynamic_1.fin", Model.Path + "res\MPI_" + i.ToString + "_Hydrodynamic_0.fin", "Could not copy or rename one of the Hydrodynamic restart files - " + Model.Name, True)
                                End If
                                If Model.HasWaterProperties Then
                                    FileCopy(PathToReadFinFiles + "MPI_" + i.ToString + "_WaterProperties_1.fin5", Model.Path + "res\MPI_" + i.ToString + "_WaterProperties_0.fin5", "Could not copy or rename one of the WaterProperties restart files - " + Model.Name, True)
                                End If

                                If Model.HasGOTM Then
                                    FileCopy(PathToReadFinFiles + "MPI_" + i.ToString + "_GOTM_1.fin", Model.Path + "res\MPI_" + i.ToString + "_GOTM_0.fin", "Could not copy or rename one of the GOTM restart files - " + Model.Name, True)
                                End If

                                If Model.HasInterfaceSedimentWater Then
                                    FileCopy(PathToReadFinFiles + "MPI_" + i.ToString + "_InterfaceSedimentWater_1.fin5", Model.Path + "res\MPI_" + i.ToString + "_InterfaceSedimentWater_0.fin5", "Could not copy or rename one of the InterfaceSedimentWater restart files - " + Model.Name, True)
                                End If
                                CountProcessors += 1
                            Next
                        Else
                            If Model.HasHydrodynamics Then
                                FileCopy(PathToReadFinFiles + "Hydrodynamic_1.fin", Model.Path + "res\Hydrodynamic_0.fin", "Could not copy or rename the Hydrodynamic restart file - " + Model.Name, True)
                            End If
                            If Model.HasWaterProperties Then
                                FileCopy(PathToReadFinFiles + "WaterProperties_1.fin5", Model.Path + "res\WaterProperties_0.fin5", "Could not copy or rename the WaterProperties restart file - " + Model.Name, True)
                            End If

                            If Model.HasGOTM Then
                                FileCopy(PathToReadFinFiles + "GOTM_1.fin", Model.Path + "res\GOTM_0.fin", "Could not copy or rename the GOTM restart file - " + Model.Name, True)
                            End If

                            If Model.HasInterfaceSedimentWater Then
                                FileCopy(PathToReadFinFiles + "InterfaceSedimentWater_1.fin5", Model.Path + "res\InterfaceSedimentWater_0.fin5", "Could not copy or rename the InterfaceSedimentWater restart file - " + Model.Name, True)
                            End If
                        End If

                    End If

                    If Model.HasLagrangian Then
                        FileCopy(PathToReadFinFiles + "Lagrangian_1.fin", Model.Path + "res\Lagrangian_0.fin", "Could not copy or rename the Lagrangian restart file - " + Model.Name, True)
                    End If

                    '---------MOHID Land
                    Dim source_string As String
                    Dim target_string As String
                    Dim filetype_string As String
                    If Model.HasBasin Then
                        filetype_string = "Basin"
                        source_string = "Basin_1.fin"
                        target_string = "res\Basin_0.fin"
                        FileCopy(PathToReadFinFiles + source_string, Model.Path + target_string, "Could not copy or rename the " + filetype_string + " restart file - " + Model.Name, True)
                    End If
                    If Model.HasDrainageNetwork Then
                        filetype_string = "Drainage Network"
                        source_string = "Drainage Network_1.fin"
                        target_string = "res\Drainage Network_0.fin"
                        FileCopy(PathToReadFinFiles + source_string, Model.Path + target_string, "Could not copy or rename the " + filetype_string + " restart file - " + Model.Name, True)
                    End If
                    If Model.HasPorousMedia Then
                        filetype_string = "Porous Media"
                        source_string = "Porous Media_1.fin"
                        target_string = "res\Porous Media_0.fin"
                        FileCopy(PathToReadFinFiles + source_string, Model.Path + target_string, "Could not copy or rename the " + filetype_string + " restart file - " + Model.Name, True)
                    End If
                    If Model.HasPorousMediaProperties Then
                        filetype_string = "PorousMedia Properties"
                        source_string = "PorousMedia Properties_1.fin"
                        target_string = "res\PorousMedia Properties_0.fin"
                        FileCopy(PathToReadFinFiles + source_string, Model.Path + target_string, "Could not copy or rename the " + filetype_string + " restart file - " + Model.Name, True)
                    End If
                    If Model.HasRunoff Then
                        filetype_string = "RunOff"
                        source_string = "RunOff_1.fin"
                        target_string = "res\RunOff_0.fin"
                        FileCopy(PathToReadFinFiles + source_string, Model.Path + target_string, "Could not copy or rename the " + filetype_string + " restart file - " + Model.Name, True)
                    End If
                    If Model.HasRunoffProperties Then
                        filetype_string = "RunOff Properties"
                        source_string = "RunOff Properties_1.fin"
                        target_string = "res\RunOff Properties_0.fin"
                        FileCopy(PathToReadFinFiles + source_string, Model.Path + target_string, "Could not copy or rename the " + filetype_string + " restart file - " + Model.Name, True)
                    End If
                    If Model.HasVegetation Then
                        filetype_string = "Vegetation"
                        source_string = "Vegetation_1.fin"
                        target_string = "res\Vegetation_0.fin"
                        FileCopy(PathToReadFinFiles + source_string, Model.Path + target_string, "Could not copy or rename the " + filetype_string + " restart file - " + Model.Name, True)
                    End If
                    If Model.HasReservoirs Then
                        filetype_string = "Reservoirs"
                        source_string = "Reservoirs_1.fin"
                        target_string = "res\Reservoirs_0.fin"
                        FileCopy(PathToReadFinFiles + source_string, Model.Path + target_string, "Could not copy or rename the " + filetype_string + " restart file - " + Model.Name, True)
                    End If
                    If Model.HasIrrigation Then
                        filetype_string = "Irrigation"
                        source_string = "Irrigation_1.fin"
                        target_string = "res\Irrigation_0.fin"
                        FileCopy(PathToReadFinFiles + source_string, Model.Path + target_string, "Could not copy or rename the " + filetype_string + " restart file - " + Model.Name, True)
                    End If
                    '---------MOHID Land

                End If
            Next
        End If

    End Sub

    Sub CreateMOHIDBatchFile()

        FatherModelWorkPath = ""

        'getting father domain exe folder
        For Each Model As Model In Models
            FatherModelWorkPath = MainPath + Model.Path + "exe"
            Exit For
        Next

        If MPI Then
            Call GetMPINbrProcessors()
            MPI_DomainDecompositionConsolidation_LogFile = "DomainDecompositionConsolidation_" + InitialDateStr + ".log"
        End If

        MOHID_Run_LogFile = "MOHID_RUN_" + InitialDateStr + ".log"

        If CreateBatchFile = True Then

            LogThis("Writing MOHID_Run batch file for todays forecast...")

            'write run batch file
            Dim ExecuteModelBatch As New OutData(MOHID_exe)
            ExecuteModelBatch.WriteDataLine("cd " + Chr(34) + FatherModelWorkPath + Chr(34))
            ExecuteModelBatch.WriteDataLine(MainPath.Substring(0, 1) + ":")
            If MPI Then
                LogThis(MPI_Exe_Path + Chr(34) + " -np " + MPI_Num_Processors.ToString + " " + Chr(34) + MainPath + "GeneralData\Exe\MohidWater.exe" + Chr(34) + " > ")
                ExecuteModelBatch.WriteDataLine(Chr(34) + MPI_Exe_Path + Chr(34) + " -np " + MPI_Num_Processors.ToString + " " + Chr(34) + MainPath + "GeneralData\Exe\MohidWater.exe" + Chr(34) + " > " + MOHID_Run_LogFile)
            Else
                ExecuteModelBatch.WriteDataLine(Chr(34) + MainPath + "GeneralData\Exe\MohidWater.exe" + Chr(34) + " > " + MOHID_Run_LogFile)
            End If
            ExecuteModelBatch.Finish()
        End If

    End Sub

    Sub RunMOHID()

        Dim ForecastRun As Process = New Process
        Dim stringOutput As String
        Try
            With ForecastRun

                If MPI Then
                    ForecastRun.StartInfo.FileName = MPI_Exe_Path
                    ForecastRun.StartInfo.Arguments = "-np " + MPI_Num_Processors.ToString + " " + Chr(34) + MOHID_exe + Chr(34)
                    ForecastRun.StartInfo.WorkingDirectory = FatherModelWorkPath
                Else
                    ForecastRun.StartInfo.FileName = MOHID_exe
                End If

                ForecastRun.StartInfo.WindowStyle = ProcessWindowStyle.Normal

                If MOHID_ScreenOutputToFile = True Then
                    .StartInfo.UseShellExecute = False
                    .StartInfo.RedirectStandardOutput = True

                    If CreateBatchFile = False Then
                        ForecastRun.StartInfo.WorkingDirectory = FatherModelWorkPath
                    End If

                    Dim aux_path As String
                    If MOHID_ScreenOutputPath = Nothing Then
                        aux_path = .StartInfo.WorkingDirectory
                    Else
                        aux_path = MOHID_ScreenOutputPath
                    End If

                    MOHID_Run_LogFile_FullPath = Path.Combine(aux_path, MOHID_Run_LogFile)
                    If MOHID_ScreenOutputPathInNetwork <> "" Then
                        MOHID_Run_LogFile_FullPath_In_Network = MOHID_ScreenOutputPathInNetwork
                    Else
                        MOHID_Run_LogFile_FullPath_In_Network = MOHID_Run_LogFile_FullPath
                    End If

                    Dim fileToSave As New StreamWriter(MOHID_Run_LogFile_FullPath)
                    fileToSave.AutoFlush = True

                    LogThis("Executing MOHID...")
                    .Start()

                    'While .HasExited = False
                    '    stringOutput = .StandardOutput.ReadLine
                    '    If Not String.IsNullOrEmpty(stringOutput) Then
                    '        fileToSave.WriteLine(stringOutput)
                    '    End If
                    'End While


                    While Not ForecastRun.StandardOutput.EndOfStream
                        stringOutput = .StandardOutput.ReadLine
                        fileToSave.WriteLine(stringOutput)
                    End While
                    fileToSave.Close()
                Else
                    LogThis("Executing MOHID...")
                    .Start()
                End If

                .WaitForExit(1000 * MOHID_MaxTime)
                If Not .HasExited Then
                    .Kill()
                    .Close()
                    Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined: " + MOHID_MaxTime.ToString + " seconds")
                End If
            End With
            ForecastRun.Close()

        Catch ex As Exception
            Call UnSuccessfullEnd("MOHID could not be executed.", ex.Message)
        End Try
        LogThis("Done!")


    End Sub

    Sub RunDomainConsolidation_MPI_Joiner_V1()
        Dim ProcessRunDomainDecompositionConsolidation As Process = New Process
        Dim stringOutput As String
        Dim MPI_DomainDecompositionConsolidation_LogFile_FullPath As String
        Dim MPI_DomainDecompositionConsolidation_LogFile_FullPath_In_Network As String
        Try
            With ProcessRunDomainDecompositionConsolidation

                .StartInfo.FileName = Path.Combine(FatherModelWorkPath, "DomainDecompositionConsolidation.exe")
                .StartInfo.WorkingDirectory = FatherModelWorkPath

                .StartInfo.WindowStyle = ProcessWindowStyle.Normal

                If MOHID_ScreenOutputToFile = True Then
                    .StartInfo.UseShellExecute = False
                    .StartInfo.RedirectStandardOutput = True

                    Dim aux_path As String
                    If MOHID_ScreenOutputPath = Nothing Then
                        aux_path = .StartInfo.WorkingDirectory
                    Else
                        aux_path = MOHID_ScreenOutputPath
                    End If

                    MPI_DomainDecompositionConsolidation_LogFile_FullPath = Path.Combine(aux_path, MPI_DomainDecompositionConsolidation_LogFile)
                    If MOHID_ScreenOutputPathInNetwork <> "" Then
                        MPI_DomainDecompositionConsolidation_LogFile_FullPath_In_Network = MOHID_ScreenOutputPathInNetwork
                    Else
                        MPI_DomainDecompositionConsolidation_LogFile_FullPath_In_Network = MPI_DomainDecompositionConsolidation_LogFile_FullPath
                    End If

                    Dim fileToSave As New StreamWriter(MPI_DomainDecompositionConsolidation_LogFile_FullPath)
                    fileToSave.AutoFlush = True

                    LogThis("Executing DomainDecompositionConsolidation.exe...")
                    .Start()

                    'While .HasExited = False
                    '    stringOutput = .StandardOutput.ReadLine
                    '    If Not String.IsNullOrEmpty(stringOutput) Then
                    '        fileToSave.WriteLine(stringOutput)
                    '    End If
                    'End While


                    While Not .StandardOutput.EndOfStream
                        stringOutput = .StandardOutput.ReadLine
                        fileToSave.WriteLine(stringOutput)
                    End While
                    fileToSave.Close()
                Else
                    LogThis("Executing DomainDecompositionConsolidation.exe...")
                    .Start()
                End If

                .WaitForExit(1000 * 1800)
                If Not .HasExited Then
                    .Kill()
                    .Close()
                    Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined for domain decomposition / consolidation: " + 1800.ToString + " seconds")
                End If
            End With
            ProcessRunDomainDecompositionConsolidation.Close()

        Catch ex As Exception
            Call UnSuccessfullEnd("MOHID could not be executed.", ex.Message)
        End Try
        LogThis("Done!")
    End Sub

    Sub RunDomainDecompositionConsolidation_MPI_Joiner_V2()
        Dim ProcessRunDomainDecompositionConsolidation As Process = New Process
        Dim stringOutput As String
        Dim MPI_DomainDecompositionConsolidation_LogFile_FullPath As String
        Dim MPI_DomainDecompositionConsolidation_LogFile_FullPath_In_Network As String
        Dim Arguments As String
        Try
            With ProcessRunDomainDecompositionConsolidation


                .StartInfo.FileName = MPI_Exe_Path
                .StartInfo.Arguments = "-np " + MPI_DDCParser_Num_Processors.ToString + " " + Chr(34) + "DDC_Parser.exe " + Chr(34) + " \ : -np " + MPI_DDCWorker_Num_Processors.ToString + " " + Chr(34) + "DDC_Worker.exe" + Chr(34)
                .StartInfo.WorkingDirectory = FatherModelWorkPath

                .StartInfo.WindowStyle = ProcessWindowStyle.Normal

                If MOHID_ScreenOutputToFile = True Then
                    .StartInfo.UseShellExecute = False
                    .StartInfo.RedirectStandardOutput = True

                    Dim aux_path As String
                    If MOHID_ScreenOutputPath = Nothing Then
                        aux_path = .StartInfo.WorkingDirectory
                    Else
                        aux_path = MOHID_ScreenOutputPath
                    End If

                    MPI_DomainDecompositionConsolidation_LogFile_FullPath = Path.Combine(aux_path, MPI_DomainDecompositionConsolidation_LogFile)
                    If MOHID_ScreenOutputPathInNetwork <> "" Then
                        MPI_DomainDecompositionConsolidation_LogFile_FullPath_In_Network = MOHID_ScreenOutputPathInNetwork
                    Else
                        MPI_DomainDecompositionConsolidation_LogFile_FullPath_In_Network = MPI_DomainDecompositionConsolidation_LogFile_FullPath
                    End If

                    Dim fileToSave As New StreamWriter(MPI_DomainDecompositionConsolidation_LogFile_FullPath)
                    fileToSave.AutoFlush = True

                    LogThis("Executing DomainDecompositionConsolidation.exe...")
                    .Start()

                    'While .HasExited = False
                    '    stringOutput = .StandardOutput.ReadLine
                    '    If Not String.IsNullOrEmpty(stringOutput) Then
                    '        fileToSave.WriteLine(stringOutput)
                    '    End If
                    'End While


                    While Not .StandardOutput.EndOfStream
                        stringOutput = .StandardOutput.ReadLine
                        fileToSave.WriteLine(stringOutput)
                    End While
                    fileToSave.Close()
                Else
                    LogThis("Executing DomainDecompositionConsolidation.exe...")
                    .Start()
                End If

                .WaitForExit(1000 * 1800)
                If Not .HasExited Then
                    .Kill()
                    .Close()
                    Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined for domain consolidation: " + 1800.ToString + " seconds")
                End If
            End With
            ProcessRunDomainDecompositionConsolidation.Close()

        Catch ex As Exception
            Call UnSuccessfullEnd("MOHID could not be executed.", ex.Message)
        End Try
        LogThis("Done!")
    End Sub

    Sub HandleSubDomainsInWW3(ByVal Model As Model)
        Dim NestFileNameFromFather As String
        Dim NestFileNameToSon As String = "nest.ww3"
        Dim ModelWorkPath As String = Path.Combine(MainPath, Model.Path)
        Dim ErrorMessage As String = "Could not copy father .ww3 file: "
        '        Dim InitialDateStr, FinalDateStr As String
        Dim FatherWorkPathComplete As String
        Dim NestFileNameFromFatherComplete As String
        If Model.HasFather Then

            LogThis("Gathering father .ww3 file...")

            NestFileNameFromFather = "nest" + Model.BoundaryFile_Suffix + ".ww3"
            InitialDateStr = InitialDate.ToString("yyyy-MM-dd")
            FinalDateStr = FinalDate.ToString("yyyy-MM-dd")

            FatherWorkPathComplete = Path.Combine(Model.FatherWorkPath, InitialDateStr + "_" + FinalDateStr)
            NestFileNameFromFatherComplete = Path.Combine(FatherWorkPathComplete, NestFileNameFromFather)
            FileCopy(NestFileNameFromFatherComplete, Path.Combine(ModelWorkPath, NestFileNameToSon), ErrorMessage + NestFileNameFromFather, True)
        End If
    End Sub

    Sub InitializeVariables()
        WPSFolder = Path.Combine(Path.Combine(MainPath, WRFModelPath), "WPS")
        WRFFolder = Path.Combine(Path.Combine(MainPath, WRFModelPath), "WRF")
        WPSDataFolder = Path.Combine(WPSFolder, "WPS\Data")
        For Each Model As Model In Models
            Model.SetWRFResultFileName()
        Next


    End Sub
    Sub PrepareConfigFiles()
        If WRF_UnGrib Or WRF_MetGrid Then
            LogThis("Preparing config files...")

            'preparing namelist.wps
            Dim NamelistFile As String = Path.Combine(WPSFolder, "namelist.wps")
            Dim sr As New StreamReader(NamelistFile)
            Dim sFile As String = String.Empty
            Dim textLine, filteredTextLine As String
            Dim ModelsNbr As Integer = Models.Count
            Dim i As Integer
            While (sr.Peek() <> -1)
                textLine = sr.ReadLine
                If InStr(textLine, "max_dom") <> 0 Then
                    textLine = " max_dom = " + ModelsNbr.ToString + ","
                ElseIf InStr(textLine, "start_date") <> 0 Then
                    textLine = " start_date = "
                    For i = 1 To ModelsNbr
                        textLine += "'" + InitialDate.ToString("yyyy-MM-dd_HH:mm:ss") + "', "
                    Next
                ElseIf InStr(textLine, "end_date") <> 0 Then
                    textLine = " end_date = "
                    For i = 1 To ModelsNbr
                        textLine += "'" + FinalDate.ToString("yyyy-MM-dd_HH:mm:ss") + "', "
                    Next
                ElseIf InStr(textLine, "interval_seconds") <> 0 Then
                    textLine = " interval_seconds = " + WRF_InputIntervalSeconds.ToString + ","
                End If

                sFile += textLine + vbCrLf
            End While

            sr.Close()
            sr = Nothing

            Dim sw As StreamWriter = New StreamWriter(NamelistFile, False)
            sw.Write(sFile)
            sw.Close()
            sw = Nothing
            sFile = String.Empty
        End If

        'preparing namelist.input
        If WRF_RunWRF Then
            Dim NamelistFile As String = Path.Combine(WRFFolder, "namelist.input")
            Dim sr2 As New StreamReader(NamelistFile)
            Dim sFile As String = String.Empty
            Dim textLine As String
            Dim ModelsNbr As Integer = Models.Count

            Dim days_, hours_, minutes_, seconds_ As String
            '            days_ = (FinalDate - InitialDate).Days
            hours_ = (FinalDate - InitialDate).TotalHours
            '            minutes_ = (FinalDate - InitialDate).Minutes
            '            seconds_ = (FinalDate - InitialDate).Seconds
            While (sr2.Peek() <> -1)
                textLine = sr2.ReadLine
                Select Case True
                    'Case InStr(textLine, "run_days") <> 0
                    '    textLine = "run_days                = " + days_.ToString + ","
                    Case InStr(textLine, "run_hours") <> 0
                        textLine = "run_hours               = " + hours_.ToString + ","
                        'Case InStr(textLine, "run_minutes") <> 0
                        '    textLine = "run_minutes             = " + minutes_.ToString + ","
                        'Case InStr(textLine, "run_seconds") <> 0
                        '    textLine = "run_seconds             = " + seconds_.ToString + ","
                    Case InStr(textLine, "start_year") <> 0
                        textLine = "start_year              = "
                        For i = 1 To ModelsNbr
                            textLine += InitialDate.ToString("yyyy") + ", "
                        Next
                    Case InStr(textLine, "start_month") <> 0
                        textLine = "start_month             = "
                        For i = 1 To ModelsNbr
                            textLine += InitialDate.ToString("MM") + ", "
                        Next
                    Case InStr(textLine, "start_day") <> 0
                        textLine = "start_day               = "
                        For i = 1 To ModelsNbr
                            textLine += InitialDate.ToString("dd") + ", "
                        Next
                    Case InStr(textLine, "start_hour") <> 0
                        textLine = "start_hour              = "
                        For i = 1 To ModelsNbr
                            textLine += InitialDate.ToString("HH") + ", "
                        Next
                    Case InStr(textLine, "start_minute") <> 0
                        textLine = "start_minute            = "
                        For i = 1 To ModelsNbr
                            textLine += InitialDate.ToString("mm") + ", "
                        Next
                    Case InStr(textLine, "start_second") <> 0
                        textLine = "start_second            = "
                        For i = 1 To ModelsNbr
                            textLine += InitialDate.ToString("ss") + ", "
                        Next
                    Case InStr(textLine, "end_year") <> 0
                        textLine = "end_year                = "
                        For i = 1 To ModelsNbr
                            textLine += FinalDate.ToString("yyyy") + ", "
                        Next
                    Case InStr(textLine, "end_month") <> 0
                        textLine = "end_month               = "
                        For i = 1 To ModelsNbr
                            textLine += FinalDate.ToString("MM") + ", "
                        Next
                    Case InStr(textLine, "end_day") <> 0
                        textLine = "end_day                 = "
                        For i = 1 To ModelsNbr
                            textLine += FinalDate.ToString("dd") + ", "
                        Next
                    Case InStr(textLine, "end_hour") <> 0
                        textLine = "end_hour                = "
                        For i = 1 To ModelsNbr
                            textLine += FinalDate.ToString("HH") + ", "
                        Next
                    Case InStr(textLine, "end_minute") <> 0
                        textLine = "end_minute              = "
                        For i = 1 To ModelsNbr
                            textLine += FinalDate.ToString("mm") + ", "
                        Next
                    Case InStr(textLine, "end_second") <> 0
                        textLine = "end_second              = "
                        For i = 1 To ModelsNbr
                            textLine += FinalDate.ToString("ss") + ", "
                        Next
                    Case InStr(textLine, "interval_seconds") <> 0
                        textLine = "interval_seconds         = " + WRF_InputIntervalSeconds.ToString + ","
                    Case InStr(textLine, "max_dom") <> 0
                        textLine = "max_dom                  = " + ModelsNbr.ToString + ","
                    Case InStr(textLine, "restart") <> 0 And InStr(textLine, "restart_interval") = 0 And InStr(textLine, "io_form_restart") = 0
                        If WRF_Restart = True Then
                            textLine = "restart                            = .true." + ","
                        Else
                            textLine = "restart                            = .false." + ","
                        End If


                End Select

                sFile += textLine + vbCrLf
            End While
            sr2.Close()
            sr2 = Nothing

            Dim sw2 As StreamWriter = New StreamWriter(NamelistFile, False)
            sw2.Write(sFile)
            sw2.Close()
            sw2 = Nothing

        End If
    End Sub

    Sub RunUnGrib()
        If WRF_UnGrib Then
            If Directory.Exists(WRF_Ungrib_SourcePath) Then
                Dim di As DirectoryInfo = New DirectoryInfo(WPSFolder)
                Dim OldGribFiles As FileInfo() = di.GetFiles("GRIBFILE.???")
                Dim targetfilename As String = ""
                Dim OldUnGribFiles As FileInfo() = di.GetFiles("ART*")
                Dim sourcefilename As String = ""
                Dim ErrorMessage As String = "Could Not copy one (Or more) Of the grib files"
                Dim Run As Process = New Process
                Dim stringOutput As String
                Dim WPSFolder_slash, WRF_Ungrib_SourcePath_slash As String
                Dim WPSLink_Run_LogFile As String
                Dim Ungrib_Run_LogFile As String

                '1. deleting old link files
                LogThis("Deleting old grib files In WPS folder")
                For Each File In OldGribFiles
                    File.Delete()
                Next

                '2.1 ---------------------creating link WPS batch file
                WPSLink_Run_LogFile = "LinkWPS" + "_" + InitialDateStr + "_" + FinalDateStr + ".log"

                WPSFolder_slash = WPSFolder.Replace("\", "/")

                WRF_Ungrib_SourcePath_slash = WRF_Ungrib_SourcePath.Replace("\", "/")
                LogThis("Writing WPS link batch file...")

                Dim LinkBatch As New OutData(Path.Combine(WPSFolder, "link.bat"))
                LinkBatch.WriteDataLine("bash -c " + Chr(34) + WPSFolder_slash + "/" + "link_grib.csh" + " " + WRF_Ungrib_SourcePath_slash + Chr(34))
                LinkBatch.Finish()


                '2.2---------------------running link WPS

                Try
                    With Run
                        Run.StartInfo.WindowStyle = ProcessWindowStyle.Normal
                        .StartInfo.FileName = "bash.exe"
                        .StartInfo.Arguments = " -c " + Chr(34) + WPSFolder_slash + "/" + "link_grib.csh" + " " + WRF_Ungrib_SourcePath_slash + Chr(34)
                        If WRF_ScreenOutputToFile = True Then
                            .StartInfo.UseShellExecute = False
                            .StartInfo.RedirectStandardOutput = True
                            .StartInfo.WorkingDirectory = WPSFolder

                            Dim fileToSave As New StreamWriter(Path.Combine(LogPath, WPSLink_Run_LogFile))
                            fileToSave.AutoFlush = True

                            LogThis("Executing WPS Link...")
                            .Start()

                            While Not Run.StandardOutput.EndOfStream
                                stringOutput = .StandardOutput.ReadLine
                                fileToSave.WriteLine(stringOutput)
                            End While
                            fileToSave.Close()
                            fileToSave = Nothing
                        Else
                            LogThis("Executing WPS link")
                            .Start()
                        End If

                        .WaitForExit(1000 * WW3_MaxTime)
                        If Not .HasExited Then
                            .Kill()
                            .Close()
                            Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined: " + WW3_MaxTime.ToString + " seconds")
                        End If
                    End With
                    Run.Close()

                Catch ex As Exception
                    Call UnSuccessfullEnd("WPS link execution run could not be executed. ", ex.Message)
                End Try

                '3. deleting old ungribbed files
                LogThis("Deleting old ungribbed files in WPS folder")
                For Each File In OldUnGribFiles
                    File.Delete()
                Next
                File.Delete(Path.Combine(WPSFolder, "ungrib.log"))

                '4 ----------Executing Ungrib
                Ungrib_Run_LogFile = "Ungrib" + "_" + InitialDateStr + "_" + FinalDateStr + ".log"

                Try
                    With Run
                        Run.StartInfo.WindowStyle = ProcessWindowStyle.Normal
                        .StartInfo.FileName = Path.Combine(WPSFolder, WRF_Exe_UnGrib)
                        If WRF_ScreenOutputToFile = True Then
                            .StartInfo.UseShellExecute = False
                            .StartInfo.RedirectStandardOutput = True
                            .StartInfo.WorkingDirectory = WPSFolder

                            Dim fileToSave As New StreamWriter(Path.Combine(LogPath, Ungrib_Run_LogFile))
                            fileToSave.AutoFlush = True

                            LogThis("Executing Ungrib...")
                            .Start()

                            While Not Run.StandardOutput.EndOfStream
                                stringOutput = .StandardOutput.ReadLine
                                fileToSave.WriteLine(stringOutput)
                            End While
                            fileToSave.Close()
                            fileToSave = Nothing
                        Else
                            LogThis("Executing Ungrib...")
                            .Start()
                        End If

                        .WaitForExit(1000 * WW3_MaxTime)
                        If Not .HasExited Then
                            .Kill()
                            .Close()
                            Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined: " + WW3_MaxTime.ToString + " seconds")
                        End If
                    End With
                    Run.Close()

                    If Not ProgramWasSuccessfull(Path.Combine(WPSFolder, "ungrib.log"), "*** Successful completion of program ungrib.exe ***", True) Then

                        Call UnSuccessfullEnd("Ungrib execution run could not be executed.")
                    Else
                        LogThis("Ungrib execution run successfully executed.")

                    End If

                Catch ex As Exception
                    Call UnSuccessfullEnd("Ungrib execution run could not be executed. ", ex.Message)
                End Try
            End If
        End If
    End Sub
    Sub RunMetgrid()
        If WRF_MetGrid Then

            Dim Metgrid_Run_LogFile As String
            Dim Run As Process = New Process
            Dim stringOutput As String
            Dim di As DirectoryInfo = New DirectoryInfo(WPSFolder)
            Dim OldMetgridFiles As FileInfo() = di.GetFiles("met_em.*.nc")


            '1. deleting old link files
            LogThis("Deleting old metgrid files in WPS folder")
            For Each File In OldMetgridFiles
                File.Delete()
            Next
            File.Delete(Path.Combine(WPSFolder, "metgrid.log"))


            '2. ----------Executing metgrid
            Metgrid_Run_LogFile = "metgrid" + "_" + InitialDateStr + "_" + FinalDateStr + ".log"

            Try
                With Run
                    Run.StartInfo.WindowStyle = ProcessWindowStyle.Normal
                    .StartInfo.FileName = Path.Combine(WPSFolder, WRF_Exe_MetGrid)
                    If WRF_ScreenOutputToFile = True Then
                        .StartInfo.UseShellExecute = False
                        .StartInfo.RedirectStandardOutput = True
                        .StartInfo.WorkingDirectory = WPSFolder

                        Dim fileToSave As New StreamWriter(Path.Combine(LogPath, Metgrid_Run_LogFile))
                        fileToSave.AutoFlush = True

                        LogThis("Executing metgrid...")
                        .Start()

                        While Not Run.StandardOutput.EndOfStream
                            stringOutput = .StandardOutput.ReadLine
                            fileToSave.WriteLine(stringOutput)
                        End While
                        fileToSave.Close()
                        fileToSave = Nothing
                    Else
                        LogThis("Executing metgrid...")
                        .Start()
                    End If

                    .WaitForExit(1000 * WW3_MaxTime)
                    If Not .HasExited Then
                        .Kill()
                        .Close()
                        Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined: " + WW3_MaxTime.ToString + " seconds")
                    End If
                End With
                Run.Close()

                If Not ProgramWasSuccessfull(Path.Combine(WPSFolder, "metgrid.log"), "*** Successful completion of program metgrid.exe ***", True) Then

                    Call UnSuccessfullEnd("Metgrid execution run could not be executed.")
                Else
                    LogThis("Metgrid execution run successfully executed.")

                End If

            Catch ex As Exception
                Call UnSuccessfullEnd("Metgrid execution run could not be executed. ", ex.Message)
            End Try
        End If

    End Sub
    Sub RunWRF()
        If WRF_RunWRF Then
            Dim WRFLink_Run_LogFile As String
            Dim Run As Process = New Process
            Dim stringOutput As String
            Dim di As DirectoryInfo = New DirectoryInfo(WRFFolder)
            Dim OldLinkFiles As FileInfo() = di.GetFiles("met_em.*.nc")
            Dim OldRealFiles1 As FileInfo() = di.GetFiles("rsl.*.*")
            Dim OldRealFiles2 As FileInfo() = di.GetFiles("wrfbdy_d??")
            Dim OldRealFiles3 As FileInfo() = di.GetFiles("wrfinput_d??")
            Dim OldWRFFiles As FileInfo() = di.GetFiles("wrfout_d*")
            Dim WPSFolder_slash, WRFFolder_slash, WRF_MPIExe, WRF_MPIExe_slash As String
            Dim WRF_Run_LogFile As String


            '1. deleting old link files
            LogThis("Deleting old link files in WRF folder")
            For Each File In OldLinkFiles
                File.Delete()
            Next


            '2.1 ---------------------creating link WRF batch file
            WRFLink_Run_LogFile = "LinkWPS" + "_" + InitialDateStr + "_" + FinalDateStr + ".log"

            WPSFolder_slash = WPSFolder.Replace("\", "/")
            WRFFolder_slash = WRFFolder.Replace("\", "/")

            LogThis("Writing link batch file for WRF link...")

            Dim LinkBatch As New OutData(Path.Combine(WRFFolder, "link.bat"))
            LinkBatch.WriteDataLine("bash -c " + Chr(34) + "ln -sf " + WPSFolder_slash + "/" + "met_em*" + " " + WRFFolder_slash + Chr(34))
            LinkBatch.Finish()


            '2.2---------------------running link WRF
            Try
                With Run
                    Run.StartInfo.WindowStyle = ProcessWindowStyle.Normal
                    .StartInfo.FileName = "bash.exe"
                    .StartInfo.Arguments = " -c " + Chr(34) + "ln -sf " + WPSFolder_slash + "/" + "met_em*" + " " + WRFFolder_slash + Chr(34)
                    If WRF_ScreenOutputToFile = True Then
                        .StartInfo.UseShellExecute = False
                        .StartInfo.RedirectStandardOutput = True
                        .StartInfo.WorkingDirectory = WPSFolder

                        Dim fileToSave As New StreamWriter(Path.Combine(LogPath, WRFLink_Run_LogFile))
                        fileToSave.AutoFlush = True

                        LogThis("Executing WRF Link...")
                        .Start()

                        While Not Run.StandardOutput.EndOfStream
                            stringOutput = .StandardOutput.ReadLine
                            fileToSave.WriteLine(stringOutput)
                        End While
                        fileToSave.Close()
                        fileToSave = Nothing
                    Else
                        LogThis("Executing WRF link")
                        .Start()
                    End If

                    .WaitForExit(1000 * WRF_MaxTime)
                    If Not .HasExited Then
                        .Kill()
                        .Close()
                        Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined: " + WW3_MaxTime.ToString + " seconds")
                    End If
                End With
                Run.Close()

            Catch ex As Exception
                Call UnSuccessfullEnd("WRF link execution run could not be executed. ", ex.Message)
            End Try

            '3. deleting old real files
            LogThis("Deleting old real files in WRF folder")

            'If WRF_Restart = False Then
            For Each File In OldRealFiles1
                    File.Delete()
                Next

                For Each File In OldRealFiles2
                    File.Delete()
                Next

                For Each File In OldRealFiles3
                    File.Delete()
                Next

            'End If

            File.Delete(Path.Combine(WRFFolder, "namelist.output"))

            '4. executing real
            WRFLink_Run_LogFile = "real" + "_" + InitialDateStr + "_" + FinalDateStr + ".log"

            Try
                With Run
                    Run.StartInfo.WindowStyle = ProcessWindowStyle.Normal
                    .StartInfo.FileName = Path.Combine(WRFFolder, WRF_Exe_Real)
                    If WRF_ScreenOutputToFile = True Then
                        .StartInfo.UseShellExecute = False
                        .StartInfo.RedirectStandardOutput = True
                        .StartInfo.WorkingDirectory = WRFFolder

                        Dim fileToSave As New StreamWriter(Path.Combine(LogPath, WRFLink_Run_LogFile))
                        fileToSave.AutoFlush = True

                        LogThis("Executing real...")
                        .Start()

                        While Not Run.StandardOutput.EndOfStream
                            stringOutput = .StandardOutput.ReadLine
                            fileToSave.WriteLine(stringOutput)
                        End While
                        fileToSave.Close()
                        fileToSave = Nothing
                    Else
                        LogThis("Executing real...")
                        .Start()
                    End If

                    .WaitForExit(1000 * WW3_MaxTime)
                    If Not .HasExited Then
                        .Kill()
                        .Close()
                        Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined: " + WW3_MaxTime.ToString + " seconds")
                    End If
                End With
                Run.Close()

                If Not ProgramWasSuccessfull(Path.Combine(WRFFolder, "rsl.out.0000"), "SUCCESS COMPLETE REAL_EM INIT", True) Then

                    Call UnSuccessfullEnd("Real execution run could not be executed.")
                Else
                    LogThis("Real execution run successfully executed.")

                End If

            Catch ex As Exception
                Call UnSuccessfullEnd("Real execution run could not be executed. ", ex.Message)
            End Try

            '5 deleting old WRF output files
            LogThis("Deleting old WRF files")
            'For Each File In OldWRFFiles
            '    File.Delete()
            'Next

            '6.1 -----creating WRF run batch file

            LogThis("Writing WRF batch file for running WRF ...")
            'WRF_MPIExe = Path.Combine(WRF_MPIPath, "mpiexec")
            Dim WRFBatch As New OutData(Path.Combine(WRFFolder, "WRF.bat"))
            If WRF_MPI Then
                WRF_MPIExe_slash = WRF_MPI_Exe_Path.Replace("C:\", "/cygdrive/c/")
                WRF_MPIExe_slash = WRF_MPIExe_slash.Replace("c:\", "/cygdrive/c/")
                WRF_MPIExe_slash = WRF_MPIExe_slash.Replace("\", "/")
                WRFBatch.WriteDataLine("bash -c " + Chr(34) + WRF_MPIExe_slash + " -n " + WRF_MPI_Num_Processors.ToString + " " + WRFFolder_slash + "/" + WRF_Exe_WRF + Chr(34))
            Else
                WRFBatch.WriteDataLine(Path.Combine(WRFFolder, WRF_Exe_WRF))

            End If
            WRFBatch.Finish()

            '6.2 executing WRF
            WRF_Run_LogFile = "WRF" + "_" + InitialDateStr + "_" + FinalDateStr + ".log"

            Try
                With Run
                    Run.StartInfo.WindowStyle = ProcessWindowStyle.Normal
                    If WRF_MPI Then
                        WRF_MPI_Exe_Path_Renamed = WRF_MPI_Exe_Path.Replace("\", "/")
                        WRF_MPI_Exe_Path_Renamed = WRF_MPI_Exe_Path_Renamed.Replace(":", "")
                        .StartInfo.FileName = "bash.exe"
                        .StartInfo.Arguments = " -c " + Chr(34) + "/cygdrive/" + WRF_MPI_Exe_Path_Renamed + " -n " + WRF_MPI_Num_Processors.ToString + " ./" + WRF_Exe_WRF + Chr(34)
                    Else
                        .StartInfo.FileName = Path.Combine(WRFFolder, WRF_Exe_WRF)

                    End If

                    If WRF_ScreenOutputToFile = True Then
                        .StartInfo.UseShellExecute = False
                        .StartInfo.RedirectStandardOutput = True
                        .StartInfo.WorkingDirectory = WRFFolder

                        Dim fileToSave As New StreamWriter(Path.Combine(LogPath, WRF_Run_LogFile))
                        fileToSave.AutoFlush = True

                        LogThis("Executing WRF...")
                        .Start()

                        While Not Run.StandardOutput.EndOfStream
                            stringOutput = .StandardOutput.ReadLine
                            fileToSave.WriteLine(stringOutput)
                        End While
                        fileToSave.Close()
                        fileToSave = Nothing
                    Else
                        LogThis("Executing WRF...")
                        .Start()
                    End If

                    .WaitForExit(1000 * WRF_MaxTime)
                    If Not .HasExited Then
                        .Kill()
                        .Close()
                        Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined: " + WRF_MaxTime.ToString + " seconds")
                    End If
                End With
                Run.Close()

                '6.3 -----Check if WRF run was successfull and write success file

                Call CheckIfRunOK()

                If WRF_HandleHDF5Outputs Then
                    Call WRF_ConvertToHDF5()
                End If


            Catch ex As Exception
                Call UnSuccessfullEnd("WRF execution run could not be executed. ", ex.Message)
            End Try

        End If
    End Sub
    Sub WRF_ConvertToHDF5()

        For Each Model As Model In Models

            If IO.File.Exists(Path.Combine(WRFFolder, "ConvertToHDF5Action.dat")) Then
                IO.File.Delete(Path.Combine(WRFFolder, "ConvertToHDF5Action.dat"))
            End If

            If IO.File.Exists("WRF.hdf5") Then
                IO.File.Delete("WRF.hdf5")
            End If
            Dim WRFResultFileName As String
            Dim NewWRFResultFileName As String = Model.WRFResultFileName.Replace("", "-")
            FileSystem.Rename(Path.Combine(WRFFolder, Model.WRFResultFileName), Path.Combine(WRFFolder, NewWRFResultFileName))
            WRFResultFileName = NewWRFResultFileName
            LogThis("Converting WRF output file to HDF5 format in model " + Model.RunID.ToString("00") + "...")

            Dim InputConfigFile As New OutData(Path.Combine(WRFFolder, "ConvertToHDF5Action.dat"))
            With InputConfigFile
                .WriteDataLine("<begin_file>")
                .WriteDataLine("ACTION                      : CONVERT WRF FORMAT")
                .WriteDataLine("FILENAME                    : " + WRFResultFileName)
                .WriteDataLine("OUTPUT_GRID_FILENAME        : " + Model.Name + ".dat")
                .WriteDataLine("OUTPUTFILENAME              : " + Model.Name + ".hdf5")
                .WriteDataLine("COMPUTE_WINDSTRESS          : 1 ")
                .WriteDataLine("COMPUTE_WINDMODULUS         : 1 ")
                .WriteDataLine("COMPUTE_RELATIVE_HUMIDITY   : 1 ")
                .WriteDataLine("COMPUTE_PRECIPITATION       : 1 ")
                .WriteDataLine("COMPUTE_MSLP                : 1 ")
                .WriteBlankLine()
                .WriteDataLine("<<BeginFields>>")
                .WriteDataLine("solar radiation")
                .WriteDataLine("air temperature")
                .WriteDataLine("wind velocity X")
                .WriteDataLine("wind velocity Y")
                .WriteDataLine("sensible heat")
                .WriteDataLine("latent heat")
                .WriteDataLine("sea water temperature")
                .WriteDataLine("downward long wave radiation")
                .WriteDataLine("upward long wave radiation")
                .WriteDataLine("top outgoing shortwave radiation")
                .WriteDataLine("atmospheric pressure")
                .WriteDataLine("terrain")
                .WriteDataLine("<<EndFields>>")
                .WriteDataLine("<end_file>")
                .Finish()

            End With

            Dim Run As Process = New Process
            Dim stringOutput As String
            Try
                With Run

                    .StartInfo.FileName = Path.Combine(WRFFolder, WRF_Exe_HandleHDF5Outputs)

                    If WRF_ScreenOutputToFile = True Then
                        .StartInfo.UseShellExecute = False
                        .StartInfo.RedirectStandardOutput = True

                        .StartInfo.WorkingDirectory = WRFFolder

                        Dim fileToSave As New StreamWriter(Path.Combine(WRFFolder, "WRF_to_HDF5.log"))
                        fileToSave.AutoFlush = True

                        LogThis("Executing ConvertToHDF5 in model " + Model.RunID.ToString("00") + "...")
                        .Start()

                        While Not .StandardOutput.EndOfStream
                            stringOutput = .StandardOutput.ReadLine
                            fileToSave.WriteLine(stringOutput)
                        End While
                        fileToSave.Close()
                        fileToSave = Nothing

                    Else
                        LogThis("Executing ConvertToHDF5 in model" + Model.RunID.ToString("00") + "...")
                        .Start()
                    End If

                    .WaitForExit(1000 * WW3_MaxTime)
                    If Not .HasExited Then
                        .Kill()
                        .Close()
                        Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined: " + WW3_MaxTime.ToString + " seconds")
                    End If

                    .Close()
                End With
            Catch ex As Exception
                Call UnSuccessfullEnd("Converting WRF output file to HDF5 format was not successful.", ex.Message)
            End Try



        Next
        LogThis("Done!")



    End Sub
    Sub ConfigureDatesInWW3()
        LogThis("Setting up running dates...")

        For Each Model As Model In Models
            Dim countlines As Integer
            Dim ModelWorkPath As String = Path.Combine(MainPath, Model.Path)
            Dim FilenameToChangeDates(3) As String
            FilenameToChangeDates(1) = Path.Combine(ModelWorkPath, "ww3_outf.inp")
            FilenameToChangeDates(2) = Path.Combine(ModelWorkPath, "ww3_outp.inp")
            FilenameToChangeDates(3) = Path.Combine(ModelWorkPath, "ww3_shel.inp")

            For i = 1 To 3
                Dim sr As New StreamReader(FilenameToChangeDates(i))
                Dim sFile As String = String.Empty
                Dim textLine, filteredTextLine, NewTextLine, textLineSplit() As String
                Dim OldChar, NewChar, NewCharIni, NewCharFin As String
                Dim LinesAfterSTOPSTRING As Integer = -1
                Dim STOPSTRING As Boolean = False
                countlines = 0
                While (sr.Peek() <> -1)
                    textLine = sr.ReadLine

                    countlines += 1

                    filteredTextLine = textLine.Replace(Chr(9), " ")
                    filteredTextLine = filteredTextLine.Replace("      ", " ")
                    filteredTextLine = filteredTextLine.Replace("     ", " ")
                    filteredTextLine = filteredTextLine.Replace("    ", " ")
                    filteredTextLine = filteredTextLine.Replace("   ", " ")
                    filteredTextLine = filteredTextLine.Replace("  ", " ")


                    If i = 1 Or i = 2 Then
                        If countlines = 2 Then
                            textLineSplit = Trim(filteredTextLine).Split(" ")
                            OldChar = textLineSplit(0)
                            NewChar = InitialDate.ToString("yyyyMMdd")
                            NewTextLine = textLine.Replace(OldChar, NewChar)
                            textLine = NewTextLine
                        End If
                    ElseIf i = 3 Then
                        'ww3_shel.inp
                        If countlines = 11 Then
                            textLineSplit = Trim(filteredTextLine).Split(" ")

                            OldChar = textLineSplit(0)
                            NewChar = InitialDate.ToString("yyyyMMdd")
                            NewTextLine = textLine.Replace(OldChar, NewChar)
                            textLine = NewTextLine

                        ElseIf countlines = 12 Then
                            textLineSplit = Trim(filteredTextLine).Split(" ")

                            OldChar = textLineSplit(0)
                            NewChar = FinalDate.ToString("yyyyMMdd")
                            NewTextLine = textLine.Replace(OldChar, NewChar)
                            textLine = NewTextLine

                        ElseIf countlines = 16 Or countlines = 59 Then
                            textLineSplit = Trim(filteredTextLine).Split(" ")
                            ' replace initial date
                            OldChar = textLineSplit(0)
                            NewCharIni = InitialDate.ToString("yyyyMMdd")
                            'NewTextLine = textLine.Replace(OldChar, NewChar)
                            'textLine = NewTextLine

                            ' replace final date
                            OldChar = textLineSplit(3)
                            NewCharFin = FinalDate.ToString("yyyyMMdd")
                            'NewTextLine = textLine.Replace(OldChar, NewChar)
                            'textLine = NewTextLine

                            textLine = "  " + NewCharIni + " " + textLineSplit(1) + " " + AdjustColumnSpaces(textLineSplit(2), 5) + "  " + NewCharFin + " " + textLineSplit(4)
                        ElseIf countlines > 18 Then
                            If InStr(textLine, "STOPSTRING") <> 0 Then
                                STOPSTRING = True
                            End If

                            If STOPSTRING = True Then
                                LinesAfterSTOPSTRING += 1
                                If LinesAfterSTOPSTRING > 0 And LinesAfterSTOPSTRING < 5 Then
                                    textLineSplit = Trim(filteredTextLine).Split(" ")
                                    ' replace initial date
                                    OldChar = textLineSplit(0)
                                    NewCharIni = InitialDate.ToString("yyyyMMdd")
                                    'NewTextLine = textLine.Replace(OldChar, NewChar)
                                    'textLine = NewTextLine

                                    ' replace final date
                                    OldChar = textLineSplit(3)
                                    NewCharFin = FinalDate.ToString("yyyyMMdd")
                                    'NewTextLine = textLine.Replace(OldChar, NewChar)
                                    'textLine = NewTextLine
                                    textLine = "  " + NewCharIni + " " + textLineSplit(1) + " " + AdjustColumnSpaces(textLineSplit(2), 5) + "  " + NewCharFin + " " + textLineSplit(4)
                                End If
                            End If
                        End If

                    End If

                    sFile += textLine + vbCrLf
                End While
                sr.Close()
                sr = Nothing

                Dim sw As StreamWriter = New StreamWriter(FilenameToChangeDates(i), False)
                sw.Write(sFile)
                sw.Close()
                sw = Nothing

            Next
        Next

    End Sub
    Function AdjustColumnSpaces(ByVal strIn As String, ByVal NbrSpaces As Integer) As String
        Dim NbrSpacesToAdd As Integer
        AdjustColumnSpaces = strIn
        NbrSpacesToAdd = NbrSpaces - strIn.Length
        For i = 1 To NbrSpacesToAdd
            AdjustColumnSpaces = " " + AdjustColumnSpaces
        Next
        Return AdjustColumnSpaces
    End Function

    Sub RunWW3()

        Dim ForecastRun As Process = New Process
        Dim stringOutput As String
        Dim ModelWorkPath, WW3ModelLogPath As String
        Dim ExeList(WW3_NbrExes) As String
        Dim ExeLogFileList(WW3_NbrExes) As String
        Dim ExeLogFileFullPathList(WW3_NbrExes) As String
        Dim CountModels As Integer = 0

        ExeList(1) = WW3_Exe_HandleGrid
        ExeList(2) = WW3_Exe_HandleInputs
        ExeList(3) = WW3_Exe
        ExeList(4) = WW3_Exe_HandleFieldOutputs
        ExeList(5) = WW3_Exe_HandlePointOutputs
        If WW3_NbrExes = 6 Then
            ExeList(6) = WW3_Exe_HandleHDF5Outputs
        End If

        For i = 1 To WW3_NbrExes
            ExeLogFileList(i) = Path.GetFileNameWithoutExtension(ExeList(i)) + "_" + InitialDateStr + "_" + FinalDateStr + ".log"
        Next
        WW3_Run_LogFile = ExeLogFileList(5)

        Try
            With ForecastRun

                ForecastRun.StartInfo.WindowStyle = ProcessWindowStyle.Normal


                ModelWorkPath = ""
                WW3ModelLogPath = ""
                For Each Model As Model In Models
                    CountModels += 1
                    LogThis("Handling Domain " + Model.Name + " (" + CountModels.ToString + " of " + Models.Count.ToString + ")")


                    ModelWorkPath = Path.Combine(MainPath, Model.Path)
                    WW3ModelLogPath = Path.Combine(ModelWorkPath, "logs")

                    If Not Directory.Exists(WW3ModelLogPath) Then
                        Directory.CreateDirectory(WW3ModelLogPath)
                    End If

                    For i = 1 To WW3_NbrExes
                        ExeLogFileFullPathList(i) = Path.Combine(WW3ModelLogPath, ExeLogFileList(i))
                    Next

                    Call CleanOutputs(Model, WW3_NbrExes, WW3ModelLogPath, ExeLogFileList)

                    Call HandleSubDomainsInWW3(Model)

                    Call GatherBoundaryConditions(Model)

                    Call GatherRestartFiles(Model)

                    WW3_Run_LogFile_FullPath = ExeLogFileFullPathList(5)

                    If WW3_ScreenOutputPathInNetwork <> "" Then
                        WW3_Run_LogFile_FullPath_In_Network = WW3_ScreenOutputPathInNetwork
                    Else
                        WW3_Run_LogFile_FullPath_In_Network = WW3_Run_LogFile_FullPath
                    End If

                    For i = 1 To WW3_NbrExes

                        '-------------------------------------------
                        ForecastRun.StartInfo.FileName = Path.Combine(ModelWorkPath, ExeList(i))

                        If WW3_ScreenOutputToFile = True Then
                            .StartInfo.UseShellExecute = False
                            .StartInfo.RedirectStandardOutput = True

                            .StartInfo.WorkingDirectory = ModelWorkPath

                            Dim fileToSave As New StreamWriter(ExeLogFileFullPathList(i))
                            fileToSave.AutoFlush = True

                            LogThis("Executing WW3 (exe " + i.ToString + " of " + WW3_NbrExes.ToString + " - " + ExeList(i) + ")...")
                            'LogThis("Executing WW3 (" + ExeList(i) + ")...")
                            'ForecastRun.StartInfo.FileName = "notepad.exe"
                            .Start()

                            While Not ForecastRun.StandardOutput.EndOfStream
                                stringOutput = .StandardOutput.ReadLine
                                fileToSave.WriteLine(stringOutput)
                            End While
                            fileToSave.Close()
                            fileToSave = Nothing

                        Else
                            LogThis("Executing WW3 (" + i.ToString + " of " + WW3_NbrExes.ToString + "- " + ExeList(i) + ")...")
                            '                            LogThis("Executing WW3 (" + ExeList(i) + ")...")
                            .Start()
                        End If

                        .WaitForExit(1000 * WW3_MaxTime)
                        If Not .HasExited Then
                            .Kill()
                            .Close()
                            Call UnSuccessfullEnd("ART Exceeded maximum time defined", "ART System automatically stopped - exceeded maximum time defined: " + WW3_MaxTime.ToString + " seconds")
                        End If

                        '----------------------------------------------------------

                    Next

                    'Check if WW3 run was successfull and write success file
                    Call CheckIfRunOK()

                    If Run_PostProcessing Then
                        Call ManagePostProcessingTools(ModelWorkPath)
                    End If

                    'Backup and store the forecast results and restart files
                    Call BackUpWW3Simulation(Model)

                Next
            End With
            ForecastRun.Close()

        Catch ex As Exception
            Call UnSuccessfullEnd("WW3 automatic run could not be executed. ", ex.Message)
        End Try
        LogThis("Done!")


    End Sub

    Sub CheckIfRunOK()

        If Run_MOHID Then

            If IO.File.Exists("MOHID_SUCCESS.dat") Then
                IO.File.Delete("MOHID_SUCCESS.dat")
            End If

            '        If ProgramWasSuccessfull(FatherModelWorkPath + "\" + MOHID_Run_LogFile, "Program Mohid Water successfully terminated") Then
            If ProgramWasSuccessfull(MOHID_Run_LogFile_FullPath, "Program Mohid Water successfully terminated") Or
                ProgramWasSuccessfull(MOHID_Run_LogFile_FullPath, "Program Mohid Land successfully terminated") Then

                Dim MOHIDSuccessRun As New OutData("MOHID_SUCCESS.dat")
                MOHIDSuccessRun.WriteBlankLine()
                MOHIDSuccessRun.WriteDataLine("OPERATIONAL MODEL SUCCESSFULL RUN FILE")
                MOHIDSuccessRun.WriteBlankLine()
                MOHIDSuccessRun.WriteDataLine("DO NOT EDIT, CHANGE, MOVE, DELETE THIS FILE!")
                MOHIDSuccessRun.WriteBlankLine()
                MOHIDSuccessRun.WriteBlankLine()
                MOHIDSuccessRun.WriteDataLine("Sucessfull MOHID forecast for the following period:")
                MOHIDSuccessRun.WriteDataLine("START", InitialDate)
                MOHIDSuccessRun.WriteDataLine("END", FinalDate)
                MOHIDSuccessRun.Finish()

            Else

                Call UnSuccessfullEnd("MOHID Run was not successfull!")

            End If
        ElseIf (Run_WW3) Then
            If IO.File.Exists("WW3_SUCCESS.dat") Then
                IO.File.Delete("WW3_SUCCESS.dat")
            End If

            If ProgramWasSuccessfull(WW3_Run_LogFile_FullPath, "End of program") Then

                Dim WW3SuccessRun As New OutData("WW3_SUCCESS.dat")
                WW3SuccessRun.WriteBlankLine()
                WW3SuccessRun.WriteDataLine("OPERATIONAL MODEL SUCCESSFULL RUN FILE")
                WW3SuccessRun.WriteBlankLine()
                WW3SuccessRun.WriteDataLine("DO NOT EDIT, CHANGE, MOVE, DELETE THIS FILE!")
                WW3SuccessRun.WriteBlankLine()
                WW3SuccessRun.WriteBlankLine()
                WW3SuccessRun.WriteDataLine("Sucessfull WW3 forecast for the following period:")
                WW3SuccessRun.WriteDataLine("START", InitialDate)
                WW3SuccessRun.WriteDataLine("END", FinalDate)
                WW3SuccessRun.Finish()

            Else

                Call UnSuccessfullEnd("WW3 Run was not successfull!")

            End If
        ElseIf (Run_WRF) Then
            If IO.File.Exists("WRF_SUCCESS.dat") Then
                IO.File.Delete("WRF_SUCCESS.dat")
            End If
            WRF_Run_LogFile_FullPath = Path.Combine(WRFFolder, "rsl.out.0000")

            Dim SuccessfulRun As Boolean
            For Each Model As Model In Models
                SuccessfulRun = False
                Dim ModelIDString As String = Model.RunID.ToString("00")
                '                Model.WRFResultFileName = "wrfout_d" + ModelIDString + "_" + InitialDate.ToString("yyyy-MM-dd_HH" + "" + "mm" + "" + "ss")
                If ProgramWasSuccessfull(WRF_Run_LogFile_FullPath, "wrf: SUCCESS COMPLETE WRF", True) And File.Exists(Path.Combine(WRFFolder, Model.WRFResultFileName)) Then
                    SuccessfulRun = True
                Else
                    Call UnSuccessfullEnd("WRF Run was not successfull!")
                End If
            Next


            Dim WRFSuccessRun As New OutData("WRF_SUCCESS.dat")
            WRFSuccessRun.WriteBlankLine()
            WRFSuccessRun.WriteDataLine("OPERATIONAL MODEL SUCCESSFULL RUN FILE")
            WRFSuccessRun.WriteBlankLine()
            WRFSuccessRun.WriteDataLine("DO NOT EDIT, CHANGE, MOVE, DELETE THIS FILE!")
            WRFSuccessRun.WriteBlankLine()
            WRFSuccessRun.WriteBlankLine()
            WRFSuccessRun.WriteDataLine("Sucessfull WRF forecast for the following period:")
            WRFSuccessRun.WriteDataLine("START", InitialDate)
            WRFSuccessRun.WriteDataLine("END", FinalDate)
            WRFSuccessRun.Finish()

        End If

    End Sub
    Sub FindOutputs(ByVal Model As Model, ByRef OutputFile As Collection)
        Dim ModelWorkPath As String = Path.Combine(MainPath, Model.Path)
        '        Dim OutputFile As New Collection
        Dim di As DirectoryInfo = New DirectoryInfo(ModelWorkPath)
        Dim nestfiles As FileInfo() = di.GetFiles("nest?.ww3")
        Dim restartfiles As FileInfo() = di.GetFiles("restart???.ww3")

        For i = 0 To restartfiles.Length - 1
            OutputFile.Add(restartfiles(i).Name)
        Next

        For i = 0 To nestfiles.Length - 1
            OutputFile.Add(nestfiles(i).Name)
        Next

        Dim outputfiles As FileInfo()
        'search different point file types
        For i = 0 To OutputExtensionList.Length - 1
            outputfiles = di.GetFiles("*" + OutputExtensionList(i))
            ' search different point files from each file type
            For j = 0 To outputfiles.Length - 1
                OutputFile.Add(outputfiles(j).Name)
            Next
        Next
        nestfiles = Nothing
        restartfiles = Nothing
    End Sub
    Sub CleanOutputs(ByVal Model As Model, ByVal NbrExes As Integer, ByVal WW3ModelLogPath As String, ByVal ExeLogFileList() As String)
        Dim ModelWorkPath As String = Path.Combine(MainPath, Model.Path)
        Dim FileToDelete As New Collection
        Dim ExeLogFileFullPathList(NbrExes) As String
        LogThis("Deleting outputs from previous run in model " + Model.Name.ToString + "...")
        Call FindOutputs(Model, FileToDelete)


        Try

            For i = 1 To NbrExes
                If ExeLogFileList(i) <> "" Then

                End If
                ExeLogFileFullPathList(i) = Path.Combine(WW3ModelLogPath, ExeLogFileList(i))
                If File.Exists(ExeLogFileFullPathList(i)) Then
                    File.Delete(ExeLogFileFullPathList(i))
                End If
            Next


            For i = 1 To FileToDelete.Count
                If File.Exists(Path.Combine(ModelWorkPath, FileToDelete(i))) Then
                    File.Delete(Path.Combine(ModelWorkPath, FileToDelete(i)))
                End If
            Next
        Catch ex As Exception
            LogThis("Could not delete old files from previous run in model " + Model.Name.ToString + " - ", ex.Message)
        End Try

    End Sub

    Sub BackUpMOHIDSimulation()


        Dim Day1Date, Day2Date As String
        Day1Date = InitialDate.ToString("yyyyMMdd") + "-000000"
        Day2Date = InitialDate.AddDays(1).ToString("yyyyMMdd") + "-000000"

        Dim CountProcessors_backupfin As Integer = 0
        Dim CountProcessors_storagefin As Integer = 0
        Dim CountProcessors_backuphdf5 As Integer = 0
        Dim CountProcessors_storagehdf5 As Integer = 0

        'backup up the restart files
        For Each Model As Model In Models

            Dim BackUpDir As String = Model.BackUpPath + "Restart\" + InitialDateStr + "_" + FinalDateStr + "\"
            Dim ErrorMessage As String = "Could not copy (backup) restart file - " + Model.Name
            If Not Model.HasSolutionFromFile Then
                LogThis("Backing up restart files from model " + Model.Name.ToString + "...")
                'Backup
                IO.Directory.CreateDirectory(BackUpDir)


                If MPI And MPI_KeepDecomposedFiles And Model.MPI_Num_Processors > 1 Then
                    For i = (CountProcessors_backupfin + 1) To (CountProcessors_backupfin + Model.MPI_Num_Processors)
                        If Model.HasHydrodynamics Then
                            '                FileCopy(Model.Path + "res\Hydrodynamic_0.fin", BackUpDir + "Hydrodynamic_0.fin", ErrorMessage, False)
                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_Hydrodynamic_1.fin", BackUpDir + "MPI_" + i.ToString + "_Hydrodynamic_1.fin", ErrorMessage, False)
                        End If

                        If Model.HasGOTM Then
                            '                    FileCopy(Model.Path + "res\GOTM_0.fin", BackUpDir + "GOTM_0.fin", ErrorMessage, False)
                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_GOTM_1.fin", BackUpDir + "MPI_" + i.ToString + "_GOTM_1.fin", ErrorMessage, False)
                        End If

                        If Model.HasWaterProperties Then
                            '                    FileCopy(Model.Path + "res\WaterProperties_0.fin5", BackUpDir + "WaterProperties_0.fin5", ErrorMessage, False)
                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_WaterProperties_1.fin5", BackUpDir + "MPI_" + i.ToString + "_WaterProperties_1.fin5", ErrorMessage, False)
                        End If

                        If Model.HasInterfaceSedimentWater Then
                            '                    FileCopy(Model.Path + "res\InterfaceSedimentWater_0.fin5", BackUpDir + "InterfaceSedimentWater_0.fin5", ErrorMessage, False)
                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_InterfaceSedimentWater_1.fin5", BackUpDir + "MPI_" + i.ToString + "_InterfaceSedimentWater_1.fin5", ErrorMessage, False)
                        End If
                        CountProcessors_backupfin += 1
                    Next
                Else

                    If Model.HasHydrodynamics Then
                        '                FileCopy(Model.Path + "res\Hydrodynamic_0.fin", BackUpDir + "Hydrodynamic_0.fin", ErrorMessage, False)
                        FileCopy(Model.Path + "res\Hydrodynamic_1.fin", BackUpDir + "Hydrodynamic_1.fin", ErrorMessage, False)
                    End If

                    If Model.HasGOTM Then
                        '                    FileCopy(Model.Path + "res\GOTM_0.fin", BackUpDir + "GOTM_0.fin", ErrorMessage, False)
                        FileCopy(Model.Path + "res\GOTM_1.fin", BackUpDir + "GOTM_1.fin", ErrorMessage, False)
                    End If

                    If Model.HasWaterProperties Then
                        '                    FileCopy(Model.Path + "res\WaterProperties_0.fin5", BackUpDir + "WaterProperties_0.fin5", ErrorMessage, False)
                        FileCopy(Model.Path + "res\WaterProperties_1.fin5", BackUpDir + "WaterProperties_1.fin5", ErrorMessage, False)
                    End If

                    If Model.HasInterfaceSedimentWater Then
                        '                    FileCopy(Model.Path + "res\InterfaceSedimentWater_0.fin5", BackUpDir + "InterfaceSedimentWater_0.fin5", ErrorMessage, False)
                        FileCopy(Model.Path + "res\InterfaceSedimentWater_1.fin5", BackUpDir + "InterfaceSedimentWater_1.fin5", ErrorMessage, False)
                    End If

                End If

            End If

            If Model.HasLagrangian Then
                IO.Directory.CreateDirectory(BackUpDir)
                '                FileCopy(Model.Path + "res\Lagrangian_0.fin", BackUpDir + "Lagrangian_0.fin", ErrorMessage, False)
                FileCopy(Model.Path + "res\Lagrangian_1.fin", BackUpDir + "Lagrangian_1.fin", ErrorMessage, False)
            End If

            '----------------MOHID LAND
            Dim source_string As String
            Dim target_string As String
            If Model.HasBasin Then
                IO.Directory.CreateDirectory(BackUpDir)
                source_string = "res\Basin_1.fin"
                target_string = "Basin_1.fin"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasDrainageNetwork Then
                IO.Directory.CreateDirectory(BackUpDir)
                source_string = "res\Drainage Network_1.fin"
                target_string = "Drainage Network_1.fin"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasPorousMedia Then
                IO.Directory.CreateDirectory(BackUpDir)
                source_string = "res\Porous Media_1.fin"
                target_string = "Porous Media_1.fin"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasPorousMediaProperties Then
                IO.Directory.CreateDirectory(BackUpDir)
                source_string = "res\PorousMedia Properties_1.fin"
                target_string = "PorousMedia Properties_1.fin"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasRunoff Then
                IO.Directory.CreateDirectory(BackUpDir)
                source_string = "res\RunOff_1.fin"
                target_string = "RunOff_1.fin"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasRunoffProperties Then
                IO.Directory.CreateDirectory(BackUpDir)
                source_string = "res\RunOff Properties_1.fin"
                target_string = "RunOff Properties_1.fin"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasVegetation Then
                IO.Directory.CreateDirectory(BackUpDir)
                source_string = "res\Vegetation_1.fin"
                target_string = "Vegetation_1.fin"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasReservoirs Then
                IO.Directory.CreateDirectory(BackUpDir)
                source_string = "res\Reservoirs_1.fin"
                target_string = "Reservoirs_1.fin"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasIrrigation Then
                IO.Directory.CreateDirectory(BackUpDir)
                source_string = "res\Irrigation_1.fin"
                target_string = "Irrigation_1.fin"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            '----------------MOHID LAND

            'storage
            If Model.StoragePath <> Nothing Then
                LogThis("Storing restart files from model " + Model.Name.ToString + "...")
                Dim StorageDir As String = Model.StoragePath + "Restart\" + InitialDateStr + "_" + FinalDateStr + "\"
                ErrorMessage = "Could not copy (storage) restart file - " + Model.Name

                If Not Model.HasSolutionFromFile Then
                    IO.Directory.CreateDirectory(StorageDir)

                    If MPI And MPI_KeepDecomposedFiles And Model.MPI_Num_Processors > 1 Then
                        For i = (CountProcessors_storagefin + 1) To (CountProcessors_storagefin + Model.MPI_Num_Processors)
                            If Model.HasHydrodynamics Then
                                '                    FileCopy(Model.Path + "res\Hydrodynamic_0.fin", StorageDir + "Hydrodynamic_0.fin", ErrorMessage, False)
                                FileCopy(Model.Path + "res\MPI_" + i.ToString + "_Hydrodynamic_1.fin", StorageDir + "MPI_" + i.ToString + "_Hydrodynamic_1.fin", ErrorMessage, False)
                            End If

                            If Model.HasGOTM Then
                                '                        FileCopy(Model.Path + "res\GOTM_0.fin", StorageDir + "GOTM_0.fin", ErrorMessage, False)
                                FileCopy(Model.Path + "res\MPI_" + i.ToString + "_GOTM_1.fin", StorageDir + "MPI_" + i.ToString + "_GOTM_1.fin", ErrorMessage, False)
                            End If

                            If Model.HasWaterProperties Then
                                '                        FileCopy(Model.Path + "res\WaterProperties_0.fin5", StorageDir + "WaterProperties_0.fin5", ErrorMessage, False)
                                FileCopy(Model.Path + "res\MPI_" + i.ToString + "_WaterProperties_1.fin5", StorageDir + "MPI_" + i.ToString + "_WaterProperties_1.fin5", ErrorMessage, False)
                            End If

                            If Model.HasInterfaceSedimentWater Then
                                '                        FileCopy(Model.Path + "res\InterfaceSedimentWater_0.fin5", StorageDir + "InterfaceSedimentWater_0.fin5", ErrorMessage, False)
                                FileCopy(Model.Path + "res\MPI_" + i.ToString + "_InterfaceSedimentWater_1.fin5", StorageDir + "MPI_" + i.ToString + "_InterfaceSedimentWater_1.fin5", ErrorMessage, False)
                            End If
                            CountProcessors_storagefin += 1
                        Next
                    Else

                        If Model.HasHydrodynamics Then
                            '                    FileCopy(Model.Path + "res\Hydrodynamic_0.fin", StorageDir + "Hydrodynamic_0.fin", ErrorMessage, False)
                            FileCopy(Model.Path + "res\Hydrodynamic_1.fin", StorageDir + "Hydrodynamic_1.fin", ErrorMessage, False)
                        End If

                        If Model.HasGOTM Then
                            '                        FileCopy(Model.Path + "res\GOTM_0.fin", StorageDir + "GOTM_0.fin", ErrorMessage, False)
                            FileCopy(Model.Path + "res\GOTM_1.fin", StorageDir + "GOTM_1.fin", ErrorMessage, False)
                        End If

                        If Model.HasWaterProperties Then
                            '                        FileCopy(Model.Path + "res\WaterProperties_0.fin5", StorageDir + "WaterProperties_0.fin5", ErrorMessage, False)
                            FileCopy(Model.Path + "res\WaterProperties_1.fin5", StorageDir + "WaterProperties_1.fin5", ErrorMessage, False)
                        End If

                        If Model.HasInterfaceSedimentWater Then
                            '                        FileCopy(Model.Path + "res\InterfaceSedimentWater_0.fin5", StorageDir + "InterfaceSedimentWater_0.fin5", ErrorMessage, False)
                            FileCopy(Model.Path + "res\InterfaceSedimentWater_1.fin5", StorageDir + "InterfaceSedimentWater_1.fin5", ErrorMessage, False)
                        End If

                    End If
                End If


                If Model.HasLagrangian Then
                    IO.Directory.CreateDirectory(StorageDir)
                    '                    FileCopy(Model.Path + "res\Lagrangian_0.fin", StorageDir + "Lagrangian_0.fin", ErrorMessage, False)
                    FileCopy(Model.Path + "res\Lagrangian_1.fin", StorageDir + "Lagrangian_1.fin", ErrorMessage, False)
                End If

                '----------------MOHID LAND
                If Model.HasBasin Then
                    IO.Directory.CreateDirectory(StorageDir)
                    source_string = "res\Basin_1.fin"
                    target_string = "Basin_1.fin"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasDrainageNetwork Then
                    IO.Directory.CreateDirectory(StorageDir)
                    source_string = "res\Drainage Network_1.fin"
                    target_string = "Drainage Network_1.fin"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasPorousMedia Then
                    IO.Directory.CreateDirectory(StorageDir)
                    source_string = "res\Porous Media_1.fin"
                    target_string = "Porous Media_1.fin"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasPorousMediaProperties Then
                    IO.Directory.CreateDirectory(StorageDir)
                    source_string = "res\PorousMedia Properties_1.fin"
                    target_string = "PorousMedia Properties_1.fin"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasRunoff Then
                    IO.Directory.CreateDirectory(StorageDir)
                    source_string = "res\RunOff_1.fin"
                    target_string = "RunOff_1.fin"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasRunoffProperties Then
                    IO.Directory.CreateDirectory(StorageDir)
                    source_string = "res\RunOff Properties_1.fin"
                    target_string = "RunOff Properties_1.fin"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasVegetation Then
                    IO.Directory.CreateDirectory(StorageDir)
                    source_string = "res\Vegetation_1.fin"
                    target_string = "Vegetation_1.fin"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasReservoirs Then
                    IO.Directory.CreateDirectory(StorageDir)
                    source_string = "res\Reservoirs_1.fin"
                    target_string = "Reservoirs_1.fin"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasIrrigation Then
                    IO.Directory.CreateDirectory(StorageDir)
                    source_string = "res\Irrigation_1.fin"
                    target_string = "Irrigation_1.fin"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                '----------------MOHID LAND

            End If

        Next

        'backup hdf5 results files
        For Each Model As Model In Models
            Dim BackUpDir As String = Model.BackUpPath + "Results_HDF\" + InitialDateStr + "_" + FinalDateStr + "\"
            Dim ErrorMessage As String = "Could not copy (backup) HDF file - " + Model.Name

            '            If Not Model.HasSolutionFromFile Then
            LogThis("Backing up HDF results files from model " + Model.Name.ToString + "...")

            IO.Directory.CreateDirectory(BackUpDir)
            'backup
            If Model.HasHydrodynamics Then
                FileCopy(Model.Path + "res\Hydrodynamic_1.hdf5", BackUpDir + "Hydrodynamic.hdf5", ErrorMessage, False)
            End If
            If Model.HasSurfaceHDF Then
                IO.Directory.CreateDirectory(BackUpDir)
                FileCopy(Model.Path + "res\Hydrodynamic_1_Surface.hdf5", BackUpDir + "Hydrodynamic_Surface.hdf5", ErrorMessage, False)
            End If

            '            If Model.HasOutputWindow Then
            Dim di As DirectoryInfo = New DirectoryInfo(Model.Path + "res\")
            Dim hydro_windows As FileInfo() = di.GetFiles("Hydrodynamic_1_w?.hdf5")
            Dim i As Integer

            For i = 1 To hydro_windows.Length
                FileCopy(Model.Path + "res\Hydrodynamic_1_w" + i.ToString + ".hdf5", BackUpDir + "Hydrodynamic_w" + i.ToString + ".hdf5", ErrorMessage, False)
                For Each ExportWindow As ExportWindow In Model.ExportWindows
                    With ExportWindow
                        If .ID = i Then
                            If .Hydrodynamics Then
                                FileCopy(Model.Path + "res\Hydrodynamic_1_w" + i.ToString + ".hdf5", Path.Combine(.DestinationPath, "Hydrodynamic_w" + i.ToString + ".hdf5"), ErrorMessage, False)
                            End If
                        End If
                    End With
                Next
            Next

            '                FileCopy(Model.Path + "res\Hydrodynamic_1_w1.hdf5", BackUpDir + "Hydrodynamic_w1.hdf5", ErrorMessage, False)
            '            End If

            Dim waterproperties_windows As FileInfo() = di.GetFiles("WaterProperties_1_w?.hdf5")
            If Model.HasWaterProperties Then

                FileCopy(Model.Path + "res\WaterProperties_1.hdf5", BackUpDir + "WaterProperties.hdf5", ErrorMessage, False)

                If Model.HasSurfaceHDF Then
                    FileCopy(Model.Path + "res\WaterProperties_1_Surface.hdf5", BackUpDir + "WaterProperties_Surface.hdf5", ErrorMessage, False)
                End If


                For i = 1 To waterproperties_windows.Length
                    FileCopy(Model.Path + "res\WaterProperties_1_w" + i.ToString + ".hdf5", BackUpDir + "WaterProperties_w" + i.ToString + ".hdf5", ErrorMessage, False)
                    For Each ExportWindow As ExportWindow In Model.ExportWindows
                        With ExportWindow
                            If .ID = i Then
                                If .WaterProperties Then
                                    FileCopy(Model.Path + "res\WaterProperties_1_w" + i.ToString + ".hdf5", Path.Combine(.DestinationPath, "WaterProperties_w" + i.ToString + ".hdf5"), ErrorMessage, False)
                                End If
                            End If
                        End With
                    Next
                Next

                'If Model.HasOutputWindow Then
                '    FileCopy(Model.Path + "res\WaterProperties_1_w1.hdf5", BackUpDir + "WaterProperties_w1.hdf5", ErrorMessage, False)
                'End If

            End If

            If Model.HasInterfaceSedimentWater Then
                FileCopy(Model.Path + "res\InterfaceSedimentWater_1.hdf5", BackUpDir + "InterfaceSedimentWater.hdf5", ErrorMessage, False)
            End If
            '            End If

            If Model.HasAtmosphere Then
                FileCopy(Model.Path + "res\Atmosphere_1.hdf5", BackUpDir + "Atmosphere.hdf5", ErrorMessage, False)
            End If


            If MPI And MPI_KeepDecomposedFiles And MPI_Num_Processors > 1 Then
                For i = (CountProcessors_backuphdf5 + 1) To (CountProcessors_backuphdf5 + Model.MPI_Num_Processors)
                    If Model.HasHydrodynamics Then
                        FileCopy(Model.Path + "res\MPI_" + i.ToString + "_Hydrodynamic_1.hdf5", BackUpDir + "MPI_" + i.ToString + "_Hydrodynamic.hdf5", ErrorMessage, False)
                    End If
                    If Model.HasSurfaceHDF Then
                        IO.Directory.CreateDirectory(BackUpDir)
                        FileCopy(Model.Path + "res\MPI_" + i.ToString + "_Hydrodynamic_1_Surface.hdf5", BackUpDir + "MPI_" + i.ToString + "_Hydrodynamic_Surface.hdf5", ErrorMessage, False)
                    End If

                    '            If Model.HasOutputWindow Then
                    Dim hydro_windows_ As FileInfo() = di.GetFiles("MPI_" + i.ToString + "_Hydrodynamic_1_w?.hdf5")
                    Dim i_ As Integer

                    For i_ = 1 To hydro_windows_.Length
                        FileCopy(Model.Path + "res\MPI_" + i.ToString + "_Hydrodynamic_1_w" + i_.ToString + ".hdf5", BackUpDir + "MPI_" + i.ToString + "_Hydrodynamic_w" + i_.ToString + ".hdf5", ErrorMessage, False)
                        For Each ExportWindow As ExportWindow In Model.ExportWindows
                            With ExportWindow
                                If .ID = i Then
                                    If .Hydrodynamics Then
                                        FileCopy(Model.Path + "res\MPI_" + i.ToString + "_Hydrodynamic_1_w" + i_.ToString + ".hdf5", Path.Combine(.DestinationPath, "MPI_" + i.ToString + "_Hydrodynamic_w" + i.ToString + ".hdf5"), ErrorMessage, False)
                                    End If
                                End If
                            End With
                        Next
                    Next

                    Dim waterproperties_windows_ As FileInfo() = di.GetFiles("MPI_" + i.ToString + "_WaterProperties_1_w?.hdf5")
                    If Model.HasWaterProperties Then

                        FileCopy(Model.Path + "res\MPI_" + i.ToString + "_WaterProperties_1.hdf5", BackUpDir + "MPI_" + i.ToString + "_WaterProperties.hdf5", ErrorMessage, False)

                        If Model.HasSurfaceHDF Then
                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_WaterProperties_1_Surface.hdf5", BackUpDir + "MPI_" + i.ToString + "_WaterProperties_Surface.hdf5", ErrorMessage, False)
                        End If


                        For i_ = 1 To waterproperties_windows_.Length
                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_WaterProperties_1_w" + i_.ToString + ".hdf5", BackUpDir + "MPI_" + i.ToString + "_WaterProperties_w" + i_.ToString + ".hdf5", ErrorMessage, False)
                            For Each ExportWindow As ExportWindow In Model.ExportWindows
                                With ExportWindow
                                    If .ID = i Then
                                        If .Hydrodynamics Then
                                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_WaterProperties_1_w" + i_.ToString + ".hdf5", Path.Combine(.DestinationPath, "MPI_" + i.ToString + "_WaterProperties_w" + i.ToString + ".hdf5"), ErrorMessage, False)
                                        End If
                                    End If
                                End With
                            Next
                        Next

                    End If

                    If Model.HasInterfaceSedimentWater Then
                        FileCopy(Model.Path + "res\MPI_" + i.ToString + "_InterfaceSedimentWater_1.hdf5", BackUpDir + "MPI_" + i.ToString + "_InterfaceSedimentWater.hdf5", ErrorMessage, False)
                    End If

                    If Model.HasAtmosphere Then
                        FileCopy(Model.Path + "res\MPI_" + i.ToString + "_Atmosphere_1.hdf5", BackUpDir + "MPI_" + i.ToString + "_Atmosphere.hdf5", ErrorMessage, False)
                    End If

                    CountProcessors_backuphdf5 += 1
                Next
            End If

            If Model.HasLagrangian Then
                FileCopy(Model.Path + "res\Lagrangian_1.hdf5", BackUpDir + "Lagrangian.hdf5", ErrorMessage, False)
            End If

            '----------------MOHID LAND
            Dim source_string As String
            Dim target_string As String
            If Model.HasBasin Then
                source_string = "res\Basin_1.hdf5"
                target_string = "Basin.hdf5"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasDrainageNetwork Then
                source_string = "res\Drainage Network_1.hdf5"
                target_string = "DrainageNetwork.hdf5"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasPorousMedia Then
                source_string = "res\Porous Media_1.hdf5"
                target_string = "PorousMedia.hdf5"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasPorousMediaProperties Then
                source_string = "res\PorousMedia Properties_1.hdf5"
                target_string = "PorousMediaProperties.hdf5"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasRunoff Then
                source_string = "res\RunOff_1.hdf5"
                target_string = "RunOff.hdf5"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasRunoffProperties Then
                source_string = "res\RunOff Properties_1.hdf5"
                target_string = "RunOffProperties.hdf5"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasVegetation Then
                source_string = "res\Vegetation_1.hdf5"
                target_string = "Vegetation.hdf5"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasReservoirs Then
                source_string = "res\Reservoirs_1.hdf5"
                target_string = "Reservoirs.hdf5"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            If Model.HasIrrigation Then
                source_string = "res\Irrigation_1.hdf5"
                target_string = "Irrigation.hdf5"
                FileCopy(Model.Path + source_string, BackUpDir + target_string, ErrorMessage, True)
            End If
            '----------------MOHID LAND

            'storage
            If Model.StoragePath <> Nothing Then
                Dim StorageDir As String = Model.StoragePath + "Results_HDF\" + InitialDateStr + "_" + FinalDateStr + "\"
                ErrorMessage = "Could not copy (storage) HDF file - " + Model.Name

                '                If Not Model.HasSolutionFromFile Then
                LogThis("Storing HDF results files from model " + Model.Name.ToString + "...")

                IO.Directory.CreateDirectory(StorageDir)

                If Model.HasHydrodynamics Then
                    FileCopy(Model.Path + "res\Hydrodynamic_1.hdf5", StorageDir + "Hydrodynamic.hdf5", ErrorMessage, False)
                End If

                If Model.HasSurfaceHDF Then
                    FileCopy(Model.Path + "res\Hydrodynamic_1_Surface.hdf5", StorageDir + "Hydrodynamic_Surface.hdf5", ErrorMessage, False)
                End If

                'If Model.HasOutputWindow Then
                '    FileCopy(Model.Path + "res\Hydrodynamic_1_w1.hdf5", StorageDir + "Hydrodynamic_w1.hdf5", ErrorMessage, False)
                'End If

                For i = 1 To hydro_windows.Length
                    FileCopy(Model.Path + "res\Hydrodynamic_1_w" + i.ToString + ".hdf5", StorageDir + "Hydrodynamic_w" + i.ToString + ".hdf5", ErrorMessage, False)
                Next

                If Model.HasWaterProperties Then

                    FileCopy(Model.Path + "res\WaterProperties_1.hdf5", StorageDir + "WaterProperties.hdf5", ErrorMessage, False)

                    If Model.HasSurfaceHDF Then
                        FileCopy(Model.Path + "res\WaterProperties_1_Surface.hdf5", StorageDir + "WaterProperties_Surface.hdf5", ErrorMessage, False)
                    End If

                    'If Model.HasOutputWindow Then
                    '    FileCopy(Model.Path + "res\WaterProperties_1_w1.hdf5", StorageDir + "WaterProperties_w1.hdf5", ErrorMessage, False)
                    'End If

                    For i = 1 To waterproperties_windows.Length
                        FileCopy(Model.Path + "res\WaterProperties_1_w" + i.ToString + ".hdf5", StorageDir + "WaterProperties_w" + i.ToString + ".hdf5", ErrorMessage, False)
                    Next

                End If

                If Model.HasInterfaceSedimentWater Then
                    FileCopy(Model.Path + "res\InterfaceSedimentWater_1.hdf5", StorageDir + "InterfaceSedimentWater.hdf5", ErrorMessage, False)
                End If

                '            End If

                If MPI And MPI_KeepDecomposedFiles And Model.MPI_Num_Processors > 1 Then
                    For i = (CountProcessors_storagehdf5 + 1) To (CountProcessors_storagehdf5 + Model.MPI_Num_Processors)


                        If Model.HasHydrodynamics Then
                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_Hydrodynamic_1.hdf5", StorageDir + "MPI_" + i.ToString + "_Hydrodynamic.hdf5", ErrorMessage, False)
                        End If

                        If Model.HasSurfaceHDF Then
                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_Hydrodynamic_1_Surface.hdf5", StorageDir + "MPI_" + i.ToString + "_Hydrodynamic_Surface.hdf5", ErrorMessage, False)
                        End If

                        Dim hydro_windows_ As FileInfo() = di.GetFiles("MPI_" + i.ToString + "_Hydrodynamic_1_w?.hdf5")
                        Dim i_ As Integer

                        For i_ = 1 To hydro_windows_.Length
                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_Hydrodynamic_1_w" + i_.ToString + ".hdf5", StorageDir + "MPI_" + i.ToString + "_Hydrodynamic_w" + i_.ToString + ".hdf5", ErrorMessage, False)
                        Next

                        If Model.HasWaterProperties Then

                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_WaterProperties_1.hdf5", StorageDir + "MPI_" + i.ToString + "_WaterProperties.hdf5", ErrorMessage, False)

                            If Model.HasSurfaceHDF Then
                                FileCopy(Model.Path + "res\MPI_" + i.ToString + "_WaterProperties_1_Surface.hdf5", StorageDir + "MPI_" + i.ToString + "_WaterProperties_Surface.hdf5", ErrorMessage, False)
                            End If

                            Dim waterproperties_windows_ As FileInfo() = di.GetFiles("MPI_" + i.ToString + "_WaterProperties_1_w?.hdf5")
                            For i_ = 1 To waterproperties_windows_.Length
                                FileCopy(Model.Path + "res\MPI_" + i.ToString + "_WaterProperties_1_w" + i_.ToString + ".hdf5", StorageDir + "MPI_" + i.ToString + "_WaterProperties_w" + i_.ToString + ".hdf5", ErrorMessage, False)
                            Next

                        End If

                        If Model.HasInterfaceSedimentWater Then
                            FileCopy(Model.Path + "res\MPI_" + i.ToString + "_InterfaceSedimentWater_1.hdf5", StorageDir + "MPI_" + i.ToString + "_InterfaceSedimentWater.hdf5", ErrorMessage, False)
                        End If

                        CountProcessors_storagehdf5 += 1
                    Next
                End If

                If Model.HasLagrangian Then
                    FileCopy(Model.Path + "res\Lagrangian_1.hdf5", StorageDir + "Lagrangian.hdf5", ErrorMessage, False)
                End If

                '----------------MOHID LAND
                If Model.HasBasin Then
                    source_string = "res\Basin_1.hdf5"
                    target_string = "Basin_1.hdf5"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasDrainageNetwork Then
                    source_string = "res\Drainage Network_1.hdf5"
                    target_string = "Drainage Network_1.hdf5"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasPorousMedia Then
                    source_string = "res\Porous Media_1.hdf5"
                    target_string = "Porous Media_1.hdf5"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasPorousMediaProperties Then
                    source_string = "res\PorousMedia Properties_1.hdf5"
                    target_string = "PorousMedia Properties_1.hdf5"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasRunoff Then
                    source_string = "res\RunOff_1.hdf5"
                    target_string = "RunOff_1.hdf5"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasRunoffProperties Then
                    source_string = "res\RunOff Properties_1.hdf5"
                    target_string = "RunOff Properties_1.hdf5"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasVegetation Then
                    source_string = "res\Vegetation_1.hdf5"
                    target_string = "Vegetation_1.hdf5"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasReservoirs Then
                    source_string = "res\Reservoirs_1.hdf5"
                    target_string = "Reservoirs_1.hdf5"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                If Model.HasIrrigation Then
                    source_string = "res\Irrigation_1.hdf5"
                    target_string = "Irrigation_1.hdf5"
                    FileCopy(Model.Path + source_string, StorageDir + target_string, ErrorMessage, True)
                End If
                '----------------MOHID LAND

            End If


        Next

        'backup time series results files
        For Each Model As Model In Models

            If Directory.Exists(Model.Path + "res\Run1") Then

                LogThis("Backing up time series results files from model " + Model.Name.ToString + "...")

                Dim ErrorMessage As String = "Could not copy time series file - " + Model.Name

                Dim TimeSeriesFiles = Directory.GetFiles(Model.Path + "res\Run1", "*.*")
                Dim TSFile As String
                'backup
                Dim BackUpDir As String = Model.BackUpPath + "Results_TimeSeries\" + InitialDateStr + "_" + FinalDateStr + "\"
                IO.Directory.CreateDirectory(BackUpDir)

                For Each TSFile In TimeSeriesFiles
                    FileCopy(TSFile, BackUpDir + GlobalFunctions.ExtractNameFromFullPath(TSFile), ErrorMessage, False)
                Next

                'storage
                If Model.StoragePath <> Nothing Then
                    LogThis("Storing time series results files from model " + Model.Name.ToString + "...")

                    Dim StorageDir As String = Model.StoragePath + "Results_TimeSeries\" + InitialDateStr + "_" + FinalDateStr + "\"
                    IO.Directory.CreateDirectory(StorageDir)

                    For Each TSFile In TimeSeriesFiles
                        FileCopy(TSFile, StorageDir + GlobalFunctions.ExtractNameFromFullPath(TSFile), ErrorMessage, False)
                    Next
                End If
            End If

        Next

        ''backup Meteo files
        'For Each Model As Model In Models

        '    If (Not Model.HasSolutionFromFile) And (Model.MeteoFile <> Nothing) Then
        '        LogThis("Backing up meteo forcing files from model " + Model.Name.ToString + "...")

        '        Dim ErrorMessage As String = "Could not copy meteo forcing file - " + Model.Name

        '        Dim BackUpDir As String = Model.BackUpPath + "Meteo\" + InitialDateStr + "_" + FinalDateStr + "\"
        '        IO.Directory.CreateDirectory(BackUpDir)
        '        FileCopy(Model.MeteoFile, BackUpDir + GlobalFunctions.ExtractNameFromFullPath(Model.MeteoFile), ErrorMessage, False)

        '        If Model.StoragePath <> Nothing Then
        '            LogThis("Storing meteo forcing files from model " + Model.Name.ToString + "...")
        '            Dim StorageDir As String = Model.StoragePath + "Meteo\" + InitialDateStr + "_" + FinalDateStr + "\"
        '            IO.Directory.CreateDirectory(StorageDir)
        '            FileCopy(Model.MeteoFile, StorageDir + GlobalFunctions.ExtractNameFromFullPath(Model.MeteoFile), ErrorMessage, False)
        '        End If


        '    End If
        'Next

        'backup Discharges files
        For Each Model As Model In Models

            If Model.HasDischarges Then
                LogThis("Backing up discharges forcing files from model " + Model.Name.ToString + "...")

                Dim ErrorMessage As String = "Could not copy discharge file - " + Model.Name.ToString

                Dim BackUpDir As String = Model.BackUpPath + "Discharges\" + InitialDateStr + "_" + FinalDateStr + "\"
                IO.Directory.CreateDirectory(BackUpDir)
                Dim dischargefile As String
                For Each dischargefile In Model.DichargesList
                    FileCopy(dischargefile, BackUpDir + GlobalFunctions.ExtractNameFromFullPath(dischargefile), ErrorMessage, False)
                Next

                If Model.StoragePath <> Nothing Then
                    LogThis("Storing discharges forcing files from model " + Model.Name.ToString + "...")
                    Dim StorageDir As String = Model.StoragePath + "Discharges\" + InitialDateStr + "_" + FinalDateStr + "\"
                    IO.Directory.CreateDirectory(StorageDir)
                    For Each dischargefile In Model.DichargesList
                        FileCopy(dischargefile, StorageDir + GlobalFunctions.ExtractNameFromFullPath(dischargefile), ErrorMessage, False)
                    Next
                End If

            End If

        Next

    End Sub

    Sub BackUpWW3Simulation(ByVal model As Model)
        Dim FolderNameByRun As String = InitialDateStr + "_" + FinalDateStr
        Dim BackupDir As String = Path.Combine(model.BackUpPath, FolderNameByRun)
        Dim StorageDir As String = Path.Combine(model.StoragePath, FolderNameByRun)
        Dim ErrorMessageBackup As String = "Could not copy (backup) file from model " + model.Name + ": "
        Dim ErrorMessageStorage As String = "Could not copy (storage) file from model " + model.Name + ": "
        Dim ModelWorkPath As String = Path.Combine(MainPath, model.Path)
        Dim FileToBackup As New Collection

        Call FindOutputs(model, FileToBackup)


        ' falta backup de hdf5!!

        'Backup
        LogThis("Backing up results files from model " + model.Name.ToString + "...")
        IO.Directory.CreateDirectory(BackupDir)
        For i = 1 To FileToBackup.Count
            FileCopy(Path.Combine(ModelWorkPath, FileToBackup(i)), Path.Combine(BackupDir, FileToBackup(i)), ErrorMessageBackup + FileToBackup(i), False)
        Next


        ''Storage
        If model.StoragePath <> Nothing Then
            LogThis("Storing up results files from model " + model.Name.ToString + "...")
            IO.Directory.CreateDirectory(StorageDir)


            FileToBackup.Clear()
            Call FindOutputs(model, FileToBackup)


            ' falta backup de hdf5!!

            For i = 1 To FileToBackup.Count
                FileCopy(Path.Combine(ModelWorkPath, FileToBackup(i)), Path.Combine(StorageDir, FileToBackup(i)), ErrorMessageBackup + FileToBackup(i), False)
            Next
        End If
    End Sub

    Sub BackupWRFSimulation()
        If WRF_RunWRF Then
            Dim FolderNameByRun As String = InitialDateStr + "_" + FinalDateStr
            For Each Model As Model In Models
                Dim BackupDir As String = Path.Combine(Model.BackUpPath, FolderNameByRun)
                Dim StorageDir As String = Path.Combine(Model.StoragePath, FolderNameByRun)
                Dim ErrorMessageBackup As String = "Could not copy (backup) file from model " + Model.Name + ": "
                Dim ErrorMessageStorage As String = "Could not copy (storage) file from model " + Model.Name + ": "
                Dim FileToCopy = Path.Combine(WRFFolder, Model.Name + ".hdf5")
                'Backup
                LogThis("Backing up results files from model " + Model.Name.ToString + "...")
                IO.Directory.CreateDirectory(BackupDir)

                FileCopy(FileToCopy, Path.Combine(BackupDir, Model.Name + ".hdf5"), ErrorMessageBackup + FileToCopy, False)

                'Storage
                If Model.StoragePath <> Nothing Then
                    LogThis("Storing results files from model " + Model.Name.ToString + "...")
                    IO.Directory.CreateDirectory(StorageDir)

                    FileCopy(FileToCopy, Path.Combine(StorageDir, Model.Name + ".hdf5"), ErrorMessageStorage + FileToCopy, False)
                End If

            Next

        End If

    End Sub
    Sub WriteFlagForTrigger(ByVal State As String, ByVal ExeName As String, ByVal FlagForTriggerFile As String)

        LogThis("Writing trigger file...")
        Try

            If State = "Finish" Then
                If ExeName = "MOHID" Or ExeName = "WW3" Or ExeName = "WRF" Then
                    If BackupResults_WereSuccessfull() = True Then
                        Dim FlagForTriggerFile_OutData As New OutData(FlagForTriggerFile)
                        With FlagForTriggerFile_OutData
                            .WriteBlankLine()
                            .WriteDataLine("FILE AUTOMATICALLY GENERATED TO BE USED AS TRIGGER")
                            .WriteBlankLine()
                            .WriteDataLine("DO NOT EDIT, CHANGE, MOVE, DELETE THIS FILE!")
                            .WriteBlankLine()
                            .WriteBlankLine()
                            .WriteDataLine(ExeName + " forecast and backup finished for the following period:")
                            .WriteDataLine("START", InitialDate)
                            .WriteDataLine("END", FinalDate)
                            .WriteBlankLine()
                            .WriteDataLine("STATUS", "FINISHED")
                            .WriteBlankLine()
                            .WriteDataLine("SYSTEM TIME", Now.ToString("yyyy-MM-dd HH:mm"))
                            .Finish()
                        End With
                    End If
                Else
                    Dim FlagForTriggerFile_OutData As New OutData(FlagForTriggerFile)
                    With FlagForTriggerFile_OutData
                        .WriteBlankLine()
                        .WriteDataLine("FILE AUTOMATICALLY GENERATED TO BE USED AS TRIGGER")
                        .WriteBlankLine()
                        .WriteDataLine("DO NOT EDIT, CHANGE, MOVE, DELETE THIS FILE!")
                        .WriteBlankLine()
                        .WriteBlankLine()
                        .WriteDataLine(ExeName + " finished for the following period:")
                        .WriteDataLine("START", InitialDate)
                        .WriteDataLine("END", FinalDate)
                        .WriteBlankLine()
                        .WriteDataLine("STATUS", "FINISHED")
                        .WriteBlankLine()
                        .WriteDataLine("SYSTEM TIME", Now.ToString("yyyy-MM-dd HH:mm"))
                        .Finish()
                    End With
                End If
            ElseIf State = "Running" Then
                Dim FlagForTriggerFile_OutData As New OutData(FlagForTriggerFile)
                With FlagForTriggerFile_OutData
                    .WriteBlankLine()
                    .WriteDataLine("FILE AUTOMATICALLY GENERATED TO BE USED AS TRIGGER")
                    .WriteBlankLine()
                    .WriteDataLine("DO NOT EDIT, CHANGE, MOVE, DELETE THIS FILE!")
                    .WriteBlankLine()
                    .WriteBlankLine()
                    .WriteDataLine(ExeName + " is running for the following period:")
                    .WriteDataLine("START", InitialDate)
                    .WriteDataLine("END", FinalDate)
                    .WriteBlankLine()
                    .WriteDataLine("STATUS", "RUNNING")
                    .WriteBlankLine()
                    .WriteDataLine("SYSTEM TIME", Now.ToString("yyyy-MM-dd HH:mm"))
                    .Finish()
                End With

            End If

        Catch ex As Exception
            LogThis("Error writing trigger file: " + ex.Message)
            AcceptableFailure = True
        End Try


    End Sub

    Sub MakePlots()

        LogThis("Copying result files to plot working folder...")

        'backup time series results files
        For Each Model As Model In Models

            If Not Model.HasSolutionFromFile Then

                Dim ErrorMessage As String = "Could not copy results files to plot folder - " + Model.Name

                If Model.HasSurfaceHDF Then
                    FileCopy(Model.Path + "res\Hydrodynamic_1_Surface.hdf5", PlotsDir + "Hydrodynamic_Surface.hdf5", ErrorMessage, True)
                End If

                If Model.HasWaterProperties And Model.HasSurfaceHDF Then
                    FileCopy(Model.Path + "res\WaterProperties_1_Surface.hdf5", PlotsDir + "WaterProperties_Surface.hdf5", ErrorMessage, True)
                End If

            End If
        Next

        LogThis("Writing Matlab batch file...")

        Dim MatlabBatchFile As New OutData(PlotsDir + "make_plots.bat")
        MatlabBatchFile.WriteDataLine("matlab -nojvm -nosplash -r mat_main_plot -minimize -wait")
        MatlabBatchFile.Finish()

        LogThis("Plotting results...")

        Dim MatlabBatch As Process = New Process
        MatlabBatch.StartInfo.FileName = PlotsDir + "make_plots.bat"
        MatlabBatch.StartInfo.WorkingDirectory = PlotsDir
        MatlabBatch.StartInfo.WindowStyle = ProcessWindowStyle.Normal
        MatlabBatch.Start()
        MatlabBatch.WaitForExit()

        LogThis("Results plotted!")

    End Sub

    Sub ManagePostProcessingTools(Optional ByVal ModelWorkPath As String = "")
        If Run_PostProcessing Then

            If ModelWorkPath = "" Then
                For Each PostProcessor As PostProcessor In PostProcessors
                    Call RunPostProcessingTools(PostProcessor)
                Next
            Else
                For Each PostProcessor As PostProcessor In PostProcessors
                    If PostProcessor.ExecuteAfterModelRun Then
                        If PostProcessor.ModelWorkPath = ModelWorkPath Then
                            Call RunPostProcessingTools(PostProcessor)
                            Exit For
                        End If
                    End If
                Next
            End If
        End If
    End Sub

    Sub RunPostProcessingTools(ByVal PostProcessor As PostProcessor)

        LogThis("PostProcessor Tool: " + PostProcessor.Name)

        If PostProcessor.InputFile = Nothing Or File.Exists(PostProcessor.InputFile) Then
            If PostProcessor.InputFile <> Nothing Then
                Dim IO1 As New DataIO()
                IO1.ChangeStream("START", InitialDate, PostProcessor.InputFile)
                IO1.ChangeStream("END", FinalDate, PostProcessor.InputFile)
            End If

            Dim AuxFolder_1 = Path.Combine(TriggerDir, "Postprocessor\")
            If Not System.IO.Directory.Exists(AuxFolder_1) Then
                Directory.CreateDirectory(AuxFolder_1)
            End If
            Dim AuxFolder = Path.Combine(AuxFolder_1, PostProcessor.Name)
            If Not System.IO.Directory.Exists(AuxFolder) Then
                Directory.CreateDirectory(AuxFolder)
            End If
            Dim FlagForTriggerFilename As String = InitialDateStr + "_" + FinalDateStr + ".dat"
            PostProcessor.FlagForTriggerFile = Path.Combine(AuxFolder, FlagForTriggerFilename)

            If IO.File.Exists(PostProcessor.FlagForTriggerFile) Then
                IO.File.Delete(PostProcessor.FlagForTriggerFile)
            End If

            'Write Running message to Trigger File
            Call WriteFlagForTrigger("Running", "Postprocessor", PostProcessor.FlagForTriggerFile)

            Call LaunchProcess(PostProcessor, SoftwareLabel)

            'Write Running message to Trigger File
            Call WriteFlagForTrigger("Finish", "Postprocessor", PostProcessor.FlagForTriggerFile)

            LogThis("Done!")
            LogThis("PostProcessor Tool: " + PostProcessor.Name + " successfully executed")

        ElseIf PostProcessor.InputFile <> Nothing And Not File.Exists(PostProcessor.InputFile) Then
            Call UnSuccessfullEnd("Input data file: " + PostProcessor.InputFile + " was not found")
        End If

    End Sub

    Sub SuccessfullForecast(ByVal i As Integer)

        LogThis("----------SUCCESSFULL FORECAST (" + i.ToString + " of " + NumberOfRuns.ToString + ")!-------------")

        Call SendLogEmail("Successfull " + "MOHID forecast from " + MailSender.ToString + " ", Now.ToString("yyyy-MM-dd HH-mm-ss"), True)

    End Sub


#End Region

#Region "Management"

    'Sub CheckLicenseStatus()
    '    '-------------------------Check Licence Status
    '    Dim r As New Random
    '    RandomControl = r.Next()

    '    Dim registryKey As RegistryKey
    '    registryKey = Registry.LocalMachine
    '    Dim registrySubKey As RegistryKey

    '    registrySubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\ART\KEY", True)

    '    'the first run must create a key.
    '    If registrySubKey Is Nothing Then
    '        registrySubKey = registryKey.CreateSubKey("SOFTWARE\ART\KEY")
    '        registrySubKey.SetValue("ART_FIRSTRUN", Now.Date.ToString("yyyy-MM-dd"))
    '    End If

    '    ' to delete
    '    'If registrySubKey.GetValue("ART_FIRSTRUN") <> Nothing Then
    '    '    registryKey.DeleteSubKey("SOFTWARE\ART\KEY")
    '    'End If
    '    'End

    '    Dim FirstRun As Date

    '    Dim MyIP As String
    '    Dim AcceptedIPList As New Collection
    '    Dim LicenseStatus As String = "rejected"
    '    AcceptedIPList.Add("193.136.129.227")

    '    MyIP = GetExternalIp()

    '    If MyIP <> Nothing Then
    '        For Each IP In AcceptedIPList
    '            If IP = MyIP Then
    '                LicenseStatus = "accepted"
    '                registrySubKey.SetValue("ART_FIRSTRUN", "9999-01-01")
    '                Exit For
    '            End If
    '        Next
    '        If LicenseStatus = "rejected" Then
    '            Call UnSuccessfullEnd("Unlicensed version (public IP not authorized)")
    '        End If
    '    Else ' if no internet is available, program runs
    '        LicenseStatus = "unknown"
    '        registrySubKey.SetValue("ART_FIRSTRUN", Now.Date.ToString("yyyy-MM-dd"))
    '    End If

    '    If registrySubKey.GetValue("ART_FIRSTRUN") Is Nothing Then
    '        LogThis("ART is not able to change or check licence version")
    '        AcceptableFailure = True
    '    Else
    '        FirstRun = registrySubKey.GetValue("ART_FIRSTRUN")

    '        ' program will stop if workstation has been more than one week without internet 
    '        If LicenseStatus = "unknown" Then
    '            If FirstRun > Now Then ' last run was with licence accepted
    '                registrySubKey.SetValue("ART_FIRSTRUN", Now.Date.ToString("yyyy-MM-dd"))
    '            Else ' last run was already with licence unknown
    '                If (Now - FirstRun).Days > 7 Then
    '                    Call UnSuccessfullEnd("Unlicensed version (public IP could not be verified for more than 7 days (" + (Now - FirstRun).Days.ToString + " days exactly)")
    '                End If
    '            End If
    '        End If

    '    End If

    '    '--------------------------------------------

    'End Sub
    Sub LogOptions()

        LogThis("Starting Automatic Running Tool.")
        If Run_MOHID Or Run_WW3 Then
            LogThis("The system consists of the following domains:")
            Dim Model As Model
            For Each Model In Models
                LogThis(Model.Name)
            Next
        End If


    End Sub

    'Sub GetDischargeFiles(ByVal DischargesConfigFile_ As String, ByRef DischargeFiles_ As Collection)

    '    Dim FirstLine, LastLine As Long
    '    Dim FullLine As String
    '    Dim ConfigFile As New EnterData(DischargesConfigFile_)

    '    Dim Found_TimeSerieConfig_Block As Boolean = False
    '    Dim strKeywordValue As String
    '    With ConfigFile
    '        Do
    '            'Extract  block
    '            .ExtractBlockFromBuffer("<BeginTimeSerieConfig>", "<EndTimeSerieConfig>", Found_TimeSerieConfig_Block)

    '            If Found_TimeSerieConfig_Block Then
    '                strKeywordValue = ""
    '                .GetDataStr("OUTPUT_FILENAME", strKeywordValue, .FromBlock)
    '                DischargeFiles_.Add(strKeywordValue)
    '            End If

    '        Loop
    '    End With




    '    FullLine = ""
    '    LogFile.GetReadingLimits(FirstLine, LastLine, EnterData.FromFile)
    '    For iLine As Long = FirstLine + 1 To LastLine - 1
    '        LogFile.GetFullLine(iLine, FullLine)
    '        If String.Equals(FullLine.Trim, SuccessStr) Then
    '            ProgramWasSuccessfull = True
    '        End If
    '    Next

    'End Sub


    Function ProgramWasSuccessfull(ByVal LogFileName_ As String, ByVal SuccessStr As String, Optional ByVal CheckIfContains As Boolean = False) As Boolean

        ProgramWasSuccessfull = False

        Dim FirstLine, LastLine As Long
        Dim FullLine As String
        Dim LogFile As New EnterData(LogFileName_)
        FullLine = ""

        LogFile.GetReadingLimits(FirstLine, LastLine, EnterData.FromFile)
        For iLine As Long = FirstLine + 1 To LastLine - 1
            LogFile.GetFullLine(iLine, FullLine)
            If CheckIfContains = False Then
                If String.Equals(FullLine.Trim, SuccessStr) Then
                    ProgramWasSuccessfull = True
                    Exit For
                End If
            Else
                If InStr(FullLine.Trim, SuccessStr) > 0 Then
                    ProgramWasSuccessfull = True
                    Exit For

                End If
            End If
        Next

    End Function

    Function BackupResults_WereSuccessfull() As Boolean

        For Each Model As Model In Models
            If Run_MOHID Then

                If Model.Name = Trigger_Model_ToCheckBackup Then
                    BackupResults_WereSuccessfull = True

                    Dim BackUpDir As String = Model.BackUpPath + "Results_HDF\" + InitialDateStr + "_" + FinalDateStr + "\"
                    'Dim ErrorMessage As String = "Could not copy (backup) HDF file - " + Model.Name

                    '            If Not Model.HasSolutionFromFile Then
                    'LogThis("Backing up HDF results files from model " + Model.Name.ToString + "...")

                    If Not File.Exists(BackUpDir + "Hydrodynamic.hdf5") Then
                        BackupResults_WereSuccessfull = False
                    End If

                    If Model.HasSurfaceHDF Then
                        If Not File.Exists(BackUpDir + "Hydrodynamic_Surface.hdf5") Then
                            BackupResults_WereSuccessfull = False
                        End If
                    End If

                    Dim di As DirectoryInfo = New DirectoryInfo(Model.Path + "res\")
                    Dim hydro_windows As FileInfo() = di.GetFiles("Hydrodynamic_1_w?.hdf5")
                    Dim i As Integer

                    For i = 1 To hydro_windows.Length
                        If Not File.Exists(BackUpDir + "Hydrodynamic_w" + i.ToString + ".hdf5") Then
                            BackupResults_WereSuccessfull = False
                        End If
                    Next

                    Dim waterproperties_windows As FileInfo() = di.GetFiles("WaterProperties_1_w?.hdf5")
                    If Model.HasWaterProperties Then

                        If Not File.Exists(BackUpDir + "WaterProperties.hdf5") Then
                            BackupResults_WereSuccessfull = False
                        End If

                        If Model.HasSurfaceHDF Then
                            If Not File.Exists(BackUpDir + "WaterProperties_Surface.hdf5") Then
                                BackupResults_WereSuccessfull = False
                            End If
                        End If

                        For i = 1 To waterproperties_windows.Length
                            If Not File.Exists(BackUpDir + "WaterProperties_w" + i.ToString + ".hdf5") Then
                                BackupResults_WereSuccessfull = False
                            End If
                        Next

                    End If

                    If Model.HasInterfaceSedimentWater Then
                        If Not File.Exists(BackUpDir + "InterfaceSedimentWater.hdf5") Then
                            BackupResults_WereSuccessfull = False
                        End If
                    End If

                    If Model.HasAtmosphere Then
                        If Not File.Exists(BackUpDir + "Atmosphere.hdf5") Then
                            BackupResults_WereSuccessfull = False
                        End If
                    End If

                    If Model.HasLagrangian Then
                        If Not File.Exists(BackUpDir + "Lagrangian.hdf5") Then
                            BackupResults_WereSuccessfull = False
                        End If
                    End If
                    Exit For
                End If
            ElseIf Run_WW3 Then
                If Model.Name = Trigger_Model_ToCheckBackup Then
                    BackupResults_WereSuccessfull = True

                    Dim BackUpDir As String = Path.Combine(Model.BackUpPath, InitialDateStr + "_" + FinalDateStr)
                    '''''''''''''''''''STILL MISSING; TO DO
                End If
            ElseIf Run_WRF Then
                If Model.Name = Trigger_Model_ToCheckBackup Then
                    BackupResults_WereSuccessfull = True

                    Dim BackUpDir As String = Path.Combine(Model.BackUpPath, InitialDateStr + "_" + FinalDateStr)
                    '''''''''''''''''''STILL MISSING; TO DO
                End If

                '''''''''''''''''''STILL MISSING; TO DO

            End If

        Next

    End Function
#End Region

#Region "Mail"

    Public Sub Read_Mohid_Settings_List(ByVal File As EnterData)
        Dim BlockFound, BlockFromBlockFound As Boolean
        Dim BeginReading, EndReading As Integer

        File.RewindBuffer(True)

        'Extracts mohid settings block
        File.ExtractBlockFromBuffer("<begin_mohid_settings>", "<end_mohid_settings>", BlockFound)

        If (BlockFound) Then

            Do While BlockFound
                If (BlockFound) Then

                    File.GetDataLog("OPENMP", OpenMP)
                    If OpenMP Then
                        File.GetDataInteger("OPENMP_NUM_THREADS", OpenMP_Num_Threads)
                    End If

                    File.GetDataStr("MAX_TIME", MOHID_MaxTime, EnterData.FromBlock)
                    File.GetDataLog("SCREEN_OUTPUT_TO_FILE", MOHID_ScreenOutputToFile, EnterData.FromBlock)
                    File.GetDataStr("EXE", MOHID_exe, EnterData.FromBlock)
                    If MOHID_exe = Nothing Then
                        '                            MOHID_exe = Path.Combine(MainPath, "GeneralData\Exe\MohidWater.exe")
                        MOHID_exe = "MOHID_Run.bat"
                        CreateBatchFile = True
                    Else
                        CreateBatchFile = False
                    End If
                    If MOHID_ScreenOutputToFile Then
                        File.GetDataStr("SCREEN_OUTPUT_PATH", MOHID_ScreenOutputPath, EnterData.FromBlock)
                        File.GetDataStr("SCREEN_OUTPUT_PATH_IN_NETWORK", MOHID_ScreenOutputPathInNetwork, EnterData.FromBlock)
                    End If


                End If
                File.ExtractBlockFromBuffer("<begin_mohid_settings>", "<end_mohid_settings>", BlockFound)
            Loop


        End If

    End Sub

    Public Sub Read_WW3_Settings_List(ByVal File As EnterData)
        Dim BlockFound, BlockFromBlockFound As Boolean
        Dim BeginReading, EndReading As Integer
        File.RewindBuffer(True)
        Dim StartLine, EndLine, ListSize, i, iLine As Integer

        'Extracts ww3 settings block
        File.ExtractBlockFromBuffer("<begin_ww3_settings>", "<end_ww3_settings>", BlockFound)

        If (BlockFound) Then

            Do While BlockFound
                If (BlockFound) Then

                    File.GetDataLog("FIRST_RUN", WW3FirstRun)

                    If OpenMP Then
                        File.GetDataInteger("OPENMP_NUM_THREADS", OpenMP_Num_Threads)
                    End If

                    File.GetDataStr("MAX_TIME", WW3_MaxTime, EnterData.FromBlock)
                    File.GetDataLog("SCREEN_OUTPUT_TO_FILE", WW3_ScreenOutputToFile, EnterData.FromBlock)
                    If WW3_ScreenOutputToFile Then
                        File.GetDataStr("SCREEN_OUTPUT_PATH", WW3_ScreenOutputPath, EnterData.FromBlock)
                        File.GetDataStr("SCREEN_OUTPUT_PATH_IN_NETWORK", WW3_ScreenOutputPathInNetwork, EnterData.FromBlock)
                    End If
                    File.GetDataStr("EXE_HANDLE_GRID", WW3_Exe_HandleGrid, EnterData.FromBlock)
                    File.GetDataStr("EXE_FIRST_TIME", WW3_Exe_FirstTime, EnterData.FromBlock)
                    File.GetDataStr("EXE_HANDLE_INPUTS", WW3_Exe_HandleInputs, EnterData.FromBlock)
                    File.GetDataStr("EXE_WW3", WW3_Exe, EnterData.FromBlock)
                    File.GetDataStr("EXE_HANDLE_FIELD_OUTPUTS", WW3_Exe_HandleFieldOutputs, EnterData.FromBlock)
                    File.GetDataStr("EXE_HANDLE_POINT_OUTPUTS", WW3_Exe_HandlePointOutputs, EnterData.FromBlock)
                    File.GetDataStr("EXE_HANDLE_HDF5_OUTPUTS", WW3_Exe_HandleHDF5Outputs, EnterData.FromBlock)
                    If WW3_Exe_HandleHDF5Outputs <> "" Then
                        WW3_NbrExes = 6
                    Else
                        WW3_NbrExes = 5

                    End If

                End If
                File.ExtractBlockFromBuffer("<begin_ww3_settings>", "<end_ww3_settings>", BlockFound)
            Loop


            File.RewindBuffer(True)
            'Extracts ww3 settings block
            File.ExtractBlockFromBuffer("<begin_ww3_settings>", "<end_ww3_settings>", BlockFound)

            'Extracts block with timeserie types to be managed
            File.ExtractBlockFromBlock("<<begin_output_extensions>>", "<<end_output_extensions>>", BlockFromBlockFound)
            File.GetReadingLimits(StartLine, EndLine, EnterData.FromBlockFromBlock)

            ListSize = EndLine - StartLine - 2

            ReDim OutputExtensionList(ListSize)
            i = 0
            For iLine = StartLine + 1 To EndLine - 1
                File.GetFullLine(iLine, OutputExtensionList(i))
                i = i + 1
            Next


        End If

    End Sub

    Public Sub GetMPINbrProcessors()
        Dim TreeFile As String
        TreeFile = Path.Combine(FatherModelWorkPath, "Tree.dat")

        Dim sr As New StreamReader(TreeFile)
        Dim sFile As String = String.Empty
        Dim i As Integer
        Dim textLine As String
        Dim textLineLength As Integer
        Dim textLineRightPosition As Integer
        Dim SumProcessors As Integer = 0
        'While (sr.Peek() <> -1)
        '    textLine = sr.ReadLine
        '    If InStr(textLine, ":") <> 0 And InStr(textLine, "+") <> 0 Then
        '        textLineLength = textLine.Length
        '        textLineRightPosition = textLineLength - InStr(textLine, ":")
        '        MPI_Decomposition.Add(CInt(Right(textLine, 2)))
        '        SumProcessors += CInt(Right(textLine, 2))
        '    End If
        'End While
        For Each Model As Model In Models
            SumProcessors += Model.MPI_Num_Processors
        Next
        MPI_Num_Processors = SumProcessors
    End Sub
    Public Sub Read_WRF_Settings_List(ByVal File As EnterData)
        Dim BlockFound, BlockFromBlockFound As Boolean
        Dim BeginReading, EndReading As Integer
        File.RewindBuffer(True)
        Dim StartLine, EndLine, ListSize, i, iLine As Integer

        'Extracts ww3 settings block
        File.ExtractBlockFromBuffer("<begin_wrf_settings>", "<end_wrf_settings>", BlockFound)

        If (BlockFound) Then

            Do While BlockFound
                If (BlockFound) Then

                    File.GetDataLog("MPI", WRF_MPI)
                    If WRF_MPI Then
                        File.GetDataInteger("MPI_NUM_PROCESSORS", WRF_MPI_Num_Processors)
                        File.GetDataStr("MPI_EXE_PATH", WRF_MPI_Exe_Path)
                    End If


                    File.GetDataStr("MAX_TIME", WRF_MaxTime, EnterData.FromBlock)
                    File.GetDataLog("SCREEN_OUTPUT_TO_FILE", WRF_ScreenOutputToFile, EnterData.FromBlock)
                    If WRF_ScreenOutputToFile Then
                        File.GetDataStr("SCREEN_OUTPUT_PATH", WRF_ScreenOutputPath, EnterData.FromBlock)
                        File.GetDataStr("SCREEN_OUTPUT_PATH_IN_NETWORK", WRF_ScreenOutputPathInNetwork, EnterData.FromBlock)
                    End If


                    File.GetDataLog("UNGRIB", WRF_UnGrib, EnterData.FromBlock)
                    File.GetDataLog("METGRID", WRF_MetGrid, EnterData.FromBlock)
                    File.GetDataLog("RUN_WRF", WRF_RunWRF, EnterData.FromBlock)
                    If WRF_RunWRF Then
                        File.GetDataLog("HANDLE_HDF5_OUTPUTS", WRF_HandleHDF5Outputs, EnterData.FromBlock)
                        File.GetDataLog("RESTART", WRF_Restart, EnterData.FromBlock)
                    End If

                    If WRF_UnGrib Then
                        File.GetDataStr("EXE_UNGRIB", WRF_Exe_UnGrib, EnterData.FromBlock)
                        File.GetDataStr("UNGRIB_SOURCE_PATH", WRF_Ungrib_SourcePath, EnterData.FromBlock)
                    End If

                    If WRF_RunWRF Then
                        File.GetDataStr("EXE_METGRID", WRF_Exe_MetGrid, EnterData.FromBlock)
                        File.GetDataStr("EXE_REAL", WRF_Exe_Real, EnterData.FromBlock)
                        File.GetDataStr("EXE_WRF", WRF_Exe_WRF, EnterData.FromBlock)
                        'File.GetDataInteger("NUMBER_PROCESSORS", WRF_NbrProcessors, EnterData.FromBlock)
                        ' File.GetDataStr("MPI_PATH", WRF_MPIPath, EnterData.FromBlock)
                    End If

                    If WRF_HandleHDF5Outputs Then
                        File.GetDataStr("EXE_HANDLE_HDF5_OUTPUTS", WRF_Exe_HandleHDF5Outputs, EnterData.FromBlock)
                    End If

                    File.GetDataStr("MODEL_PATH", WRFModelPath, EnterData.FromBlock)

                    File.GetDataInteger("INPUT_INTERVAL_SECONDS", WRF_InputIntervalSeconds, EnterData.FromBlock)

                End If
                'Extracts wrf settings block
                File.ExtractBlockFromBuffer("<begin_wrf_settings>", "<end_wrf_settings>", BlockFound)
            Loop

        End If

    End Sub

    Public Sub Read_PreProcessing_List(ByVal File As EnterData)
        Dim BlockFound, BlockFromBlockFound As Boolean
        File.RewindBuffer(True)

        'Extracts preprocessing block
        File.ExtractBlockFromBuffer("<begin_preprocessing>", "<end_preprocessing>", BlockFound)

        If (BlockFound) Then

            File.ExtractBlockFromBlock("<<begin_software_to_run>>", "<<end_software_to_run>>", BlockFromBlockFound)
            Do While BlockFromBlockFound
                If (BlockFromBlockFound) Then

                    Dim NewPreProcessor As New PreProcessor
                    NewPreProcessor.LoadData(File)
                    PreProcessors.Add(NewPreProcessor, NewPreProcessor.Name)


                End If
                File.ExtractBlockFromBlock("<<begin_software_to_run>>", "<<end_software_to_run>>", BlockFromBlockFound)
            Loop


        End If

    End Sub

    Public Sub Read_PostProcessing_List(ByVal File As EnterData)
        Dim BlockFound, BlockFromBlockFound As Boolean
        File.RewindBuffer(True)

        'Extracts postprocessing block
        File.ExtractBlockFromBuffer("<begin_postprocessing>", "<end_postprocessing>", BlockFound)

        If (BlockFound) Then

            File.ExtractBlockFromBlock("<<begin_software_to_run>>", "<<end_software_to_run>>", BlockFromBlockFound)
            Do While BlockFromBlockFound
                If (BlockFromBlockFound) Then

                    Dim NewPostProcessor As New PostProcessor

                    NewPostProcessor.LoadData(File)
                    PostProcessors.Add(NewPostProcessor, NewPostProcessor.Name)


                End If
                File.ExtractBlockFromBlock("<<begin_software_to_run>>", "<<end_software_to_run>>", BlockFromBlockFound)
            Loop


        End If

    End Sub

    Public Sub Read_Model_List(ByVal File As EnterData)
        Dim BlockFound, BlockFromBlockFound As Boolean
        Dim BeginReading, EndReading As Integer


        File.RewindBuffer(True)

        'Extracts first block
        File.ExtractBlockFromBuffer("<begin_model>", "<end_model>", BlockFound)

        If Not BlockFound Then
            UnSuccessfullEnd("No models found in input data file!")
        End If

        Do While BlockFound

            File.GetReadingLimits(BeginReading, EndReading, EnterData.FromBlock)
            If (BlockFound) Then
                Dim NewModel As New Model
                NewModel.LoadData(File)
                Models.Add(NewModel, NewModel.Name)


                File.ExtractBlockFromBlock("<<begin_export_window>>", "<<end_export_window>>", BlockFromBlockFound)
                Do While BlockFromBlockFound
                    File.GetReadingLimits(BeginReading, EndReading, EnterData.FromBlockFromBlock)
                    If (BlockFromBlockFound) Then
                        Dim NewExportWindow As New ExportWindow
                        NewExportWindow.LoadData(File)
                        NewModel.ExportWindows.Add(NewExportWindow, NewExportWindow.ID)
                    End If
                    File.ExtractBlockFromBlock("<<begin_export_window>>", "<<end_export_window>>", BlockFromBlockFound)
                Loop




            End If
            File.ExtractBlockFromBuffer("<begin_model>", "<end_model>", BlockFound)
        Loop

    End Sub

    Public Function App_Path() As String
        Return System.AppDomain.CurrentDomain.BaseDirectory()
    End Function
#End Region

#Region "Destructor"

    Sub SuccessfullEnd(ByVal i As Integer)

        LogThis("----------SUCCESSFULL FORECAST (" + i.ToString + " of " + NumberOfRuns.ToString + ")!-------------")

        If i = NumberOfRuns Then
            LogThis("Successfull Automatic Running Tool execution.")
            If CheckDiskSpace Then
                Call PublishDiskSpaceInformation()
            End If
            Call CloseLogFile()
            Call SendLogEmail("Successfull " + "MOHID forecast " + "(" + i.ToString + " of " + NumberOfRuns.ToString + ") and ART execution completed from " + MailSender.ToString + " ", Now.ToString("yyyy-MM-dd HH-mm-ss"), True)
            End
        Else
            If CheckDiskSpace Then
                Call PublishDiskSpaceInformation()
            End If
            Call SendLogEmail("Successfull Mohid forecast" + "(" + i.ToString + " of " + NumberOfRuns.ToString + ") from " + MailSender.ToString + " ", Now.ToString("yyyy-MM-dd HH-mm-ss"), True)
        End If


    End Sub

    Sub GetDiskSpaceinformation()
        Dim p1 As Process = New Process
        Dim p2 As Process = New Process

        ModelDisk = System.IO.Path.GetPathRoot(MainPath)

        If InStr(1, ModelDisk, "\\") Then
            p1.StartInfo.FileName = "net.exe"
            p1.StartInfo.Arguments = "use " + DriveAvailableToMap + " " + MainPath
            p1.Start()
            p1.WaitForExit()
            ModelDisk_FreeSpace = My.Computer.FileSystem.GetDriveInfo(DriveAvailableToMap).AvailableFreeSpace / 1024 / 1024 / 1024
            ModelDisk_TotalSpace = My.Computer.FileSystem.GetDriveInfo(DriveAvailableToMap).TotalSize / 1024 / 1024 / 1024
            ModelDisk_FreeSpace_Percentage = ModelDisk_FreeSpace / ModelDisk_TotalSpace * 100
            p2.StartInfo.FileName = "net.exe"
            p2.StartInfo.Arguments = " use " + DriveAvailableToMap + " /delete /y"
            p2.StartInfo.RedirectStandardOutput = True
            p2.StartInfo.UseShellExecute = False
            p2.Start()
            p2.WaitForExit()

            '            Dim stringOutput As String
            '            While Not p2.StandardOutput.EndOfStream
            ' stringOutput += p2.StandardOutput.ReadLine
            ' End While
        Else
            ModelDisk_FreeSpace = My.Computer.FileSystem.GetDriveInfo(ModelDisk).AvailableFreeSpace / 1024 / 1024 / 1024
            ModelDisk_TotalSpace = My.Computer.FileSystem.GetDriveInfo(ModelDisk).TotalSize / 1024 / 1024 / 1024
            ModelDisk_FreeSpace_Percentage = ModelDisk_FreeSpace / ModelDisk_TotalSpace * 100

        End If

        If Run_MOHID Then

            Dim BackupDiskBefore As String = ""
            Dim StorageDiskBefore As String = ""
            Dim n As Integer
            n = 0

            For Each Model As Model In Models
                n = n + 1

                If BackupDiskBefore <> System.IO.Path.GetPathRoot(Model.BackUpPath) Then
                    If InStr(1, Model.BackUpPath, "\\") Then
                        p1.StartInfo.FileName = "net.exe"
                        p1.StartInfo.Arguments = "use " + DriveAvailableToMap + " " + Model.BackUpPath
                        p1.Start()
                        p1.WaitForExit()

                        BackupDisk.Add(System.IO.Path.GetPathRoot(Model.BackUpPath))
                        BackupDiskBefore = System.IO.Path.GetPathRoot(Model.BackUpPath)
                        BackupDisk_FreeSpace.Add(My.Computer.FileSystem.GetDriveInfo(DriveAvailableToMap).AvailableFreeSpace / 1024 / 1024 / 1024)
                        BackupDisk_TotalSpace.Add(My.Computer.FileSystem.GetDriveInfo(DriveAvailableToMap).TotalSize / 1024 / 1024 / 1024)
                        BackupDisk_FreeSpace_Percentage.Add(BackupDisk_FreeSpace(n) / BackupDisk_TotalSpace(n) * 100)

                        p2.StartInfo.FileName = "net.exe"
                        p2.StartInfo.Arguments = " use " + DriveAvailableToMap + " /delete /y"
                        p2.StartInfo.RedirectStandardOutput = True
                        p2.StartInfo.UseShellExecute = False
                        p2.Start()
                        p2.WaitForExit()

                    Else
                        BackupDisk.Add(System.IO.Path.GetPathRoot(Model.BackUpPath))
                        BackupDiskBefore = System.IO.Path.GetPathRoot(Model.BackUpPath)
                        BackupDisk_FreeSpace.Add(My.Computer.FileSystem.GetDriveInfo(BackupDisk(n)).AvailableFreeSpace / 1024 / 1024 / 1024)
                        BackupDisk_TotalSpace.Add(My.Computer.FileSystem.GetDriveInfo(BackupDisk(n)).TotalSize / 1024 / 1024 / 1024)
                        BackupDisk_FreeSpace_Percentage.Add(BackupDisk_FreeSpace(n) / BackupDisk_TotalSpace(n) * 100)
                    End If

                End If

                If Model.StoragePath <> Nothing Then
                    If StorageDiskBefore <> System.IO.Path.GetPathRoot(Model.StoragePath) Then
                        If InStr(1, Model.StoragePath, "\\") Then

                            p1.StartInfo.FileName = "net.exe"
                            p1.StartInfo.Arguments = "use " + DriveAvailableToMap + " " + Model.StoragePath
                            p1.Start()
                            p1.WaitForExit()

                            StorageDisk.Add(System.IO.Path.GetPathRoot(Model.StoragePath))
                            StorageDiskBefore = Model.StoragePath
                            StorageDisk_FreeSpace.Add(My.Computer.FileSystem.GetDriveInfo(DriveAvailableToMap).AvailableFreeSpace / 1024 / 1024 / 1024)
                            StorageDisk_TotalSpace.Add(My.Computer.FileSystem.GetDriveInfo(DriveAvailableToMap).TotalSize / 1024 / 1024 / 1024)
                            StorageDisk_FreeSpace_Percentage.Add(StorageDisk_FreeSpace(n) / StorageDisk_TotalSpace(n) * 100)

                            p2.StartInfo.FileName = "net.exe"
                            p2.StartInfo.Arguments = " use " + DriveAvailableToMap + " /delete /y"
                            p2.StartInfo.RedirectStandardOutput = True
                            p2.StartInfo.UseShellExecute = False
                            p2.Start()
                            p2.WaitForExit()
                        Else
                            StorageDisk.Add(System.IO.Path.GetPathRoot(Model.StoragePath))
                            StorageDiskBefore = Model.StoragePath
                            StorageDisk_FreeSpace.Add(My.Computer.FileSystem.GetDriveInfo(StorageDisk(n)).AvailableFreeSpace / 1024 / 1024 / 1024)
                            StorageDisk_TotalSpace.Add(My.Computer.FileSystem.GetDriveInfo(StorageDisk(n)).TotalSize / 1024 / 1024 / 1024)
                            StorageDisk_FreeSpace_Percentage.Add(StorageDisk_FreeSpace(n) / StorageDisk_TotalSpace(n) * 100)

                        End If
                    End If
                End If

            Next

        End If


    End Sub

    Sub PublishDiskSpaceInformation()
        Dim MessageToEmail As String = ""
        Dim SendAlertSpaceEmail As Boolean = False

        If ModelDisk_FreeSpace < MinFreeDiskSpace Or ModelDisk_FreeSpace_Percentage < MinFreeDiskSpace_Percentage Then
            LogThis("ATENTION!! Available Free Space on Main Disk " + ModelDisk + " : " + CInt(ModelDisk_FreeSpace).ToString + " GB (" + CInt(ModelDisk_FreeSpace_Percentage).ToString + "%)")
            MessageToEmail = "Available Free Space on Main Disk " + ModelDisk + " : " + CInt(ModelDisk_FreeSpace).ToString + " GB (" + CInt(ModelDisk_FreeSpace_Percentage).ToString + "%)" + vbCrLf
            SendAlertSpaceEmail = True
        Else
            LogThis("Available Free Space on Main Disk " + ModelDisk + " : " + CInt(ModelDisk_FreeSpace).ToString + " GB (" + CInt(ModelDisk_FreeSpace_Percentage).ToString + "%)")
        End If

        Dim n As Integer
        For n = 1 To BackupDisk.Count
            If BackupDisk_FreeSpace(n) < MinFreeDiskSpace Or BackupDisk_FreeSpace_Percentage(n) < MinFreeDiskSpace_Percentage Then
                LogThis("ATTENTION!! Available Free Space on Backup Disk " + BackupDisk(n) + " : " + CInt(BackupDisk_FreeSpace(n)).ToString + " GB (" + CInt(BackupDisk_FreeSpace_Percentage(n)).ToString + "%)")
                MessageToEmail += "Available Free Space on Backup Disk " + BackupDisk(n) + " : " + CInt(BackupDisk_FreeSpace(n)).ToString + " GB (" + CInt(BackupDisk_FreeSpace_Percentage(n)).ToString + "%)" + vbCrLf
                SendAlertSpaceEmail = True
            Else
                LogThis("Available Free Space on Backup Disk " + BackupDisk(n) + " : " + CInt(BackupDisk_FreeSpace(n)).ToString + " GB (" + CInt(BackupDisk_FreeSpace_Percentage(n)).ToString + "%)")
            End If
        Next

        For n = 1 To StorageDisk.Count
            If StorageDisk_FreeSpace(n) < MinFreeDiskSpace Or StorageDisk_FreeSpace_Percentage(n) < MinFreeDiskSpace_Percentage Then
                LogThis("ATTENTION!! Available Free Space on Storage Disk " + StorageDisk(n) + " : " + CInt(StorageDisk_FreeSpace(n)).ToString + " GB (" + CInt(StorageDisk_FreeSpace_Percentage(n)).ToString + "%)")
                MessageToEmail += "Available Free Space on Storage Disk " + StorageDisk(n) + " : " + CInt(StorageDisk_FreeSpace(n)).ToString + " GB (" + CInt(StorageDisk_FreeSpace_Percentage(n)).ToString + "%)" + vbCrLf
                SendAlertSpaceEmail = True
            Else
                LogThis("Available Free Space on Storage Disk " + StorageDisk(n) + " : " + CInt(StorageDisk_FreeSpace(n)).ToString + " GB (" + CInt(StorageDisk_FreeSpace_Percentage(n)).ToString + "%)")
            End If
        Next

        If SendAlertSpaceEmail = True Then
            Dim intro_body As String = "Low space on disk(s) associated to " + MailSender.ToString + vbCrLf + MessageToEmail + vbCrLf + "Replace the disk(s) as soon as possible."
            SendEmail("Low space on disk(s) associated", intro_body, True)
        End If

        'Clear collections
        BackupDisk.Clear()
        BackupDisk_FreeSpace.Clear()
        BackupDisk_TotalSpace.Clear()
        BackupDisk_FreeSpace_Percentage.Clear()
        StorageDisk.Clear()
        StorageDisk_FreeSpace.Clear()
        StorageDisk_TotalSpace.Clear()
        StorageDisk_FreeSpace_Percentage.Clear()

    End Sub
    Private Function GetFreeSpace(ByVal Drive As String) As Double
        GetFreeSpace = My.Computer.FileSystem.GetDriveInfo(Drive).AvailableFreeSpace / 1024 / 1024 / 1024
    End Function

    Private Function GetTotalSpace(ByVal Drive As String) As Double
        GetTotalSpace = My.Computer.FileSystem.GetDriveInfo(Drive).TotalSize / 1024 / 1024 / 1024
    End Function

#End Region

End Module
