setlocal

set layout1=layout1.html
set layout2=layout2.html
set dest=".."

del %dest%\*.html

for %%I in (*.html) do (
	if not %%I == %layout1% (
		if not %%I == %layout2% (
		type %layout1% > %dest%\%%~nxI
		type %%I       >> %dest%\%%~nxI
		type %layout2% >> %dest%\%%~nxI
		)
	)
)


for %%I in (Tutorial\*.html) do (
	type %layout1% > %dest%\%%~nxI
	type %%I       >> %dest%\%%~nxI
	type %layout2% >> %dest%\%%~nxI
)


endlocal

