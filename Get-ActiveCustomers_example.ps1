$(Invoke-RestMethod http://localhost:8888 -Method Post -Body "MDB://C:\Data\Sample.mdb`r`n //// `r`n SELECT * FROM Customers WHERE Active").Values | ForEach-Object {$_ -join "`t"}
