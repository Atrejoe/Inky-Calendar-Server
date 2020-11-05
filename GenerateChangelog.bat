:: fetch all tags & changes from remote, otherwise changes cannot be determined accurately

:: Assert changelog tool is installed
dotnet tool update --tool-path tools/changelog ^
CS.Changelog.Console

:: Prompt for release name
set /p name="Specify release name (optional):"

tools\changelog\changelog.exe ^
 --verbosity Info ^
 --repositoryurl "https://github.com/Atrejoe/Inky-Calendar-Server/commit/{0}" ^
 --filename "changelog" ^
 --outputformat Markdown ^
 --releasename "%name%"
