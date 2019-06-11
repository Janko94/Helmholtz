Imports System.IO
Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization


Public Class Example
    
    Public Function CastDataForCbr(ByVal docDataNew As Object, ByVal docDataOld As Object) As Object

        For Each docNewproperty As PropertyInfo In docDataNew.GetType().GetProperties()
            Dim propertyName As String = docNewproperty.Name

            If propertyName.Contains("1Member") AndAlso propertyName.EndsWith("1Member") Then
                propertyName = propertyName.Replace("1Member", "1")
            End If

            If docDataOld.GetType().GetProperty(propertyName) IsNot Nothing OrElse docDataOld.GetType().GetProperty(String.Format("{0}Member", propertyName)) IsNot Nothing Then

                If docDataOld.GetType().GetProperty(String.Format("{0}Member", propertyName)) IsNot Nothing Then
                    propertyName += "Member"
                End If

                Dim propertyOldToObject As Object = docDataOld.GetType().GetProperty(propertyName).GetValue(docDataOld)
                If propertyOldToObject IsNot Nothing Then
                    Dim type As Type = docNewproperty.PropertyType
                    If type.IsClass AndAlso Not docNewproperty.PropertyType.IsGenericType AndAlso Not docNewproperty.PropertyType = GetType(String) Then
                        Dim propertyNewToObject As Object = GetObj(type)
                        If propertyNewToObject IsNot Nothing Then
                            CastDataForCbr(propertyNewToObject, propertyOldToObject)
                            docDataNew.GetType().GetProperty(docNewproperty.Name).SetValue(docDataNew, propertyNewToObject)
                        End If
                    Else
                        If propertyOldToObject IsNot Nothing Then
                            If GetType(IList).IsAssignableFrom(type) AndAlso type.IsGenericType Then
                                Dim propertyNewToObject As Object = GetObj(type)
                                For Each item As Object In propertyOldToObject
                                    Dim item1 As Object = GetObj(docNewproperty.PropertyType.GenericTypeArguments()(0))
                                    item1 = CastDataForCbr(item1, item)
                                    propertyNewToObject.GetType().GetMethod("Add").Invoke(propertyNewToObject, New Object() {item1})
                                Next
                                docDataNew.GetType().GetProperty(docNewproperty.Name).SetValue(docDataNew, propertyNewToObject)
                            Else
                                docDataNew.GetType().GetProperty(docNewproperty.Name).SetValue(docDataNew, propertyOldToObject)
                            End If
                        End If

                    End If
                End If
            End If
        Next
        Return docDataNew
    End Function
    Private Function GetObj(ByVal ObjectType As Type) As Object
        Dim connectionArray() As Object = New Object() {}
        'If ObjectType.Name = "CaseCommon" Then
        '    connectionArray = Nothing
        'End If
        If ObjectType IsNot Nothing Then
            Dim info() As ConstructorInfo = ObjectType.GetConstructors()
            If info.Length > 0 Then
                For i As Integer = 0 To info.Length
                    Try
                        Return info(i).Invoke(connectionArray)
                    Catch ex As Exception
                        'try to find the right constructor
                    End Try
                Next
            End If
        End If
        Return Nothing
    End Function
End Class


