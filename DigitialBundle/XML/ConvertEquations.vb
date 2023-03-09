Imports System.Xml
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.Runtime.InteropServices.ComTypes
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Imaging
Imports MTSDKDN
Imports IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject

Namespace ConvertEquations

    Class MTSDK
        Public Sub New()
        End Sub

        Protected m_bDidInit As Boolean = False

        Public Function Init() As Boolean
            If Not m_bDidInit Then
                Dim result As Int32 = MathTypeSDK.Instance.MTAPIConnectMgn(MTApiStartValues.mtinitLAUNCH_AS_NEEDED, 30)

                If result = MathTypeReturnValue.mtOK Then
                    m_bDidInit = True
                    Return True
                Else
                    Return False
                End If
            End If

            Return True
        End Function

        Public Function DeInit() As Boolean
            If m_bDidInit Then
                m_bDidInit = False
                MathTypeSDK.Instance.MTAPIDisconnectMgn()
            End If

            Return True
        End Function
    End Class

    MustInherit Class EquationOutput
        Public Sub New(ByVal strOutTrans As String)
            If Not String.IsNullOrEmpty(strOutTrans) Then
                Me.strOutTrans = strOutTrans
            Else
                Me.strOutTrans = String.Empty
            End If
        End Sub

        Protected Sub New()
        End Sub

        Protected m_iType As Short

        Public Property iType As Short
            Get
                Return m_iType
            End Get
            Protected Set(ByVal value As Short)
                m_iType = value
            End Set
        End Property

        Protected m_iFormat As Short

        Public Property iFormat As Short
            Get
                Return m_iFormat
            End Get
            Protected Set(ByVal value As Short)
                m_iFormat = value
            End Set
        End Property

        Private m_strFileName As String

        Public Property strFileName As String
            Get
                Return m_strFileName
            End Get
            Set(ByVal value As String)
                m_strFileName = value
            End Set
        End Property

        Private m_strEquation As String

        Public Property strEquation As String
            Get
                Return m_strEquation
            End Get
            Set(ByVal value As String)
                m_strEquation = value
            End Set
        End Property

        Protected m_strOutTrans As String

        Public Property strOutTrans As String
            Get
                Return m_strOutTrans
            End Get
            Set(ByVal value As String)
                m_strOutTrans = value
            End Set
        End Property

        Public MustOverride Function Put() As Boolean
    End Class

    MustInherit Class EquationOutputClipboard
        Inherits EquationOutput

        Public Sub New(ByVal strOutTrans As String)
            MyBase.New(strOutTrans)
            strFileName = String.Empty
            iType = MTXFormEqn.mtxfmCLIPBOARD
        End Sub

        Public Sub New()
            MyBase.New()
            strFileName = String.Empty
            iType = MTXFormEqn.mtxfmCLIPBOARD
        End Sub

        Public Overrides Function Put() As Boolean
            Return True
        End Function
    End Class

    Class EquationOutputClipboardText
        Inherits EquationOutputClipboard

        Public Sub New(ByVal strOutTrans As String)
            MyBase.New(strOutTrans)
            iFormat = MTXFormEqn.mtxfmTEXT
        End Sub

        Public Sub New()
            MyBase.New()
            iFormat = MTXFormEqn.mtxfmTEXT
        End Sub

        Public Overrides Function ToString() As String
            Return "Clipboard Text"
        End Function
    End Class

    MustInherit Class EquationOutputFile
        Inherits EquationOutput

        Public Sub New(ByVal strFileName As String, ByVal strOutTrans As String)
            MyBase.New(strOutTrans)
            Me.strFileName = strFileName
            iType = MTXFormEqn.mtxfmFILE
        End Sub

        Protected Sub New(ByVal strFileName As String)
            MyBase.New()
            Me.strFileName = strFileName
            iType = MTXFormEqn.mtxfmFILE
        End Sub

        Public Overrides Function Put() As Boolean
            Return True
        End Function
    End Class

    Class EquationOutputFileGIF
        Inherits EquationOutputFile

        Public Sub New(ByVal strFileName As String)
            MyBase.New(strFileName)
            iFormat = MTXFormEqn.mtxfmGIF
        End Sub

        Public Overrides Function ToString() As String
            Return "GIF file"
        End Function
    End Class

    Class EquationOutputFileWMF
        Inherits EquationOutputFile

        Public Sub New(ByVal strFileName As String)
            MyBase.New(strFileName)
            iFormat = MTXFormEqn.mtxfmPICT
        End Sub

        Public Overrides Function ToString() As String
            Return "WMF file"
        End Function
    End Class

    Class EquationOutputFileEPS
        Inherits EquationOutputFile

        Public Sub New(ByVal strFileName As String)
            MyBase.New(strFileName)
            iFormat = MTXFormEqn.mtxfmEPS_NONE
        End Sub

        Public Overrides Function ToString() As String
            Return "EPS file"
        End Function
    End Class

    Class EquationOutputFileText
        Inherits EquationOutputFile

        Public Sub New(ByVal strFileName As String, ByVal strOutTrans As String)
            MyBase.New(strFileName, strOutTrans)
            iType = MTXFormEqn.mtxfmLOCAL
            iFormat = MTXFormEqn.mtxfmTEXT
        End Sub

        Public Overrides Function Put() As Boolean
            Try
                Dim stream As FileStream = New FileStream(strFileName, FileMode.OpenOrCreate, FileAccess.Write)
                Dim writer As StreamWriter = New StreamWriter(stream)
                writer.WriteLine(strEquation)
                writer.Close()
                stream.Close()
                Return True
            Catch e As Exception
                Console.WriteLine(e.Message)
                Return False
            End Try
        End Function

        Public Overrides Function ToString() As String
            Return "Text file"
        End Function
    End Class

    MustInherit Class EquationInput
        Public Sub New(ByVal strInTrans As String)
            If Not String.IsNullOrEmpty(strInTrans) Then
                Me.strInTrans = strInTrans
            Else
                Me.strInTrans = String.Empty
            End If
        End Sub

        Protected m_iType As Short

        Public Property iType As Short
            Get
                Return m_iType
            End Get
            Protected Set(ByVal value As Short)
                m_iType = value
            End Set
        End Property

        Protected m_iFormat As Short

        Public Property iFormat As Short
            Get
                Return m_iFormat
            End Get
            Protected Set(ByVal value As Short)
                m_iFormat = value
            End Set
        End Property

        Protected m_strEquation As String

        Public Property strEquation As String
            Get
                Return m_strEquation
            End Get
            Set(ByVal value As String)
                m_strEquation = value
            End Set
        End Property

        Protected m_bEquation As Byte()

        Public Property bEquation As Byte()
            Get
                Return m_bEquation
            End Get
            Set(ByVal value As Byte())
                m_bEquation = value
            End Set
        End Property

        Protected m_bMTEF As Byte()

        Public Property bMTEF As Byte()
            Get
                Return m_bMTEF
            End Get
            Set(ByVal value As Byte())
                m_bMTEF = value
            End Set
        End Property

        Protected m_iMTEF_Length As Integer

        Public Property iMTEF_Length As Integer
            Get
                Return m_iMTEF_Length
            End Get
            Set(ByVal value As Integer)
                m_iMTEF_Length = value
            End Set
        End Property

        Protected m_strMTEF As String

        Public Property strMTEF As String
            Get
                Return m_strMTEF
            End Get
            Set(ByVal value As String)
                m_strMTEF = value
            End Set
        End Property

        Protected m_strInTrans As String

        Public Property strInTrans As String
            Get
                Return m_strInTrans
            End Get
            Set(ByVal value As String)
                m_strInTrans = value
            End Set
        End Property

        Protected m_strFileName As String

        Public Property strFileName As String
            Get
                Return m_strFileName
            End Get
            Set(ByVal value As String)
                m_strFileName = value
            End Set
        End Property

        Protected sdk As MTSDK = New MTSDK()
        Public MustOverride Function [Get]() As Boolean
        Public MustOverride Function GetMTEF() As Boolean
    End Class

    MustInherit Class EquationInputClipboard
        Inherits EquationInput

        Public Sub New(ByVal strInTrans As String)
            MyBase.New(strInTrans)
            iType = MTXFormEqn.mtxfmCLIPBOARD
        End Sub
    End Class

    Class EquationInputClipboardText
        Inherits EquationInputClipboard

        Public Sub New(ByVal strInTrans As String)
            MyBase.New(strInTrans)
            iFormat = MTXFormEqn.mtxfmTEXT
        End Sub

        Public Overrides Function [Get]() As Boolean
            Return True
        End Function

        Public Overrides Function GetMTEF() As Boolean
            Return True
        End Function

        Public Overrides Function ToString() As String
            Return "Clipboard text"
        End Function
    End Class

    Class EquationInputClipboardEmbeddedObject
        Inherits EquationInputClipboard

        Public Sub New()
            MyBase.New(ClipboardFormats.cfEmbeddedObj)
            iFormat = MTXFormEqn.mtxfmMTEF
        End Sub

        Public Overrides Function [Get]() As Boolean
            Return True
        End Function

        Public Overrides Function GetMTEF() As Boolean
            Return True
        End Function

        Public Overrides Function ToString() As String
            Return "Clipboard Embedded Object"
        End Function
    End Class

    MustInherit Class EquationInputFile
        Inherits EquationInput

        Public Sub New(ByVal strFileName As String, ByVal strInTrans As String)
            MyBase.New(strInTrans)
            Me.strFileName = strFileName
            iType = MTXFormEqn.mtxfmLOCAL
        End Sub
    End Class

    Class EquationInputFileText
        Inherits EquationInputFile

        Public Sub New(ByVal strFileName As String, ByVal strInTrans As String)
            MyBase.New(strFileName, strInTrans)
            iFormat = MTXFormEqn.mtxfmMTEF
        End Sub

        Public Overrides Function ToString() As String
            Return "Text file"
        End Function

        Public Overrides Function [Get]() As Boolean
            Try
                strEquation = System.IO.File.ReadAllText(strFileName)
                Return True
            Catch e As Exception
                Console.WriteLine(e.Message)
                Return False
            End Try
        End Function

        Public Overrides Function GetMTEF() As Boolean
            Dim bReturn As Boolean = False
            If Not sdk.Init() Then Return bReturn
            Dim dataObject As IDataObject = MathTypeSDK.getIDataObject()

            If dataObject Is Nothing Then
                sdk.DeInit()
                Return bReturn
            End If

            Dim formatEtc As FORMATETC = New FORMATETC()
            Dim stgMedium As STGMEDIUM = New STGMEDIUM()

            Try
                formatEtc.cfFormat = CShort(DataFormats.GetFormat(strInTrans).Id)
                formatEtc.dwAspect = DVASPECT.DVASPECT_CONTENT
                formatEtc.lindex = -1
                formatEtc.ptd = CType(0, IntPtr)
                formatEtc.tymed = TYMED.TYMED_HGLOBAL
                stgMedium.unionmember = Marshal.StringToHGlobalAuto(strEquation)
                stgMedium.tymed = TYMED.TYMED_HGLOBAL
                stgMedium.pUnkForRelease = 0
                dataObject.SetData(formatEtc, stgMedium, False)
                formatEtc.cfFormat = CShort(DataFormats.GetFormat("MathType EF").Id)
                formatEtc.dwAspect = DVASPECT.DVASPECT_CONTENT
                formatEtc.lindex = -1
                formatEtc.ptd = CType(0, IntPtr)
                formatEtc.tymed = TYMED.TYMED_ISTORAGE
                stgMedium = New STGMEDIUM()
                stgMedium.tymed = TYMED.TYMED_NULL
                stgMedium.pUnkForRelease = 0
                dataObject.GetData(formatEtc, stgMedium)
            Catch e As COMException
                Console.WriteLine("MathML conversion to MathType threw an exception: " & Environment.NewLine & e.ToString())
                sdk.DeInit()
                Return bReturn
            End Try

            Dim handleRef As HandleRef = New HandleRef(Nothing, stgMedium.unionmember)

            Try
                Dim ptrToHandle As IntPtr = MathTypeSDK.GlobalLock(handleRef)
                m_iMTEF_Length = MathTypeSDK.GlobalSize(handleRef)
                m_bMTEF = New Byte(m_iMTEF_Length - 1) {}
                Marshal.Copy(ptrToHandle, m_bMTEF, 0, m_iMTEF_Length)
                m_strMTEF = System.Text.ASCIIEncoding.ASCII.GetString(m_bMTEF)
                bReturn = True
            Catch e As Exception
                Console.WriteLine("Generation of image from MathType failed: " & Environment.NewLine & e.ToString())
            Finally
                MathTypeSDK.GlobalUnlock(handleRef)
            End Try

            sdk.DeInit()
            Return bReturn
        End Function
    End Class

    Class EquationInputFileWMF2
        Inherits EquationInputFile

        Public Sub New(ByVal strFileName As String)
            MyBase.New(strFileName, "")
            iFormat = MTXFormEqn.mtxfmEPS_WMF
            iType = MTXFormEqn.mtxfmFILE
        End Sub

        Public Overrides Function [Get]() As Boolean
            Return True
        End Function

        Public Overrides Function GetMTEF() As Boolean
            Return True
        End Function
    End Class

    Class EquationInputFileWMF
        Inherits EquationInputFile

        Public Sub New(ByVal strFileName As String)
            MyBase.New(strFileName, "")
            iFormat = MTXFormEqn.mtxfmMTEF
        End Sub

        Public Overrides Function [Get]() As Boolean
            Return True
        End Function

        Public Overrides Function ToString() As String
            Return "WMF file"
        End Function

        Public Overrides Function GetMTEF() As Boolean
            Play()
            If Not Succeeded() Then Return False
            Return True
        End Function

        Protected Class WmfForm
            Inherits Form

            Public Sub New()
            End Sub
        End Class

        Protected wf As WmfForm = New WmfForm()

        <StructLayout(LayoutKind.Sequential, Pack:=1)>
        Protected Structure wmfHeader
            Public iComment As Int16
            Public ix1 As Int16
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=8)>
            Public strSig As String
            Public iVer As Int16
            Public iTotalLen As Int32
            Public iDataLen As Int32
        End Structure

        Protected m_wmfHeader As wmfHeader
        Protected m_metafile As Metafile
        Protected Const m_strSig As String = "AppsMFC"
        Protected m_succeeded As Boolean = False

        Protected Sub Play()
            Try
                m_succeeded = False
                Dim metafileDelegate As Graphics.EnumerateMetafileProc
                Dim destPoint As Point
                m_metafile = New Metafile(strFileName)
                metafileDelegate = New Graphics.EnumerateMetafileProc(AddressOf MetafileCallback)
                destPoint = New Point(20, 10)
                Dim graphics As Graphics = wf.CreateGraphics()
                graphics.EnumerateMetafile(m_metafile, destPoint, metafileDelegate)
            Catch e As Exception
                Console.WriteLine(e.Message)
            End Try
        End Sub

        Protected Function Succeeded() As Boolean
            Return m_succeeded
        End Function

        Protected Function MetafileCallback(ByVal recordType As EmfPlusRecordType, ByVal flags As Integer, ByVal dataSize As Integer, ByVal data As IntPtr, ByVal callbackData As PlayRecordCallback) As Boolean
            Dim dataArray As Byte() = Nothing

            If data <> IntPtr.Zero Then
                dataArray = New Byte(dataSize - 1) {}
                Marshal.Copy(data, dataArray, 0, dataSize)

                If recordType = EmfPlusRecordType.WmfEscape AndAlso dataSize >= Marshal.SizeOf(m_wmfHeader) AndAlso Not m_succeeded Then
                    m_wmfHeader = CType(RawDeserialize(dataArray, 0, m_wmfHeader.[GetType]()), wmfHeader)

                    If m_wmfHeader.strSig.Equals(m_strSig, StringComparison.CurrentCultureIgnoreCase) Then
                        Dim enc As System.Text.ASCIIEncoding = New System.Text.ASCIIEncoding()
                        Dim strCompanyInfo As String = enc.GetString(dataArray, Marshal.SizeOf(m_wmfHeader), m_wmfHeader.iDataLen)
                        Dim iNull As Integer = strCompanyInfo.IndexOf(vbNullChar)

                        If iNull >= 0 Then
                            Dim mtefStart As Integer = Marshal.SizeOf(m_wmfHeader) + iNull + 1
                            iMTEF_Length = m_wmfHeader.iDataLen
                            bMTEF = New Byte(iMTEF_Length - 1) {}
                            Array.Copy(dataArray, mtefStart, bMTEF, 0, iMTEF_Length)
                            m_succeeded = True
                        End If
                    End If
                End If
            End If

            m_metafile.PlayRecord(recordType, flags, dataSize, dataArray)
            Return True
        End Function

        Protected Shared Function RawDeserialize(ByVal rawData As Byte(), ByVal position As Integer, ByVal anyType As Type) As Object
            Dim rawsize As Integer = Marshal.SizeOf(anyType)
            If rawsize > rawData.Length Then Return Nothing
            Dim buffer As IntPtr = Marshal.AllocHGlobal(rawsize)
            Marshal.Copy(rawData, position, buffer, rawsize)
            Dim retobj As Object = Marshal.PtrToStructure(buffer, anyType)
            Marshal.FreeHGlobal(buffer)
            Return retobj
        End Function
    End Class

    Class EquationInputFileGIF
        Inherits EquationInputFile

        Public Sub New(ByVal strFileName As String)
            MyBase.New(strFileName, "")
            iFormat = MTXFormEqn.mtxfmMTEF
        End Sub

        Public Overrides Function ToString() As String
            Return "GIF file"
        End Function

        Public Overrides Function [Get]() As Boolean
            Try
                Dim stream As FileStream = New FileStream(strFileName, FileMode.Open, FileAccess.Read)
                Dim reader As BinaryReader = New BinaryReader(stream)
                Dim iArrayLength As Integer = CInt(stream.Length)
                bEquation = reader.ReadBytes(iArrayLength)
                reader.Close()
                stream.Close()
                Return True
            Catch e As Exception
                Console.WriteLine(e.Message)
                Return False
            End Try
        End Function

        Private signature As Byte() = {&H21, &HFF, &HB, &H4D, &H61, &H74, &H68, &H54, &H79, &H70, &H65, &H30, &H30, &H31}

        Public Overrides Function GetMTEF() As Boolean
            Try
                Dim iSigStart As Integer = 0

                While (CSharpImpl.__Assign(iSigStart, Array.IndexOf(bEquation, signature(0), iSigStart))) >= 0

                    If CompareArrays(bEquation, iSigStart, bEquation.Length, signature, 0, signature.Length) Then
                        Dim iIndex As Integer = iSigStart + signature.Length
                        iMTEF_Length = 0
                        Dim bLen As Byte

                        Try

                            While (CSharpImpl.__Assign(bLen, CByte(bEquation.GetValue(iIndex)))) > 0
                                Array.Resize(m_bMTEF, iMTEF_Length + bLen)
                                Array.Copy(bEquation, iIndex + 1, bMTEF, iMTEF_Length, bLen)
                                iMTEF_Length += bLen
                                iIndex += bLen + 1
                            End While

                        Catch e As Exception
                            Console.WriteLine(e.Message)
                            Return False
                        End Try

                        Return True
                    End If

                    iSigStart += 1
                End While

                Return False
            Catch e As Exception
                Console.WriteLine(e.Message)
                Return False
            End Try
        End Function

        Protected Function CompareArrays(ByVal left As Array, ByVal leftStart As Integer, ByVal leftLen As Integer, ByVal right As Array, ByVal rightStart As Integer, ByVal rightLen As Integer) As Boolean
            Dim leftCompareNum As Integer = leftLen - leftStart
            Dim rightCompareNum As Integer = rightLen - rightStart
            Dim compareNum As Integer = If(leftCompareNum > rightCompareNum, rightCompareNum, leftCompareNum)

            For x As Integer = 0 To compareNum - 1
                If CByte(left.GetValue(leftStart + x)) <> CByte(right.GetValue(rightStart + x)) Then Return False
            Next

            Return True
        End Function

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class

    Class EquationInputFileEPS
        Inherits EquationInputFile

        Public Sub New(ByVal strFileName As String)
            MyBase.New(strFileName, "")
            iFormat = MTXFormEqn.mtxfmTEXT
        End Sub

        Public Overrides Function ToString() As String
            Return "EPS file"
        End Function

        Public Overrides Function [Get]() As Boolean
            Try
                strEquation = System.IO.File.ReadAllText(strFileName)
                Return True
            Catch e As Exception
                Console.WriteLine(e.Message)
                Return False
            End Try
        End Function

        Public Overrides Function GetMTEF() As Boolean
            Const strSig1 As String = "MathType"
            Const strSig2 As String = "MTEF"
            Dim iSig1Start As Integer = 0

            While (CSharpImpl.__Assign(iSig1Start, strEquation.IndexOf(strSig1, iSig1Start))) >= 0
                Dim iSig2Start As Integer = strEquation.IndexOf(strSig2, iSig1Start + 1)
                Dim iDelimStart As Integer = iSig1Start + strSig1.Length
                Dim iDelimLen As Integer = iSig2Start - iDelimStart

                If iSig2Start < 0 OrElse iDelimLen <> 1 Then
                    iSig1Start += 1
                    Continue While
                End If

                Dim strDelim As String = strEquation.Substring(iDelimStart, iDelimLen)
                Dim id1 As Integer = strEquation.IndexOf(strDelim, iSig1Start)
                Dim id2 As Integer = strEquation.IndexOf(strDelim, id1 + 1)
                Dim id3 As Integer = strEquation.IndexOf(strDelim, id2 + 1)
                Dim id4 As Integer = strEquation.IndexOf(strDelim, id3 + 1)
                Dim id5 As Integer = strEquation.IndexOf(strDelim, id4 + 1)
                Dim id6 As Integer = strEquation.IndexOf(strDelim, id5 + 1)
                m_strMTEF = strEquation.Substring(iSig1Start, id6 - iSig1Start + 1)
                bMTEF = System.Text.Encoding.ASCII.GetBytes(m_strMTEF)
                iMTEF_Length = bMTEF.Length
                Return True
            End While

            Return False
        End Function

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class

    Class ConvertEquation
        Protected m_ei As EquationInput
        Protected m_eo As EquationOutput
        Protected m_sdk As MTSDK = New MTSDK()

        Public Sub New()
        End Sub

        Public Overridable Function Convert(ByVal ei As EquationInput, ByVal eo As EquationOutput) As Boolean
            m_ei = ei
            m_eo = eo
            Return Convert()
        End Function

        Protected Overridable Function Convert() As Boolean
            Dim bReturn As Boolean = False
            Console.WriteLine("Converting {0} to {1}", m_ei.ToString(), m_eo.ToString())
            Console.WriteLine("Get equation: {0}", m_ei.strFileName)

            If m_ei.[Get]() Then
                Console.WriteLine("Get MTEF")

                If m_ei.GetMTEF() Then
                    Console.WriteLine("Convert Equation")

                    If ConvertToOutput() Then
                        Console.WriteLine("Write equation: {0}", m_eo.strFileName)
                        If m_eo.Put() Then bReturn = True
                    End If
                End If
            End If

            Console.WriteLine("Convert success: {0}" & vbCrLf, bReturn.ToString())
            Return bReturn
        End Function

        Protected Function SetTranslator() As Boolean
            If String.IsNullOrEmpty(m_eo.strOutTrans) Then Return True
            'Dim stat As Int32 = MathTypeSDK.Instance.MTXFormSetTranslatorMgn(MTXFormSetTranslator.mtxfmTRANSL_INC_NAME + MTXFormSetTranslator.mtxfmTRANSL_INC_DATA, m_eo.strOutTrans)
            Dim stat As Int32 = MathTypeSDK.Instance.MTXFormSetTranslatorMgn(MTXFormSetTranslator.mtxfmTRANSL_INC_NONE, m_eo.strOutTrans)
            Return stat = MathTypeReturnValue.mtOK
        End Function

        Protected Function ConvertToOutput() As Boolean
            Dim bResult As Boolean = False

            Try
                If Not m_sdk.Init() Then Return False

                If MathTypeSDK.Instance.MTXFormResetMgn() = MathTypeReturnValue.mtOK AndAlso SetTranslator() Then
                    Dim stat As Int32 = 0
                    Dim iBufferLength As Int32 = 5000
                    Dim strDest As StringBuilder = New StringBuilder(iBufferLength)
                    Dim dims As MTAPI_DIMS = New MTAPI_DIMS()
                    stat = MathTypeSDK.Instance.MTXFormEqnMgn(m_ei.iType, m_ei.iFormat, m_ei.bMTEF, m_ei.iMTEF_Length, m_eo.iType, m_eo.iFormat, strDest, iBufferLength, m_eo.strFileName, dims)

                    If stat = MathTypeReturnValue.mtOK Then
                        m_eo.strEquation = strDest.ToString()
                        bResult = True
                    End If
                End If

                m_sdk.DeInit()
            Catch e As Exception
                Console.WriteLine(e.Message)
            End Try

            Return bResult
        End Function

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class

    Class Program
        Public Function GetInputFolder(ByVal strFile As String) As String
            Dim fi As FileInfo = New FileInfo(Application.ExecutablePath)
            Dim strRet As String = System.IO.Path.Combine(fi.Directory.Parent.Parent.FullName, "Data")
            Return System.IO.Path.Combine(strRet, strFile)
        End Function

        Protected iFileNum As Integer = 0

        Public Function GetOutputFile(ByVal strExt As String) As String
            Dim strRet As String = Path.GetTempPath()
            Dim strFileName As String
            strFileName = String.Format("Output{0}.{1}", Math.Min(System.Threading.Interlocked.Increment(iFileNum), iFileNum - 1), strExt)
            Return System.IO.Path.Combine(strRet, strFileName)
        End Function

        Public Sub MessagePause(ByVal strMessage As String)
            Console.WriteLine(strMessage)
            Console.ReadKey(True)
        End Sub

        Public Sub ClipboardToClipboard(ByVal p As Program, ByVal ce As ConvertEquation)
            p.MessagePause("Copy MathML to the clipboard, then press any key")
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputClipboardText())
            p.MessagePause("Inspect the clipboard, then press any key")
            p.MessagePause("Copy Base 64 MTEF to the clipboard, then press any key")
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputClipboardText())
            p.MessagePause("Inspect the clipboard, then press any key")
            p.MessagePause("From MS Word, copy a MathType equation to the clipboard, then press any key")
            ce.Convert(New EquationInputClipboardEmbeddedObject(), New EquationOutputClipboardText())
            p.MessagePause("Inspect the clipboard, then press any key")
        End Sub

        Public Sub ClipboardToFile(ByVal p As Program, ByVal ce As ConvertEquation)
            p.MessagePause("Copy MathML to the clipboard, then press any key")
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileEPS(p.GetOutputFile("eps")))
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileGIF(p.GetOutputFile("gif")))
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileWMF(p.GetOutputFile("wmf")))
            p.MessagePause("Copy Base 64 MTEF to the clipboard, then press any key")
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileEPS(p.GetOutputFile("eps")))
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileGIF(p.GetOutputFile("gif")))
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileWMF(p.GetOutputFile("wmf")))
            p.MessagePause("From MS Word, copy a MathType equation to the clipboard, then press any key")
            ce.Convert(New EquationInputClipboardEmbeddedObject(), New EquationOutputFileEPS(p.GetOutputFile("eps")))
            ce.Convert(New EquationInputClipboardEmbeddedObject(), New EquationOutputFileGIF(p.GetOutputFile("gif")))
            ce.Convert(New EquationInputClipboardEmbeddedObject(), New EquationOutputFileWMF(p.GetOutputFile("wmf")))
        End Sub

        Public Sub ClipboardToLocal(ByVal p As Program, ByVal ce As ConvertEquation)
            p.MessagePause("Copy MathML to the clipboard, then press any key")
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileText(p.GetOutputFile("txt"), "MathML2 (m namespace).tdl"))
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileText(p.GetOutputFile("txt"), ""))
            p.MessagePause("Copy Base 64 MTEF to the clipboard, then press any key")
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileText(p.GetOutputFile("txt"), "Texvc.tdl"))
            ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileText(p.GetOutputFile("txt"), ""))
            p.MessagePause("From MS Word, copy a MathType equation to the clipboard, then press any key")
            ce.Convert(New EquationInputClipboardEmbeddedObject(), New EquationOutputFileText(p.GetOutputFile("txt"), "Texvc.tdl"))
            ce.Convert(New EquationInputClipboardEmbeddedObject(), New EquationOutputFileText(p.GetOutputFile("txt"), ""))
        End Sub

        Public Sub FileToClipboard(ByVal p As Program, ByVal ce As ConvertEquation)
            ce.Convert(New EquationInputFileEPS(p.GetInputFolder("Equation3.eps")), New EquationOutputClipboardText())
            p.MessagePause("Inspect the clipboard, then press any key")
            ce.Convert(New EquationInputFileGIF(p.GetInputFolder("Equation2.gif")), New EquationOutputClipboardText())
            p.MessagePause("Inspect the clipboard, then press any key")
            ce.Convert(New EquationInputFileWMF(p.GetInputFolder("Equation1.wmf")), New EquationOutputClipboardText())
            p.MessagePause("Inspect the clipboard, then press any key")
        End Sub

        Public Sub FileToFile(ByVal p As Program, ByVal ce As ConvertEquation)
            ce.Convert(New EquationInputFileWMF(p.GetInputFolder("Equation1.wmf")), New EquationOutputFileEPS(p.GetOutputFile("eps")))
            ce.Convert(New EquationInputFileWMF(p.GetInputFolder("Equation1.wmf")), New EquationOutputFileGIF(p.GetOutputFile("gif")))
            ce.Convert(New EquationInputFileWMF(p.GetInputFolder("Equation1.wmf")), New EquationOutputFileWMF(p.GetOutputFile("wmf")))
            ce.Convert(New EquationInputFileGIF(p.GetInputFolder("Equation2.gif")), New EquationOutputFileEPS(p.GetOutputFile("eps")))
            ce.Convert(New EquationInputFileGIF(p.GetInputFolder("Equation2.gif")), New EquationOutputFileGIF(p.GetOutputFile("gif")))
            ce.Convert(New EquationInputFileGIF(p.GetInputFolder("Equation2.gif")), New EquationOutputFileWMF(p.GetOutputFile("wmf")))
            ce.Convert(New EquationInputFileEPS(p.GetInputFolder("Equation3.eps")), New EquationOutputFileEPS(p.GetOutputFile("eps")))
            ce.Convert(New EquationInputFileEPS(p.GetInputFolder("Equation3.eps")), New EquationOutputFileGIF(p.GetOutputFile("gif")))
            ce.Convert(New EquationInputFileEPS(p.GetInputFolder("Equation3.eps")), New EquationOutputFileWMF(p.GetOutputFile("wmf")))
        End Sub

        Public Sub FileToLocal(ByVal p As Program, ByVal ce As ConvertEquation)
            ce.Convert(New EquationInputFileWMF(p.GetInputFolder("Equation1.wmf")), New EquationOutputFileText(p.GetOutputFile("txt"), "MathML2 (m namespace).tdl"))
            ce.Convert(New EquationInputFileWMF(p.GetInputFolder("Equation1.wmf")), New EquationOutputFileText(p.GetOutputFile("txt"), "Texvc.tdl"))
            ce.Convert(New EquationInputFileWMF(p.GetInputFolder("Equation1.wmf")), New EquationOutputFileText(p.GetOutputFile("txt"), ""))
            ce.Convert(New EquationInputFileGIF(p.GetInputFolder("Equation2.gif")), New EquationOutputFileText(p.GetOutputFile("txt"), "MathML2 (m namespace).tdl"))
            ce.Convert(New EquationInputFileGIF(p.GetInputFolder("Equation2.gif")), New EquationOutputFileText(p.GetOutputFile("txt"), ""))
            ce.Convert(New EquationInputFileEPS(p.GetInputFolder("Equation3.eps")), New EquationOutputFileText(p.GetOutputFile("txt"), "MathML2 (m namespace).tdl"))
            ce.Convert(New EquationInputFileEPS(p.GetInputFolder("Equation3.eps")), New EquationOutputFileText(p.GetOutputFile("txt"), ""))
        End Sub

        Public Sub LocalToClipboard(ByVal p As Program, ByVal ce As ConvertEquation)
            ce.Convert(New EquationInputFileText(p.GetInputFolder("MathML.txt"), ClipboardFormats.cfMML), New EquationOutputClipboardText())
            p.MessagePause("Inspect the clipboard, then press any key")
            ce.Convert(New EquationInputFileText(p.GetInputFolder("Base64MTEF.txt"), ClipboardFormats.cfMML), New EquationOutputClipboardText())
            p.MessagePause("Inspect the clipboard, then press any key")
        End Sub

        Public Sub LocalToFile(ByVal p As Program, ByVal ce As ConvertEquation)
            ce.Convert(New EquationInputFileText(p.GetInputFolder("TeX.txt"), ClipboardFormats.cfTeX), New EquationOutputFileEPS(p.GetOutputFile("eps")))
            ce.Convert(New EquationInputFileText(p.GetInputFolder("MathML.txt"), ClipboardFormats.cfMML), New EquationOutputFileGIF(p.GetOutputFile("gif")))
            ce.Convert(New EquationInputFileText(p.GetInputFolder("TeX.txt"), ClipboardFormats.cfTeX), New EquationOutputFileGIF(p.GetOutputFile("gif")))
            ce.Convert(New EquationInputFileText(p.GetInputFolder("MathML.txt"), ClipboardFormats.cfMML), New EquationOutputFileWMF(p.GetOutputFile("wmf")))
            ce.Convert(New EquationInputFileText(p.GetInputFolder("Base64MTEF.txt"), ClipboardFormats.cfMML), New EquationOutputFileEPS(p.GetOutputFile("eps")))
            ce.Convert(New EquationInputFileText(p.GetInputFolder("Base64MTEF.txt"), ClipboardFormats.cfMML), New EquationOutputFileGIF(p.GetOutputFile("gif")))
            ce.Convert(New EquationInputFileText(p.GetInputFolder("Base64MTEF.txt"), ClipboardFormats.cfMML), New EquationOutputFileWMF(p.GetOutputFile("wmf")))
        End Sub

        Public Sub LocalToLocal(ByVal p As Program, ByVal ce As ConvertEquation)
            ce.Convert(New EquationInputFileText(p.GetInputFolder("TeX.txt"), ClipboardFormats.cfTeX), New EquationOutputFileText(p.GetOutputFile("txt"), "MathML2 (m namespace).tdl"))
            ce.Convert(New EquationInputFileText(p.GetInputFolder("TeX.txt"), ClipboardFormats.cfTeX), New EquationOutputFileText(p.GetOutputFile("txt"), ""))
            ce.Convert(New EquationInputFileText(p.GetInputFolder("Base64MTEF.txt"), ClipboardFormats.cfMML), New EquationOutputFileText(p.GetOutputFile("txt"), "MathML2 (m namespace).tdl"))
            ce.Convert(New EquationInputFileText(p.GetInputFolder("Base64MTEF.txt"), ClipboardFormats.cfMML), New EquationOutputFileText(p.GetOutputFile("txt"), ""))
        End Sub

        Public Function ConvertMML2EPS(ByVal OutputPath As String) As Boolean
            Dim p As Program = New Program()
            Dim ce As ConvertEquation = New ConvertEquation()

            Try
                ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileEPS(OutputPath))
                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return False
            End Try
        End Function

        Public Sub Temp(ByVal ce As ConvertEquation)
            Dim xmlDoc As XmlDocument = New XmlDocument()
            xmlDoc.PreserveWhitespace = True
            xmlDoc.XmlResolver = Nothing

            Try
                xmlDoc.LoadXml(File.ReadAllText("D:\Support\MathML_EPS\input\ISTE_9781498757003_C005_ed_docbook.xml").Replace("&", "&amp;"))
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return
            End Try

            Clipboard.Clear()
            Dim NameSpaceManager As XmlNamespaceManager = New XmlNamespaceManager(xmlDoc.NameTable)
            NameSpaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance")
            NameSpaceManager.AddNamespace("aid5", "http://ns.adobe.com/AdobeInDesign/5.0/")
            NameSpaceManager.AddNamespace("aid", "http://ns.adobe.com/AdobeInDesign/4.0/")
            NameSpaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink/")
            NameSpaceManager.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")
            Dim MathNodeList As XmlNodeList = xmlDoc.SelectNodes("//equation")

            For m As Int32 = 0 To MathNodeList.Count - 1 - 1

                If MathNodeList(m).NodeType <> XmlNodeType.Element Then
                    Continue For
                End If

                Try
                    Dim MathContent As String = MathNodeList(m).InnerXml
                    Dim MathNode As XmlElement = (CType(MathNodeList(m).ChildNodes(0), XmlElement))
                    Dim MathFilename As String = MathNode.GetAttribute("id")
                    Clipboard.SetText(MathContent)
                    ce.Convert(New EquationInputClipboardText(ClipboardFormats.cfMML), New EquationOutputFileEPS("D:\Support\MathML_EPS\input\Data\" & MathFilename & ".eps"))
                    Clipboard.Clear()
                Catch ex As Exception
                    Continue For
                End Try
            Next
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class
End Namespace
