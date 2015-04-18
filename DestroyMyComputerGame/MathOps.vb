Option Strict On
Imports System.Math 'For REALLY obvious reasons, this class is helpful

Public Class MathOps

    'Finds the nearest power of two
    'Reference: http://en.wikipedia.org/wiki/Power_of_two#Algorithm_to_convert_any_number_into_nearest_power_of_two_number
    'Author: Dylan Taylor
    Public Shared Function NearestPowerOfTwo(ByVal number As Integer) As Integer
        Return CInt(2 ^ roundUp(LogBase(2, (number))))
    End Function

    'Finds a logarithm using a base other than 10
    'Author: Dylan Taylor
    Public Shared Function LogBase(ByVal base As Integer, ByVal x As Double) As Double
        Return (Log(x) / Log(base))
    End Function


    'Rounds a decimal number up to the nearest integer, never rounding down
    'Author: Dylan Taylor
    Public Shared Function roundUp(ByVal dblValue As Double) As Integer
        'Converts the number to a string, and finds the first decimal point in the string, or 0 if no decimal point is found
        Dim decimalPointPosition As Integer = InStr(1, CStr(dblValue), ".", vbTextCompare)
        'Returns an integer 1 more than the original value if the number contains a value
        'Return (If((decimalPointPosition > 0), ((CDbl(Left(CStr(dblValue), decimalPointPosition))) + 1), dblValue))
        Return CInt(If((decimalPointPosition > 0), CDbl(Int(dblValue)) + 1, dblValue))
    End Function
    
    'Locates ''numPoints'' points on a circle
    'Reference: http://en.wikipedia.org/wiki/Circle#Cartesian_coordinates
    'Rewritten in Visual Basic by Dylan Taylor
    Public Shared Function GetPointsOnCircle(ByVal center As System.Drawing.Point, ByVal radius As Integer, ByVal numPoints As Integer) As List(Of System.Drawing.Point)
        Dim alpha As Double = (6.283) / numPoints 'Math.PI * 2 = 6.283185
    	Dim theta as Double
    	Dim points As List(Of System.Drawing.Point) = New List(Of System.Drawing.Point)
    	Dim pX As Integer
    	Dim pY As Integer
    	For I As Integer = 0 To numPoints
    		theta = alpha * I    		
    		pX = CInt(center.X + (Math.cos(theta) * radius))
    		pY = CInt(center.Y + (Math.sin(theta) * radius))
            'Game.SayLine("Adding point " & I & " of " & numPoints & " around center (" & center.X & ", " _
            '	& center.Y & ") -- X: " & pX & " Y: " & pY)
    		points.Add(New System.Drawing.Point(pX, pY))
    	Next I
    	Return points
    End Function
End Class
