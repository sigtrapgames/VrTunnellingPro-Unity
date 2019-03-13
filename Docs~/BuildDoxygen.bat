@echo off
doxygen docs.cfg
echo ===================
if /i "%errorlevel%" EQU "0" (
	echo Build Complete
) else (
	echo Build Error: %errorlevel%
)