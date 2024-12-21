dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./lcov.info
move lcov.info ..\Niobium.Html
cd ..\Niobium.Html
code
pause