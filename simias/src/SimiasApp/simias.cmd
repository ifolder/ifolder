@if not exist web\inspection.wsil copy etc\inspection.wsil web\inspection.wsil >nul
@web\bin\SimiasApp.exe --applications /:web,/simias10:web --port 8086
