'
' VisualStudio 2005 Macros for Poderosa Project.
' 
' Copyright 2011 The Poderosa Project.
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
'
Imports System
Imports System.Collections
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics

Public Module PoderosaVSMacro

    '''<summary>
    '''Formats all C# files in the current solution
    '''</symmary>
    Public Sub FormatAllCSFiles()
        DTE.Documents.CloseAll(vsSaveChanges.vsSaveChangesPrompt)

        Dim OldProperties As New Hashtable
        SetEditorProperties(OldProperties)

        Dim Prj As Project
        Dim Item As ProjectItem

        For Each Prj In DTE.Solution.Projects
            For Each Item In Prj.ProjectItems
                FormatCSFiles(Item)
            Next
        Next

        RestoreEditorProperties(OldProperties)
    End Sub

    Private Sub FormatCSFiles(ByRef Item As ProjectItem)
        Dim SubItem As ProjectItem

        For Each SubItem In Item.ProjectItems
            FormatCSFiles(SubItem)
        Next

        If Item.Name.ToLower().EndsWith(".cs") Then
            Debug.Print("Format : " + Item.Name)
            Item.Open(Constants.vsViewKindCode)
            Item.Document.Activate()
            DTE.ExecuteCommand("Edit.FormatDocument")
            Item.Document.Close(vsSaveChanges.vsSaveChangesYes)
        Else
            Debug.Print("Skip   : " + Item.Name)
        End If
    End Sub


    '''<summary>
    '''Set format preferences of the C# editor
    '''</symmary>
    Public Sub SetEditorProperties()
        If MsgBox("Format preferences of the C# editor will be changed." + Environment.NewLine + "Continue ?", _
                MsgBoxStyle.YesNo, "Confirmation") <> MsgBoxResult.Yes Then
            Exit Sub
        End If

        Dim Dummy As New Hashtable
        SetEditorProperties(Dummy)

        MsgBox("C# editor's preferences were changed.", MsgBoxStyle.OkOnly, "Information")
    End Sub

    '''<summary>
    '''Dump current format preferences to the debug console
    '''</symmary>
    Public Sub DumpCurrentEditorProperties()
        Dim Props As Properties
        Dim P As EnvDTE.Property
        Dim L As New ArrayList

        Props = DTE.Properties("TextEditor", "CSharp")
        For Each P In Props
            L.Add(P.Name + " = " + P.Value.ToString())
        Next

        Props = DTE.Properties("TextEditor", "CSharp-Specific")
        For Each P In Props
            L.Add(P.Name + " = " + P.Value.ToString())
        Next

        L.Sort()

        Dim S As String
        For Each S In L
            Debug.Print(S)
        Next
    End Sub

    Private Sub SetEditorProperties(ByRef OldProperties As IDictionary)

        ' See http://msdn.microsoft.com/en-us/library/ms165644%28v=VS.80%29.aspx

        Dim Props As Properties
        Dim P As EnvDTE.Property

        Props = DTE.Properties("TextEditor", "CSharp")
        For Each P In Props
            Select Case P.Name
                Case "IndentSize"
                    OldProperties(P.Name) = P.Value
                    P.Value = 4
                Case "TabSize"
                    OldProperties(P.Name) = P.Value
                    P.Value = 4
                Case "InsertTabs"
                    OldProperties(P.Name) = P.Value
                    P.Value = False
            End Select
        Next

        Props = DTE.Properties("TextEditor", "CSharp-Specific")

        Dim L As New ArrayList
        For Each P In Props
            L.Add(P.Name + " = " + P.Value.ToString())
        Next
        L.Sort()
        Dim S As String
        For Each S In L
            Debug.Print(S)
        Next

        For Each P In Props
            'Debug.Print(P.Name + " = " + P.Value.ToString())
            Select Case P.Name
                Case "NewLines_Braces_AnonymousMethod"
                    OldProperties(P.Name) = P.Value
                    'P.Value = 1 'Default
                    P.Value = 0
                Case "NewLines_Braces_ArrayInitializer"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "NewLines_Braces_ControlFlow"
                    OldProperties(P.Name) = P.Value
                    'P.Value = 1 'Default
                    P.Value = 0
                Case "NewLines_Braces_Method"
                    OldProperties(P.Name) = P.Value
                    'P.Value = 1 'Default
                    P.Value = 0
                Case "NewLines_Braces_Type"
                    OldProperties(P.Name) = P.Value
                    'P.Value = 1 'Default
                    P.Value = 0
                Case "NewLines_Keywords_Catch"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "NewLines_Keywords_Else"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "NewLines_Keywords_Finally"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Indent_BlockContents"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Indent_Braces"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Indent_CaseContents"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Indent_CaseLabels"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Indent_FlushLabelsLeft"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Indent_UnindentLabels"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Space_AfterBasesColon"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Space_AfterCast"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_AfterComma"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Space_AfterDot"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_AfterMethodCallName"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_AfterMethodDeclarationName"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_AfterSemicolonsInForStatement"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Space_AroundBinaryOperator"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Space_BeforeBasesColon"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Space_BeforeComma"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_BeforeDot"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_BeforeOpenSquare"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_BeforeSemicolonsInForStatement"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_BetweenEmptyMethodCallParentheses"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_BetweenEmptyMethodDeclarationParentheses"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_BetweenEmptySquares"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_InControlFlowConstruct"
                    OldProperties(P.Name) = P.Value
                    P.Value = 1 'Default
                Case "Space_Normalize"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_WithinCastParentheses"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_WithinExpressionParentheses"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_WithinMethodCallParentheses"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_WithinMethodDeclarationParentheses"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_WithinOtherParentheses"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Space_WithinSquares"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Wrapping_IgnoreSpacesAroundBinaryOperators"
                    OldProperties(P.Name) = P.Value
                    P.Value = 0 'Default
                Case "Wrapping_KeepStatementsOnSingleLine"
                    OldProperties(P.Name) = P.Value
                    'P.Value = 1 'Default
                    P.Value = 0
                Case "Wrapping_PreserveSingleLine"
                    OldProperties(P.Name) = P.Value
                    'P.Value = 1 'Default
                    P.Value = 0
            End Select
        Next
    End Sub

    Private Sub RestoreEditorProperties(ByRef OldProperties As IDictionary)
        Dim Props As Properties
        Dim P As EnvDTE.Property

        Props = DTE.Properties("TextEditor", "CSharp")
        For Each P In Props
            If OldProperties.Contains(P.Name) Then
                P.Value = OldProperties(P.Name)
            End If
        Next

        Props = DTE.Properties("TextEditor", "CSharp-Specific")
        For Each P In Props
            If OldProperties.Contains(P.Name) Then
                P.Value = OldProperties(P.Name)
            End If
        Next
    End Sub

End Module
