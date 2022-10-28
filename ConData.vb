Option Explicit On
Option Strict On

Imports System.Data.OleDb
Imports System.Text

Module Module1
    Sub Main()
        Dim oConn As New OleDbConnection ' Data objects
        Dim oCmd As New OleDbCommand
        Dim oReader As OleDbDataReader

        Dim sQry As String = ""
        Dim sQryType As String = ""    ' SELECT, INSERT, UPDATE or DELETE
        Dim SelectQry As Boolean = True  ' Assume query is of type Select - for safety sake.

        Dim cSuperType As Hashtable = New Hashtable() ' Type/SubType hashtable

        cSuperType.Add("Boolean", "Boolean")
        cSuperType.Add("String", "String") : cSuperType.Add("Char", "String") : cSuperType.Add("Guid", "String")
        cSuperType.Add("DateTime", "Date") : cSuperType.Add("TimeSpan", "Date")
        cSuperType.Add("Byte", "Number") : cSuperType.Add("SByte", "Number")
        cSuperType.Add("Double", "Number") : cSuperType.Add("Decimal", "Number") : cSuperType.Add("Single", "Number")
        cSuperType.Add("Int64", "Number") : cSuperType.Add("Int32", "Number") : cSuperType.Add("Int16", "Number")
        cSuperType.Add("UInt64", "Number") : cSuperType.Add("UInt32", "Number") : cSuperType.Add("UInt16", "Number")

        cSuperType.Add("Byte[]", "Date") ' To handle SQL server's annoying datestamp type.

        'cSuperType.Add("Null",     "null") - handled by oReader.IsDBNull

        Dim E As Integer = 0
        Dim E_Desc As String = ""

        Dim nFieldCount As Integer = 0 ' temp vars
        Dim nRecordCount As Integer = 0

        Dim sShortType As String = "" ' temp vars
        Dim sShortSubType As String = ""
        Dim sVal As String = ""
        Dim sVal_Qt As String = ""
        Dim nLooper As Integer = 0

        Dim sDBType As String

        Dim sMdb As String = ""  ' "C:\Path\Somefile.mdb"  (also from MDB://C:\Path\Somefile.mdb)

        Dim sSQLFullURI As String      ' SQL://Server\Instance/Database
        Dim sSQLServInst As String = "" ' Server\Instance
        Dim sSQLDatabase As String = "" ' Database

        Dim sParams As String

        '    Examples...
        'sParams = "C:\SomeFolder\MyData.mdb"
        'sParams = "MDB://C:\SomeFolder\MyData.mdb"
        'sParams = "SQL://Server\Instance/Database"

        sParams = Command().Trim()

        If sParams.Substring(0, 6).ToUpper = "SQL://" Then
            sDBType = "SQL"

            sSQLFullURI = sParams.Substring(6, sParams.Length - 6) ' Server\Instance/Database

            sSQLServInst = sSQLFullURI.Split(CType("/", Char()))(0) ' Server\Instance
            sSQLDatabase = sSQLFullURI.Split(CType("/", Char()))(1) ' Database

        ElseIf sParams.Substring(0, 6).ToUpper() = "MDB://" Then
            sDBType = "MDB"
            sMdb = sParams.Substring(6, sParams.Length - 6)
        Else
            sDBType = "MDB"
            sMdb = sParams
        End If

        sQry = Console.In.ReadToEnd.Trim()

        sQry = sQry.Replace(vbCrLf, " ") _
                   .Replace(vbCr, " ") _
                   .Replace(vbLf, " ") _
                   .Replace(vbTab, " ") _
                   .Trim()

        sQryType = sQry.Substring(0, 6).ToUpper()
        SelectQry = Not (sQryType = "INSERT" Or sQryType = "UPDATE" Or sQryType = "DELETE")
        ' Only these are Non-select (writing) queries.
        ' Detect potential error conditions...

        If E = 0 And sQry = "" Then ' Check for Query errors with STDIN
            E = -65539
            E_Desc = "   Note from ConData:  Query text input is blank!" &
                     "   Query text must be provided through STDIN (standard input). "
        End If

        If E = 0 And Not (sQryType = "SELECT" Or sQryType = "INSERT" Or
                          sQryType = "UPDATE" Or sQryType = "DELETE") Then
            E = -65540
            E_Desc = "   Note from ConData:  Query type not supported! " &
                 "   Please try one of the 4 types instead:  SELECT, INSERT, UPDATE or DELETE. " &
                 "   Your data:  " & sQry & " "
        End If

        If sDBType = "MDB" Then
            If E = 0 And sMdb = "" Then ' Check for Mdb file errors with Command parameter
                E = -65536
                E_Desc = "   Note from ConData: Parameter is blank!  Mdb file not specified! " &
                         "   Should be full path name of Mdb file. " &
                         "   Example:  ConData.exe c:\folder\myfile.mdb "
            End If

            If E = 0 And Not (sMdb.Substring(sMdb.Length - 4).ToUpper = ".MDB" _
                               Or sMdb.Substring(sMdb.Length - 4).ToUpper = ".ACCDB") Then
                E = -65538
                E_Desc = "   Note from ConData:  File specified is not of type .mdb or .accdb! " &
                         "   Should be full path name of mdb/accdb file. " &
                         "   Example:  ConData.exe c:\folder\myfile.mdb " &
                     "   Your data:  " & sMdb & " "
            End If
            '        AndAlso >-   -   -  -  -  - - - - - - - ----> If E != 0, Don't bother checking Dir(sMbd).
            '                                                      A.k.a "short circuiting logic" / "smart if"
            If E = 0 AndAlso Dir(sMdb) = "" Then ' Not exists?
                E = -65537
                E_Desc = "   Note from ConData: File (FileName=" & sMdb & ") not found! " &
                         "   Should be full path name of Mdb file. " &
                         "   Example:  ConData.exe c:\folder\myfile.mdb "
            End If
        End If

        If sDBType = "SQL" Then
            If E = 0 And sSQLServInst = "" Then
                E = -32001
                E_Desc = "   Note from ConData: Server Instance should not be blank. "
            End If

            If E = 0 And sSQLDatabase = "" Then
                E = -32002
                E_Desc = "   Note from ConData: Database should not be blank. "
            End If
        End If

        ' RUN QUERY ############################################################################
        '            ( If no errors then Attempt to open the database and run the Query... )

        If E = 0 Then ' Attempt to open the database...
            If sDBType = "MDB" Then         'Provider=Microsoft.Jet.OLEDB.4.0;Data Source= ' for legacy use this.
                oConn = New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & sMdb)
            ElseIf sDBType = "SQL" Then
                oConn = New OleDbConnection("Provider=SQLNCLI10;Trusted_Connection=Yes;Database=" _
                                          & sSQLDatabase & ";SERVER=" & sSQLServInst)
            Else
                E = -1509
                E_Desc = "Note from ConData:   URI Scheme not understood."
            End If

            oCmd = New OleDbCommand(sQry, oConn)

            Try
                oConn.Open()
            Catch
                E = Err.Number
                E_Desc = Err.Description.Replace("""", "").Replace("\", "") &
                         "   ... Note from ConData / oConn.Open(): " &
                         "   Meaning:  Error opening the database. "
            End Try
        End If

        If E = 0 Then ' Attempt Query...
            If SelectQry Then ' SELECT
                Try
                    oReader = oCmd.ExecuteReader()
                Catch
                    E = Err.Number
                    E_Desc = Err.Description.Replace("""", "").Replace("\", "") &
                             " ... Note from ConData / oCmd.ExecuteReader(): " &
                       "   Meaning:  There was an error executing a SELECT Query " &
                       "   (for which results were expected back). "
                End Try
            Else              ' INSERT, UPDATE or DELETE
                Try
                    nRecordCount = oCmd.ExecuteNonQuery() ' Number of records effected
                Catch
                    nRecordCount = 0
                    E = Err.Number
                    E_Desc = Err.Description.Replace("""", "").Replace("\", "") &
                             "   ... Note from ConData / oCmd.ExecuteNonQuery(): " &
                             "   Meaning:  There was an error executing a NON-SELECT Query, " &
                             "   that is -- An UPDATE, INSERT, or DELETE. "
                End Try
            End If
        End If

        E_Desc = E_Desc.Replace("\", "\\") ' escape backslashes in path names.

        Console.WriteLine("{")
        Console.WriteLine("  ""ErrNum"":      " & E & ",")
        Console.WriteLine("  ""ErrDesc"":     " & """" & E_Desc & """" & ",")
        Console.WriteLine("  ""StartTime"":   " & """" & Now() & """" & ",")

        If E = 0 And SelectQry Then ' Start building the Values 2D array...

            nRecordCount = 0

            While oReader.Read() ' Each line...
                nRecordCount += 1 ' Increment row count

                If nRecordCount = 1 Then ' --------------------------------------------------1------- FIRST Row (Only!)

                    nFieldCount = oReader.FieldCount ' Number of columns

                    Console.WriteLine("  ""FieldCount"":   " & nFieldCount & ",")

                    Console.Write("  ""FieldIDs"":") ' Write field headers...
                    Console.Write("    {")
                    For nLooper = 0 To nFieldCount - 1 ' Each field

                        Console.Write("""" & oReader.GetName(nLooper) & """: " & nLooper)

                        If nLooper < nFieldCount - 1 Then ' ~~~~~~~~ All EXCEPT Last Field
                            Console.Write(", ")
                        End If

                    Next nLooper
                    Console.WriteLine("},")

                    Console.Write("  ""SubTypes"":")
                    Console.Write("    [")
                    For nLooper = 0 To nFieldCount - 1 ' Each field
                        If oReader.IsDBNull(nLooper) Then
                            Console.Write("""Null""")
                        Else
                            Console.Write("""" & Mid(oReader.GetFieldType(nLooper).ToString, 8) & """")
                        End If
                        If nLooper < nFieldCount - 1 Then ' ~~~~~~~~ All EXCEPT Last field
                            Console.Write(", ")
                        End If

                    Next nLooper
                    Console.WriteLine("],")

                    Console.Write("  ""Types"":")
                    Console.Write("       [")
                    For nLooper = 0 To nFieldCount - 1 ' Each field
                        If oReader.IsDBNull(nLooper) Then
                            Console.Write("""null""")
                        Else
                            Console.Write("""" & cSuperType(Mid(oReader.GetFieldType(nLooper).ToString, 8)) _
                                                 .ToString.ToLower() & """")
                        End If

                        If nLooper < nFieldCount - 1 Then ' ~~~~~~~~ All EXCEPT Last field
                            Console.Write(", ")
                        End If
                    Next nLooper
                    Console.WriteLine("],")

                    Console.Write("  ""Names"":")
                    Console.Write("       [")
                    For nLooper = 0 To nFieldCount - 1 ' Each field

                        Console.Write("""" & oReader.GetName(nLooper) & """")

                        If nLooper < nFieldCount - 1 Then ' ~~~~~~~~ All EXCEPT Last field
                            Console.Write(", ")
                        End If

                    Next nLooper
                    Console.WriteLine("],")

                    Console.WriteLine("  ""Values"":") ' Now start Values 2D Array...
                    Console.WriteLine("  [")
                    Console.Write("    [")

                Else ' > 1  -------------------------------------------------------------------2,3,4,5- EVERY OTHER Row

                    Console.WriteLine("],")
                    Console.Write("    [")
                End If

                ' ---------------------------------------------------------------------------1,2,3,4,5- EVERY Row

                For nLooper = 0 To nFieldCount - 1 ' Each field

                    ' ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Every field

                    sShortSubType = oReader.GetFieldType(nLooper).ToString.Split(CType(".", Char()))(1) ' Remove prefix: System.
                    sShortType = cSuperType(sShortSubType).ToString ' Hashtable lookup

                    sVal_Qt = IIf((sShortType = "String" Or sShortType = "Date"), """", "").ToString()

                    If oReader.IsDBNull(nLooper) Then
                        Console.Write("null")
                    Else
                        Console.Write(sVal_Qt)

                        Console.Write(oReader(nLooper).ToString() _
                                                      .Replace("\", "\\") _
                                                      .Replace("""", "\""") _
                                                      .Replace(vbCrLf, " ") _
                                                      .Replace(vbCr, " ") _
                                                      .Replace(vbLf, " ") _
                                                      .Replace(vbTab, " ") _
                                                      .Replace("True", "true") _
                                                      .Replace("False", "false"))
                        Console.Write(sVal_Qt)
                    End If

                    If nLooper < nFieldCount - 1 Then ' ~~~~~~~~~~~~ All EXCEPT Last field
                        Console.Write(", ")
                    End If

                Next nLooper

            End While

            If nRecordCount > 0 Then ' --------------------------------------------------------------5- After LAST Row

                Console.WriteLine("]")
                Console.WriteLine("  ],")

            End If ' -------------------------------------------------------------------------------------------------

        End If

        Console.Write("  ""RecordCount"": " & nRecordCount & "," & vbCrLf &
                      "  ""EndTime"":     " & """" & Now() & """" & vbCrLf)

        Console.Write("}")

        If oConn.State <> ConnectionState.Closed Then oConn.Close()

        oCmd = Nothing
        oConn = Nothing
    End Sub
End Module
