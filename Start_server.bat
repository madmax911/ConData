@Echo off
Setlocal EnableDelayedExpansion

C:
Cd \Data

:StartNode

Echo.
Echo Node start on %date% %time%
Echo.

node server.js

Echo.
Echo Node ended on %date% %time%
Echo.
Goto Choose


:Choose

Choice /T 2 /C RPQC /N /D R /M "(R)estart, (P)ause, (Q)uit, (C)ustom"

Set MyChoice="%ERRORLEVEL%"

If %MyChoice%=="1" Goto StartNode
If %MyChoice%=="2" Pause
If %MyChoice%=="3" Goto EndScript
If %MyChoice%=="4" Goto Custom
Goto Choose


:Custom

Echo Custom action called!  Returning to menu...
Goto Choose






:EndScript

Echo.
Echo Script ended on %date% %time%
Echo.
Rem Pause
